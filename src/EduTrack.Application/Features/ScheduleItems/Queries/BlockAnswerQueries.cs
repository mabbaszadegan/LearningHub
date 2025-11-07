using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.Queries;

/// <summary>
/// Query to get block statistics for a student
/// </summary>
public record GetBlockStatisticsQuery(
    string StudentId,
    int? ScheduleItemId = null,
    string? BlockId = null,
    int? StudentProfileId = null
) : IRequest<Result<List<BlockStatisticsDto>>>;

/// <summary>
/// Query to get student review items (blocks with errors or attempts)
/// </summary>
public record GetStudentReviewItemsQuery(
    string StudentId,
    bool? OnlyWithErrors = null,
    bool? OnlyNeverCorrect = null,
    bool? OnlyRecentMistakes = null,
    int? Limit = null,
    int? StudentProfileId = null
) : IRequest<Result<List<StudentReviewItemDto>>>;

/// <summary>
/// Query to get block attempts for a student and block
/// </summary>
public record GetBlockAttemptsQuery(
    string StudentId,
    int ScheduleItemId,
    string BlockId,
    int? StudentProfileId = null
) : IRequest<Result<List<BlockAttemptSummaryDto>>>;

