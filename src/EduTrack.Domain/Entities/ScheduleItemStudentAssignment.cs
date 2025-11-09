namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItemStudentAssignment entity - represents assignment of a schedule item to specific students
/// </summary>
public class ScheduleItemStudentAssignment
{
    public int Id { get; private set; }
    public int ScheduleItemId { get; private set; }
    public int StudentProfileId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    
    // Navigation properties
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public StudentProfile StudentProfile { get; private set; } = null!;

    public string StudentUserId => StudentProfile?.UserId ?? string.Empty;
    
    // Private constructor for EF Core
    private ScheduleItemStudentAssignment() { }
    
    public static ScheduleItemStudentAssignment Create(int scheduleItemId, int studentProfileId)
    {
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (studentProfileId <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));
        
        return new ScheduleItemStudentAssignment
        {
            ScheduleItemId = scheduleItemId,
            StudentProfileId = studentProfileId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
