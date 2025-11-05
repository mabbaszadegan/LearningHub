using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

/// <summary>
/// Handler for getting block statistics
/// </summary>
public class GetBlockStatisticsQueryHandler : IRequestHandler<GetBlockStatisticsQuery, Result<List<BlockStatisticsDto>>>
{
    private readonly IScheduleItemBlockStatisticsRepository _statisticsRepository;
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public GetBlockStatisticsQueryHandler(
        IScheduleItemBlockStatisticsRepository statisticsRepository,
        IScheduleItemRepository scheduleItemRepository)
    {
        _statisticsRepository = statisticsRepository;
        _scheduleItemRepository = scheduleItemRepository;
    }

    public async Task<Result<List<BlockStatisticsDto>>> Handle(GetBlockStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Domain.Entities.ScheduleItemBlockStatistics> statistics;

            if (request.ScheduleItemId.HasValue && !string.IsNullOrEmpty(request.BlockId))
            {
                // Get specific block statistics
                var stat = await _statisticsRepository.GetByStudentAndBlockAsync(
                    request.StudentId,
                    request.ScheduleItemId.Value,
                    request.BlockId,
                    cancellationToken);

                statistics = stat != null ? new[] { stat } : Enumerable.Empty<Domain.Entities.ScheduleItemBlockStatistics>();
            }
            else if (request.ScheduleItemId.HasValue)
            {
                // Get all blocks for a schedule item
                statistics = await _statisticsRepository.GetByStudentAndScheduleItemAsync(
                    request.StudentId,
                    request.ScheduleItemId.Value,
                    cancellationToken);
            }
            else
            {
                // Get all blocks for student
                statistics = await _statisticsRepository.GetByStudentAsync(request.StudentId, cancellationToken);
            }

            // Get schedule items for titles
            var scheduleItemIds = statistics.Select(s => s.ScheduleItemId).Distinct().ToList();
            var scheduleItems = await _scheduleItemRepository.GetAll()
                .Where(si => scheduleItemIds.Contains(si.Id))
                .ToListAsync(cancellationToken);

            var scheduleItemsDict = scheduleItems.ToDictionary(si => si.Id, si => si);

            var result = statistics.Select(s =>
            {
                var scheduleItem = scheduleItemsDict.GetValueOrDefault(s.ScheduleItemId);
                return new BlockStatisticsDto
                {
                    ScheduleItemId = s.ScheduleItemId,
                    ScheduleItemTitle = scheduleItem?.Title ?? "Unknown",
                    ScheduleItemType = s.ScheduleItemType,
                    BlockId = s.BlockId,
                    BlockInstruction = s.BlockInstruction,
                    BlockOrder = s.BlockOrder,
                    TotalAttempts = s.TotalAttempts,
                    CorrectAttempts = s.CorrectAttempts,
                    IncorrectAttempts = s.IncorrectAttempts,
                    SuccessRate = s.SuccessRate,
                    ConsecutiveIncorrectAttempts = s.ConsecutiveIncorrectAttempts,
                    LastAttemptAt = s.LastAttemptAt,
                    LastCorrectAt = s.LastCorrectAt
                };
            }).ToList();

            return Result<List<BlockStatisticsDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<BlockStatisticsDto>>.Failure($"Error getting block statistics: {ex.Message}");
        }
    }
}

