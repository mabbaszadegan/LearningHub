namespace EduTrack.Application.Common.Models.TeachingPlans;

public class GroupProgressDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int TeachingPlanId { get; set; }
    public string TeachingPlanTitle { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalScheduleItems { get; set; }
    public int CompletedSubmissions { get; set; }
    public int OverdueSubmissions { get; set; }
    public double CompletionPercentage { get; set; }
    public double AverageScore { get; set; }
    public List<StudentProgressDto> StudentProgress { get; set; } = new();
}
