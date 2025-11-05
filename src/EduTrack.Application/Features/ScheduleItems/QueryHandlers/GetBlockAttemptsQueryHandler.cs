using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

/// <summary>
/// Handler for getting block attempts
/// </summary>
public class GetBlockAttemptsQueryHandler : IRequestHandler<GetBlockAttemptsQuery, Result<List<BlockAttemptSummaryDto>>>
{
    private readonly IScheduleItemBlockAttemptRepository _attemptRepository;

    public GetBlockAttemptsQueryHandler(IScheduleItemBlockAttemptRepository attemptRepository)
    {
        _attemptRepository = attemptRepository;
    }

    public async Task<Result<List<BlockAttemptSummaryDto>>> Handle(GetBlockAttemptsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var attempts = await _attemptRepository.GetByStudentAndBlockAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.BlockId,
                cancellationToken);

            var result = attempts
                .OrderByDescending(a => a.AttemptedAt)
                .Select(a => new BlockAttemptSummaryDto
                {
                    AttemptId = a.Id,
                    IsCorrect = a.IsCorrect,
                    PointsEarned = a.PointsEarned,
                    MaxPoints = a.MaxPoints,
                    AttemptedAt = a.AttemptedAt
                })
                .ToList();

            return Result<List<BlockAttemptSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<BlockAttemptSummaryDto>>.Failure($"Error getting block attempts: {ex.Message}");
        }
    }
}

