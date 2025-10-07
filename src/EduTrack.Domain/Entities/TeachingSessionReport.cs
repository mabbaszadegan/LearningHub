using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class TeachingSessionReport
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public string? Title { get; set; }
    public DateTime SessionDate { get; set; }
    public SessionMode Mode { get; set; }
    public string? Location { get; set; }
    public string TopicsJson { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? StatsJson { get; set; }
    public string? AttachmentsJson { get; set; }
    public int CreatedByTeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TeachingPlan TeachingPlan { get; set; } = null!;
    public ICollection<TeachingSessionAttendance> Attendance { get; set; } = new List<TeachingSessionAttendance>();
}
