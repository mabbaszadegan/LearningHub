using EduTrack.Domain.Enums;
using EduTrack.Application.Common.Models.Courses;

namespace EduTrack.Application.Common.Models.StudySessions;

/// <summary>
/// DTO for ScheduleItem entity
/// </summary>
public class ScheduleItemDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContentJson { get; set; }
    public ScheduleItemType Type { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for StudySession entity
/// </summary>
public class StudySessionDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }
    public int? StudentProfileId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ScheduleItemDto? ScheduleItem { get; set; }
}

/// <summary>
/// DTO for creating a new study session
/// </summary>
public class CreateStudySessionDto
{
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }
    public int? StudentProfileId { get; set; }
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
/// DTO for study session history with course information
/// </summary>
public class StudySessionHistoryDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }
    public int? StudentProfileId { get; set; }
    public string ScheduleItemTitle { get; set; } = string.Empty;
    public string? ScheduleItemDescription { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
    public string FormattedDuration => FormatDuration(Duration);
    public string RelativeTime => GetRelativeTime(StartedAt);
    
    private static string FormatDuration(TimeSpan duration)
    {
        return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }
    
    private static string GetRelativeTime(DateTimeOffset dateTime)
    {
        var now = DateTimeOffset.Now;
        var diff = now - dateTime;
        
        if (diff.TotalDays >= 1)
        {
            return $"{(int)diff.TotalDays} روز پیش";
        }
        else if (diff.TotalHours >= 1)
        {
            return $"{(int)diff.TotalHours} ساعت پیش";
        }
        else if (diff.TotalMinutes >= 1)
        {
            return $"{(int)diff.TotalMinutes} دقیقه پیش";
        }
        else
        {
            return "همین الان";
        }
    }
}

/// <summary>
/// DTO for course study history
/// </summary>
public class CourseStudyHistoryDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }
    public DateTimeOffset LastStudyDate { get; set; }
    public int TotalStudyTimeSeconds { get; set; }
    public int StudySessionsCount { get; set; }
    public int CompletedItems { get; set; }
    public int TotalItems { get; set; }
    public TimeSpan TotalStudyTime => TimeSpan.FromSeconds(TotalStudyTimeSeconds);
    public string FormattedTotalTime => FormatDuration(TotalStudyTime);
    public string RelativeTime => GetRelativeTime(LastStudyDate);
    public double ProgressPercentage => TotalItems > 0 ? (CompletedItems * 100.0 / TotalItems) : 0;
    
    private static string FormatDuration(TimeSpan duration)
    {
        return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }
    
    private static string GetRelativeTime(DateTimeOffset dateTime)
    {
        var now = DateTimeOffset.Now;
        var diff = now - dateTime;
        
        if (diff.TotalDays >= 1)
        {
            return $"{(int)diff.TotalDays} روز پیش";
        }
        else if (diff.TotalHours >= 1)
        {
            return $"{(int)diff.TotalHours} ساعت پیش";
        }
        else if (diff.TotalMinutes >= 1)
        {
            return $"{(int)diff.TotalMinutes} دقیقه پیش";
        }
        else
        {
            return "همین الان";
        }
    }
}