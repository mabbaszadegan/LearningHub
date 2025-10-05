using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Domain.Repositories;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.CourseEnrollment.Queries;

/// <summary>
/// Handler for GetStudentCourseEnrollmentsQuery
/// </summary>
public class GetStudentCourseEnrollmentsQueryHandler : IRequestHandler<GetStudentCourseEnrollmentsQuery, Result<List<StudentCourseEnrollmentSummaryDto>>>
{
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;
    private readonly IRepository<Domain.Entities.CourseAccess> _accessRepository;

    public GetStudentCourseEnrollmentsQueryHandler(
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository,
        IRepository<Domain.Entities.CourseAccess> accessRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _accessRepository = accessRepository;
    }

    public async Task<Result<List<StudentCourseEnrollmentSummaryDto>>> Handle(GetStudentCourseEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _enrollmentRepository.GetAll()
            .Include(e => e.Course)
            .Include(e => e.Student)
            .Where(e => e.StudentId == request.StudentId);

        if (!request.IncludeCompleted)
        {
            query = query.Where(e => !e.IsCompleted);
        }

        if (!request.IncludeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        var enrollments = await query
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);

        var result = new List<StudentCourseEnrollmentSummaryDto>();

        foreach (var enrollment in enrollments)
        {
            // Get course access
            var access = await _accessRepository.GetAll()
                .FirstOrDefaultAsync(a => a.CourseId == enrollment.CourseId && a.StudentId == enrollment.StudentId, cancellationToken);

            // Calculate course statistics
            var totalLessons = enrollment.Course.Modules.Sum(m => m.Lessons.Count);
            var completedLessons = 0; // TODO: Calculate from progress tracking
            var totalExams = 0; // TODO: Calculate from course exams
            var completedExams = 0; // TODO: Calculate from attempts
            var averageScore = 0.0; // TODO: Calculate from attempts

            var summaryDto = new StudentCourseEnrollmentSummaryDto
            {
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.Course.Title,
                CourseDescription = enrollment.Course.Description,
                CourseThumbnail = enrollment.Course.Thumbnail,
                EnrolledAt = enrollment.EnrolledAt,
                ProgressPercentage = enrollment.ProgressPercentage,
                IsCompleted = enrollment.IsCompleted,
                LastAccessedAt = enrollment.LastAccessedAt,
                AccessLevel = access?.AccessLevel ?? CourseAccessLevel.None,
                AccessLevelName = GetAccessLevelName(access?.AccessLevel ?? CourseAccessLevel.None),
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                TotalExams = totalExams,
                CompletedExams = completedExams,
                AverageScore = averageScore
            };

            result.Add(summaryDto);
        }

        return Result<List<StudentCourseEnrollmentSummaryDto>>.Success(result);
    }

    private static string GetAccessLevelName(CourseAccessLevel accessLevel)
    {
        return accessLevel switch
        {
            CourseAccessLevel.None => "بدون دسترسی",
            CourseAccessLevel.ViewOnly => "فقط مشاهده",
            CourseAccessLevel.Lessons => "دسترسی به درس‌ها",
            CourseAccessLevel.Exams => "دسترسی به آزمون‌ها",
            CourseAccessLevel.Resources => "دسترسی به منابع",
            CourseAccessLevel.Full => "دسترسی کامل",
            _ => "نامشخص"
        };
    }
}
