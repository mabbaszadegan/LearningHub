using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

public class GetScheduleItemsByTeachingPlanQueryHandler : IRequestHandler<GetScheduleItemsByTeachingPlanQuery, Result<List<ScheduleItemDto>>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IStudentGroupRepository _groupRepository;

    public GetScheduleItemsByTeachingPlanQueryHandler(
        IScheduleItemRepository scheduleItemRepository,
        IStudentGroupRepository groupRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _groupRepository = groupRepository;
    }

    public async Task<Result<List<ScheduleItemDto>>> Handle(GetScheduleItemsByTeachingPlanQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleItems = await _scheduleItemRepository.GetScheduleItemsByTeachingPlanAsync(request.TeachingPlanId, cancellationToken);
            
            var scheduleItemDtos = new List<ScheduleItemDto>();
            
            foreach (var item in scheduleItems)
            {
                var groupName = string.Empty;
                if (item.GroupId.HasValue)
                {
                    var group = await _groupRepository.GetByIdAsync(item.GroupId.Value, cancellationToken);
                    groupName = group?.Name ?? string.Empty;
                }

                var dto = new ScheduleItemDto
                {
                    Id = item.Id,
                    TeachingPlanId = item.TeachingPlanId,
                    GroupId = item.GroupId,
                    GroupName = groupName,
                    Type = item.Type,
                    TypeName = GetTypeName(item.Type),
                    Title = item.Title,
                    Description = item.Description,
                    StartDate = item.StartDate,
                    DueDate = item.DueDate,
                    IsMandatory = item.IsMandatory,
                    MaxScore = item.MaxScore,
                    CurrentStep = item.CurrentStep,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    Status = GetStatus(item),
                    StatusText = GetStatusText(GetStatus(item)),
                    GroupIds = item.GroupAssignments.Select(ga => ga.StudentGroupId).ToList(),
                    SubChapterIds = item.SubChapterAssignments.Select(sca => sca.SubChapterId).ToList(),
                    StudentIds = item.StudentAssignments.Select(sa => sa.StudentId).ToList()
                };

                scheduleItemDtos.Add(dto);
            }

            return Result<List<ScheduleItemDto>>.Success(scheduleItemDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ScheduleItemDto>>.Failure($"خطا در دریافت آیتم‌های برنامه: {ex.Message}");
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
