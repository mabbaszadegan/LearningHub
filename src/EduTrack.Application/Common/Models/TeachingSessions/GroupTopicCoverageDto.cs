namespace EduTrack.Application.Common.Models.TeachingSessions;

public class GroupTopicCoverageDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<TopicCoverageItemDto> SubTopicCoverages { get; set; } = new();
    public List<TopicCoverageItemDto> LessonCoverages { get; set; } = new();
}
