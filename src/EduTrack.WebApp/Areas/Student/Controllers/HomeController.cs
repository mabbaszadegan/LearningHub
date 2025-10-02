using EduTrack.Application.Features.Classroom.Queries;
using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Features.Exams.Queries;
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

        // Get student dashboard data
        var dashboardData = new StudentDashboardViewModel
        {
            StudentName = currentUser.FullName,
            TotalClasses = await GetStudentClassesCount(currentUser.Id),
            CompletedLessons = await GetCompletedLessonsCount(currentUser.Id),
            TotalExams = await GetStudentExamsCount(currentUser.Id),
            AverageScore = await GetStudentAverageScore(currentUser.Id),
            RecentClasses = await GetStudentRecentClasses(currentUser.Id),
            UpcomingExams = await GetStudentUpcomingExams(currentUser.Id),
            ProgressStats = await GetStudentProgressStats(currentUser.Id)
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
}
