namespace EduTrack.Domain.Entities;

/// <summary>
/// TeachingPlanProgress entity - represents overall progress tracking for each subtopic per group
/// </summary>
public class TeachingPlanProgress
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public int SubTopicId { get; set; }
    public int StudentGroupId { get; set; }
    
    /// <summary>
    /// وضعیت کلی این زیرمبحث برای این گروه
    /// (0=NotStarted, 1=InProgress, 2=Completed, 3=NeedsReview, 4=Mastered)
    /// </summary>
    public int OverallStatus { get; set; } = 0;
    
    /// <summary>
    /// تاریخ اولین تدریس
    /// </summary>
    public DateTimeOffset? FirstTaughtDate { get; set; }
    
    /// <summary>
    /// تاریخ آخرین تدریس
    /// </summary>
    public DateTimeOffset? LastTaughtDate { get; set; }
    
    /// <summary>
    /// تعداد جلسات تدریس شده
    /// </summary>
    public int SessionsCount { get; set; } = 0;
    
    /// <summary>
    /// درصد کلی پیشرفت
    /// </summary>
    public int OverallProgressPercentage { get; set; } = 0;
    
    /// <summary>
    /// زمان ایجاد و به‌روزرسانی
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation properties
    public TeachingPlan TeachingPlan { get; set; } = null!;
    public SubChapter SubTopic { get; set; } = null!;
    public StudentGroup StudentGroup { get; set; } = null!;
}
