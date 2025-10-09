using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.Courses;

public class ResourceDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
