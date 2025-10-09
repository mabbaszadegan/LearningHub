namespace EduTrack.Application.Common.Models.TeachingSessions;

public class GroupDataDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
    public GroupFeedbackDto? ExistingFeedback { get; set; }
}
