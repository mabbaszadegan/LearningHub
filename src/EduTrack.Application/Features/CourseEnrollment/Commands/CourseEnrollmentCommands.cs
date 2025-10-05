using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.CourseEnrollment.Commands;

/// <summary>
/// Command to enroll a student in a course
/// </summary>
public record EnrollInCourseCommand(
    int CourseId,
    string StudentId) : IRequest<Result<CourseEnrollmentDto>>;

/// <summary>
/// Command to unenroll a student from a course
/// </summary>
public record UnenrollFromCourseCommand(
    int CourseId,
    string StudentId) : IRequest<Result<bool>>;

/// <summary>
/// Command to grant course access to a student
/// </summary>
public record GrantCourseAccessCommand(
    int CourseId,
    string StudentId,
    CourseAccessLevel AccessLevel,
    string? GrantedBy = null,
    DateTimeOffset? ExpiresAt = null,
    string? Notes = null) : IRequest<Result<CourseAccessDto>>;

/// <summary>
/// Command to revoke course access from a student
/// </summary>
public record RevokeCourseAccessCommand(
    int CourseId,
    string StudentId,
    string? RevokedBy = null) : IRequest<Result<bool>>;

/// <summary>
/// Command to update course enrollment progress
/// </summary>
public record UpdateCourseProgressCommand(
    int CourseId,
    string StudentId,
    int ProgressPercentage) : IRequest<Result<bool>>;

/// <summary>
/// Command to complete course enrollment
/// </summary>
public record CompleteCourseEnrollmentCommand(
    int CourseId,
    string StudentId) : IRequest<Result<bool>>;

/// <summary>
/// Command to update course access level
/// </summary>
public record UpdateCourseAccessLevelCommand(
    int CourseId,
    string StudentId,
    CourseAccessLevel NewAccessLevel,
    string? UpdatedBy = null) : IRequest<Result<bool>>;
