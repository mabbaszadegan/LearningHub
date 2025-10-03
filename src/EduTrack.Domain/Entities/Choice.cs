namespace EduTrack.Domain.Entities;

/// <summary>
/// Choice entity - represents a choice option for a question
/// </summary>
public class Choice
{
    public int Id { get; private set; }
    public int QuestionId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int Order { get; private set; }

    // Navigation properties
    public Question Question { get; private set; } = null!;

    // Private constructor for EF Core
    private Choice() { }

    public static Choice Create(int questionId, string text, bool isCorrect, int order)
    {
        if (questionId <= 0)
            throw new ArgumentException("Question ID must be greater than 0", nameof(questionId));
        
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new Choice
        {
            QuestionId = questionId,
            Text = text,
            IsCorrect = isCorrect,
            Order = order
        };
    }

    public void UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        Text = text;
    }

    public void SetCorrect(bool isCorrect)
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
