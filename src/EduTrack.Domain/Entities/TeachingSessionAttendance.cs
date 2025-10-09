using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class TeachingSessionAttendance
{
    public int Id { get; set; }
    public int TeachingSessionReportId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public decimal? ParticipationScore { get; set; }
    public string? Comment { get; set; }

    // Navigation properties
    public TeachingSessionReport TeachingSessionReport { get; set; } = null!;
    public User Student { get; set; } = null!;
}
