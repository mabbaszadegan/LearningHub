using EduTrack.Application.Common.Models;
using ScheduleItemGroupAssignmentDto = EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemGroupAssignmentDto;
using ScheduleItemStudentAssignmentDto = EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemStudentAssignmentDto;
using ScheduleItemSubChapterAssignmentDto = EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemSubChapterAssignmentDto;
using TeachingPlanScheduleItemDto = EduTrack.Application.Common.Models.TeachingPlans.ScheduleItemDto;
using TeachingPlanScheduleItemStatus = EduTrack.Application.Common.Models.TeachingPlans.ScheduleItemStatus;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.CourseEnrollment.Queries;

/// <summary>
/// Handler for GetCourseScheduleItemsQuery
/// </summary>
public class GetCourseScheduleItemsQueryHandler : IRequestHandler<GetCourseScheduleItemsQuery, Result<List<TeachingPlanScheduleItemDto>>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;

    public GetCourseScheduleItemsQueryHandler(
        IScheduleItemRepository scheduleItemRepository,
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<Result<List<TeachingPlanScheduleItemDto>>> Handle(GetCourseScheduleItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if student is enrolled in the course
            var enrollmentQuery = _enrollmentRepository.GetAll()
                .Where(e => e.CourseId == request.CourseId && e.StudentId == request.StudentId && e.IsActive);

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
                return Result<List<TeachingPlanScheduleItemDto>>.Failure("Student is not enrolled in this course");
            }

            List<ScheduleItem> scheduleItems;

            if (request.StudentProfileId.HasValue)
            {
                var accessibleItems = await _scheduleItemRepository.GetScheduleItemsAccessibleToStudentAsync(
                    request.StudentId,
                    request.StudentProfileId.Value,
                    cancellationToken);

                scheduleItems = accessibleItems
                    .Where(si =>
                        (si.CourseId.HasValue && si.CourseId == request.CourseId) ||
                        (si.TeachingPlan != null && si.TeachingPlan.CourseId == request.CourseId))
                    .OrderBy(si => si.StartDate)
                    .ToList();
            }
            else
            {
                // Legacy fallback: return all course items (pre profile-access behaviour)
                scheduleItems = await _scheduleItemRepository.GetAll()
                    .Include(si => si.TeachingPlan)
                    .Include(si => si.GroupAssignments)
                        .ThenInclude(ga => ga.StudentGroup)
                    .Include(si => si.SubChapterAssignments)
                        .ThenInclude(sca => sca.SubChapter)
                            .ThenInclude(sc => sc.Chapter)
                    .Include(si => si.StudentAssignments)
                        .ThenInclude(sa => sa.StudentProfile)
                    .Include(si => si.Lesson)
                    .Where(si => (si.CourseId.HasValue && si.CourseId == request.CourseId) ||
                                 (si.TeachingPlan != null && si.TeachingPlan.CourseId == request.CourseId))
                    .OrderBy(si => si.StartDate)
                    .ToListAsync(cancellationToken);
            }

            // Convert to DTOs
            var scheduleItemDtos = scheduleItems.Select(si => new TeachingPlanScheduleItemDto
            {
                Id = si.Id,
                TeachingPlanId = si.TeachingPlanId,
                CourseId = si.CourseId ?? si.TeachingPlan?.CourseId,
                    SessionReportId = si.SessionReportId,
                GroupId = si.GroupId,
                GroupName = si.Group?.Name,
                LessonId = si.LessonId,
                LessonTitle = si.Lesson?.Title,
                Type = si.Type,
                Title = si.Title,
                Description = si.Description,
                StartDate = si.StartDate,
                DueDate = si.DueDate,
                IsMandatory = si.IsMandatory,
                DisciplineHint = si.DisciplineHint,
                ContentJson = si.ContentJson,
                MaxScore = si.MaxScore,
                CreatedAt = si.CreatedAt,
                UpdatedAt = si.UpdatedAt,
                IsOverdue = si.IsOverdue(),
                IsUpcoming = si.IsUpcoming(),
                IsActive = si.IsActive(),
                TimeUntilDue = si.GetTimeUntilDue(),
                SubmissionCount = 0, // TODO: Calculate from submissions
                CompletedSubmissions = 0, // TODO: Calculate from submissions
                GroupAssignments = si.GroupAssignments.Select(ga => new ScheduleItemGroupAssignmentDto
                {
                    Id = ga.Id,
                    ScheduleItemId = ga.ScheduleItemId,
                    StudentGroupId = ga.StudentGroupId,
                    GroupName = ga.StudentGroup?.Name ?? ""
                }).ToList(),
                SubChapterAssignments = si.SubChapterAssignments.Select(sca => new ScheduleItemSubChapterAssignmentDto
                {
                    Id = sca.Id,
                    ScheduleItemId = sca.ScheduleItemId,
                    SubChapterId = sca.SubChapterId,
                    SubChapterTitle = sca.SubChapter?.Title ?? "",
                    ChapterTitle = sca.SubChapter?.Chapter?.Title ?? ""
                }).ToList(),
                StudentAssignments = si.StudentAssignments.Select(sa => new ScheduleItemStudentAssignmentDto
                {
                    Id = sa.Id,
                    ScheduleItemId = sa.ScheduleItemId,
                    StudentProfileId = sa.StudentProfileId,
                    StudentUserId = sa.StudentProfile?.UserId ?? string.Empty,
                    StudentDisplayName = sa.StudentProfile?.DisplayName ?? string.Empty
                }).ToList(),
                StudentProfileIds = si.StudentAssignments.Select(sa => sa.StudentProfileId).ToList(),
                IsAssignedToAllGroups = si.IsAssignedToAllGroups(),
                CurrentStep = si.CurrentStep,
                Status = si.IsCompleted ? TeachingPlanScheduleItemStatus.Completed : 
                        si.IsUpcoming() ? TeachingPlanScheduleItemStatus.Published :
                        si.IsActive() ? TeachingPlanScheduleItemStatus.Active : TeachingPlanScheduleItemStatus.Expired,
                StatusText = si.IsCompleted ? "تکمیل شده" : 
                           si.IsUpcoming() ? "آینده" :
                           si.IsActive() ? "فعال" : "منقضی شده",
                TypeName = GetScheduleItemTypeName(si.Type)
            }).ToList();

            return Result<List<TeachingPlanScheduleItemDto>>.Success(scheduleItemDtos);
        }
        catch (Exception ex)
        {
            return Result<List<TeachingPlanScheduleItemDto>>.Failure($"خطا در بارگذاری آیتم‌های برنامه: {ex.Message}");
        }
    }

    private static string GetScheduleItemTypeName(Domain.Enums.ScheduleItemType type)
    {
        return type switch
        {
            Domain.Enums.ScheduleItemType.Reminder => "یادآوری",
            Domain.Enums.ScheduleItemType.Writing => "نوشتاری",
            Domain.Enums.ScheduleItemType.Audio => "صوتی",
            Domain.Enums.ScheduleItemType.GapFill => "جای خالی",
            Domain.Enums.ScheduleItemType.MultipleChoice => "چند گزینه‌ای",
            Domain.Enums.ScheduleItemType.Match => "تطبیق",
            Domain.Enums.ScheduleItemType.ErrorFinding => "پیدا کردن خطا",
            Domain.Enums.ScheduleItemType.CodeExercise => "تمرین کد",
            Domain.Enums.ScheduleItemType.Quiz => "کوییز",
            _ => "نامشخص"
        };
    }
}
