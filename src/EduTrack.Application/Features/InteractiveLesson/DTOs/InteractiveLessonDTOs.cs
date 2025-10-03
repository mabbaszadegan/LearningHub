using EduTrack.Domain.Enums;

namespace EduTrack.Application.Features.InteractiveLesson.DTOs;

public class InteractiveLessonDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<InteractiveContentItemDto> ContentItems { get; set; } = new();
}

public class InteractiveContentItemDto
{
    public int Id { get; set; }
    public int InteractiveLessonId { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? EducationalContentId { get; set; }
    public int? InteractiveQuestionId { get; set; }
    public EducationalContentDto? EducationalContent { get; set; }
    public InteractiveQuestionDto? InteractiveQuestion { get; set; }
}

public class InteractiveQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InteractiveQuestionType Type { get; set; }
    public int? ImageFileId { get; set; }
    public string? CorrectAnswer { get; set; }
    public int Points { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<QuestionChoiceDto> Choices { get; set; } = new();
}

public class QuestionChoiceDto
{
    public int Id { get; set; }
    public int InteractiveQuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class StudentAnswerDto
{
    public int Id { get; set; }
    public int InteractiveQuestionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public int? SelectedChoiceId { get; set; }
    public bool? BooleanAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public DateTimeOffset AnsweredAt { get; set; }
    public DateTimeOffset? GradedAt { get; set; }
    public string? Feedback { get; set; }
}

public class EducationalContentDto
{
    public int Id { get; set; }
    public int SubChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EducationalContentType Type { get; set; }
    public string? TextContent { get; set; }
    public int? FileId { get; set; }
    public string? ExternalUrl { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class InteractiveLessonAssignmentDto
{
    public int Id { get; set; }
    public int InteractiveLessonId { get; set; }
    public int ClassId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsActive { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public InteractiveLessonDto? InteractiveLesson { get; set; }
    public string? ClassName { get; set; }
}
