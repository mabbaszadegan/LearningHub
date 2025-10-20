using EduTrack.Application.Features.StudySessions.Commands;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.EducationalContent.Queries;
using EduTrack.Application.Common.Models.StudySessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Domain.Entities;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class EducationalContentController : Controller
{
    private readonly ILogger<EducationalContentController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public EducationalContentController(
        ILogger<EducationalContentController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    /// <summary>
    /// Display educational content for study with timer and statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Study(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get educational content with study statistics
        var contentResult = await _mediator.Send(new GetEducationalContentWithStudyStatsQuery(id, currentUser.Id));
        if (!contentResult.IsSuccess || contentResult.Value == null)
        {
            TempData["Error"] = "محتوا یافت نشد";
            return RedirectToAction("Index", "Home");
        }

        // Check if there's an active study session
        var activeSessionResult = await _mediator.Send(new GetActiveStudySessionQuery(currentUser.Id, id));
        var activeSession = activeSessionResult.IsSuccess ? activeSessionResult.Value : null;

        ViewBag.ActiveSession = activeSession;
        ViewBag.CurrentUserId = currentUser.Id;

        return View(contentResult.Value);
    }

    /// <summary>
    /// Start a new study session
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartStudySession(int educationalContentId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new StartStudySessionCommand(currentUser.Id, educationalContentId));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, sessionId = result.Value!.Id });
    }

    /// <summary>
    /// Complete a study session
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CompleteStudySession(int sessionId, int durationSeconds)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new CompleteStudySessionCommand(sessionId, durationSeconds));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, message = "زمان مطالعه با موفقیت ثبت شد" });
    }

    /// <summary>
    /// Update study session duration (for real-time updates)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateStudySessionDuration(int sessionId, int durationSeconds)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new UpdateStudySessionDurationCommand(sessionId, durationSeconds));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true });
    }

    /// <summary>
    /// Get study session statistics for a content item
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStudyStatistics(int educationalContentId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, educationalContentId));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, statistics = result.Value });
    }
}
