using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.TeachingSessions;

public class StudentAttendanceDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public decimal? ParticipationScore { get; set; }
    public string? Comment { get; set; }
}
