namespace EduTrack.Application.Common.Models.Exams;

public class ChoiceDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}
