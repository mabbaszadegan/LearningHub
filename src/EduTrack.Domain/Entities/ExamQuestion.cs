namespace EduTrack.Domain.Entities;

/// <summary>
/// ExamQuestion entity - represents the relationship between an exam and a question
/// </summary>
public class ExamQuestion
{
    public int Id { get; private set; }
    public int ExamId { get; private set; }
    public int QuestionId { get; private set; }
    public int Order { get; private set; }

    // Navigation properties
    public Exam Exam { get; private set; } = null!;
    public Question Question { get; private set; } = null!;

    // Private constructor for EF Core
    private ExamQuestion() { }

    public static ExamQuestion Create(int examId, int questionId, int order)
    {
        if (examId <= 0)
            throw new ArgumentException("Exam ID must be greater than 0", nameof(examId));
        
        if (questionId <= 0)
            throw new ArgumentException("Question ID must be greater than 0", nameof(questionId));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new ExamQuestion
        {
            ExamId = examId,
            QuestionId = questionId,
            Order = order
        };
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        Order = order;
    }
}
