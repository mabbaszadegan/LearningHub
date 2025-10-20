using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// StudySession entity - represents a student's study session for educational content
/// Tracks multiple study attempts and time spent on each content item
/// </summary>
public class StudySession
{
    public int Id { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public int EducationalContentId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public int DurationSeconds { get; private set; }
    public bool IsCompleted { get; private set; } = false;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public User Student { get; private set; } = null!;
    public EducationalContent EducationalContent { get; private set; } = null!;

    // Private constructor for EF Core
    private StudySession() { }

    public static StudySession Create(string studentId, int educationalContentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));
        
        if (educationalContentId <= 0)
            throw new ArgumentException("EducationalContent ID must be greater than 0", nameof(educationalContentId));

        return new StudySession
        {
            StudentId = studentId,
            EducationalContentId = educationalContentId,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DurationSeconds = 0,
            IsCompleted = false
        };
    }

    public void Complete(int durationSeconds)
    {
        if (durationSeconds < 0)
            throw new ArgumentException("Duration cannot be negative", nameof(durationSeconds));

        EndedAt = DateTimeOffset.UtcNow;
        DurationSeconds = durationSeconds;
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
