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
    
    // Step completion fields
    public int CurrentStep { get; set; } = 1;
    public bool IsCompleted { get; set; } = false;

    public TeachingPlan TeachingPlan { get; set; } = null!;
    public ICollection<TeachingSessionAttendance> Attendance { get; set; } = new List<TeachingSessionAttendance>();
    
    // Navigation properties for new entities
    public ICollection<TeachingSessionPlan> Plans { get; set; } = new List<TeachingSessionPlan>();
    public ICollection<TeachingSessionExecution> Executions { get; set; } = new List<TeachingSessionExecution>();
    public ICollection<TeachingSessionTopicCoverage> TopicCoverages { get; set; } = new List<TeachingSessionTopicCoverage>();
}
