using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.Courses;

public class SubChapterDto
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int ContentCount { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
    public List<EducationalContentDto> EducationalContents { get; set; } = new();
    
    // Coverage statistics
    public int CoverageCount { get; set; }
    public double AverageProgressPercentage { get; set; }
    
    // Schedule Item statistics
    public Dictionary<ScheduleItemType, int> ScheduleItemStats { get; set; } = new();
}
