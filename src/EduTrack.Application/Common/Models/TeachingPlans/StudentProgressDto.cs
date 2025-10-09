namespace EduTrack.Application.Common.Models.TeachingPlans;

public class StudentProgressDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int CompletedSubmissions { get; set; }
    public int TotalSubmissions { get; set; }
    public double CompletionPercentage { get; set; }
    public double AverageScore { get; set; }
    public int OverdueCount { get; set; }
    public DateTimeOffset? LastActivity { get; set; }
}
