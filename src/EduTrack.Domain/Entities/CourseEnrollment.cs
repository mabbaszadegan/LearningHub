using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// CourseEnrollment entity - represents a student's direct enrollment in a course
/// This is separate from Class enrollment and allows students to access course content directly
/// </summary>
public class CourseEnrollment
{
    public int Id { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public int CourseId { get; private set; }
    public DateTimeOffset EnrolledAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? LastAccessedAt { get; private set; }
    public int ProgressPercentage { get; private set; } = 0;
    public LearningMode LearningMode { get; private set; } = LearningMode.SelfStudy;

    // Navigation properties
    public User Student { get; private set; } = null!;
    public Course Course { get; private set; } = null!;

    // Private constructor for EF Core
    private CourseEnrollment() { }

    public static CourseEnrollment Create(string studentId, int courseId, LearningMode learningMode = LearningMode.SelfStudy)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));
        
        if (courseId <= 0)
            throw new ArgumentException("Course ID must be greater than 0", nameof(courseId));

        return new CourseEnrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            LearningMode = learningMode,
            EnrolledAt = DateTimeOffset.UtcNow,
            IsActive = true,
            LastAccessedAt = DateTimeOffset.UtcNow,
            ProgressPercentage = 0
        };
    }

    public void Complete()
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Course enrollment is already completed");

        CompletedAt = DateTimeOffset.UtcNow;
        ProgressPercentage = 100;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateProgress(int percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Progress percentage must be between 0 and 100", nameof(percentage));

        ProgressPercentage = percentage;
        LastAccessedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLearningMode(LearningMode learningMode)
    {
        LearningMode = learningMode;
    }

    public bool IsCompleted => CompletedAt.HasValue;
    public bool IsInProgress => IsActive && !IsCompleted;

    public TimeSpan? GetDuration()
    {
        if (!CompletedAt.HasValue)
            return null;

        return CompletedAt.Value - EnrolledAt;
    }

    public TimeSpan GetTimeSinceLastAccess()
    {
        return DateTimeOffset.UtcNow - (LastAccessedAt ?? EnrolledAt);
    }
}
