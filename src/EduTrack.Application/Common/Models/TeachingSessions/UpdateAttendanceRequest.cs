namespace EduTrack.Application.Common.Models.TeachingSessions;

public class UpdateAttendanceRequest
{
    public int SessionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int AttendanceId { get; set; }
    public int Status { get; set; }
    public decimal? ParticipationScore { get; set; }
    public string? Comment { get; set; }
}
