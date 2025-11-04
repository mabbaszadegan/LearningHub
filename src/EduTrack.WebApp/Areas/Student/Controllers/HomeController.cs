using EduTrack.Application.Features.Classroom.Queries;
using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Features.Exams.Queries;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.CourseEnrollment.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.WebApp.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get enrolled courses
        var enrolledCoursesResult = await _mediator.Send(new GetStudentCourseEnrollmentsQuery(currentUser.Id));
        var enrolledCourses = enrolledCoursesResult.IsSuccess && enrolledCoursesResult.Value != null 
            ? enrolledCoursesResult.Value 
            : new List<Application.Features.CourseEnrollment.DTOs.StudentCourseEnrollmentSummaryDto>();

        // Get student dashboard data
        var dashboardData = new StudentDashboardViewModel
        {
            StudentName = currentUser.FullName,
            StudentFirstName = currentUser.FirstName,
            TotalClasses = await GetStudentClassesCount(currentUser.Id),
            CompletedLessons = await GetCompletedLessonsCount(currentUser.Id),
            TotalExams = await GetStudentExamsCount(currentUser.Id),
            AverageScore = await GetStudentAverageScore(currentUser.Id),
            RecentClasses = await GetStudentRecentClasses(currentUser.Id),
            UpcomingExams = await GetStudentUpcomingExams(currentUser.Id),
            ProgressStats = await GetStudentProgressStats(currentUser.Id),
            LastStudySessions = await GetLastStudySessions(currentUser.Id),
            LastStudyCourses = await GetLastStudyCourses(currentUser.Id),
            StudyStatistics = await GetStudyStatistics(currentUser.Id),
            AccessibleScheduleItems = await GetAccessibleScheduleItems(currentUser.Id),
            EnrolledCourses = enrolledCourses
        };

        return View(dashboardData);
    }

    public async Task<IActionResult> MyProgress()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get detailed progress data
        var progressData = await _mediator.Send(new GetProgressByStudentQuery(currentUser.Id, 1, 50));

        return View(progressData);
    }

    private async Task<int> GetStudentClassesCount(string studentId)
    {
        try
        {
            var enrollments = await _mediator.Send(new GetEnrollmentsByStudentQuery(studentId, 1, 100));
            return enrollments.Items.Count(e => e.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student classes count");
            return 0;
        }
    }

    private async Task<int> GetCompletedLessonsCount(string studentId)
    {
        try
        {
            var progress = await _mediator.Send(new GetProgressByStudentQuery(studentId, 1, 100));
            return progress.Items.Count(p => p.Status == Domain.Enums.ProgressStatus.Done);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed lessons count");
            return 0;
        }
    }

    private Task<int> GetStudentExamsCount(string studentId)
    {
        // TODO: Implement when exam queries are available
        return Task.FromResult(0);
    }

    private Task<double> GetStudentAverageScore(string studentId)
    {
        // TODO: Implement when exam results are available
        return Task.FromResult(0.0);
    }

    private async Task<List<object>> GetStudentRecentClasses(string studentId)
    {
        try
        {
            var enrollments = await _mediator.Send(new GetEnrollmentsByStudentQuery(studentId, 1, 5));
            return enrollments.Items.Take(5).Cast<object>().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student recent classes");
            return new List<object>();
        }
    }

    private Task<List<object>> GetStudentUpcomingExams(string studentId)
    {
        // TODO: Implement when exam queries are available
        return Task.FromResult(new List<object>());
    }

    private async Task<object> GetStudentProgressStats(string studentId)
    {
        try
        {
            var progress = await _mediator.Send(new GetProgressByStudentQuery(studentId, 1, 100));
            var total = progress.Items.Count;
            var completed = progress.Items.Count(p => p.Status == Domain.Enums.ProgressStatus.Done);
            var inProgress = progress.Items.Count(p => p.Status == Domain.Enums.ProgressStatus.InProgress);
            var notStarted = progress.Items.Count(p => p.Status == Domain.Enums.ProgressStatus.NotStarted);

            return new
            {
                Total = total,
                Completed = completed,
                InProgress = inProgress,
                NotStarted = notStarted,
                CompletionPercentage = total > 0 ? (completed * 100.0 / total) : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student progress stats");
            return new { Total = 0, Completed = 0, InProgress = 0, NotStarted = 0, CompletionPercentage = 0.0 };
        }
    }

    private async Task<List<EduTrack.Application.Common.Models.StudySessions.StudySessionHistoryDto>> GetLastStudySessions(string studentId)
    {
        try
        {
            var result = await _mediator.Send(new GetLastStudySessionsQuery(studentId, 5));
            return result.IsSuccess && result.Value != null ? result.Value : new List<EduTrack.Application.Common.Models.StudySessions.StudySessionHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last study sessions");
            return new List<EduTrack.Application.Common.Models.StudySessions.StudySessionHistoryDto>();
        }
    }

    private async Task<List<EduTrack.Application.Common.Models.StudySessions.CourseStudyHistoryDto>> GetLastStudyCourses(string studentId)
    {
        try
        {
            var result = await _mediator.Send(new GetLastStudyCoursesQuery(studentId, 5));
            return result.IsSuccess && result.Value != null ? result.Value : new List<EduTrack.Application.Common.Models.StudySessions.CourseStudyHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last study courses");
            return new List<EduTrack.Application.Common.Models.StudySessions.CourseStudyHistoryDto>();
        }
    }

    private async Task<StudyStatisticsDto> GetStudyStatistics(string studentId)
    {
        try
        {
            _logger.LogInformation("Getting study statistics for student: {StudentId}", studentId);
            
            var now = DateTimeOffset.Now;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddDays(-30);

            // Get all study sessions for the student
            var allSessionsResult = await _mediator.Send(new GetAllStudySessionsQuery(studentId));
            if (!allSessionsResult.IsSuccess || allSessionsResult.Value == null)
            {
                _logger.LogWarning("Failed to get study sessions for student: {StudentId}, Error: {Error}", 
                    studentId, allSessionsResult.Error);
                
                // Fallback to old method
                var fallbackResult = await _mediator.Send(new GetLastStudySessionsQuery(studentId, 1000));
                if (!fallbackResult.IsSuccess || fallbackResult.Value == null)
                {
                    _logger.LogWarning("Fallback also failed for student: {StudentId}", studentId);
                    return new StudyStatisticsDto();
                }
                allSessionsResult = fallbackResult;
            }

            var allSessions = allSessionsResult.Value;
            _logger.LogInformation("Found {Count} study sessions for student: {StudentId}", 
                allSessions.Count, studentId);

            // Calculate study time for different periods
            var todayStudyMinutes = allSessions
                .Where(s => s.StartedAt.Date == today)
                .Sum(s => s.DurationSeconds) / 60;

            var lastWeekStudyMinutes = allSessions
                .Where(s => s.StartedAt >= weekAgo)
                .Sum(s => s.DurationSeconds) / 60;

            var lastMonthStudyMinutes = allSessions
                .Where(s => s.StartedAt >= monthAgo)
                .Sum(s => s.DurationSeconds) / 60;

            var totalStudyMinutes = allSessions
                .Sum(s => s.DurationSeconds) / 60;

            _logger.LogInformation("Study statistics calculated - Today: {Today}min, Week: {Week}min, Month: {Month}min, Total: {Total}min", 
                todayStudyMinutes, lastWeekStudyMinutes, lastMonthStudyMinutes, totalStudyMinutes);

            // Calculate averages
            var studyDays = allSessions
                .Select(s => s.StartedAt.Date)
                .Distinct()
                .Count();

            var studyWeeks = allSessions
                .Select(s => GetWeekOfYear(s.StartedAt))
                .Distinct()
                .Count();

            var studyMonths = allSessions
                .Select(s => new { s.StartedAt.Year, s.StartedAt.Month })
                .Distinct()
                .Count();

            var averageDailyStudyMinutes = studyDays > 0 ? (double)totalStudyMinutes / studyDays : 0;
            var averageWeeklyStudyMinutes = studyWeeks > 0 ? (double)totalStudyMinutes / studyWeeks : 0;
            var averageMonthlyStudyMinutes = studyMonths > 0 ? (double)totalStudyMinutes / studyMonths : 0;

            return new StudyStatisticsDto
            {
                TodayStudyMinutes = (int)todayStudyMinutes,
                LastWeekStudyMinutes = (int)lastWeekStudyMinutes,
                LastMonthStudyMinutes = (int)lastMonthStudyMinutes,
                TotalStudyMinutes = (int)totalStudyMinutes,
                AverageDailyStudyMinutes = averageDailyStudyMinutes,
                AverageWeeklyStudyMinutes = averageWeeklyStudyMinutes,
                AverageMonthlyStudyMinutes = averageMonthlyStudyMinutes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting study statistics for student: {StudentId}", studentId);
            return new StudyStatisticsDto();
        }
    }

    private static int GetWeekOfYear(DateTimeOffset date)
    {
        var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date.DateTime, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Saturday);
    }

    private async Task<List<EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemDto>> GetAccessibleScheduleItems(string studentId)
    {
        try
        {
            var result = await _mediator.Send(new GetScheduleItemsAccessibleToStudentQuery(studentId));
            return result.IsSuccess && result.Value != null ? result.Value : new List<EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessible schedule items for student: {StudentId}", studentId);
            return new List<EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemDto>();
        }
    }
}