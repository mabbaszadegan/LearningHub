using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class TeachingSessionReport
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset SessionDate { get; set; }
    public SessionMode Mode { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string CreatedByTeacherId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public TeachingPlan TeachingPlan { get; set; } = null!;
    public ICollection<TeachingSessionAttendance> Attendance { get; set; } = new List<TeachingSessionAttendance>();
    
    // Navigation properties for new entities
    public ICollection<TeachingSessionPlan> Plans { get; set; } = new List<TeachingSessionPlan>();
    public ICollection<TeachingSessionExecution> Executions { get; set; } = new List<TeachingSessionExecution>();
    public ICollection<TeachingSessionTopicCoverage> TopicCoverages { get; set; } = new List<TeachingSessionTopicCoverage>();
}
