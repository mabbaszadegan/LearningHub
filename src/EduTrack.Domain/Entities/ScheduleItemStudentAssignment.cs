namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItemStudentAssignment entity - represents assignment of a schedule item to specific students
/// </summary>
public class ScheduleItemStudentAssignment
{
    public int Id { get; private set; }
    public int ScheduleItemId { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    
    // Navigation properties
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public User Student { get; private set; } = null!;
    
    // Private constructor for EF Core
    private ScheduleItemStudentAssignment() { }
    
    public static ScheduleItemStudentAssignment Create(int scheduleItemId, string studentId)
    {
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));
        
        return new ScheduleItemStudentAssignment
        {
            ScheduleItemId = scheduleItemId,
            StudentId = studentId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
