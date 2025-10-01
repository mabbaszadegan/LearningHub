using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Exams.Queries;
using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IMediator mediator, ILogger<HomeController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get courses for the main dashboard
            var courses = await _mediator.Send(new GetCoursesQuery(1, 10, true));
            
            // Get recent exams for dashboard
            var recentExams = await _mediator.Send(new GetExamsQuery(1, 5, true));
            
            // Get user progress if logged in
            var userId = User.Identity?.Name;
            var userProgress = userId != null ? await _mediator.Send(new GetProgressByStudentQuery(userId, 1, 10)) : null;
            
            // Calculate progress metrics
            var completedCourses = userProgress?.Items.Count(p => p.Status == Domain.Enums.ProgressStatus.Done) ?? 0;
            var completedLessons = userProgress?.Items.Count(p => p.LessonId.HasValue && p.Status == Domain.Enums.ProgressStatus.Done) ?? 0;
            var completedExams = userProgress?.Items.Count(p => p.ExamId.HasValue && p.Status == Domain.Enums.ProgressStatus.Done) ?? 0;
            var overallProgress = courses.TotalCount > 0 ? (double)completedCourses / courses.TotalCount * 100 : 0;
            
            // Create dashboard view model
            var dashboardData = new DashboardViewModel
            {
                Courses = courses,
                RecentExams = recentExams,
                UserProgress = userProgress,
                TotalCourses = courses.TotalCount,
                TotalLessons = courses.Items.Sum(c => c.LessonCount),
                TotalModules = courses.Items.Sum(c => c.ModuleCount),
                IsUserLoggedIn = !string.IsNullOrEmpty(userId),
                CompletedCourses = completedCourses,
                CompletedLessons = completedLessons,
                CompletedExams = completedExams,
                OverallProgress = overallProgress
            };
            
            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            // Fallback to basic courses data
            var courses = await _mediator.Send(new GetCoursesQuery(1, 10, true));
            var recentExams = await _mediator.Send(new GetExamsQuery(1, 5, true));
            
            var fallbackData = new DashboardViewModel
            {
                Courses = courses,
                RecentExams = recentExams,
                UserProgress = null,
                TotalCourses = courses.TotalCount,
                TotalLessons = courses.Items.Sum(c => c.LessonCount),
                TotalModules = courses.Items.Sum(c => c.ModuleCount),
                IsUserLoggedIn = false,
                CompletedCourses = 0,
                CompletedLessons = 0,
                CompletedExams = 0,
                OverallProgress = 0
            };
            
            return View(fallbackData);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
