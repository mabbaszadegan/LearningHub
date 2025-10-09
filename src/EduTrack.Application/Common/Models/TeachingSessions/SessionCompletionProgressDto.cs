namespace EduTrack.Application.Common.Models.TeachingSessions;

public class SessionCompletionProgressDto
{
    public int SessionId { get; set; }
    public int CurrentStep { get; set; }
    public bool IsCompleted { get; set; }
    public List<StepCompletionDto> Steps { get; set; } = new();
}
