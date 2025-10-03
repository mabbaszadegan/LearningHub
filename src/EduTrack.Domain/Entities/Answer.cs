namespace EduTrack.Domain.Entities;

/// <summary>
/// Answer entity - represents a student's answer to a question
/// </summary>
public class Answer
{
    public int Id { get; private set; }
    public int AttemptId { get; private set; }
    public int QuestionId { get; private set; }
    public string? TextAnswer { get; private set; }
    public int? SelectedChoiceId { get; private set; }
    public bool IsCorrect { get; private set; }
    public DateTimeOffset AnsweredAt { get; private set; }

    // Navigation properties
    public Attempt Attempt { get; private set; } = null!;
    public Question Question { get; private set; } = null!;
    public Choice? SelectedChoice { get; private set; }

    // Private constructor for EF Core
    private Answer() { }

    public static Answer Create(int attemptId, int questionId, string? textAnswer, 
        int? selectedChoiceId, bool isCorrect)
    {
        if (attemptId <= 0)
            throw new ArgumentException("Attempt ID must be greater than 0", nameof(attemptId));
        
        if (questionId <= 0)
            throw new ArgumentException("Question ID must be greater than 0", nameof(questionId));

        return new Answer
        {
            AttemptId = attemptId,
            QuestionId = questionId,
            TextAnswer = textAnswer,
            SelectedChoiceId = selectedChoiceId,
            IsCorrect = isCorrect,
            AnsweredAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateTextAnswer(string? textAnswer)
    {
        TextAnswer = textAnswer;
    }

    public void UpdateSelectedChoice(int? selectedChoiceId)
    {
        SelectedChoiceId = selectedChoiceId;
    }

    public void SetCorrect(bool isCorrect)
    {
        IsCorrect = isCorrect;
    }
}
