using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher,Admin")]
public class TeachingSessionsController : Controller
{
    private readonly ILogger<TeachingSessionsController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public TeachingSessionsController(
        ILogger<TeachingSessionsController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Dashboard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get all teaching plans for the current teacher
        var teachingPlansResult = await _mediator.Send(new GetTeachingPlansByTeacherQuery(currentUser.Id));
        if (!teachingPlansResult.IsSuccess)
        {
            TempData["ErrorMessage"] = teachingPlansResult.Error ?? "خطا در بارگذاری پلن‌های آموزشی.";
            return View(new List<TeachingSessionReportDto>());
        }

        // Get all sessions from all teaching plans
        var allSessions = new List<TeachingSessionReportDto>();
        if (teachingPlansResult.Value != null)
        {
            foreach (var plan in teachingPlansResult.Value)
            {
                var sessionsResult = await _mediator.Send(new ListTeachingSessionReportsQuery(plan.Id));
                if (sessionsResult.IsSuccess && sessionsResult.Value != null)
                {
                    allSessions.AddRange(sessionsResult.Value);
                }
            }
        }

        return View(allSessions);
    }

    public async Task<IActionResult> Index(int planId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(planId));
        if (!teachingPlanResult.IsSuccess || teachingPlanResult.Value == null)
        {
            TempData["ErrorMessage"] = teachingPlanResult.Error ?? "پلن آموزشی یافت نشد.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        var sessionReportsResult = await _mediator.Send(new ListTeachingSessionReportsQuery(planId));
        if (!sessionReportsResult.IsSuccess)
        {
            TempData["ErrorMessage"] = sessionReportsResult.Error ?? "خطا در بارگذاری گزارش‌های جلسات.";
            return View(new List<TeachingSessionReportDto>());
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlanResult.Value.Title;
        ViewBag.CourseId = teachingPlanResult.Value.CourseId;
        ViewBag.CourseTitle = teachingPlanResult.Value.CourseTitle;
        return View(sessionReportsResult.Value);
    }

    public async Task<IActionResult> Create(int planId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(planId));
        if (!teachingPlanResult.IsSuccess || teachingPlanResult.Value == null)
        {
            TempData["ErrorMessage"] = teachingPlanResult.Error ?? "پلن آموزشی یافت نشد.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlanResult.Value.Title;
        ViewBag.CourseId = teachingPlanResult.Value.CourseId;
        ViewBag.CourseTitle = teachingPlanResult.Value.CourseTitle;
        ViewBag.SessionModes = new SelectList(Enum.GetValues(typeof(SessionMode)));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeachingSessionReportCommand command)
    {
        if (!ModelState.IsValid)
        {
            var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(command.TeachingPlanId));
            if (teachingPlanResult.IsSuccess && teachingPlanResult.Value != null)
            {
                ViewBag.TeachingPlanId = command.TeachingPlanId;
                ViewBag.TeachingPlanTitle = teachingPlanResult.Value.Title;
                ViewBag.SessionModes = new SelectList(Enum.GetValues(typeof(SessionMode)), command.Mode);
            }
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "گزارش جلسه با موفقیت ایجاد شد.";
            return RedirectToAction(nameof(Index), new { planId = command.TeachingPlanId });
        }

        TempData["ErrorMessage"] = result.Error ?? "خطا در ایجاد گزارش جلسه.";
        var teachingPlanResultError = await _mediator.Send(new GetTeachingPlanByIdQuery(command.TeachingPlanId));
        if (teachingPlanResultError.IsSuccess && teachingPlanResultError.Value != null)
        {
            ViewBag.TeachingPlanId = command.TeachingPlanId;
            ViewBag.TeachingPlanTitle = teachingPlanResultError.Value.Title;
            ViewBag.CourseId = teachingPlanResultError.Value.CourseId;
            ViewBag.CourseTitle = teachingPlanResultError.Value.CourseTitle;
            ViewBag.SessionModes = new SelectList(Enum.GetValues(typeof(SessionMode)), command.Mode);
        }
        return View(command);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sessionReportResult = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(id));
        if (!sessionReportResult.IsSuccess || sessionReportResult.Value == null)
        {
            TempData["ErrorMessage"] = sessionReportResult.Error ?? "گزارش جلسه یافت نشد.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        // Get course information for breadcrumb
        var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(sessionReportResult.Value.TeachingPlanId));
        if (teachingPlanResult.IsSuccess && teachingPlanResult.Value != null)
        {
            ViewBag.CourseId = teachingPlanResult.Value.CourseId;
            ViewBag.CourseTitle = teachingPlanResult.Value.CourseTitle;
        }

        return View(sessionReportResult.Value);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var sessionReportResult = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(id));
        if (!sessionReportResult.IsSuccess || sessionReportResult.Value == null)
        {
            TempData["ErrorMessage"] = sessionReportResult.Error ?? "Session report not found.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        ViewBag.SessionModes = new SelectList(Enum.GetValues(typeof(SessionMode)), sessionReportResult.Value.Mode);
        return View(sessionReportResult.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateTeachingSessionReportCommand command)
    {
        if (!ModelState.IsValid)
        {
            var sessionReportResult = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(command.Id));
            if (sessionReportResult.IsSuccess && sessionReportResult.Value != null)
            {
                ViewBag.SessionModes = new SelectList(Enum.GetValues(typeof(SessionMode)), command.Mode);
            }
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Session report updated successfully.";
            return RedirectToAction(nameof(Details), new { id = command.Id });
        }

        TempData["ErrorMessage"] = result.Error ?? "Error updating session report.";
        var sessionReportResultError = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(command.Id));
        if (sessionReportResultError.IsSuccess && sessionReportResultError.Value != null)
        {
            ViewBag.SessionModes = new SelectList(Enum.GetValues(typeof(SessionMode)), command.Mode);
        }
        return View(command);
    }

    [HttpPost]
    public async Task<IActionResult> SaveAttendance(int reportId, List<TeachingSessionAttendanceDto> attendance)
    {
        var command = new RecordAttendanceCommand(reportId, attendance);
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "Attendance saved successfully." });
        }

        return Json(new { success = false, message = result.Error ?? "Error saving attendance." });
    }

    public async Task<IActionResult> CreateScheduleItemFromReport(int reportId)
    {
        var sessionReportResult = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(reportId));
        if (!sessionReportResult.IsSuccess || sessionReportResult.Value == null)
        {
            TempData["ErrorMessage"] = sessionReportResult.Error ?? "Session report not found.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        ViewBag.ReportId = reportId;
        ViewBag.SessionReport = sessionReportResult.Value;
        ViewBag.ScheduleItemTypes = new SelectList(Enum.GetValues(typeof(ScheduleItemType)));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateScheduleItemFromReport(CreateTargetedScheduleItemsFromReportCommand command)
    {
        if (!ModelState.IsValid)
        {
            var sessionReportResult = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(command.ReportId));
            if (sessionReportResult.IsSuccess && sessionReportResult.Value != null)
            {
                ViewBag.ReportId = command.ReportId;
                ViewBag.SessionReport = sessionReportResult.Value;
                ViewBag.ScheduleItemTypes = new SelectList(Enum.GetValues(typeof(ScheduleItemType)), command.Type);
            }
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Schedule items created successfully.";
            return RedirectToAction(nameof(Details), new { id = command.ReportId });
        }

        TempData["ErrorMessage"] = result.Error ?? "Error creating schedule items.";
        var sessionReportResultError = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(command.ReportId));
        if (sessionReportResultError.IsSuccess && sessionReportResultError.Value != null)
        {
            ViewBag.ReportId = command.ReportId;
            ViewBag.SessionReport = sessionReportResultError.Value;
            ViewBag.ScheduleItemTypes = new SelectList(Enum.GetValues(typeof(ScheduleItemType)), command.Type);
        }
        return View(command);
    }

    public async Task<IActionResult> Complete(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var completionDataResult = await _mediator.Send(new GetSessionCompletionDataQuery(id));
        if (!completionDataResult.IsSuccess || completionDataResult.Value == null)
        {
            TempData["ErrorMessage"] = completionDataResult.Error ?? "خطا در بارگذاری اطلاعات جلسه.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        // Debug: Log completion data
        var completionData = completionDataResult.Value;
        System.Diagnostics.Debug.WriteLine($"Completion Data - Groups Count: {completionData.Groups.Count}");
        System.Diagnostics.Debug.WriteLine($"Completion Data - AvailableSubTopics Count: {completionData.AvailableSubTopics.Count}");
        System.Diagnostics.Debug.WriteLine($"Completion Data - AvailableLessons Count: {completionData.AvailableLessons.Count}");
        System.Diagnostics.Debug.WriteLine($"Completion Data - PlannedItems Count: {completionData.PlannedItems?.Count ?? 0}");

        // Create simple JSON data for JavaScript using System.Text.Json
        var jsonOptions = new JsonSerializerSettings
        {
        };

        var groupsJson = JsonConvert.SerializeObject(completionData.Groups.Select(g => new { g.Id, g.Name, g.MemberCount }), jsonOptions);
        var subtopicsJson = JsonConvert.SerializeObject(completionData.AvailableSubTopics.Select(s => new { s.Id, s.Title, s.ChapterTitle }), jsonOptions);
        var lessonsJson = JsonConvert.SerializeObject(completionData.AvailableLessons.Select(l => new { l.Id, l.Title, l.ModuleTitle }), jsonOptions);
        var plannedItemsJson = JsonConvert.SerializeObject((completionData.PlannedItems ?? new List<PlannedItemDto>()).Select(p => new { p.StudentGroupId, p.PlannedObjectives }), jsonOptions);

        ViewBag.GroupsJson = groupsJson;
        ViewBag.AvailableSubTopicsJson = subtopicsJson;
        ViewBag.AvailableLessonsJson = lessonsJson;
        ViewBag.PlannedItemsJson = plannedItemsJson;

        // Debug: Log JSON strings
        System.Diagnostics.Debug.WriteLine($"Groups JSON: {groupsJson}");
        System.Diagnostics.Debug.WriteLine($"SubTopics JSON: {subtopicsJson}");
        System.Diagnostics.Debug.WriteLine($"Lessons JSON: {lessonsJson}");
        System.Diagnostics.Debug.WriteLine($"PlannedItems JSON: {plannedItemsJson}");

        // Get course information for breadcrumb
        var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(completionDataResult.Value.TeachingPlanId));
        if (teachingPlanResult.IsSuccess && teachingPlanResult.Value != null)
        {
            ViewBag.CourseId = teachingPlanResult.Value.CourseId;
            ViewBag.CourseTitle = teachingPlanResult.Value.CourseTitle;
        }

        return View(completionDataResult.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCompletion(SaveSessionCompletionCommand command)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "خطا در اعتبارسنجی داده‌ها.";
            return RedirectToAction("Complete", new { id = command.TeachingSessionReportId });
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "گزارش جلسه با موفقیت تکمیل شد.";
            return RedirectToAction("Details", new { id = command.TeachingSessionReportId });
        }

        TempData["ErrorMessage"] = result.Error ?? "خطا در ذخیره گزارش جلسه.";
        return RedirectToAction("Complete", new { id = command.TeachingSessionReportId });
    }

    [HttpGet]
    public async Task<IActionResult> HasPlan(int sessionId)
    {
        var hasPlanResult = await _mediator.Send(new CheckIfSessionHasPlanQuery(sessionId));
        return Json(hasPlanResult.IsSuccess ? hasPlanResult.Value : false);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableContent(int planId)
    {
        var subtopicsResult = await _mediator.Send(new GetSubTopicsByTeachingPlanQuery(planId));
        var lessonsResult = await _mediator.Send(new GetLessonsByTeachingPlanQuery(planId));
        
        return Json(new {
            subtopics = subtopicsResult.IsSuccess ? subtopicsResult.Value : new List<SubTopicDto>(),
            lessons = lessonsResult.IsSuccess ? lessonsResult.Value : new List<LessonDto>()
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetPlannedItems(int sessionId)
    {
        var plannedItemsResult = await _mediator.Send(new GetPlannedItemsQuery(sessionId));
        return Json(plannedItemsResult.IsSuccess ? plannedItemsResult.Value : new List<PlannedItemDto>());
    }
}
