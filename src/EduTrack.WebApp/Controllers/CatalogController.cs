using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EduTrack.WebApp.Controllers;

public class CatalogController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<CatalogController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CatalogController(IMediator mediator, ILogger<CatalogController> logger, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    // GET: Courses
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var courses = await _mediator.Send(new GetCoursesQuery(pageNumber, pageSize, true));
        return View(courses);
    }

    // GET: Courses/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var result = await _mediator.Send(new GetCourseByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // GET: Courses/Create
    [Authorize(Roles = "Admin,Teacher")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(CreateCourseCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Course created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }

    // GET: Courses/Edit/5
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetCourseByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var command = new UpdateCourseCommand(
            result.Value!.Id,
            result.Value.Title,
            result.Value.Description,
            result.Value.Thumbnail,
            result.Value.IsActive,
            result.Value.Order);

        return View(command);
    }

    // POST: Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(UpdateCourseCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Course updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }

    // GET: Courses/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new GetCourseByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // POST: Courses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _mediator.Send(new DeleteCourseCommand(id));
        if (result.IsSuccess)
        {
            TempData["Success"] = "Course deleted successfully.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Modules for a course
    public async Task<IActionResult> Modules(int courseId)
    {
        var result = await _mediator.Send(new GetModulesByCourseIdQuery(courseId));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // GET: Lessons for a module
    public async Task<IActionResult> Lessons(int moduleId)
    {
        var result = await _mediator.Send(new GetLessonsByModuleIdQuery(moduleId));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // GET: Create Module
    [Authorize(Roles = "Admin,Teacher")]
    public IActionResult CreateModule(int courseId)
    {
        var command = new CreateModuleCommand(courseId, string.Empty, string.Empty, 0);
        return View(command);
    }

    // POST: Create Module
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateModule(CreateModuleCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Module created successfully.";
                return RedirectToAction(nameof(Modules), new { courseId = command.CourseId });
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }

    // GET: Create Lesson
    [Authorize(Roles = "Admin,Teacher")]
    public IActionResult CreateLesson(int moduleId)
    {
        var command = new CreateLessonCommand(moduleId, string.Empty, string.Empty, string.Empty, 0, 0);
        return View(command);
    }

    // POST: Create Lesson
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateLesson(CreateLessonCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Lesson created successfully.";
                return RedirectToAction(nameof(Lessons), new { moduleId = command.ModuleId });
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }

    // GET: Create Resource
    [Authorize(Roles = "Admin,Teacher")]
    public IActionResult CreateResource(int lessonId)
    {
        var command = new CreateResourceCommand(lessonId, string.Empty, string.Empty, Domain.Enums.ResourceType.Document, string.Empty, string.Empty, null, string.Empty, 0);
        return View(command);
    }

    // POST: Create Resource
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateResource(CreateResourceCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Resource created successfully.";
                return RedirectToAction(nameof(Lessons), new { moduleId = command.LessonId });
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }
}
