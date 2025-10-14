using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

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
