using EduTrack.Application.Features.StudySessions.Commands;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Domain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EduTrack.WebApp.Areas.Student.Controllers;

public class CreateAndCompleteStudySessionRequest
{
    public int ScheduleItemId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset EndedAt { get; set; }
}

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

        // Get schedule item details
        var scheduleItemResult = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
        if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
        {
            TempData["Error"] = "آیتم آموزشی یافت نشد";
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

        // Check if there's an active study session
        var activeSessionResult = await _mediator.Send(new GetActiveStudySessionQuery(currentUser.Id, id));
        var activeSession = activeSessionResult.IsSuccess ? activeSessionResult.Value : null;

        // Parse content JSON based on type
        var parsedContent = ParseContentJson(scheduleItem.ContentJson, scheduleItem.Type);

        ViewBag.ActiveSession = activeSession;
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.CourseId = teachingPlan.CourseId;
        ViewBag.ParsedContent = parsedContent;

        return View(scheduleItem);
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

    private static object? ParseContentJson(string contentJson, ScheduleItemType type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentJson))
            {
                return null;
            }

            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            
            return type switch
            {
                ScheduleItemType.Reminder => JsonConvert.DeserializeObject<ReminderContent>(contentJson, jsonSettings),
                ScheduleItemType.Writing => JsonConvert.DeserializeObject<WritingContent>(contentJson, jsonSettings),
                ScheduleItemType.Audio => JsonConvert.DeserializeObject<AudioContent>(contentJson, jsonSettings),
                ScheduleItemType.GapFill => JsonConvert.DeserializeObject<GapFillContent>(contentJson, jsonSettings),
                ScheduleItemType.MultipleChoice => JsonConvert.DeserializeObject<MultipleChoiceContent>(contentJson, jsonSettings),
                ScheduleItemType.Match => JsonConvert.DeserializeObject<MatchingContent>(contentJson, jsonSettings),
                ScheduleItemType.ErrorFinding => JsonConvert.DeserializeObject<ErrorFindingContent>(contentJson, jsonSettings),
                ScheduleItemType.CodeExercise => JsonConvert.DeserializeObject<CodeExerciseContent>(contentJson, jsonSettings),
                ScheduleItemType.Quiz => JsonConvert.DeserializeObject<QuizContent>(contentJson, jsonSettings),
                _ => null
            };
        }
        catch
        {
            // Log error but don't throw - return null to show empty content
            return null;
        }
    }
}
