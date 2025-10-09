using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using MediatR;

namespace EduTrack.Application.Features.CourseEnrollment.Queries;

/// <summary>
/// Query to get all course enrollments for a student
/// </summary>
public record GetStudentCourseEnrollmentsQuery(
    string StudentId,
    bool IncludeCompleted = true,
    bool IncludeInactive = false) : IRequest<Result<List<StudentCourseEnrollmentSummaryDto>>>;

/// <summary>
/// Query to get course enrollment details
/// </summary>
public record GetCourseEnrollmentQuery(
    int CourseId,
    string StudentId) : IRequest<Result<CourseEnrollmentDto>>;

/// <summary>
/// Query to get course access details
/// </summary>
public record GetCourseAccessQuery(
    int CourseId,
    string StudentId) : IRequest<Result<CourseAccessDto>>;

/// <summary>
/// Query to get all students enrolled in a course
/// </summary>
public record GetCourseStudentsQuery(
    int CourseId,
    bool IncludeCompleted = true,
    bool IncludeInactive = false,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<CourseEnrollmentDto>>>;

/// <summary>
/// Query to get course enrollment statistics
/// </summary>
public record GetCourseEnrollmentStatsQuery(
    int CourseId) : IRequest<Result<CourseEnrollmentStatsDto>>;

/// <summary>
/// Query to get available courses for enrollment
/// </summary>
public record GetAvailableCoursesForEnrollmentQuery(
    string StudentId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<CourseDto>>>;

/// <summary>
/// Query to check if student can enroll in a course
/// </summary>
public record CanEnrollInCourseQuery(
    int CourseId,
    string StudentId) : IRequest<Result<bool>>;

/// <summary>
/// Query to get student's course progress
/// </summary>
public record GetStudentCourseProgressQuery(
    int CourseId,
    string StudentId) : IRequest<Result<StudentCourseProgressDto>>;

/// <summary>
/// Query to get recent course enrollments
/// </summary>
public record GetRecentCourseEnrollmentsQuery(
    int Count = 10) : IRequest<Result<List<CourseEnrollmentDto>>>;
