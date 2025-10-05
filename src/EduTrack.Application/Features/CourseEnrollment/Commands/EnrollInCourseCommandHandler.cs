using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.CourseEnrollment.Commands;

/// <summary>
/// Validator for EnrollInCourseCommand
/// </summary>
public class EnrollInCourseCommandValidator : AbstractValidator<EnrollInCourseCommand>
{
    public EnrollInCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Course ID is required");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
    }
}

/// <summary>
/// Handler for EnrollInCourseCommand
/// </summary>
public class EnrollInCourseCommandHandler : IRequestHandler<EnrollInCourseCommand, Result<CourseEnrollmentDto>>
{
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<Domain.Entities.CourseAccess> _accessRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly UserManager<User> _userManager;

    public EnrollInCourseCommandHandler(
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository,
        IRepository<Course> courseRepository,
        IRepository<Domain.Entities.CourseAccess> accessRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        IClock clock,
        UserManager<User> userManager)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _accessRepository = accessRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _userManager = userManager;
    }

    public async Task<Result<CourseEnrollmentDto>> Handle(EnrollInCourseCommand request, CancellationToken cancellationToken)
    {
        // Validate course exists
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            return Result<CourseEnrollmentDto>.Failure("Course not found");
        }

        if (!course.IsActive)
        {
            return Result<CourseEnrollmentDto>.Failure("Course is not active");
        }

        // Validate student exists and has student role
        var student = await _userService.GetUserByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Result<CourseEnrollmentDto>.Failure("Student not found");
        }

        var studentRoles = await _userManager.GetRolesAsync(student);
        if (!studentRoles.Contains("Student"))
        {
            return Result<CourseEnrollmentDto>.Failure("User does not have student role");
        }

        // Check if student is already enrolled
        var existingEnrollment = await _enrollmentRepository.GetAll()
            .FirstOrDefaultAsync(e => e.CourseId == request.CourseId && e.StudentId == request.StudentId, cancellationToken);

        if (existingEnrollment != null)
        {
            if (existingEnrollment.IsActive)
            {
                return Result<CourseEnrollmentDto>.Failure("Student is already enrolled in this course");
            }
            else
            {
                // Reactivate existing enrollment
                existingEnrollment.Activate();
                existingEnrollment.UpdateLastAccessed();
                await _enrollmentRepository.UpdateAsync(existingEnrollment, cancellationToken);
            }
        }
        else
        {
            // Create new enrollment
            var enrollment = Domain.Entities.CourseEnrollment.Create(request.StudentId, request.CourseId);
            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

            // Grant default course access (Full access)
            var access = Domain.Entities.CourseAccess.Create(request.StudentId, request.CourseId, CourseAccessLevel.Full);
            await _accessRepository.AddAsync(access, cancellationToken);

            existingEnrollment = enrollment;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return enrollment DTO
        var enrollmentDto = new CourseEnrollmentDto
        {
            Id = existingEnrollment.Id,
            StudentId = existingEnrollment.StudentId,
            CourseId = existingEnrollment.CourseId,
            StudentName = student.FullName,
            CourseTitle = course.Title,
            EnrolledAt = existingEnrollment.EnrolledAt,
            CompletedAt = existingEnrollment.CompletedAt,
            IsActive = existingEnrollment.IsActive,
            LastAccessedAt = existingEnrollment.LastAccessedAt,
            ProgressPercentage = existingEnrollment.ProgressPercentage,
            IsCompleted = existingEnrollment.IsCompleted,
            Duration = existingEnrollment.GetDuration(),
            TimeSinceLastAccess = existingEnrollment.GetTimeSinceLastAccess()
        };

        return Result<CourseEnrollmentDto>.Success(enrollmentDto);
    }
}
