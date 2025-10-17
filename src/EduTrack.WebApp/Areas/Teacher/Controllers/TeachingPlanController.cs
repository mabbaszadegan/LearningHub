using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Application.Common.Models.TeachingPlans;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class TeachingPlanController : BaseTeacherController
{
    private readonly ILogger<TeachingPlanController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public TeachingPlanController(
        ILogger<TeachingPlanController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IPageTitleSectionService pageTitleSectionService) : base(pageTitleSectionService)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify course exists and user has access
        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null)
        {
            return NotFound("Course not found");
        }

        if (course.Value.CreatedBy != currentUser.Id)
        {
            return Forbid("You don't have permission to access this course");
        }

        var teachingPlans = await _mediator.Send(new GetTeachingPlansByCourseQuery(courseId));
        if (!teachingPlans.IsSuccess)
        {
            return View(new List<TeachingPlanDto>());
        }

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Value.Title;
        
        // Setup page title section
        await SetPageTitleSectionAsync(PageType.TeachingPlansIndex, courseId);
        
        return View(teachingPlans.Value);
    }

    public async Task<IActionResult> Create(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify course exists and user has access
        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null)
        {
            return NotFound("Course not found");
        }

        if (course.Value.CreatedBy != currentUser.Id)
        {
            return Forbid("You don't have permission to create teaching plans for this course");
        }

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Value.Title;
        
        // Setup page title section
        await SetPageTitleSectionAsync(PageType.TeachingPlanCreate, courseId);
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeachingPlanCommand command)
    {
        if (!ModelState.IsValid)
        {
            var course = await _mediator.Send(new GetCourseByIdQuery(command.CourseId));
            ViewBag.CourseId = command.CourseId;
            ViewBag.CourseTitle = course.Value?.Title ?? "Unknown Course";
            
            // Setup page title section
            await SetPageTitleSectionAsync(PageType.TeachingPlanCreate, command.CourseId);
            
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while creating the teaching plan");
            var course = await _mediator.Send(new GetCourseByIdQuery(command.CourseId));
            ViewBag.CourseId = command.CourseId;
            ViewBag.CourseTitle = course.Value?.Title ?? "Unknown Course";
            
            // Setup page title section
            await SetPageTitleSectionAsync(PageType.TeachingPlanCreate, command.CourseId);
            
            return View(command);
        }

        return RedirectToAction(nameof(Index), new { courseId = command.CourseId });
    }

    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var teachingPlan = await _mediator.Send(new GetTeachingPlanWithAllQuery(id));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("Teaching plan not found");
        }

        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to view this teaching plan");
        }

        // Setup page title section
        await SetPageTitleSectionAsync(PageType.TeachingPlanDetails, id);

        return View(teachingPlan.Value);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(id));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("Teaching plan not found");
        }

        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to edit this teaching plan");
        }

        var command = new UpdateTeachingPlanCommand(
            teachingPlan.Value.Id,
            teachingPlan.Value.Title,
            teachingPlan.Value.Description,
            teachingPlan.Value.Objectives);

        ViewBag.CourseId = teachingPlan.Value.CourseId;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        
        // Setup page title section
        await SetPageTitleSectionAsync(PageType.TeachingPlanEdit, id);
        
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateTeachingPlanCommand command)
    {
        if (!ModelState.IsValid)
        {
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(command.Id));
            ViewBag.CourseId = teachingPlan.Value?.CourseId ?? 0;
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            
            // Setup page title section
            await SetPageTitleSectionAsync(PageType.TeachingPlanEdit, command.Id);
            
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while updating the teaching plan");
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(command.Id));
            ViewBag.CourseId = teachingPlan.Value?.CourseId ?? 0;
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            
            // Setup page title section
            await SetPageTitleSectionAsync(PageType.TeachingPlanEdit, command.Id);
            
            return View(command);
        }

        return RedirectToAction(nameof(Index), new { courseId = result.Value?.CourseId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(id));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("Teaching plan not found");
        }

        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to delete this teaching plan");
        }

        var result = await _mediator.Send(new DeleteTeachingPlanCommand(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "An error occurred while deleting the teaching plan";
        }
        else
        {
            TempData["Success"] = "Teaching plan deleted successfully";
        }

        return RedirectToAction(nameof(Index), new { courseId = teachingPlan.Value.CourseId });
    }
}
