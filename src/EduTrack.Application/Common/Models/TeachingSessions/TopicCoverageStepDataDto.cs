namespace EduTrack.Application.Common.Models.TeachingSessions;

public class TopicCoverageStepDataDto
{
    public int SessionId { get; set; }
    public List<GroupTopicCoverageDto> GroupTopicCoverages { get; set; } = new();
}
