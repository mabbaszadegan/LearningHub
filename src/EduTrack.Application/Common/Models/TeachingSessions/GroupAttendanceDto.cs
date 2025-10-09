namespace EduTrack.Application.Common.Models.TeachingSessions;

public class GroupAttendanceDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<StudentAttendanceDto> Students { get; set; } = new();
}
