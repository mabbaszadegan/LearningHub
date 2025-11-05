namespace EduTrack.Application.Common.Models.ScheduleItems;

/// <summary>
/// Result of block answer validation
/// </summary>
public class BlockValidationResult
{
    public bool IsCorrect { get; set; }
    public decimal PointsEarned { get; set; }
    public decimal MaxPoints { get; set; }
    public Dictionary<string, object>? CorrectAnswer { get; set; }
    public Dictionary<string, object>? SubmittedAnswer { get; set; }
    public string? Feedback { get; set; }
    public Dictionary<string, object>? DetailedFeedback { get; set; }
}

/// <summary>
/// DTO for submitting a block answer
/// </summary>
public class SubmitBlockAnswerDto
{
    public int ScheduleItemId { get; set; }
    public string BlockId { get; set; } = string.Empty;
    public Dictionary<string, object> SubmittedAnswer { get; set; } = new();
}

/// <summary>
/// DTO for block answer result
/// </summary>
public class BlockAnswerResultDto
{
    public string BlockId { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public decimal PointsEarned { get; set; }
    public decimal MaxPoints { get; set; }
    public Dictionary<string, object>? CorrectAnswer { get; set; }
    public Dictionary<string, object>? SubmittedAnswer { get; set; }
    public string? Feedback { get; set; }
    public Dictionary<string, object>? DetailedFeedback { get; set; }
}

/// <summary>
/// DTO for block statistics
/// </summary>
public class BlockStatisticsDto
{
    public int ScheduleItemId { get; set; }
    public string ScheduleItemTitle { get; set; } = string.Empty;
    public Domain.Enums.ScheduleItemType ScheduleItemType { get; set; }
    public string BlockId { get; set; } = string.Empty;
    public string? BlockInstruction { get; set; }
    public int? BlockOrder { get; set; }
    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public int IncorrectAttempts { get; set; }
    public decimal SuccessRate { get; set; }
    public int ConsecutiveIncorrectAttempts { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? LastCorrectAt { get; set; }
}

/// <summary>
/// DTO for student review items
/// </summary>
public class StudentReviewItemDto
{
    public int ScheduleItemId { get; set; }
    public string ScheduleItemTitle { get; set; } = string.Empty;
    public Domain.Enums.ScheduleItemType ScheduleItemType { get; set; }
    public string BlockId { get; set; } = string.Empty;
    public string? BlockInstruction { get; set; }
    public int? BlockOrder { get; set; }
    public int TotalAttempts { get; set; }
    public int IncorrectAttempts { get; set; }
    public decimal SuccessRate { get; set; }
    public int ConsecutiveIncorrectAttempts { get; set; }
    public DateTimeOffset LastAttemptAt { get; set; }
    public List<BlockAttemptSummaryDto> RecentAttempts { get; set; } = new();
}

/// <summary>
/// DTO for block attempt summary
/// </summary>
public class BlockAttemptSummaryDto
{
    public int AttemptId { get; set; }
    public bool IsCorrect { get; set; }
    public decimal PointsEarned { get; set; }
    public decimal MaxPoints { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
}

