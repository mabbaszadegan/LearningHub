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
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly UserManager<User> _userManager;

    public EnrollInCourseCommandHandler(
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

        StudentProfile? studentProfile = null;
        if (request.StudentProfileId.HasValue)
        {
            studentProfile = await _studentProfileRepository.GetByIdForUserAsync(request.StudentProfileId.Value, request.StudentId, cancellationToken);
            if (studentProfile == null)
            {
                return Result<CourseEnrollmentDto>.Failure("پروفایل دانش‌آموز معتبر نیست");
            }
        }

        // Check if student is already enrolled for this profile context
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

        var existingEnrollment = await enrollmentQuery.FirstOrDefaultAsync(cancellationToken);

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
                existingEnrollment.AssignStudentProfile(request.StudentProfileId);
                await _enrollmentRepository.UpdateAsync(existingEnrollment, cancellationToken);
            }
        }
        else
        {
            // Create new enrollment
            var enrollment = Domain.Entities.CourseEnrollment.Create(request.StudentId, request.CourseId, LearningMode.SelfStudy, request.StudentProfileId);
            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

            // Grant default course access (Full access) or reactivate existing access record
            var existingAccess = await _accessRepository
                .GetAll()
                .FirstOrDefaultAsync(a => a.CourseId == request.CourseId && a.StudentId == request.StudentId, cancellationToken);

            if (existingAccess != null)
            {
                existingAccess.Activate();
                existingAccess.UpdateAccessLevel(CourseAccessLevel.Full);
                await _accessRepository.UpdateAsync(existingAccess, cancellationToken);
            }
            else
            {
                var access = Domain.Entities.CourseAccess.Create(request.StudentId, request.CourseId, CourseAccessLevel.Full);
                await _accessRepository.AddAsync(access, cancellationToken);
            }

            existingEnrollment = enrollment;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return enrollment DTO
        var enrollmentDto = new CourseEnrollmentDto
        {
            Id = existingEnrollment.Id,
            StudentId = existingEnrollment.StudentId,
            StudentProfileId = existingEnrollment.StudentProfileId,
            StudentProfileName = studentProfile?.DisplayName ?? existingEnrollment.StudentProfile?.DisplayName,
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
