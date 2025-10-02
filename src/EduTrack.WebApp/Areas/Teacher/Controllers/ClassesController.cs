using EduTrack.Application.Features.Classroom.Commands;
using EduTrack.Application.Features.Classroom.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class ClassesController : Controller
{
    private readonly ILogger<ClassesController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ClassesController(
        ILogger<ClassesController> logger, 
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

        // Get teacher's classes
        var classes = await _mediator.Send(new GetClassesQuery(1, 50, true));
        var teacherClasses = classes.Items.Where(c => c.TeacherId == currentUser.Id).ToList();

        return View(teacherClasses);
    }

    public async Task<IActionResult> Create()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        await LoadCreateViewData(currentUser.Id);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClassCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        if (ModelState.IsValid)
        {
            // Set the current user as teacher
            command = command with { TeacherId = currentUser.Id };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "کلاس با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = result.Error;
            }
        }

        await LoadCreateViewData(currentUser.Id);
        return View(command);
    }

    private async Task LoadCreateViewData(string teacherId)
    {
        try
        {
            // Load courses created by this teacher
            var courses = await _mediator.Send(new GetCoursesQuery(1, 100, true));
            var teacherCourses = courses.Items.Where(c => c.CreatedBy == teacherId).ToList();
            ViewBag.Courses = new SelectList(teacherCourses, "Id", "Title");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading courses for class creation");
            ViewBag.Courses = new SelectList(new List<object>(), "Id", "Title");
        }
    }
}