namespace EduTrack.Application.Common.Models.TeachingSessions;

public class StepCompletionDto
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string StepTitle { get; set; } = string.Empty;
    public string StepDescription { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? CompletionData { get; set; } // JSON data for this step
}
