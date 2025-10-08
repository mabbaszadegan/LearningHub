namespace EduTrack.Domain.Entities;

/// <summary>
/// TeachingSessionTopicCoverage entity - represents detailed coverage tracking for each topic
/// </summary>
public class TeachingSessionTopicCoverage
{
    public int Id { get; set; }
    public int TeachingSessionReportId { get; set; }
    public int StudentGroupId { get; set; }
    
    /// <summary>
    /// نوع موضوع (SubTopic, Lesson, Additional)
    /// </summary>
    public string TopicType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID موضوع (SubTopicId یا LessonId)
    /// </summary>
    public int? TopicId { get; set; }
    
    /// <summary>
    /// عنوان موضوع (برای موارد اضافی)
    /// </summary>
    public string? TopicTitle { get; set; }
    
    /// <summary>
    /// آیا در برنامه بود؟
    /// </summary>
    public bool WasPlanned { get; set; } = false;
    
    /// <summary>
    /// آیا پوشش داده شد؟
    /// </summary>
    public bool WasCovered { get; set; } = false;
    
    /// <summary>
    /// درصد پوشش (0-100)
    /// </summary>
    public int CoveragePercentage { get; set; } = 0;
    
    /// <summary>
    /// وضعیت پوشش (0=NotCovered, 1=PartiallyCovered, 2=FullyCovered, 3=Postponed)
    /// </summary>
    public int CoverageStatus { get; set; } = 0;
    
    /// <summary>
    /// یادداشت‌های معلم
    /// </summary>
    public string? TeacherNotes { get; set; }
    
    /// <summary>
    /// مشکلات خاص
    /// </summary>
    public string? Challenges { get; set; }
    
    /// <summary>
    /// زمان ایجاد
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation properties
    public TeachingSessionReport TeachingSessionReport { get; set; } = null!;
    public StudentGroup StudentGroup { get; set; } = null!;
}
