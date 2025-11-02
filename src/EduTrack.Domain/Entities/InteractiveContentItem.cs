using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// InteractiveContentItem entity - represents an item in the interactive lesson sequence
/// Can be either educational content or interactive question
/// </summary>
public class InteractiveContentItem
{
    public int Id { get; private set; }
    public int InteractiveLessonId { get; private set; }
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    
    // Content reference
    public int? InteractiveQuestionId { get; private set; }
    
    // Navigation properties
    public InteractiveLesson InteractiveLesson { get; private set; } = null!;
    public InteractiveQuestion? InteractiveQuestion { get; private set; }

    // Private constructor for EF Core
    private InteractiveContentItem() { }

    public static InteractiveContentItem Create(int interactiveLessonId, int order, 
        int? interactiveQuestionId = null)
    {
        if (interactiveLessonId <= 0)
            throw new ArgumentException("InteractiveLesson ID must be greater than 0", nameof(interactiveLessonId));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        if (!interactiveQuestionId.HasValue)
            throw new ArgumentException("InteractiveQuestionId is required", nameof(interactiveQuestionId));

        return new InteractiveContentItem
        {
            InteractiveLessonId = interactiveLessonId,
            Order = order,
            InteractiveQuestionId = interactiveQuestionId,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        Order = order;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsInteractiveQuestion()
    {
        return InteractiveQuestionId.HasValue;
    }

    public InteractiveContentType GetContentType()
    {
        return InteractiveContentType.Question;
    }
}

