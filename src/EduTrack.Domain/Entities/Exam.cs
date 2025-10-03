namespace EduTrack.Domain.Entities;

/// <summary>
/// Exam entity - represents an exam containing questions
/// </summary>
public class Exam
{
    private readonly List<ExamQuestion> _examQuestions = new();
    private readonly List<Attempt> _attempts = new();

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public int PassingScore { get; private set; } = 75;
    public bool ShowSolutions { get; private set; } = true;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // Navigation properties
    public IReadOnlyCollection<ExamQuestion> ExamQuestions => _examQuestions.AsReadOnly();
    public IReadOnlyCollection<Attempt> Attempts => _attempts.AsReadOnly();

    // Private constructor for EF Core
    private Exam() { }

    public static Exam Create(string title, string? description, int durationMinutes, 
        int passingScore, bool showSolutions, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be greater than 0", nameof(durationMinutes));
        
        if (passingScore < 0 || passingScore > 100)
            throw new ArgumentException("Passing score must be between 0 and 100", nameof(passingScore));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        return new Exam
        {
            Title = title,
            Description = description,
            DurationMinutes = durationMinutes,
            PassingScore = passingScore,
            ShowSolutions = showSolutions,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDuration(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be greater than 0", nameof(durationMinutes));

        DurationMinutes = durationMinutes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePassingScore(int passingScore)
    {
        if (passingScore < 0 || passingScore > 100)
            throw new ArgumentException("Passing score must be between 0 and 100", nameof(passingScore));

        PassingScore = passingScore;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetShowSolutions(bool showSolutions)
    {
        ShowSolutions = showSolutions;
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

    public void AddQuestion(Question question, int order)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        if (_examQuestions.Any(eq => eq.QuestionId == question.Id))
            throw new InvalidOperationException("Question already exists in this exam");

        var examQuestion = ExamQuestion.Create(Id, question.Id, order);
        _examQuestions.Add(examQuestion);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveQuestion(Question question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        var examQuestion = _examQuestions.FirstOrDefault(eq => eq.QuestionId == question.Id);
        if (examQuestion != null)
        {
            _examQuestions.Remove(examQuestion);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalQuestions()
    {
        return _examQuestions.Count;
    }

    public int GetTotalPoints()
    {
        return _examQuestions.Sum(eq => eq.Question.Points);
    }

    public bool CanBeTakenBy(string studentId)
    {
        // Business rule: Check if student has already passed this exam
        var previousAttempt = _attempts.FirstOrDefault(a => a.StudentId == studentId && a.IsPassed);
        return previousAttempt == null;
    }

    public bool HasQuestions()
    {
        return _examQuestions.Any();
    }

    public int GetTotalDurationMinutes()
    {
        return DurationMinutes;
    }
}
