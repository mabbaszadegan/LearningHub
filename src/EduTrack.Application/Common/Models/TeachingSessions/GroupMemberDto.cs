namespace EduTrack.Application.Common.Models.TeachingSessions;

public class GroupMemberDto
{
    public int Id { get; set; }
    public int StudentGroupId { get; set; }
    public int StudentProfileId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public StudentAttendanceDto? ExistingAttendance { get; set; }
}
