using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.Commands;

/// <summary>
/// Command to submit a block answer for validation and storage
/// </summary>
public record SubmitBlockAnswerCommand(
    int ScheduleItemId,
    string BlockId,
    string StudentId,
    Dictionary<string, object> SubmittedAnswer
) : IRequest<Result<BlockAnswerResultDto>>;

