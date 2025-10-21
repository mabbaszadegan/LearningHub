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

public class CompleteStudySessionRequest
{
    public int StudySessionId { get; set; }
}

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

        // Get schedule item details with study stats
        var scheduleItemWithStatsResult = await _mediator.Send(new GetScheduleItemWithStudyStatsQuery(id, currentUser.Id));
        if (!scheduleItemWithStatsResult.IsSuccess || scheduleItemWithStatsResult.Value == null)
        {
            TempData["Error"] = "آیتم آموزشی یافت نشد";
            return RedirectToAction("Index", "Home");
        }

        var scheduleItemWithStats = scheduleItemWithStatsResult.Value;

        // Check if there's an active study session
        var activeSessionResult = await _mediator.Send(new GetActiveStudySessionQuery(currentUser.Id, id));
        var activeSession = activeSessionResult.IsSuccess ? activeSessionResult.Value : null;

        ViewBag.ActiveSession = activeSession;
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.ScheduleItem = scheduleItemWithStats;
        ViewBag.CourseId = scheduleItemWithStats.CourseId; // Add course ID for navigation

        return View("~/Areas/Student/Views/EducationalContent/Study.cshtml", scheduleItemWithStats);
    }

    /// <summary>
    /// Start a new study session for schedule item
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartStudySession(int scheduleItemId)
    {
        try
        {
            _logger.LogInformation("StartStudySession called with ScheduleItemId: {ScheduleItemId}", scheduleItemId);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found");
                return Json(new { success = false, error = "کاربر یافت نشد" });
            }

            _logger.LogInformation("User found: {UserId}", currentUser.Id);

            // For schedule items, we'll use the schedule item ID as educational content ID
            // In a real implementation, you'd need to map schedule items to educational content
            var result = await _mediator.Send(new StartStudySessionCommand(currentUser.Id, scheduleItemId));
            if (!result.IsSuccess)
            {
                _logger.LogError("StartStudySessionCommand failed: {Error}", result.Error);
                return Json(new { success = false, error = result.Error });
            }

            _logger.LogInformation("Study session started successfully with ID: {SessionId}", result.Value!.Id);
            return Json(new { success = true, sessionId = result.Value!.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StartStudySession");
            return Json(new { success = false, error = "خطا در شروع جلسه مطالعه" });
        }
    }

    /// <summary>
    /// Test endpoint to check if study session can be completed
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TestCompleteStudySession([FromBody] CompleteStudySessionRequest request)
    {
        try
        {
            _logger.LogInformation("TestCompleteStudySession called with StudySessionId: {StudySessionId}", 
                request.StudySessionId);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found");
                return Json(new { success = false, error = "کاربر یافت نشد" });
            }

            _logger.LogInformation("User found: {UserId}", currentUser.Id);

            // First, check if the study session exists
            var studySession = await _mediator.Send(new GetStudySessionByIdQuery(request.StudySessionId));
            if (!studySession.IsSuccess)
            {
                _logger.LogError("Study session not found: {Error}", studySession.Error);
                return Json(new { success = false, error = $"جلسه مطالعه یافت نشد: {studySession.Error}" });
            }

            _logger.LogInformation("Study session found: {SessionId}", studySession.Value!.Id);

            var result = await _mediator.Send(new CompleteStudySessionCommand(request.StudySessionId));
            if (!result.IsSuccess)
            {
                _logger.LogError("CompleteStudySessionCommand failed: {Error}", result.Error);
                return Json(new { success = false, error = result.Error });
            }

            _logger.LogInformation("Study session completed successfully with duration: {DurationSeconds} seconds", 
                result.Value!.DurationSeconds);
            return Json(new { success = true, message = "زمان مطالعه با موفقیت ثبت شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TestCompleteStudySession");
            return Json(new { success = false, error = $"خطا در ثبت زمان مطالعه: {ex.Message}" });
        }
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
