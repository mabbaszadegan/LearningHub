namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItemGroupAssignment entity - represents assignment of a schedule item to specific student groups
/// </summary>
public class ScheduleItemGroupAssignment
{
    public int Id { get; private set; }
    public int ScheduleItemId { get; private set; }
    public int StudentGroupId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public StudentGroup StudentGroup { get; private set; } = null!;

    // Private constructor for EF Core
    private ScheduleItemGroupAssignment() { }

    public static ScheduleItemGroupAssignment Create(int scheduleItemId, int studentGroupId)
    {
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (studentGroupId <= 0)
            throw new ArgumentException("Student Group ID must be greater than 0", nameof(studentGroupId));

        return new ScheduleItemGroupAssignment
        {
            ScheduleItemId = scheduleItemId,
            StudentGroupId = studentGroupId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
