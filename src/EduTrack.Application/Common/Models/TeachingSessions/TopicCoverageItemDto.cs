namespace EduTrack.Application.Common.Models.TeachingSessions;

public class TopicCoverageItemDto
{
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public bool WasPlanned { get; set; }
    public bool WasCovered { get; set; }
    public int CoveragePercentage { get; set; }
    public string? TeacherNotes { get; set; }
    public string? Challenges { get; set; }
}
