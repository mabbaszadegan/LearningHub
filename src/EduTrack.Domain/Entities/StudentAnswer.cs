namespace EduTrack.Domain.Entities;

/// <summary>
/// StudentAnswer entity - represents a student's answer to an interactive question
/// </summary>
public class StudentAnswer
{
    public int Id { get; private set; }
    public int InteractiveQuestionId { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public int? StudentProfileId { get; private set; }
    public string? AnswerText { get; private set; }
    public int? SelectedChoiceId { get; private set; }
    public bool? BooleanAnswer { get; private set; }
    public bool IsCorrect { get; private set; }
    public int PointsEarned { get; private set; }
    public DateTimeOffset AnsweredAt { get; private set; }
    public DateTimeOffset? GradedAt { get; private set; }
    public string? Feedback { get; private set; }

    // Navigation properties
    public InteractiveQuestion InteractiveQuestion { get; private set; } = null!;
    public User Student { get; private set; } = null!;
    public QuestionChoice? SelectedChoice { get; private set; }
    public StudentProfile? StudentProfile { get; private set; }

    // Private constructor for EF Core
    private StudentAnswer() { }

    public static StudentAnswer Create(int interactiveQuestionId, string studentId,
        string? answerText = null, int? selectedChoiceId = null, bool? booleanAnswer = null, int? studentProfileId = null)
    {
        if (interactiveQuestionId <= 0)
            throw new ArgumentException("InteractiveQuestion ID must be greater than 0", nameof(interactiveQuestionId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

        if (studentProfileId.HasValue && studentProfileId.Value <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));

        return new StudentAnswer
        {
            InteractiveQuestionId = interactiveQuestionId,
            StudentId = studentId,
            StudentProfileId = studentProfileId,
            AnswerText = answerText,
            SelectedChoiceId = selectedChoiceId,
            BooleanAnswer = booleanAnswer,
            AnsweredAt = DateTimeOffset.UtcNow,
            IsCorrect = false,
            PointsEarned = 0
        };
    }

    public void Grade(bool isCorrect, int pointsEarned, string? feedback = null)
    {
        IsCorrect = isCorrect;
        PointsEarned = pointsEarned;
        Feedback = feedback;
        GradedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateFeedback(string? feedback)
    {
        Feedback = feedback;
    }

    public bool HasTextAnswer()
    {
        return !string.IsNullOrWhiteSpace(AnswerText);
    }

    public bool HasSelectedChoice()
    {
        return SelectedChoiceId.HasValue;
    }

    public bool HasBooleanAnswer()
    {
        return BooleanAnswer.HasValue;
    }

    public bool IsGraded()
    {
        return GradedAt.HasValue;
    }

    public void AssignStudentProfile(int? studentProfileId)
    {
        if (studentProfileId.HasValue && studentProfileId.Value <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));

        StudentProfileId = studentProfileId;
    }
}

