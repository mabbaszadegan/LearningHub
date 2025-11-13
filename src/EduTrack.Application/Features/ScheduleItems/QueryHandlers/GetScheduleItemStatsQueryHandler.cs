using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;
using System.Linq;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

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
            var planLevelItems = scheduleItems
                .Where(item => !item.SessionReportId.HasValue)
                .ToList();
            
            var stats = new ScheduleItemStatsDto
            {
                TotalItems = planLevelItems.Count,
                PublishedItems = planLevelItems.Count(i => i.StartDate > DateTimeOffset.UtcNow),
                ActiveItems = planLevelItems.Count(i => i.StartDate <= DateTimeOffset.UtcNow && (!i.DueDate.HasValue || i.DueDate.Value >= DateTimeOffset.UtcNow)),
                CompletedItems = 0, // You might need to implement completion tracking
                OverdueItems = planLevelItems.Count(i => i.DueDate.HasValue && i.DueDate.Value < DateTimeOffset.UtcNow),
                ItemsByType = planLevelItems.GroupBy(i => i.Type).ToDictionary(g => g.Key, g => g.Count()),
                ItemsByGroup = planLevelItems.Where(i => i.GroupId.HasValue).GroupBy(i => i.GroupId!.Value).ToDictionary(g => g.Key, g => g.Count())
            };

            return Result<ScheduleItemStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<ScheduleItemStatsDto>.Failure($"خطا در بارگذاری آمار آیتم‌ها: {ex.Message}");
        }
    }
}
