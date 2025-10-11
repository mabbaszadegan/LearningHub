using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                    LessonId = item.LessonId,
                    Type = item.Type,
                    TypeName = GetTypeName(item.Type),
                    Title = item.Title,
                    Description = item.Description,
                    StartDate = item.StartDate,
                    DueDate = item.DueDate,
                    IsMandatory = item.IsMandatory,
                    DisciplineHint = item.DisciplineHint,
                    ContentJson = item.ContentJson,
                    MaxScore = item.MaxScore,
                    SessionReportId = item.SessionReportId,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    Status = GetItemStatus(item),
                    StatusText = GetStatusText(GetItemStatus(item))
                };

                scheduleItemDtos.Add(dto);
            }

            return Result<List<ScheduleItemDto>>.Success(scheduleItemDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ScheduleItemDto>>.Failure($"خطا در بارگذاری آیتم‌های آموزشی: {ex.Message}");
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
            ScheduleItemType.Quiz => "کویز",
            _ => "نامشخص"
        };
    }

    private static ScheduleItemStatus GetItemStatus(ScheduleItem item)
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
                LessonId = scheduleItem.LessonId,
                Type = scheduleItem.Type,
                TypeName = GetTypeName(scheduleItem.Type),
                Title = scheduleItem.Title,
                Description = scheduleItem.Description,
                StartDate = scheduleItem.StartDate,
                DueDate = scheduleItem.DueDate,
                IsMandatory = scheduleItem.IsMandatory,
                DisciplineHint = scheduleItem.DisciplineHint,
                ContentJson = scheduleItem.ContentJson,
                MaxScore = scheduleItem.MaxScore,
                SessionReportId = scheduleItem.SessionReportId,
                CreatedAt = scheduleItem.CreatedAt,
                UpdatedAt = scheduleItem.UpdatedAt,
                Status = GetItemStatus(scheduleItem),
                StatusText = GetStatusText(GetItemStatus(scheduleItem))
            };

            return Result<ScheduleItemDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ScheduleItemDto>.Failure($"خطا در بارگذاری آیتم آموزشی: {ex.Message}");
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
            ScheduleItemType.Quiz => "کویز",
            _ => "نامشخص"
        };
    }

    private static ScheduleItemStatus GetItemStatus(ScheduleItem item)
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

public class GetScheduleItemStatsQueryHandler : IRequestHandler<GetScheduleItemStatsQuery, Result<ScheduleItemStatsDto>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public GetScheduleItemStatsQueryHandler(IScheduleItemRepository scheduleItemRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
    }

    public async Task<Result<ScheduleItemStatsDto>> Handle(GetScheduleItemStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleItems = await _scheduleItemRepository.GetScheduleItemsByTeachingPlanAsync(request.TeachingPlanId, cancellationToken);
            
            var stats = new ScheduleItemStatsDto
            {
                TotalItems = scheduleItems.Count(),
                PublishedItems = scheduleItems.Count(i => i.StartDate > DateTimeOffset.UtcNow),
                ActiveItems = scheduleItems.Count(i => i.StartDate <= DateTimeOffset.UtcNow && (!i.DueDate.HasValue || i.DueDate.Value >= DateTimeOffset.UtcNow)),
                CompletedItems = 0, // You might need to implement completion tracking
                OverdueItems = scheduleItems.Count(i => i.DueDate.HasValue && i.DueDate.Value < DateTimeOffset.UtcNow),
                ItemsByType = scheduleItems.GroupBy(i => i.Type).ToDictionary(g => g.Key, g => g.Count()),
                ItemsByGroup = scheduleItems.Where(i => i.GroupId.HasValue).GroupBy(i => i.GroupId!.Value).ToDictionary(g => g.Key, g => g.Count())
            };

            return Result<ScheduleItemStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<ScheduleItemStatsDto>.Failure($"خطا در بارگذاری آمار آیتم‌ها: {ex.Message}");
        }
    }
}
