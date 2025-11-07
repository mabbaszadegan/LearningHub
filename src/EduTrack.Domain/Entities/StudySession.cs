using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// StudySession entity - represents a student's study session for schedule items
/// Tracks multiple study attempts and time spent on each schedule item
/// </summary>
public class StudySession
{
    public int Id { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public int? StudentProfileId { get; private set; }
    public int ScheduleItemId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public int DurationSeconds { get; private set; }
    public bool IsCompleted { get; private set; } = false;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public User Student { get; private set; } = null!;
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public StudentProfile? StudentProfile { get; private set; }

    // Private constructor for EF Core
    private StudySession() { }

    public static StudySession CreateCompleted(string studentId, int scheduleItemId, DateTimeOffset startedAt, DateTimeOffset endedAt, int? studentProfileId = null)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

        if (studentProfileId.HasValue && studentProfileId.Value <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));
        
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (endedAt <= startedAt)
            throw new ArgumentException("End time must be after start time", nameof(endedAt));

        var now = DateTimeOffset.UtcNow;
        var duration = endedAt - startedAt;
        
        return new StudySession
        {
            StudentId = studentId,
            StudentProfileId = studentProfileId,
            ScheduleItemId = scheduleItemId,
            StartedAt = startedAt,
            EndedAt = endedAt,
            DurationSeconds = (int)Math.Floor(duration.TotalSeconds),
            IsCompleted = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static StudySession Create(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

        if (studentProfileId.HasValue && studentProfileId.Value <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));
        
        if (scheduleItemId <= 0)
            throw new ArgumentException("ScheduleItem ID must be greater than 0", nameof(scheduleItemId));

        return new StudySession
        {
            StudentId = studentId,
            StudentProfileId = studentProfileId,
            ScheduleItemId = scheduleItemId,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DurationSeconds = 0,
            IsCompleted = false
        };
    }

    public void Complete()
    {
        EndedAt = DateTimeOffset.UtcNow;
        
        // Calculate duration automatically from StartedAt and EndedAt
        var duration = EndedAt.Value - StartedAt;
        DurationSeconds = (int)Math.Floor(duration.TotalSeconds);
        
        // Add some logging for debugging
        Console.WriteLine($"StudySession Complete - StartedAt: {StartedAt}, EndedAt: {EndedAt}, Duration: {DurationSeconds} seconds");
        
        IsCompleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDuration(int durationSeconds)
    {
        if (durationSeconds < 0)
            throw new ArgumentException("Duration cannot be negative", nameof(durationSeconds));

        DurationSeconds = durationSeconds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public TimeSpan GetDuration()
    {
        return TimeSpan.FromSeconds(DurationSeconds);
    }

    public TimeSpan GetElapsedTime()
    {
        var endTime = EndedAt ?? DateTimeOffset.UtcNow;
        return endTime - StartedAt;
    }

    public bool IsActive => !IsCompleted && EndedAt == null;
}
