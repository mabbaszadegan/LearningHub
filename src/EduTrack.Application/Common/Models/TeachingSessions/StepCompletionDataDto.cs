namespace EduTrack.Application.Common.Models.TeachingSessions;

public class StepCompletionDataDto
{
    public int SessionId { get; set; }
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string CompletionData { get; set; } = string.Empty; // JSON data
    public bool IsCompleted { get; set; }
}
