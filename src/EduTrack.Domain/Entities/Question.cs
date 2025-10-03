using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// Question entity - represents an exam question
/// </summary>
public class Question
{
    private readonly List<Choice> _choices = new();
    private readonly List<ExamQuestion> _examQuestions = new();
    private readonly List<Answer> _answers = new();

    public int Id { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public QuestionType Type { get; private set; }
    public string? Explanation { get; private set; }
    public int Points { get; private set; } = 1;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // Navigation properties
    public IReadOnlyCollection<Choice> Choices => _choices.AsReadOnly();
    public IReadOnlyCollection<ExamQuestion> ExamQuestions => _examQuestions.AsReadOnly();
    public IReadOnlyCollection<Answer> Answers => _answers.AsReadOnly();

    // Private constructor for EF Core
    private Question() { }

    public static Question Create(string text, QuestionType type, string? explanation, 
        int points, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        
        if (points <= 0)
            throw new ArgumentException("Points must be greater than 0", nameof(points));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        return new Question
        {
            Text = text,
            Type = type,
            Explanation = explanation,
            Points = points,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        Text = text;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateExplanation(string? explanation)
    {
        Explanation = explanation;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePoints(int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be greater than 0", nameof(points));

        Points = points;
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

    public void AddChoice(Choice choice)
    {
        if (choice == null)
            throw new ArgumentNullException(nameof(choice));

        if (_choices.Any(c => c.Id == choice.Id))
            throw new InvalidOperationException("Choice already exists for this question");

        _choices.Add(choice);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveChoice(Choice choice)
    {
        if (choice == null)
            throw new ArgumentNullException(nameof(choice));

        var choiceToRemove = _choices.FirstOrDefault(c => c.Id == choice.Id);
        if (choiceToRemove != null)
        {
            _choices.Remove(choiceToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool HasCorrectAnswer()
    {
        return _choices.Any(c => c.IsCorrect);
    }

    public int GetCorrectChoicesCount()
    {
        return _choices.Count(c => c.IsCorrect);
    }

    public bool IsMultipleChoice()
    {
        return Type == QuestionType.MultipleChoice;
    }

    public bool IsTrueFalse()
    {
        return Type == QuestionType.TrueFalse;
    }

    public bool IsShortAnswer()
    {
        return Type == QuestionType.ShortAnswer;
    }
}
