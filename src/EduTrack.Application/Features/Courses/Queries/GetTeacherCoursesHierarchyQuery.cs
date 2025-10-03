using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.Courses.Queries;

public record GetTeacherCoursesHierarchyQuery(string TeacherId) : IRequest<Result<List<TeacherCourseHierarchyDto>>>;

public class TeacherCourseHierarchyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public int ChapterCount { get; set; }
    public List<TeacherChapterHierarchyDto> Chapters { get; set; } = new();
}

public class TeacherChapterHierarchyDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public int SubChapterCount { get; set; }
    public List<TeacherSubChapterHierarchyDto> SubChapters { get; set; } = new();
}

public class TeacherSubChapterHierarchyDto
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public int ContentCount { get; set; }
}
