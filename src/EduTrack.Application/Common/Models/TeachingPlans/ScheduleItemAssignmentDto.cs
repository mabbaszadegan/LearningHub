namespace EduTrack.Application.Common.Models.TeachingPlans;

public class ScheduleItemAssignmentDto
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
}
