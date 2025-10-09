using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.TeachingSessions;

public class TeachingSessionReportDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public string TeachingPlanTitle { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTimeOffset SessionDate { get; set; }
    public SessionMode Mode { get; set; }
    public string? Location { get; set; }
    public string TopicsJson { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? StatsJson { get; set; }
    public string? AttachmentsJson { get; set; }
    public string CreatedByTeacherId { get; set; } = string.Empty;
    public string CreatedByTeacherName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int AttendanceCount { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public List<TeachingSessionAttendanceDto> Attendance { get; set; } = new();
}
