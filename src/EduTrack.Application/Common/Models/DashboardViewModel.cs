using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Common.Models.Exams;

namespace EduTrack.Application.Common.Models;

public class DashboardViewModel
{
    public PaginatedList<CourseDto> Courses { get; set; } = null!;
    public PaginatedList<ExamDto> RecentExams { get; set; } = null!;
    public PaginatedList<ProgressDto>? UserProgress { get; set; }
    public int TotalCourses { get; set; }
    public int TotalLessons { get; set; }
    public int TotalModules { get; set; }
    public bool IsUserLoggedIn { get; set; }
    public int CompletedCourses { get; set; }
    public int CompletedLessons { get; set; }
    public int CompletedExams { get; set; }
    public double OverallProgress { get; set; }
}
