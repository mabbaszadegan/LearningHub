using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

public class ProgressController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(IMediator mediator, ILogger<ProgressController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // GET: Progress
    public async Task<IActionResult> Index()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "You must be logged in to view progress.";
            return RedirectToAction("Index", "Home");
        }

        var progress = await _mediator.Send(new GetProgressByStudentQuery(userId, 1, 50));
        return View(progress);
    }

    // GET: Progress/Stats
    public async Task<IActionResult> Stats()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "You must be logged in to view stats.";
            return RedirectToAction("Index", "Home");
        }

        var result = await _mediator.Send(new GetStudentStatsQuery(userId));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index", "Home");
        }

        return View(result.Value);
    }

    // GET: Progress/Lesson/5
    public async Task<IActionResult> Lesson(int id)
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "You must be logged in to view lesson progress.";
            return RedirectToAction("Index", "Home");
        }

        var result = await _mediator.Send(new GetProgressByLessonQuery(userId, id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index", "Home");
        }

        return View(result.Value);
    }

    // GET: Progress/Exam/5
    public async Task<IActionResult> Exam(int id)
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "You must be logged in to view exam progress.";
            return RedirectToAction("Index", "Home");
        }

        var result = await _mediator.Send(new GetProgressByExamQuery(userId, id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index", "Home");
        }

        return View(result.Value);
    }
}
