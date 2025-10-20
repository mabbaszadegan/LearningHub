using EduTrack.Domain.Enums;
using EduTrack.Application.Common.Models.Courses;

namespace EduTrack.Application.Common.Models.StudySessions;

/// <summary>
/// DTO for StudySession entity
/// </summary>
public class StudySessionDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int EducationalContentId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public EducationalContentDto? EducationalContent { get; set; }
}

/// <summary>
/// DTO for creating a new study session
/// </summary>
public class CreateStudySessionDto
{
    public string StudentId { get; set; } = string.Empty;
    public int EducationalContentId { get; set; }
}

/// <summary>
/// DTO for completing a study session
/// </summary>
public class CompleteStudySessionDto
{
    public int Id { get; set; }
    public int DurationSeconds { get; set; }
}

/// <summary>
/// DTO for study session statistics
/// </summary>
public class StudySessionStatisticsDto
{
    public int TotalStudyTimeSeconds { get; set; }
    public int StudySessionsCount { get; set; }
    public TimeSpan TotalStudyTime => TimeSpan.FromSeconds(TotalStudyTimeSeconds);
    public TimeSpan AverageStudyTime => StudySessionsCount > 0 
        ? TimeSpan.FromSeconds(TotalStudyTimeSeconds / StudySessionsCount) 
        : TimeSpan.Zero;
    public DateTimeOffset? LastStudyDate { get; set; }
    public List<StudySessionDto> RecentSessions { get; set; } = new();
}

/// <summary>
/// DTO for educational content with study statistics
/// </summary>
public class EducationalContentWithStudyStatsDto
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
    public StudySessionStatisticsDto StudyStatistics { get; set; } = new();
}
