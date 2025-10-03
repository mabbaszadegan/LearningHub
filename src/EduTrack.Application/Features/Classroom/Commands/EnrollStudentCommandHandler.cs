using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Classroom.Commands;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("Class ID is required");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
    }
}

public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Result<bool>>
{
    private readonly IRepository<Enrollment> _enrollmentRepository;
    private readonly IRepository<Class> _classRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public EnrollStudentCommandHandler(
        IRepository<Enrollment> enrollmentRepository,
        IRepository<Class> classRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _enrollmentRepository = enrollmentRepository;
        _classRepository = classRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<bool>> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await _classRepository.GetByIdAsync(request.ClassId, cancellationToken);
        if (classEntity == null)
        {
            return Result<bool>.Failure("Class not found");
        }

        var student = await _userService.GetUserByIdAsync(request.StudentId, cancellationToken);
        if (student == null || student.Role != Domain.Enums.UserRole.Student)
        {
            return Result<bool>.Failure("Student not found or invalid role");
        }

        // Check if student is already enrolled
        var existingEnrollment = await _enrollmentRepository.GetAll()
            .FirstOrDefaultAsync(e => e.ClassId == request.ClassId && e.StudentId == request.StudentId, cancellationToken);

        if (existingEnrollment != null)
        {
            return Result<bool>.Failure("Student is already enrolled in this class");
        }

        var enrollment = Enrollment.Create(request.ClassId, request.StudentId);

        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
