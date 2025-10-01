using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

public class CatalogController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(IMediator mediator, ILogger<CatalogController> logger)
    {
        _mediator = mediator;
        _logger = logger;
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
    public IActionResult Create()
    {
        return View();
    }

    // POST: Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
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
}
