using EduTrack.Application.Common.Models.StudySessions;

namespace EduTrack.WebApp.Models;

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public int TotalClasses { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalExams { get; set; }
    public double AverageScore { get; set; }
    public List<object> RecentClasses { get; set; } = new();
    public List<object> UpcomingExams { get; set; } = new();
    public object ProgressStats { get; set; } = new();
    public List<StudySessionHistoryDto> LastStudySessions { get; set; } = new();
    public List<CourseStudyHistoryDto> LastStudyCourses { get; set; } = new();
}
