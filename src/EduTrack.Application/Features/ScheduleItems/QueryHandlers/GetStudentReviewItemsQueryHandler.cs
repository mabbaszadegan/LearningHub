using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

/// <summary>
/// Handler for getting student review items
/// </summary>
public class GetStudentReviewItemsQueryHandler : IRequestHandler<GetStudentReviewItemsQuery, Result<List<StudentReviewItemDto>>>
{
    private readonly IScheduleItemBlockStatisticsRepository _statisticsRepository;
    private readonly IScheduleItemBlockAttemptRepository _attemptRepository;
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public GetStudentReviewItemsQueryHandler(
        IScheduleItemBlockStatisticsRepository statisticsRepository,
        IScheduleItemBlockAttemptRepository attemptRepository,
        IScheduleItemRepository scheduleItemRepository)
    {
        _statisticsRepository = statisticsRepository;
        _attemptRepository = attemptRepository;
        _scheduleItemRepository = scheduleItemRepository;
    }

    public async Task<Result<List<StudentReviewItemDto>>> Handle(GetStudentReviewItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Domain.Entities.ScheduleItemBlockStatistics> statistics;

            if (request.OnlyNeverCorrect == true)
            {
                statistics = await _statisticsRepository.GetBlocksNeverCorrectAsync(request.StudentId, cancellationToken);
            }
            else if (request.OnlyRecentMistakes == true)
            {
                statistics = await _statisticsRepository.GetBlocksWithRecentMistakesAsync(request.StudentId, cancellationToken);
            }
            else if (request.OnlyWithErrors == true)
            {
                statistics = await _statisticsRepository.GetBlocksWithMostErrorsAsync(request.StudentId, request.Limit ?? 10, cancellationToken);
            }
            else
            {
                statistics = await _statisticsRepository.GetByStudentAsync(request.StudentId, cancellationToken);
            }

            if (request.Limit.HasValue)
            {
                statistics = statistics.Take(request.Limit.Value);
            }

            // Get schedule items for titles
            var scheduleItemIds = statistics.Select(s => s.ScheduleItemId).Distinct().ToList();
            var scheduleItems = await _scheduleItemRepository.GetAll()
                .Where(si => scheduleItemIds.Contains(si.Id))
                .ToListAsync(cancellationToken);

            var scheduleItemsDict = scheduleItems.ToDictionary(si => si.Id, si => si);

            // Get recent attempts for each block
            var result = new List<StudentReviewItemDto>();

            foreach (var stat in statistics)
            {
                var scheduleItem = scheduleItemsDict.GetValueOrDefault(stat.ScheduleItemId);
                if (scheduleItem == null) continue;

                var recentAttempts = await _attemptRepository.GetByStudentAndBlockAsync(
                    request.StudentId,
                    stat.ScheduleItemId,
                    stat.BlockId,
                    cancellationToken);

                var attemptSummaries = recentAttempts
                    .OrderByDescending(a => a.AttemptedAt)
                    .Take(5)
                    .Select(a => new BlockAttemptSummaryDto
                    {
                        AttemptId = a.Id,
                        IsCorrect = a.IsCorrect,
                        PointsEarned = a.PointsEarned,
                        MaxPoints = a.MaxPoints,
                        AttemptedAt = a.AttemptedAt
                    })
                    .ToList();

                result.Add(new StudentReviewItemDto
                {
                    ScheduleItemId = stat.ScheduleItemId,
                    ScheduleItemTitle = scheduleItem.Title,
                    ScheduleItemType = stat.ScheduleItemType,
                    BlockId = stat.BlockId,
                    BlockInstruction = stat.BlockInstruction,
                    BlockOrder = stat.BlockOrder,
                    TotalAttempts = stat.TotalAttempts,
                    IncorrectAttempts = stat.IncorrectAttempts,
                    SuccessRate = stat.SuccessRate,
                    ConsecutiveIncorrectAttempts = stat.ConsecutiveIncorrectAttempts,
                    LastAttemptAt = stat.LastAttemptAt ?? DateTimeOffset.MinValue,
                    RecentAttempts = attemptSummaries
                });
            }

            return Result<List<StudentReviewItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<StudentReviewItemDto>>.Failure($"Error getting review items: {ex.Message}");
        }
    }
}

