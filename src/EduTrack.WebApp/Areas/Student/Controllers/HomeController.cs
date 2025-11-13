using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Exams;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Application.Features.CourseEnrollment.Queries;
using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Domain.Entities;
using EduTrack.WebApp.Models;
using EduTrack.WebApp.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScheduleItemDto = EduTrack.Application.Common.Models.ScheduleItems.ScheduleItemDto;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;
    private readonly IStudentProfileContext _studentProfileContext;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IStudentProfileContext studentProfileContext)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
        _studentProfileContext = studentProfileContext;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        var activeProfileName = await _studentProfileContext.GetActiveProfileNameAsync();
        
        var dashboardData = new StudentDashboardViewModel
        {
            StudentName = currentUser.FullName,
            StudentFirstName = currentUser.FirstName,
            TotalClasses = 0,
            CompletedLessons = 0,
            TotalExams = 0,
            AverageScore = 0,
            ProgressStats = new { },
            ActiveStudentProfileId = activeProfileId,
            ActiveStudentProfileName = activeProfileName
        };

        return View(dashboardData);
    }

    [HttpGet("enrolled-courses-section")]
    public async Task<IActionResult> EnrolledCoursesSection(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync(cancellationToken);
        if (!activeProfileId.HasValue)
        {
            ViewBag.SectionState = "profile-required";
            ViewBag.SectionMessage = "برای مشاهده دوره‌های ثبت‌نام شده، ابتدا یک پروفایل فعال انتخاب کنید.";
            return PartialView("_EnrolledCourses", Array.Empty<StudentCourseEnrollmentSummaryDto>());
        }

        try
        {
            var enrolledCoursesResult = await _mediator.Send(new GetStudentCourseEnrollmentsQuery(currentUser.Id, activeProfileId), cancellationToken);

            if (!enrolledCoursesResult.IsSuccess || enrolledCoursesResult.Value == null)
            {
                ViewBag.SectionState = "error";
                ViewBag.SectionMessage = enrolledCoursesResult.Error ?? "خطا در بارگذاری دوره‌های ثبت‌نام شده.";
                return PartialView("_EnrolledCourses", Array.Empty<StudentCourseEnrollmentSummaryDto>());
            }

            return PartialView("_EnrolledCourses", enrolledCoursesResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load enrolled courses for student {StudentId}", currentUser.Id);
            ViewBag.SectionState = "error";
            ViewBag.SectionMessage = "در بارگذاری دوره‌های ثبت‌نام شده خطایی رخ داد.";
            return PartialView("_EnrolledCourses", Array.Empty<StudentCourseEnrollmentSummaryDto>());
        }
    }

    [HttpGet("recent-courses-section")]
    public async Task<IActionResult> RecentCoursesSection(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync(cancellationToken);
        if (!activeProfileId.HasValue)
        {
            ViewBag.SectionState = "profile-required";
            ViewBag.SectionMessage = "برای مشاهده دوره‌های اخیر، ابتدا یک پروفایل فعال انتخاب کنید.";
            return PartialView("_RecentCourses", Array.Empty<CourseStudyHistoryDto>());
        }

        try
        {
            var courses = await GetLastStudyCourses(currentUser.Id, activeProfileId, cancellationToken);
            return PartialView("_RecentCourses", courses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent courses for student {StudentId}", currentUser.Id);
            ViewBag.SectionState = "error";
            ViewBag.SectionMessage = "در بارگذاری دوره‌های اخیر خطایی رخ داد.";
            return PartialView("_RecentCourses", Array.Empty<CourseStudyHistoryDto>());
        }
    }

    [HttpGet("progress-summary")]
    public async Task<IActionResult> GetProgressSummary(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var progressResult = await _mediator.Send(new GetProgressByStudentQuery(currentUser.Id, 1, 100), cancellationToken);
        var progressItems = progressResult?.Items?.ToList() ?? new List<ProgressDto>();
        var completedLessons = progressItems.Count(p => p.Status == Domain.Enums.ProgressStatus.Done);
        var stats = BuildProgressStats(progressItems);

        return Ok(new
        {
            success = true,
            data = new
            {
                stats,
                completedLessons,
                totalItems = progressItems.Count
            }
        });
    }

    [HttpGet("study-statistics")]
    public async Task<IActionResult> GetStudyStatisticsData(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync(cancellationToken);
        if (!activeProfileId.HasValue)
        {
            return Ok(new ProfileAwareResponse<StudyStatisticsDto>
            {
                Success = false,
                RequiresProfile = true,
                Error = "برای مشاهده آمار مطالعه، ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.",
                Data = new StudyStatisticsDto()
            });
        }

        var statistics = await GetStudyStatistics(currentUser.Id, activeProfileId, cancellationToken);
        return Ok(new ProfileAwareResponse<StudyStatisticsDto>
        {
            Success = true,
            Data = statistics
        });
    }

    [HttpGet("last-study-sessions")]
    public async Task<IActionResult> GetLastStudySessionsData(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync(cancellationToken);
        if (!activeProfileId.HasValue)
        {
            return Ok(new ProfileAwareResponse<List<StudySessionHistoryDto>>
            {
                Success = false,
                RequiresProfile = true,
                Error = "برای مشاهده تاریخچه مطالعه، ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.",
                Data = new List<StudySessionHistoryDto>()
            });
        }

        var sessions = await GetLastStudySessions(currentUser.Id, activeProfileId, cancellationToken);
        return Ok(new ProfileAwareResponse<List<StudySessionHistoryDto>>
        {
            Success = true,
            Data = sessions
        });
    }

    [HttpGet("accessible-schedule-items")]
    public async Task<IActionResult> GetAccessibleScheduleItemsData(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync(cancellationToken);
        if (!activeProfileId.HasValue)
        {
            return Ok(new ProfileAwareResponse<List<ScheduleItemDto>>
            {
                Success = false,
                RequiresProfile = true,
                Error = "برای مشاهده آیتم‌های قابل دسترس، ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.",
                Data = new List<ScheduleItemDto>()
            });
        }

        var scheduleItems = await GetAccessibleScheduleItems(currentUser.Id, activeProfileId.Value, cancellationToken);
        return Ok(new ProfileAwareResponse<List<ScheduleItemDto>>
        {
            Success = true,
            Data = scheduleItems
        });
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

    private static object BuildProgressStats(IReadOnlyCollection<ProgressDto> progressItems)
    {
        var total = progressItems.Count;
        var completed = progressItems.Count(p => p.Status == Domain.Enums.ProgressStatus.Done);
        var inProgress = progressItems.Count(p => p.Status == Domain.Enums.ProgressStatus.InProgress);
        var notStarted = progressItems.Count(p => p.Status == Domain.Enums.ProgressStatus.NotStarted);

        return new
        {
            Total = total,
            Completed = completed,
            InProgress = inProgress,
            NotStarted = notStarted,
            CompletionPercentage = total > 0 ? (completed * 100.0 / total) : 0
        };
    }

    private Task<double> GetStudentAverageScore(string studentId)
    {
        // TODO: Implement when exam results are available
        return Task.FromResult(0.0);
    }

    private Task<List<object>> GetStudentUpcomingExams(string studentId)
    {
        // TODO: Implement when exam queries are available
        return Task.FromResult(new List<object>());
    }

    private async Task<List<StudySessionHistoryDto>> GetLastStudySessions(string studentId, int? studentProfileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetLastStudySessionsQuery(studentId, 5, studentProfileId), cancellationToken);
            return result.IsSuccess && result.Value != null ? result.Value : new List<StudySessionHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last study sessions");
            return new List<StudySessionHistoryDto>();
        }
    }

    private async Task<List<CourseStudyHistoryDto>> GetLastStudyCourses(string studentId, int? studentProfileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetLastStudyCoursesQuery(studentId, 5, studentProfileId), cancellationToken);
            return result.IsSuccess && result.Value != null ? result.Value : new List<CourseStudyHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last study courses");
            return new List<CourseStudyHistoryDto>();
        }
    }

    private async Task<StudyStatisticsDto> GetStudyStatistics(string studentId, int? studentProfileId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting study statistics for student: {StudentId}", studentId);
            
            var now = DateTimeOffset.Now;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddDays(-30);

            // Get all study sessions for the student
            var allSessionsResult = await _mediator.Send(new GetAllStudySessionsQuery(studentId, studentProfileId), cancellationToken);
            if (!allSessionsResult.IsSuccess || allSessionsResult.Value == null)
            {
                _logger.LogWarning("Failed to get study sessions for student: {StudentId}, Error: {Error}", 
                    studentId, allSessionsResult.Error);
                
                // Fallback to old method
                var fallbackResult = await _mediator.Send(new GetLastStudySessionsQuery(studentId, 1000, studentProfileId), cancellationToken);
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

    private async Task<List<ScheduleItemDto>> GetAccessibleScheduleItems(string studentId, int studentProfileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetScheduleItemsAccessibleToStudentQuery(studentId, studentProfileId), cancellationToken);
            return result.IsSuccess && result.Value != null ? result.Value : new List<ScheduleItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessible schedule items for student: {StudentId}", studentId);
            return new List<ScheduleItemDto>();
        }
    }
}