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
    public FileDto? ImageFile { get; set; }
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
    public FileDto? File { get; set; }
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

// Enhanced Interactive Lesson DTOs
public class InteractiveLessonStageDto
{
    public int Id { get; set; }
    public int InteractiveLessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InteractiveLessonStageType StageType { get; set; }
    public ContentArrangementType ArrangementType { get; set; }
    public string? TextContent { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<StageContentItemDto> ContentItems { get; set; } = new();
}

public class StageContentItemDto
{
    public int Id { get; set; }
    public int InteractiveLessonStageId { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? EducationalContentId { get; set; }
    public int? InteractiveQuestionId { get; set; }
    public InteractiveContentType ContentType { get; set; }
    
    // Navigation properties for display
    public EducationalContentDto? EducationalContent { get; set; }
    public InteractiveQuestionDto? InteractiveQuestion { get; set; }
}

public class InteractiveLessonSubChapterDto
{
    public int Id { get; set; }
    public int InteractiveLessonId { get; set; }
    public int SubChapterId { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Navigation properties for display
    public SubChapterDto? SubChapter { get; set; }
    
    // Convenience properties for view binding
    public string Title => SubChapter?.Title ?? string.Empty;
    public string ChapterTitle => SubChapter?.ChapterTitle ?? string.Empty;
}

public class SubChapterDto
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<EducationalContentDto> EducationalContents { get; set; } = new();
    
    // Navigation properties for display
    public ChapterDto? Chapter { get; set; }
    
    // Convenience properties for view binding
    public string ChapterTitle => Chapter?.Title ?? string.Empty;
}

public class ChapterDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<SubChapterDto> SubChapters { get; set; } = new();
}

public class FileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MD5Hash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

// Enhanced InteractiveLesson DTO
public class InteractiveLessonWithStagesDto : InteractiveLessonDto
{
    public string CourseTitle { get; set; } = string.Empty;
    public List<InteractiveLessonStageDto> Stages { get; set; } = new();
    public List<InteractiveLessonSubChapterDto> SubChapters { get; set; } = new();
}
