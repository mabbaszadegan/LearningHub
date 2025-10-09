using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.Courses;

public class LessonDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<ResourceDto> Resources { get; set; } = new();
    public ProgressStatus? ProgressStatus { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
}
