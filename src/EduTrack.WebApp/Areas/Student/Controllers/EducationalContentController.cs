using EduTrack.Application.Features.StudySessions.Commands;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
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

        // Get schedule item details
        var scheduleItemResult = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
        if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
        {
            TempData["Error"] = "محتوا یافت نشد";
            return RedirectToAction("Index", "Home");
        }

        var scheduleItem = scheduleItemResult.Value;

        // Get teaching plan to get course ID
        var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.TeachingPlanId));
        if (!teachingPlanResult.IsSuccess || teachingPlanResult.Value == null)
        {
            TempData["Error"] = "طرح تدریس یافت نشد";
            return RedirectToAction("Index", "Home");
        }

        var teachingPlan = teachingPlanResult.Value;

        // Get study statistics
        var statisticsResult = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, id));
        var statistics = statisticsResult.IsSuccess ? statisticsResult.Value : new StudySessionStatisticsDto();

        // Create a combined object for the view
        var contentWithStats = new
        {
            Id = scheduleItem.Id,
            TeachingPlanId = scheduleItem.TeachingPlanId,
            CourseId = teachingPlan.CourseId,
            Title = scheduleItem.Title,
            Description = scheduleItem.Description,
            ContentJson = scheduleItem.ContentJson,
            Type = scheduleItem.Type,
            CreatedAt = scheduleItem.CreatedAt,
            UpdatedAt = scheduleItem.UpdatedAt,
            StudyStatistics = statistics
        };

        // Check if there's an active study session
        var activeSessionResult = await _mediator.Send(new GetActiveStudySessionQuery(currentUser.Id, id));
        var activeSession = activeSessionResult.IsSuccess ? activeSessionResult.Value : null;

        ViewBag.ActiveSession = activeSession;
        ViewBag.CurrentUserId = currentUser.Id;

        return View(contentWithStats);
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
    /// Create and complete a study session in one operation
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAndCompleteStudySession([FromBody] CreateAndCompleteStudySessionRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new CreateAndCompleteStudySessionCommand(
            currentUser.Id, 
            request.ScheduleItemId, 
            request.StartedAt, 
            request.EndedAt));
        
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, sessionId = result.Value!.Id });
    }

    /// <summary>
    /// Delete a study session (for exit without saving)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteStudySession(int sessionId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new DeleteStudySessionCommand(sessionId));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, message = "جلسه مطالعه حذف شد" });
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
