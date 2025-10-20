using EduTrack.Application.Features.StudySessions.Commands;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Common.Models.TeachingPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Domain.Entities;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class ScheduleItemController : Controller
{
    private readonly ILogger<ScheduleItemController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ScheduleItemController(
        ILogger<ScheduleItemController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    /// <summary>
    /// Display schedule item for study with timer and statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Study(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get schedule item details
        var scheduleItemResult = await _mediator.Send(new GetScheduleItemByIdQuery(id));
        if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
        {
            TempData["Error"] = "آیتم آموزشی یافت نشد";
            return RedirectToAction("Index", "Home");
        }

        var scheduleItem = scheduleItemResult.Value;

        // For now, we'll create a mock EducationalContentWithStudyStatsDto
        // In a real implementation, you'd need to map ScheduleItem to EducationalContent
        var mockContent = new EducationalContentWithStudyStatsDto
        {
            Id = scheduleItem.Id,
            Title = scheduleItem.Title,
            Description = scheduleItem.Description,
            Type = EduTrack.Domain.Enums.EducationalContentType.Text, // Default type
            TextContent = scheduleItem.ContentJson, // Use ContentJson as text content
            IsActive = true,
            Order = 0,
            CreatedAt = scheduleItem.CreatedAt,
            UpdatedAt = scheduleItem.UpdatedAt,
            CreatedBy = "System",
            StudyStatistics = new StudySessionStatisticsDto
            {
                TotalStudyTimeSeconds = 0,
                StudySessionsCount = 0,
                LastStudyDate = null,
                RecentSessions = new List<StudySessionDto>()
            }
        };

        // Check if there's an active study session
        var activeSessionResult = await _mediator.Send(new GetActiveStudySessionQuery(currentUser.Id, id));
        var activeSession = activeSessionResult.IsSuccess ? activeSessionResult.Value : null;

        ViewBag.ActiveSession = activeSession;
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.ScheduleItem = scheduleItem;

        return View("~/Areas/Student/Views/EducationalContent/Study.cshtml", mockContent);
    }

    /// <summary>
    /// Start a new study session for schedule item
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartStudySession(int scheduleItemId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // For schedule items, we'll use the schedule item ID as educational content ID
        // In a real implementation, you'd need to map schedule items to educational content
        var result = await _mediator.Send(new StartStudySessionCommand(currentUser.Id, scheduleItemId));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, sessionId = result.Value!.Id });
    }

    /// <summary>
    /// Complete a study session for schedule item
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
    /// Update study session duration for schedule item
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
    /// Get study session statistics for a schedule item
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStudyStatistics(int scheduleItemId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // For schedule items, we'll use the schedule item ID as educational content ID
        var result = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, scheduleItemId));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, statistics = result.Value });
    }
}
