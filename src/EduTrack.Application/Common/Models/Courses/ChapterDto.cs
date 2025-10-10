namespace EduTrack.Application.Common.Models.Courses;

public class ChapterDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int SubChapterCount { get; set; }
    public List<SubChapterDto> SubChapters { get; set; } = new();
    
    // Coverage statistics
    public int TotalCoverageCount { get; set; }
    public double AverageProgressPercentage { get; set; }
}
