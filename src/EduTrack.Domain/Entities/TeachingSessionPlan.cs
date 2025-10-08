namespace EduTrack.Domain.Entities;

/// <summary>
/// TeachingSessionPlan entity - represents the planned content for a teaching session
/// </summary>
public class TeachingSessionPlan
{
    public int Id { get; set; }
    public int TeachingSessionReportId { get; set; }
    public int StudentGroupId { get; set; }
    
    /// <summary>
    /// اهداف برنامه‌ریزی شده برای این گروه
    /// </summary>
    public string? PlannedObjectives { get; set; }
    
    /// <summary>
    /// زیرمباحث برنامه‌ریزی شده (JSON array of SubTopic IDs)
    /// </summary>
    public string? PlannedSubTopicsJson { get; set; }
    
    /// <summary>
    /// دروس برنامه‌ریزی شده (JSON array of Lesson IDs)
    /// </summary>
    public string? PlannedLessonsJson { get; set; }
    
    /// <summary>
    /// مباحث اضافی که معلم می‌خواهد اضافه کند
    /// </summary>
    public string? AdditionalTopics { get; set; }
    
    /// <summary>
    /// زمان برنامه‌ریزی
    /// </summary>
    public DateTimeOffset PlannedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation properties
    public TeachingSessionReport TeachingSessionReport { get; set; } = null!;
    public StudentGroup StudentGroup { get; set; } = null!;
}
