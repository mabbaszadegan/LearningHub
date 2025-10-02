using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Exams.Queries;
using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;

    public HomeController(IMediator mediator, ILogger<HomeController> logger, UserManager<User> userManager)
    {
        _mediator = mediator;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // If user is not authenticated, show landing page
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View("Landing");
        }

        // If user is authenticated, redirect to role-based dashboard
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            return user.Role switch
            {
                UserRole.Teacher => RedirectToAction("TeacherDashboard"),
                UserRole.Student => RedirectToAction("StudentDashboard"),
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("StudentDashboard")
            };
        }

        // Fallback to landing page
        return View("Landing");
    }

    [Authorize]
    public async Task<IActionResult> StudentDashboard()
    {
        try
        {
            // Get courses for the student dashboard
            var courses = await _mediator.Send(new GetCoursesQuery(1, 10, true));
            
            // Get recent exams for dashboard
            var recentExams = await _mediator.Send(new GetExamsQuery(1, 5, true));
            
            // Get user progress
            var userId = User.Identity?.Name;
            var userProgress = userId != null ? await _mediator.Send(new GetProgressByStudentQuery(userId, 1, 10)) : null;
            
            // Calculate progress metrics
            var completedCourses = userProgress?.Items.Count(p => p.Status == ProgressStatus.Done) ?? 0;
            var completedLessons = userProgress?.Items.Count(p => p.LessonId.HasValue && p.Status == ProgressStatus.Done) ?? 0;
            var completedExams = userProgress?.Items.Count(p => p.ExamId.HasValue && p.Status == ProgressStatus.Done) ?? 0;
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
                IsUserLoggedIn = true,
                CompletedCourses = completedCourses,
                CompletedLessons = completedLessons,
                CompletedExams = completedExams,
                OverallProgress = overallProgress
            };
            
            return View("StudentDashboard", dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading student dashboard data");
            return View("Error");
        }
    }

    [Authorize]
    public async Task<IActionResult> TeacherDashboard()
    {
        try
        {
            // Get courses for the teacher dashboard
            var courses = await _mediator.Send(new GetCoursesQuery(1, 10, true));
            
            // Get recent exams for dashboard
            var recentExams = await _mediator.Send(new GetExamsQuery(1, 5, true));
            
            // Create teacher-specific dashboard data
            var dashboardData = new DashboardViewModel
            {
                Courses = courses,
                RecentExams = recentExams,
                UserProgress = null,
                TotalCourses = courses.TotalCount,
                TotalLessons = courses.Items.Sum(c => c.LessonCount),
                TotalModules = courses.Items.Sum(c => c.ModuleCount),
                IsUserLoggedIn = true,
                CompletedCourses = 0,
                CompletedLessons = 0,
                CompletedExams = 0,
                OverallProgress = 0
            };
            
            return View("TeacherDashboard", dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading teacher dashboard data");
            return View("Error");
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
