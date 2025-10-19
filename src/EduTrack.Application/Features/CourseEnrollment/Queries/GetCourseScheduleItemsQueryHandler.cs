using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.CourseEnrollment.Queries;

/// <summary>
/// Handler for GetCourseScheduleItemsQuery
/// </summary>
public class GetCourseScheduleItemsQueryHandler : IRequestHandler<GetCourseScheduleItemsQuery, Result<List<ScheduleItemDto>>>
{
    private readonly IRepository<ScheduleItem> _scheduleItemRepository;
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;

    public GetCourseScheduleItemsQueryHandler(
        IRepository<ScheduleItem> scheduleItemRepository,
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<Result<List<ScheduleItemDto>>> Handle(GetCourseScheduleItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if student is enrolled in the course
            var enrollment = await _enrollmentRepository.GetAll()
                .FirstOrDefaultAsync(e => e.CourseId == request.CourseId && e.StudentId == request.StudentId && e.IsActive, cancellationToken);

            if (enrollment == null)
            {
                return Result<List<ScheduleItemDto>>.Failure("Student is not enrolled in this course");
            }

            // Get schedule items for the course
            var scheduleItems = await _scheduleItemRepository.GetAll()
                .Include(si => si.TeachingPlan)
                .Include(si => si.GroupAssignments)
                    .ThenInclude(ga => ga.StudentGroup)
                .Include(si => si.SubChapterAssignments)
                    .ThenInclude(sca => sca.SubChapter)
                .Include(si => si.StudentAssignments)
                    .ThenInclude(sa => sa.Student)
                .Include(si => si.Lesson)
                .Where(si => si.TeachingPlan.CourseId == request.CourseId)
                .OrderBy(si => si.StartDate)
                .ToListAsync(cancellationToken);

            // Convert to DTOs
            var scheduleItemDtos = scheduleItems.Select(si => new ScheduleItemDto
            {
                Id = si.Id,
                TeachingPlanId = si.TeachingPlanId,
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
                    SubChapterTitle = sca.SubChapter?.Title ?? ""
                }).ToList(),
                StudentAssignments = si.StudentAssignments.Select(sa => new ScheduleItemStudentAssignmentDto
                {
                    Id = sa.Id,
                    ScheduleItemId = sa.ScheduleItemId,
                    StudentId = sa.StudentId,
                    StudentName = $"{sa.Student?.FirstName} {sa.Student?.LastName}".Trim()
                }).ToList(),
                IsAssignedToAllGroups = si.IsAssignedToAllGroups(),
                CurrentStep = si.CurrentStep,
                Status = si.IsCompleted ? ScheduleItemStatus.Completed : 
                        si.IsUpcoming() ? ScheduleItemStatus.Published :
                        si.IsActive() ? ScheduleItemStatus.Active : ScheduleItemStatus.Expired,
                StatusText = si.IsCompleted ? "تکمیل شده" : 
                           si.IsUpcoming() ? "آینده" :
                           si.IsActive() ? "فعال" : "منقضی شده",
                TypeName = GetScheduleItemTypeName(si.Type)
            }).ToList();

            return Result<List<ScheduleItemDto>>.Success(scheduleItemDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ScheduleItemDto>>.Failure($"خطا در بارگذاری آیتم‌های برنامه: {ex.Message}");
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
