namespace EduTrack.Domain.Entities;

/// <summary>
/// InteractiveLessonAssignment entity - represents assignment of interactive lessons to classes
/// </summary>
public class InteractiveLessonAssignment
{
    public int Id { get; private set; }
    public int InteractiveLessonId { get; private set; }
    public int ClassId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string AssignedBy { get; private set; } = string.Empty;

    // Navigation properties
    public InteractiveLesson InteractiveLesson { get; private set; } = null!;
    public Class Class { get; private set; } = null!;

    // Private constructor for EF Core
    private InteractiveLessonAssignment() { }

    public static InteractiveLessonAssignment Create(int interactiveLessonId, int classId, 
        string assignedBy, DateTimeOffset? dueDate = null)
    {
        if (interactiveLessonId <= 0)
            throw new ArgumentException("InteractiveLesson ID must be greater than 0", nameof(interactiveLessonId));
        
        if (classId <= 0)
            throw new ArgumentException("Class ID must be greater than 0", nameof(classId));
        
        if (string.IsNullOrWhiteSpace(assignedBy))
            throw new ArgumentException("AssignedBy cannot be null or empty", nameof(assignedBy));

        return new InteractiveLessonAssignment
        {
            InteractiveLessonId = interactiveLessonId,
            ClassId = classId,
            AssignedBy = assignedBy,
            DueDate = dueDate,
            AssignedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateDueDate(DateTimeOffset? dueDate)
    {
        DueDate = dueDate;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

