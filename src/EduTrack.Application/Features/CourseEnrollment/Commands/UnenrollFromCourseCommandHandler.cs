using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.CourseEnrollment.Commands;

/// <summary>
/// Validator for UnenrollFromCourseCommand
/// </summary>
public class UnenrollFromCourseCommandValidator : AbstractValidator<UnenrollFromCourseCommand>
{
    public UnenrollFromCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Course ID is required");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
    }
}

/// <summary>
/// Handler for UnenrollFromCourseCommand
/// </summary>
public class UnenrollFromCourseCommandHandler : IRequestHandler<UnenrollFromCourseCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<Domain.Entities.CourseAccess> _accessRepository;
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly UserManager<User> _userManager;

    public UnenrollFromCourseCommandHandler(
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository,
        IRepository<Course> courseRepository,
        IRepository<Domain.Entities.CourseAccess> accessRepository,
        IStudentProfileRepository studentProfileRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        IClock clock,
        UserManager<User> userManager)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _accessRepository = accessRepository;
        _studentProfileRepository = studentProfileRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(UnenrollFromCourseCommand request, CancellationToken cancellationToken)
    {
        // Validate course exists
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            return Result<bool>.Failure("Course not found");
        }

        // Validate student exists and has student role
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

        if (request.StudentProfileId.HasValue)
        {
            var profile = await _studentProfileRepository.GetByIdForUserAsync(request.StudentProfileId.Value, request.StudentId, cancellationToken);
            if (profile == null)
            {
                return Result<bool>.Failure("پروفایل دانش‌آموز معتبر نیست");
            }
        }

        // Check if student is enrolled for this profile context
        var enrollmentQuery = _enrollmentRepository.GetAll()
            .Where(e => e.CourseId == request.CourseId && e.StudentId == request.StudentId);

        if (request.StudentProfileId.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.StudentProfileId == request.StudentProfileId);
        }
        else
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.StudentProfileId == null);
        }

        var enrollment = await enrollmentQuery.FirstOrDefaultAsync(cancellationToken);

        if (enrollment == null)
        {
            return Result<bool>.Failure("Student is not enrolled in this course");
        }

        if (!enrollment.IsActive)
        {
            return Result<bool>.Failure("Student is already unenrolled from this course");
        }

        // Deactivate enrollment instead of deleting it to maintain history
        enrollment.Deactivate();
        await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        // Also revoke course access
        var access = await _accessRepository.GetAll()
            .FirstOrDefaultAsync(a => a.CourseId == request.CourseId && a.StudentId == request.StudentId, cancellationToken);

        if (access != null)
        {
            access.RevokeAccess();
            await _accessRepository.UpdateAsync(access, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
