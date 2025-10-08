namespace EduTrack.Domain.Enums;

/// <summary>
/// وضعیت پوشش موضوعات در جلسه
/// </summary>
public enum TopicCoverageStatus
{
    /// <summary>
    /// پوشش داده نشده
    /// </summary>
    NotCovered = 0,
    
    /// <summary>
    /// نیمه پوشش داده شده
    /// </summary>
    PartiallyCovered = 1,
    
    /// <summary>
    /// کاملاً پوشش داده شده
    /// </summary>
    FullyCovered = 2,
    
    /// <summary>
    /// به تعویق افتاده
    /// </summary>
    Postponed = 3,
    
    /// <summary>
    /// مرور شده
    /// </summary>
    Reviewed = 4
}
