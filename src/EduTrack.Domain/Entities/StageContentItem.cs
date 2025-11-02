using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// StageContentItem entity - represents a content item within a stage
/// Can reference educational content from sub-chapters or interactive questions
/// </summary>
public class StageContentItem
{
    public int Id { get; private set; }
    public int InteractiveLessonStageId { get; private set; }
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    
    // Content reference
    public int? InteractiveQuestionId { get; private set; }
    
    // Navigation properties
    public InteractiveLessonStage InteractiveLessonStage { get; private set; } = null!;
    public InteractiveQuestion? InteractiveQuestion { get; private set; }

    // Private constructor for EF Core
    private StageContentItem() { }

    public static StageContentItem Create(int interactiveLessonStageId, int order, 
        int? interactiveQuestionId = null)
    {
        if (interactiveLessonStageId <= 0)
            throw new ArgumentException("InteractiveLessonStage ID must be greater than 0", nameof(interactiveLessonStageId));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        if (!interactiveQuestionId.HasValue)
            throw new ArgumentException("InteractiveQuestionId is required", nameof(interactiveQuestionId));

        return new StageContentItem
        {
            InteractiveLessonStageId = interactiveLessonStageId,
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
