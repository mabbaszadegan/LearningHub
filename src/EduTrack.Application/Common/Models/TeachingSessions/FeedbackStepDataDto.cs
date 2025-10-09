namespace EduTrack.Application.Common.Models.TeachingSessions;

public class FeedbackStepDataDto
{
    public int SessionId { get; set; }
    public List<GroupFeedbackDto> GroupFeedbacks { get; set; } = new();
}
