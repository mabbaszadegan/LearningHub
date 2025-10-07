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
            TempData["ErrorMessage"] = teachingPlanResult.Error ?? "Teaching Plan not found.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        var sessionReportsResult = await _mediator.Send(new ListTeachingSessionReportsQuery(planId));
        if (!sessionReportsResult.IsSuccess)
        {
            TempData["ErrorMessage"] = sessionReportsResult.Error ?? "Error loading session reports.";
            return View(new List<TeachingSessionReportDto>());
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlanResult.Value.Title;
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
            TempData["ErrorMessage"] = teachingPlanResult.Error ?? "Teaching Plan not found.";
            return RedirectToAction("Index", "TeachingPlan");
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlanResult.Value.Title;
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
            TempData["SuccessMessage"] = "Session report created successfully.";
            return RedirectToAction(nameof(Index), new { planId = command.TeachingPlanId });
        }

        TempData["ErrorMessage"] = result.Error ?? "Error creating session report.";
        var teachingPlanResultError = await _mediator.Send(new GetTeachingPlanByIdQuery(command.TeachingPlanId));
        if (teachingPlanResultError.IsSuccess && teachingPlanResultError.Value != null)
        {
            ViewBag.TeachingPlanId = command.TeachingPlanId;
            ViewBag.TeachingPlanTitle = teachingPlanResultError.Value.Title;
            ViewBag.SessionModes = new SelectList(Enum.GetValues(typeof(SessionMode)), command.Mode);
        }
        return View(command);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sessionReportResult = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(id));
        if (!sessionReportResult.IsSuccess || sessionReportResult.Value == null)
        {
            TempData["ErrorMessage"] = sessionReportResult.Error ?? "Session report not found.";
            return RedirectToAction("Index", "TeachingPlan");
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
}
