using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class TeachingSessionAttendance
{
    public int Id { get; set; }
    public int TeachingSessionReportId { get; set; }
    public int StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public decimal? ParticipationScore { get; set; }
    public string? Comment { get; set; }

    public TeachingSessionReport TeachingSessionReport { get; set; } = null!;
}
