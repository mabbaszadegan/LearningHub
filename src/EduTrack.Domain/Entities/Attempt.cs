namespace EduTrack.Domain.Entities;

/// <summary>
/// Attempt entity - represents a student's attempt at an exam
/// </summary>
public class Attempt
{
    private readonly List<Answer> _answers = new();

    public int Id { get; private set; }
    public int ExamId { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int? Score { get; private set; }
    public int? TotalQuestions { get; private set; }
    public int? CorrectAnswers { get; private set; }
    public bool IsPassed { get; private set; }
    public TimeSpan? Duration { get; private set; }

    // Navigation properties
    public Exam Exam { get; private set; } = null!;
    public User Student { get; private set; } = null!;
    public IReadOnlyCollection<Answer> Answers => _answers.AsReadOnly();

    // Private constructor for EF Core
    private Attempt() { }

    public static Attempt Create(int examId, string studentId)
    {
        if (examId <= 0)
            throw new ArgumentException("Exam ID must be greater than 0", nameof(examId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

        return new Attempt
        {
            ExamId = examId,
            StudentId = studentId,
            StartedAt = DateTimeOffset.UtcNow
        };
    }

    public void Submit(int score, int totalQuestions, int correctAnswers, bool isPassed)
    {
        if (SubmittedAt.HasValue)
            throw new InvalidOperationException("Attempt is already submitted");

        if (score < 0)
            throw new ArgumentException("Score cannot be negative", nameof(score));
        
        if (totalQuestions <= 0)
            throw new ArgumentException("Total questions must be greater than 0", nameof(totalQuestions));
        
        if (correctAnswers < 0)
            throw new ArgumentException("Correct answers cannot be negative", nameof(correctAnswers));

        SubmittedAt = DateTimeOffset.UtcNow;
        CompletedAt = DateTimeOffset.UtcNow;
        Score = score;
        TotalQuestions = totalQuestions;
        CorrectAnswers = correctAnswers;
        IsPassed = isPassed;
        Duration = CompletedAt.Value - StartedAt;
    }

    public void AddAnswer(Answer answer)
    {
        if (answer == null)
            throw new ArgumentNullException(nameof(answer));

        if (_answers.Any(a => a.Id == answer.Id))
            throw new InvalidOperationException("Answer already exists for this attempt");

        _answers.Add(answer);
    }

    public void RemoveAnswer(Answer answer)
    {
        if (answer == null)
            throw new ArgumentNullException(nameof(answer));

        var answerToRemove = _answers.FirstOrDefault(a => a.Id == answer.Id);
        if (answerToRemove != null)
        {
            _answers.Remove(answerToRemove);
        }
    }

    public bool IsSubmitted => SubmittedAt.HasValue;
    public bool IsCompleted => CompletedAt.HasValue;
    public bool IsExpired(TimeSpan examDuration) => DateTimeOffset.UtcNow - StartedAt > examDuration;

    public double GetScorePercentage()
    {
        if (!Score.HasValue || !TotalQuestions.HasValue || TotalQuestions.Value == 0)
            return 0.0;

        return (double)Score.Value / TotalQuestions.Value * 100;
    }

    public TimeSpan GetElapsedTime()
    {
        var endTime = CompletedAt ?? DateTimeOffset.UtcNow;
        return endTime - StartedAt;
    }
}
