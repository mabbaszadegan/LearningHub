namespace EduTrack.Domain.Enums;

/// <summary>
/// وضعیت کلی پیشرفت زیرمباحث
/// </summary>
public enum SubTopicProgressStatus
{
    /// <summary>
    /// شروع نشده
    /// </summary>
    NotStarted = 0,
    
    /// <summary>
    /// در حال انجام
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// تکمیل شده
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// نیاز به مرور
    /// </summary>
    NeedsReview = 3,
    
    /// <summary>
    /// مسلط شده
    /// </summary>
    Mastered = 4
}
