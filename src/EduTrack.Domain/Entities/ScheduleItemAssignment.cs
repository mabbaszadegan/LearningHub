namespace EduTrack.Domain.Entities;

public class ScheduleItemAssignment
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public int? StudentId { get; set; }
    public int? GroupId { get; set; }

    public ScheduleItem ScheduleItem { get; set; } = null!;
}
