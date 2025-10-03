namespace EduTrack.Domain.Entities;

/// <summary>
/// QuestionChoice entity - represents a choice option for multiple choice questions
/// </summary>
public class QuestionChoice
{
    public int Id { get; private set; }
    public int InteractiveQuestionId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public InteractiveQuestion InteractiveQuestion { get; private set; } = null!;

    // Private constructor for EF Core
    private QuestionChoice() { }

    public static QuestionChoice Create(int interactiveQuestionId, string text, bool isCorrect, int order)
    {
        if (interactiveQuestionId <= 0)
            throw new ArgumentException("InteractiveQuestion ID must be greater than 0", nameof(interactiveQuestionId));
        
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new QuestionChoice
        {
            InteractiveQuestionId = interactiveQuestionId,
            Text = text,
            IsCorrect = isCorrect,
            Order = order,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        Text = text;
    }

    public void UpdateIsCorrect(bool isCorrect)
    {
        IsCorrect = isCorrect;
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        Order = order;
    }
}

