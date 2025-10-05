using EduTrack.Domain.Enums;

namespace EduTrack.Application.Features.CourseEnrollment.DTOs;

/// <summary>
/// DTO for CourseEnrollment entity
/// </summary>
public class CourseEnrollmentDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTimeOffset EnrolledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? Duration { get; set; }
    public TimeSpan TimeSinceLastAccess { get; set; }
}

/// <summary>
/// DTO for CourseAccess entity
/// </summary>
public class CourseAccessDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public CourseAccessLevel AccessLevel { get; set; }
    public string AccessLevelName { get; set; } = string.Empty;
    public DateTimeOffset GrantedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string? GrantedBy { get; set; }
    public string? Notes { get; set; }
    public bool IsExpired { get; set; }
    public bool IsValid { get; set; }
}

/// <summary>
/// DTO for student's course enrollment summary
/// </summary>
public class StudentCourseEnrollmentSummaryDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseDescription { get; set; }
    public string? CourseThumbnail { get; set; }
    public DateTimeOffset EnrolledAt { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }
    public CourseAccessLevel AccessLevel { get; set; }
    public string AccessLevelName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalExams { get; set; }
    public int CompletedExams { get; set; }
    public double AverageScore { get; set; }
}

/// <summary>
/// DTO for course enrollment statistics
/// </summary>
public class CourseEnrollmentStatsDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public double AverageProgress { get; set; }
    public double CompletionRate { get; set; }
    public DateTimeOffset? LastEnrollmentDate { get; set; }
    public List<CourseEnrollmentDto> RecentEnrollments { get; set; } = new();
}

/// <summary>
/// DTO for student's course progress details
/// </summary>
public class StudentCourseProgressDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public DateTimeOffset EnrolledAt { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalExams { get; set; }
    public int CompletedExams { get; set; }
    public double AverageScore { get; set; }
    public List<LessonProgressDto> LessonProgress { get; set; } = new();
    public List<ExamProgressDto> ExamProgress { get; set; } = new();
}

/// <summary>
/// DTO for lesson progress within a course
/// </summary>
public class LessonProgressDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int TimeSpentMinutes { get; set; }
}

/// <summary>
/// DTO for exam progress within a course
/// </summary>
public class ExamProgressDto
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public double Percentage { get; set; }
    public bool IsPassed { get; set; }
}
