using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Interfaces;

/// <summary>
/// Interface for validating block answers based on schedule item type
/// </summary>
public interface IBlockAnswerValidator
{
    ScheduleItemType SupportedType { get; }
    
    Task<BlockValidationResult> ValidateAnswerAsync(
        int scheduleItemId,
        string blockId,
        Dictionary<string, object> submittedAnswer,
        CancellationToken cancellationToken = default);
}

