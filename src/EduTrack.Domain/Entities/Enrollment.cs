namespace EduTrack.Domain.Entities;

/// <summary>
/// Enrollment entity - represents a student's enrollment in a class
/// </summary>
public class Enrollment
{
    public int Id { get; private set; }
    public int ClassId { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public DateTimeOffset EnrolledAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public Class Class { get; private set; } = null!;
    public User Student { get; private set; } = null!;

    // Private constructor for EF Core
    private Enrollment() { }

    public static Enrollment Create(int classId, string studentId)
    {
        if (classId <= 0)
            throw new ArgumentException("Class ID must be greater than 0", nameof(classId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

        return new Enrollment
        {
            ClassId = classId,
            StudentId = studentId,
            EnrolledAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void Complete()
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Enrollment is already completed");

        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsCompleted => CompletedAt.HasValue;

    public TimeSpan? GetDuration()
    {
        if (!CompletedAt.HasValue)
            return null;

        return CompletedAt.Value - EnrolledAt;
    }
}
