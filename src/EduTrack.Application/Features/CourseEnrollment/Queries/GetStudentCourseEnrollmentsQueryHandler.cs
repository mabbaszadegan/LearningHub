using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Domain.Repositories;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Extensions;
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
    private readonly IRepository<Domain.Entities.ScheduleItem> _scheduleItemRepository;
    private readonly IRepository<Domain.Entities.Chapter> _chapterRepository;
    private readonly IRepository<Domain.Entities.SubChapter> _subChapterRepository;
    private readonly IRepository<Domain.Entities.TeachingPlan> _teachingPlanRepository;

    public GetStudentCourseEnrollmentsQueryHandler(
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository,
        IRepository<Domain.Entities.CourseAccess> accessRepository,
        IRepository<Domain.Entities.ScheduleItem> scheduleItemRepository,
        IRepository<Domain.Entities.Chapter> chapterRepository,
        IRepository<Domain.Entities.SubChapter> subChapterRepository,
        IRepository<Domain.Entities.TeachingPlan> teachingPlanRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _accessRepository = accessRepository;
        _scheduleItemRepository = scheduleItemRepository;
        _chapterRepository = chapterRepository;
        _subChapterRepository = subChapterRepository;
        _teachingPlanRepository = teachingPlanRepository;
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

            // Get schedule items statistics for this course
            var scheduleItemStats = await GetScheduleItemStats(enrollment.CourseId, request.StudentId, cancellationToken);

            // Get course structure statistics
            var totalChapters = await _chapterRepository.GetAll()
                .CountAsync(c => c.CourseId == enrollment.CourseId, cancellationToken);
            
            var totalSubChapters = await _subChapterRepository.GetAll()
                .CountAsync(sc => sc.Chapter.CourseId == enrollment.CourseId, cancellationToken);
            
            var totalTeachingPlans = await _teachingPlanRepository.GetAll()
                .CountAsync(tp => tp.CourseId == enrollment.CourseId, cancellationToken);

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
                AverageScore = averageScore,
                TotalChapters = totalChapters,
                TotalSubChapters = totalSubChapters,
                TotalTeachingPlans = totalTeachingPlans,
                ScheduleItemStats = scheduleItemStats,
                TotalScheduleItems = scheduleItemStats.Sum(s => s.TotalCount),
                CompletedScheduleItems = scheduleItemStats.Sum(s => s.CompletedCount)
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

    private async Task<List<ScheduleItemTypeStatsDto>> GetScheduleItemStats(int courseId, string studentId, CancellationToken cancellationToken)
    {
        // Get all schedule items for this course
        var scheduleItems = await _scheduleItemRepository.GetAll()
            .Include(si => si.TeachingPlan)
            .Where(si => si.TeachingPlan.CourseId == courseId)
            .ToListAsync(cancellationToken);

        // Group by type and calculate statistics
        var statsByType = scheduleItems
            .GroupBy(si => si.Type)
            .Select(g => new ScheduleItemTypeStatsDto
            {
                Type = g.Key,
                TypeName = g.Key.GetDisplayName(),
                TotalCount = g.Count(),
                CompletedCount = 0, // TODO: Calculate from student progress
                CompletionPercentage = 0.0, // TODO: Calculate from student progress
                IconClass = GetScheduleItemIcon(g.Key),
                ColorClass = GetScheduleItemColor(g.Key)
            })
            .Where(s => s.TotalCount > 0) // Only include types that have items
            .OrderBy(s => s.Type)
            .ToList();

        return statsByType;
    }

    private static string GetScheduleItemIcon(ScheduleItemType type)
    {
        return type switch
        {
            ScheduleItemType.Reminder => "fas fa-bell",
            ScheduleItemType.Writing => "fas fa-pen",
            ScheduleItemType.Audio => "fas fa-microphone",
            ScheduleItemType.GapFill => "fas fa-edit",
            ScheduleItemType.MultipleChoice => "fas fa-list-ul",
            ScheduleItemType.Match => "fas fa-link",
            ScheduleItemType.ErrorFinding => "fas fa-search",
            ScheduleItemType.CodeExercise => "fas fa-code",
            ScheduleItemType.Quiz => "fas fa-question-circle",
            _ => "fas fa-tasks"
        };
    }

    private static string GetScheduleItemColor(ScheduleItemType type)
    {
        return type switch
        {
            ScheduleItemType.Reminder => "text-info",
            ScheduleItemType.Writing => "text-primary",
            ScheduleItemType.Audio => "text-success",
            ScheduleItemType.GapFill => "text-warning",
            ScheduleItemType.MultipleChoice => "text-danger",
            ScheduleItemType.Match => "text-purple",
            ScheduleItemType.ErrorFinding => "text-orange",
            ScheduleItemType.CodeExercise => "text-dark",
            ScheduleItemType.Quiz => "text-indigo",
            _ => "text-secondary"
        };
    }
}
