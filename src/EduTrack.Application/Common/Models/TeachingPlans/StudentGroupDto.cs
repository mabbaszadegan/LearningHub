using EduTrack.Application.Common.Models.TeachingSessions;

namespace EduTrack.Application.Common.Models.TeachingPlans;

public class StudentGroupDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
}
