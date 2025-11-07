using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.CourseEnrollment.DTOs;

namespace EduTrack.WebApp.Models;

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string StudentFirstName { get; set; } = string.Empty;
    public int? ActiveStudentProfileId { get; set; }
    public string? ActiveStudentProfileName { get; set; }
    public int TotalClasses { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalExams { get; set; }
    public double AverageScore { get; set; }
    public List<object> RecentClasses { get; set; } = new();
    public List<object> UpcomingExams { get; set; } = new();
    public object ProgressStats { get; set; } = new();
    public List<StudySessionHistoryDto> LastStudySessions { get; set; } = new();
    public List<CourseStudyHistoryDto> LastStudyCourses { get; set; } = new();
    public StudyStatisticsDto StudyStatistics { get; set; } = new();
    public List<EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemDto> AccessibleScheduleItems { get; set; } = new();
    public List<StudentCourseEnrollmentSummaryDto> EnrolledCourses { get; set; } = new();
}

public class StudyStatisticsDto
{
    public int TodayStudyMinutes { get; set; }
    public int LastWeekStudyMinutes { get; set; }
    public int LastMonthStudyMinutes { get; set; }
    public int TotalStudyMinutes { get; set; }
    public double AverageDailyStudyMinutes { get; set; }
    public double AverageWeeklyStudyMinutes { get; set; }
    public double AverageMonthlyStudyMinutes { get; set; }
    
    public string TodayStudyTime => FormatTime(TodayStudyMinutes);
    public string LastWeekStudyTime => FormatTime(LastWeekStudyMinutes);
    public string LastMonthStudyTime => FormatTime(LastMonthStudyMinutes);
    public string TotalStudyTime => FormatTime(TotalStudyMinutes);
    public string AverageDailyStudyTime => FormatTime((int)AverageDailyStudyMinutes);
    public string AverageWeeklyStudyTime => FormatTime((int)AverageWeeklyStudyMinutes);
    public string AverageMonthlyStudyTime => FormatTime((int)AverageMonthlyStudyMinutes);
    
    private static string FormatTime(int minutes)
    {
        if (minutes < 60)
            return $"{minutes} دقیقه";
        
        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;
        
        if (remainingMinutes == 0)
            return $"{hours} ساعت";
        
        return $"{hours} ساعت و {remainingMinutes} دقیقه";
    }
    
    // Test method to generate sample data
    public static StudyStatisticsDto GetTestData()
    {
        return new StudyStatisticsDto
        {
            TodayStudyMinutes = 45,
            LastWeekStudyMinutes = 320,
            LastMonthStudyMinutes = 1200,
            TotalStudyMinutes = 4500,
            AverageDailyStudyMinutes = 25.5,
            AverageWeeklyStudyMinutes = 180.0,
            AverageMonthlyStudyMinutes = 600.0
        };
    }
}
