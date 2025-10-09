namespace EduTrack.Application.Common.Models.TeachingSessions;

public class PlannedItemDto
{
    public int StudentGroupId { get; set; }
    public string? PlannedObjectives { get; set; }
    public List<int> PlannedSubTopics { get; set; } = new();
    public List<int> PlannedLessons { get; set; } = new();
    public string? AdditionalTopics { get; set; }
}
