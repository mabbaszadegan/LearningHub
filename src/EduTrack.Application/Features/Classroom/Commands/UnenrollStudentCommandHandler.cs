using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Application.Features.Classroom.Commands;

public class UnenrollStudentCommandValidator : AbstractValidator<UnenrollStudentCommand>
{
    public UnenrollStudentCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("Class ID is required");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
    }
}

public class UnenrollStudentCommandHandler : IRequestHandler<UnenrollStudentCommand, Result<bool>>
{
    private readonly IRepository<Enrollment> _enrollmentRepository;
    private readonly IRepository<Class> _classRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly UserManager<User> _userManager;

    public UnenrollStudentCommandHandler(
        IRepository<Enrollment> enrollmentRepository,
        IRepository<Class> classRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        IClock clock,
        UserManager<User> userManager)
    {
        _enrollmentRepository = enrollmentRepository;
        _classRepository = classRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(UnenrollStudentCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await _classRepository.GetByIdAsync(request.ClassId, cancellationToken);
        if (classEntity == null)
        {
            return Result<bool>.Failure("Class not found");
        }

        var student = await _userService.GetUserByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Result<bool>.Failure("Student not found");
        }

        var studentRoles = await _userManager.GetRolesAsync(student);
        if (!studentRoles.Contains("Student"))
        {
            return Result<bool>.Failure("User does not have student role");
        }

        var enrollment = await _enrollmentRepository.GetAll()
            .FirstOrDefaultAsync(e => e.ClassId == request.ClassId && 
                                    e.StudentId == request.StudentId && 
                                    e.IsActive, cancellationToken);

        if (enrollment == null)
        {
            return Result<bool>.Failure("Student is not enrolled in this class");
        }

        // Soft delete - mark as inactive instead of deleting
        enrollment.Deactivate();
        enrollment.Complete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
