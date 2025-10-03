using EduTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int ModuleCount { get; set; }
    public int LessonCount { get; set; }
}

public class ModuleDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int LessonCount { get; set; }
}

public class LessonDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<ResourceDto> Resources { get; set; } = new();
    public ProgressStatus? ProgressStatus { get; set; }
}

public class ResourceDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ClassDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int StudentCount { get; set; }
}

public class ExamDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public bool ShowSolutions { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<ChoiceDto> Choices { get; set; } = new();
}

public class ChoiceDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}

public class AttemptDto
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int? TotalQuestions { get; set; }
    public int? CorrectAnswers { get; set; }
    public bool IsPassed { get; set; }
    public TimeSpan? Duration { get; set; }
}

public class ProgressDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int? LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public int? ExamId { get; set; }
    public string? ExamTitle { get; set; }
    public ProgressStatus Status { get; set; }
    public int CorrectCount { get; set; }
    public int Streak { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class ClassSummaryDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int CompletedStudents { get; set; }
    public double CompletionPercentage { get; set; }
    public double AverageScore { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
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
    public int SubChapterCount { get; set; }
    public List<SubChapterDto> SubChapters { get; set; } = new();
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
    public int ContentCount { get; set; }
    public List<EducationalContentDto> EducationalContents { get; set; } = new();
}

public class FileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MD5Hash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int ReferenceCount { get; set; }
    public string? FileUrl { get; set; }
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

public class DashboardViewModel
{
    public PaginatedList<CourseDto> Courses { get; set; } = null!;
    public PaginatedList<ExamDto> RecentExams { get; set; } = null!;
    public PaginatedList<ProgressDto>? UserProgress { get; set; }
    public int TotalCourses { get; set; }
    public int TotalLessons { get; set; }
    public int TotalModules { get; set; }
    public bool IsUserLoggedIn { get; set; }
    public int CompletedCourses { get; set; }
    public int CompletedLessons { get; set; }
    public int CompletedExams { get; set; }
    public double OverallProgress { get; set; }
}