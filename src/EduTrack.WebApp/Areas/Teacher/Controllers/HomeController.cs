using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.Exams.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Application.Common.Models.Exams;
using EduTrack.Application.Common.Models.Courses;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class HomeController : BaseTeacherController
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public HomeController(
        ILogger<HomeController> logger, 
        UserManager<User> userManager,
        IMediator mediator,
        IPageTitleSectionService pageTitleSectionService) : base(pageTitleSectionService)
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

        // Get teacher dashboard data
        var dashboardData = new DashboardViewModel
        {
            TotalCourses = await GetTeacherCoursesCount(currentUser.Id),
            Courses = await GetTeacherCourses(currentUser.Id),
            RecentExams = await GetTeacherExams(currentUser.Id)
        };

        // Setup page title section
        await SetPageTitleSectionAsync(PageType.TeacherDashboard);

        return View("TeacherDashboard", dashboardData);
    }

    public async Task<IActionResult> MyStudents()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // TODO: Get teacher's students from classes
        var students = new List<object>(); // Placeholder for now

        return View(students);
    }

    private async Task<int> GetTeacherCoursesCount(string teacherId)
    {
        try
        {
            var courses = await _mediator.Send(new GetCoursesQuery(1, 100, true));
            return courses.Items.Count(c => c.CreatedBy == teacherId);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<PaginatedList<CourseDto>> GetTeacherCourses(string teacherId)
    {
        try
        {
            var courses = await _mediator.Send(new GetCoursesQuery(1, 10, true));
            var teacherCourses = courses.Items.Where(c => c.CreatedBy == teacherId).ToList();
            
            return new PaginatedList<CourseDto>(teacherCourses, teacherCourses.Count, 1, 10);
        }
        catch
        {
            return new PaginatedList<CourseDto>(new List<CourseDto>(), 0, 1, 10);
        }
    }

    private async Task<PaginatedList<ExamDto>> GetTeacherExams(string teacherId)
    {
        try
        {
            var exams = await _mediator.Send(new GetExamsQuery(1, 10, true));
            return exams;
        }
        catch
        {
            return new PaginatedList<ExamDto>(new List<ExamDto>(), 0, 1, 10);
        }
    }
}