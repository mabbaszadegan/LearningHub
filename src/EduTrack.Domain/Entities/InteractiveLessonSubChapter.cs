namespace EduTrack.Domain.Entities;

/// <summary>
/// InteractiveLessonSubChapter entity - represents the many-to-many relationship 
/// between InteractiveLesson and SubChapter
/// </summary>
public class InteractiveLessonSubChapter
{
    public int Id { get; private set; }
    public int InteractiveLessonId { get; private set; }
    public int SubChapterId { get; private set; }
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public InteractiveLesson InteractiveLesson { get; private set; } = null!;
    public SubChapter SubChapter { get; private set; } = null!;

    // Private constructor for EF Core
    private InteractiveLessonSubChapter() { }

    public static InteractiveLessonSubChapter Create(int interactiveLessonId, int subChapterId, int order = 0)
    {
        if (interactiveLessonId <= 0)
            throw new ArgumentException("InteractiveLesson ID must be greater than 0", nameof(interactiveLessonId));
        
        if (subChapterId <= 0)
            throw new ArgumentException("SubChapter ID must be greater than 0", nameof(subChapterId));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new InteractiveLessonSubChapter
        {
            InteractiveLessonId = interactiveLessonId,
            SubChapterId = subChapterId,
            Order = order,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        Order = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
