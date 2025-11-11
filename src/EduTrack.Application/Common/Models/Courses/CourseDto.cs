using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.Courses;

public class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public int? ThumbnailFileId { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DisciplineType DisciplineType { get; set; }
    // ModuleCount and LessonCount removed - Modules removed, using Chapters/SubChapters instead
    public int ChapterCount { get; set; }
    public int ClassCount { get; set; }
    public List<ChapterDto> Chapters { get; set; } = new();
    public int StudentCount { get; set; }
}
