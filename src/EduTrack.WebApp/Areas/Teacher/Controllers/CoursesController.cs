using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class CoursesController : Controller
{
    private readonly ILogger<CoursesController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public CoursesController(
        ILogger<CoursesController> logger, 
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

        // Get teacher's courses
        var courses = await _mediator.Send(new GetCoursesQuery(1, 50, true));
        var teacherCourses = courses.Items.Where(c => c.CreatedBy == currentUser.Id).ToList();

        return View(teacherCourses);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseCommand command)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Public" });
            }

            // The CreatedBy is set automatically in the handler using ICurrentUserService

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "دوره با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = result.Error;
            }
        }

        return View(command);
    }

    [HttpGet]
    public async Task<IActionResult> GetHierarchy()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        try
        {
            var result = await _mediator.Send(new GetTeacherCoursesHierarchyQuery(currentUser.Id));
            if (result.IsSuccess)
            {
                return Json(new { success = true, data = result.Value });
            }
            else
            {
                return Json(new { success = false, error = result.Error });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher courses hierarchy");
            return Json(new { success = false, error = "خطا در دریافت اطلاعات" });
        }
    }
}