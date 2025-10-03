using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// InteractiveQuestion entity - represents an interactive question
/// </summary>
public class InteractiveQuestion
{
    private readonly List<QuestionChoice> _choices = new();

    public int Id { get; private set; }
    public string QuestionText { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public InteractiveQuestionType Type { get; private set; }
    public int? ImageFileId { get; private set; }
    public string? CorrectAnswer { get; private set; }
    public int Points { get; private set; } = 1;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public File? ImageFile { get; private set; }
    public IReadOnlyCollection<QuestionChoice> Choices => _choices.AsReadOnly();

    // Private constructor for EF Core
    private InteractiveQuestion() { }

    public static InteractiveQuestion Create(string questionText, InteractiveQuestionType type, 
        string? description = null, int? imageFileId = null, string? correctAnswer = null, int points = 1)
    {
        if (string.IsNullOrWhiteSpace(questionText))
            throw new ArgumentException("Question text cannot be null or empty", nameof(questionText));
        
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));

        return new InteractiveQuestion
        {
            QuestionText = questionText,
            Description = description,
            Type = type,
            ImageFileId = imageFileId,
            CorrectAnswer = correctAnswer,
            Points = points,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateQuestionText(string questionText)
    {
        if (string.IsNullOrWhiteSpace(questionText))
            throw new ArgumentException("Question text cannot be null or empty", nameof(questionText));

        QuestionText = questionText;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateCorrectAnswer(string? correctAnswer)
    {
        CorrectAnswer = correctAnswer;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePoints(int points)
    {
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));

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

    public void AddChoice(QuestionChoice choice)
    {
        if (choice == null)
            throw new ArgumentNullException(nameof(choice));

        if (Type != InteractiveQuestionType.MultipleChoice && Type != InteractiveQuestionType.MultipleSelect)
            throw new InvalidOperationException("Can only add choices to multiple choice questions");

        if (_choices.Any(c => c.Id == choice.Id))
            throw new InvalidOperationException("Choice already exists in this question");

        _choices.Add(choice);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveChoice(QuestionChoice choice)
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

    public bool HasImage()
    {
        return ImageFileId.HasValue;
    }

    public bool HasChoices()
    {
        return _choices.Any();
    }

    public bool IsMultipleChoice()
    {
        return Type == InteractiveQuestionType.MultipleChoice || Type == InteractiveQuestionType.MultipleSelect;
    }

    public bool IsTextInput()
    {
        return Type == InteractiveQuestionType.TextInput;
    }

    public bool IsImageSelection()
    {
        return Type == InteractiveQuestionType.ImageSelection;
    }

    public bool IsTrueFalse()
    {
        return Type == InteractiveQuestionType.TrueFalse;
    }
}

