namespace EduTrack.Domain.Entities;

/// <summary>
/// TeachingSessionExecution entity - represents the actual execution results of a teaching session
/// </summary>
public class TeachingSessionExecution
{
    public int Id { get; set; }
    public int TeachingSessionReportId { get; set; }
    public int StudentGroupId { get; set; }
    
    /// <summary>
    /// اهداف واقعاً محقق شده
    /// </summary>
    public string? AchievedObjectives { get; set; }
    
    /// <summary>
    /// زیرمباحث واقعاً پوشش داده شده (JSON array)
    /// </summary>
    public string? AchievedSubTopicsJson { get; set; }
    
    /// <summary>
    /// دروس واقعاً ارائه شده (JSON array)
    /// </summary>
    public string? AchievedLessonsJson { get; set; }
    
    /// <summary>
    /// مباحث اضافی که در جلسه اضافه شد
    /// </summary>
    public string? AdditionalTopicsCovered { get; set; }
    
    /// <summary>
    /// مباحث برنامه‌ریزی شده که پوشش داده نشد
    /// </summary>
    public string? UncoveredPlannedTopics { get; set; }
    
    /// <summary>
    /// دلایل عدم پوشش
    /// </summary>
    public string? UncoveredReasons { get; set; }
    
    /// <summary>
    /// فیدبک کلی گروه
    /// </summary>
    public string? GroupFeedback { get; set; }
    
    /// <summary>
    /// سطح درک گروه (1-5)
    /// </summary>
    public int UnderstandingLevel { get; set; } = 3;
    
    /// <summary>
    /// سطح مشارکت گروه (1-5)
    /// </summary>
    public int ParticipationLevel { get; set; } = 3;
    
    /// <summary>
    /// میزان رضایت معلم از گروه (1-5)
    /// </summary>
    public int TeacherSatisfaction { get; set; } = 3;
    
    /// <summary>
    /// مشکلات و چالش‌ها
    /// </summary>
    public string? Challenges { get; set; }
    
    /// <summary>
    /// پیشنهادات برای جلسه بعد
    /// </summary>
    public string? NextSessionRecommendations { get; set; }
    
    /// <summary>
    /// زمان تکمیل
    /// </summary>
    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation properties
    public TeachingSessionReport TeachingSessionReport { get; set; } = null!;
    public StudentGroup StudentGroup { get; set; } = null!;
}
