namespace EduTrack.Application.Common.Models.TeachingSessions;

public class GroupFeedbackDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int UnderstandingLevel { get; set; }
    public int ParticipationLevel { get; set; }
    public int TeacherSatisfaction { get; set; }
    public string? GroupFeedback { get; set; }
    public string? Challenges { get; set; }
    public string? NextSessionRecommendations { get; set; }
}
