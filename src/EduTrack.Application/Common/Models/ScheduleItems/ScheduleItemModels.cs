using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.ScheduleItems;

// Main DTOs
public class ScheduleItemDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public List<int> GroupIds { get; set; } = new List<int>();
    public List<int> SubChapterIds { get; set; } = new List<int>();
    public List<string> StudentIds { get; set; } = new List<string>();
    public ScheduleItemType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string PersianStartDate { get; set; } = string.Empty;
    public string PersianDueDate { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public DisciplineType? DisciplineHint { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public decimal? MaxScore { get; set; }
    public int? SessionReportId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ScheduleItemStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public bool IsCompleted { get; set; }
}

public class ScheduleItemListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ScheduleItemType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsMandatory { get; set; }
    public decimal? MaxScore { get; set; }
    public ScheduleItemStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
}

// Request/Response Models
public class CreateScheduleItemRequest
{
    public int TeachingPlanId { get; set; }
    public int? GroupId { get; set; } // Legacy single group assignment
    public ScheduleItemType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsMandatory { get; set; }
    public DisciplineType? DisciplineHint { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public decimal? MaxScore { get; set; }
    
    // New properties for multiple assignments
    public List<int>? GroupIds { get; set; }
    public List<int>? SubChapterIds { get; set; }
    public List<string>? StudentIds { get; set; }
}

public class UpdateScheduleItemRequest
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public int? GroupId { get; set; } // Legacy single group assignment
    public ScheduleItemType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsMandatory { get; set; }
    public DisciplineType? DisciplineHint { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public decimal? MaxScore { get; set; }
    
    // New properties for multiple assignments
    public List<int>? GroupIds { get; set; }
    public List<int>? SubChapterIds { get; set; }
    public List<string>? StudentIds { get; set; }
}

// Content Models for different item types
public class MultipleChoiceContent
{
    public string Question { get; set; } = string.Empty;
    public List<MultipleChoiceOption> Options { get; set; } = new();
    public string AnswerType { get; set; } = "single"; // single, multiple
    public bool RandomizeOptions { get; set; }
    public List<int> CorrectAnswers { get; set; } = new();
}

public class MultipleChoiceOption
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class GapFillContent
{
    public string Text { get; set; } = string.Empty;
    public List<GapFillGap> Gaps { get; set; } = new();
    public string AnswerType { get; set; } = "exact"; // exact, similar, keyword
    public bool CaseSensitive { get; set; }
}

public class GapFillGap
{
    public int Index { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public List<string> AlternativeAnswers { get; set; } = new();
    public string Hint { get; set; } = string.Empty;
}

public class MatchingContent
{
    public List<MatchingItem> LeftItems { get; set; } = new();
    public List<MatchingItem> RightItems { get; set; } = new();
    public List<MatchingConnection> Connections { get; set; } = new();
}

public class MatchingItem
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class MatchingConnection
{
    public int LeftIndex { get; set; }
    public int RightIndex { get; set; }
}

public class CodeExerciseContent
{
    public string ProblemStatement { get; set; } = string.Empty;
    public string ProgrammingLanguage { get; set; } = string.Empty;
    public string InitialCode { get; set; } = string.Empty;
    public List<CodeTestCase> TestCases { get; set; } = new();
    public int TimeLimitMinutes { get; set; } = 30;
}

public class CodeTestCase
{
    public int Index { get; set; }
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
}

public class WritingContent
{
    public string Prompt { get; set; } = string.Empty;
    public int WordLimit { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
}

public class AudioContent
{
    public string Instruction { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public bool AllowRecording { get; set; }
    public string RecordingInstructions { get; set; } = string.Empty;
}

public class ErrorFindingContent
{
    public string Text { get; set; } = string.Empty;
    public List<ErrorFindingError> Errors { get; set; } = new();
    public bool ShowLineNumbers { get; set; }
}

public class ErrorFindingError
{
    public int LineNumber { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string CorrectText { get; set; } = string.Empty;
}

public class QuizContent
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuizQuestion> Questions { get; set; } = new();
    public int TimeLimitMinutes { get; set; }
    public bool RandomizeQuestions { get; set; }
    public bool ShowResultsImmediately { get; set; }
}

public class QuizQuestion
{
    public int Index { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public ScheduleItemType QuestionType { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public decimal Points { get; set; }
}

public class ReminderContent
{
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "normal"; // low, normal, high
    public bool SendNotification { get; set; }
    public List<ContentBlock> Blocks { get; set; } = new();
}

public class ContentBlock
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ContentBlockType Type { get; set; }
    public int Order { get; set; }
    public ContentBlockData Data { get; set; } = new();
}

public class ContentBlockData
{
    // Text block data
    public string? Content { get; set; }
    public string? TextContent { get; set; }
    
    // Media block data (image, video, audio)
    public string? FileId { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    
    // Image/Video specific settings
    public string? Size { get; set; } = "medium"; // small, medium, large, full
    public string? Position { get; set; } = "center"; // left, center, right
    public string? Caption { get; set; }
    public string? CaptionPosition { get; set; } = "bottom"; // top, bottom, overlay
    
    // Audio specific settings
    public bool IsRecorded { get; set; } = false;
    public int? Duration { get; set; } // in seconds
    
    // Code block specific settings
    public string? CodeContent { get; set; }
    public string? Language { get; set; } = "plaintext";
    public string? Theme { get; set; } = "default";
    public string? CodeTitle { get; set; }
    public bool ShowLineNumbers { get; set; } = true;
    public bool EnableCopyButton { get; set; } = true;
}

public enum ContentBlockType
{
    Text = 0,
    Image = 1,
    Video = 2,
    Audio = 3,
    Code = 4
}

// Stats DTO
public class ScheduleItemStatsDto
{
    public int TotalItems { get; set; }
    public int PublishedItems { get; set; }
    public int ActiveItems { get; set; }
    public int CompletedItems { get; set; }
    public int OverdueItems { get; set; }
    public Dictionary<ScheduleItemType, int> ItemsByType { get; set; } = new();
    public Dictionary<int, int> ItemsByGroup { get; set; } = new();
}

// Enums
public enum ScheduleItemStatus
{
    Draft = 0,
    Published = 1,
    Active = 2,
    Completed = 3,
    Expired = 4
}

public record SaveScheduleItemStepRequest
{
    public int? Id { get; init; }
    public int TeachingPlanId { get; init; }
    public int Step { get; init; }
    public ScheduleItemType? Type { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public bool? IsMandatory { get; init; }
    public string? ContentJson { get; init; }
    public decimal? MaxScore { get; init; }
    public int? GroupId { get; init; }
    public string? PersianStartDate { get; init; }
    public string? PersianDueDate { get; init; }
    public string? StartTime { get; init; }
    public string? DueTime { get; init; }
    public List<int>? GroupIds { get; init; }
    public List<int>? SubChapterIds { get; init; }
    public List<string>? StudentIds { get; init; }
}

public record CompleteScheduleItemRequest
{
    public int Id { get; init; }
}
