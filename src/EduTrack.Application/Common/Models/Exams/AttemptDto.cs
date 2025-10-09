namespace EduTrack.Application.Common.Models.Exams;

public class AttemptDto
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int? TotalQuestions { get; set; }
    public int? CorrectAnswers { get; set; }
    public bool IsPassed { get; set; }
    public TimeSpan? Duration { get; set; }
}
