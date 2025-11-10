using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.Courses;

/// <summary>
/// DTO for displaying course information in student catalog with comprehensive statistics
/// </summary>
public class StudentCourseCatalogDto
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
    public DisciplineType DisciplineType { get; set; }
    
    // Course Statistics
    public int ModuleCount { get; set; }
    public int LessonCount { get; set; }
    public int ChapterCount { get; set; }
    public int SubChapterCount { get; set; }
    public int EducationalContentCount { get; set; }
    public int ExamCount { get; set; }
    public int ClassCount { get; set; }
    
    // Enrollment Status
    public bool IsEnrolled { get; set; }
    public bool CanEnroll { get; set; }
}
