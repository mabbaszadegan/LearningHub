namespace EduTrack.Application.Common.Models.TeachingSessions;

public class AttendanceStepDataDto
{
    public int SessionId { get; set; }
    public List<GroupAttendanceDto> GroupAttendances { get; set; } = new();
}
