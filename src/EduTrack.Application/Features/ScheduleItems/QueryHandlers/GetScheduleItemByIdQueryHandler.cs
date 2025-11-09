using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

public class GetScheduleItemByIdQueryHandler : IRequestHandler<GetScheduleItemByIdQuery, Result<ScheduleItemDto>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IStudentGroupRepository _groupRepository;

    public GetScheduleItemByIdQueryHandler(
        IScheduleItemRepository scheduleItemRepository,
        IStudentGroupRepository groupRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _groupRepository = groupRepository;
    }

    public async Task<Result<ScheduleItemDto>> Handle(GetScheduleItemByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.Id, cancellationToken);
            if (scheduleItem == null)
            {
                return Result<ScheduleItemDto>.Failure("آیتم آموزشی یافت نشد.");
            }

            var groupName = string.Empty;
            if (scheduleItem.GroupId.HasValue)
            {
                var group = await _groupRepository.GetByIdAsync(scheduleItem.GroupId.Value, cancellationToken);
                groupName = group?.Name ?? string.Empty;
            }

            var dto = new ScheduleItemDto
            {
                Id = scheduleItem.Id,
                TeachingPlanId = scheduleItem.TeachingPlanId,
                GroupId = scheduleItem.GroupId,
                GroupName = groupName,
                Type = scheduleItem.Type,
                TypeName = GetTypeName(scheduleItem.Type),
                Title = scheduleItem.Title,
                Description = scheduleItem.Description,
                StartDate = scheduleItem.StartDate,
                DueDate = scheduleItem.DueDate,
                IsMandatory = scheduleItem.IsMandatory,
                MaxScore = scheduleItem.MaxScore,
                CurrentStep = scheduleItem.CurrentStep,
                CreatedAt = scheduleItem.CreatedAt,
                UpdatedAt = scheduleItem.UpdatedAt,
                Status = GetStatus(scheduleItem),
                StatusText = GetStatusText(GetStatus(scheduleItem)),
                ContentJson = scheduleItem.ContentJson,
                GroupIds = scheduleItem.GroupAssignments.Select(ga => ga.StudentGroupId).ToList(),
                SubChapterIds = scheduleItem.SubChapterAssignments.Select(sca => sca.SubChapterId).ToList(),
                StudentAssignments = scheduleItem.StudentAssignments.Select(sa => new ScheduleItemStudentAssignmentDto
                {
                    Id = sa.Id,
                    ScheduleItemId = sa.ScheduleItemId,
                    StudentProfileId = sa.StudentProfileId,
                    StudentUserId = sa.StudentProfile?.UserId ?? string.Empty,
                    StudentDisplayName = sa.StudentProfile?.DisplayName ?? string.Empty,
                    CreatedAt = sa.CreatedAt
                }).ToList(),
                StudentProfileIds = scheduleItem.StudentAssignments.Select(sa => sa.StudentProfileId).ToList()
            };

            return Result<ScheduleItemDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ScheduleItemDto>.Failure($"خطا در دریافت آیتم آموزشی: {ex.Message}");
        }
    }

    private static string GetTypeName(ScheduleItemType type)
    {
        return type switch
        {
            ScheduleItemType.Reminder => "یادآوری",
            ScheduleItemType.Writing => "نوشتاری",
            ScheduleItemType.Audio => "صوتی",
            ScheduleItemType.GapFill => "پر کردن جای خالی",
            ScheduleItemType.MultipleChoice => "چند گزینه‌ای",
            ScheduleItemType.Match => "تطبیق",
            ScheduleItemType.ErrorFinding => "پیدا کردن خطا",
            ScheduleItemType.CodeExercise => "تمرین کد",
            ScheduleItemType.Quiz => "کوئیز",
            _ => "نامشخص"
        };
    }

    private static ScheduleItemStatus GetStatus(ScheduleItemDto item)
    {
        var now = DateTimeOffset.UtcNow;
        
        if (item.DueDate.HasValue && now > item.DueDate.Value)
            return ScheduleItemStatus.Expired;
        
        if (now >= item.StartDate && (!item.DueDate.HasValue || now <= item.DueDate.Value))
            return ScheduleItemStatus.Active;
        
        if (now < item.StartDate)
            return ScheduleItemStatus.Published;
        
        return ScheduleItemStatus.Draft;
    }

    private static ScheduleItemStatus GetStatus(ScheduleItem item)
    {
        var now = DateTimeOffset.UtcNow;
        
        if (item.DueDate.HasValue && now > item.DueDate.Value)
            return ScheduleItemStatus.Expired;
        
        if (now >= item.StartDate && (!item.DueDate.HasValue || now <= item.DueDate.Value))
            return ScheduleItemStatus.Active;
        
        if (now < item.StartDate)
            return ScheduleItemStatus.Published;
        
        return ScheduleItemStatus.Draft;
    }

    private static string GetStatusText(ScheduleItemStatus status)
    {
        return status switch
        {
            ScheduleItemStatus.Draft => "پیش‌نویس",
            ScheduleItemStatus.Published => "منتشر شده",
            ScheduleItemStatus.Active => "فعال",
            ScheduleItemStatus.Completed => "تکمیل شده",
            ScheduleItemStatus.Expired => "منقضی شده",
            _ => "نامشخص"
        };
    }
}
