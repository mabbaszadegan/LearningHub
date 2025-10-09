namespace EduTrack.Application.Common.Models.TeachingSessions;

public class TabCompletionDataDto
{
    public int SessionId { get; set; }
    public int GroupId { get; set; }
    public int TabIndex { get; set; }
    public string CompletionData { get; set; } = string.Empty; // JSON data
    public bool IsCompleted { get; set; }
}
