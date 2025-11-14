using EduTrack.Application.Common.Models.Statistics;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Application.Features.Progress.Queries;

namespace EduTrack.WebApp.Areas.Student.Models;

public class StudentStatisticsViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string StudentFirstName { get; set; } = string.Empty;
    public int? ActiveStudentProfileId { get; set; }
    public string? ActiveStudentProfileName { get; set; }
    public ProgressOverviewViewModel ProgressStats { get; set; } = new();
    public List<StudentCourseEnrollmentSummaryDto> EnrolledCourses { get; set; } = new();
    public StudentStatsDto? StudentStatistics { get; set; }
    public LearningStatisticsDto LearningStatistics { get; set; } = new();
}

public class ProgressOverviewViewModel
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public int NotStarted { get; set; }
    public double CompletionPercentage { get; set; }
}

