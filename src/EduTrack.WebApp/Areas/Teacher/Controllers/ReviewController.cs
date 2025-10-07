using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class ReviewController : Controller
{
    private readonly ILogger<ReviewController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ReviewController(
        ILogger<ReviewController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Submissions(int? planId = null, int? groupId = null, int? itemId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        List<SubmissionDto> submissions = new();

        if (itemId.HasValue)
        {
            // Get submissions for a specific schedule item
            var result = await _mediator.Send(new GetSubmissionsByScheduleItemQuery(itemId.Value));
            if (result.IsSuccess)
            {
                submissions = result.Value?.ToList() ?? new List<SubmissionDto>();
            }
        }
        else if (groupId.HasValue)
        {
            // Get submissions for all items in a group
            var scheduleItems = await _mediator.Send(new GetScheduleItemsByGroupQuery(groupId.Value));
            if (scheduleItems.IsSuccess)
            {
                var allSubmissions = new List<SubmissionDto>();
                foreach (var item in scheduleItems.Value ?? new List<ScheduleItemDto>())
                {
                    var itemSubmissions = await _mediator.Send(new GetSubmissionsByScheduleItemQuery(item.Id));
                    if (itemSubmissions.IsSuccess)
                    {
                        allSubmissions.AddRange(itemSubmissions.Value ?? new List<SubmissionDto>());
                    }
                }
                submissions = allSubmissions;
            }
        }
        else if (planId.HasValue)
        {
            // Get submissions for all items in a teaching plan
            var scheduleItems = await _mediator.Send(new GetScheduleItemsByTeachingPlanQuery(planId.Value));
            if (scheduleItems.IsSuccess)
            {
                var allSubmissions = new List<SubmissionDto>();
                foreach (var item in scheduleItems.Value ?? new List<ScheduleItemDto>())
                {
                    var itemSubmissions = await _mediator.Send(new GetSubmissionsByScheduleItemQuery(item.Id));
                    if (itemSubmissions.IsSuccess)
                    {
                        allSubmissions.AddRange(itemSubmissions.Value ?? new List<SubmissionDto>());
                    }
                }
                submissions = allSubmissions;
            }
        }
        else
        {
            // Get all submissions needing review
            var result = await _mediator.Send(new GetSubmissionsNeedingReviewQuery());
            if (result.IsSuccess)
            {
                submissions = result.Value?.ToList() ?? new List<SubmissionDto>();
            }
        }

        // Filter by teacher's teaching plans
        submissions = submissions.Where(s => 
        {
            // This would need to be enhanced to check if the teacher owns the teaching plan
            // For now, we'll show all submissions
            return true;
        }).ToList();

        ViewBag.PlanId = planId;
        ViewBag.GroupId = groupId;
        ViewBag.ItemId = itemId;
        return View(submissions);
    }

    public async Task<IActionResult> Grade(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var submission = await _mediator.Send(new GetSubmissionByIdQuery(id));
        if (!submission.IsSuccess || submission.Value == null)
        {
            return NotFound("Submission not found");
        }

        // Verify user has permission to grade this submission
        // This would need to check if the teacher owns the teaching plan
        // For now, we'll allow any teacher to grade

        ViewBag.Submission = submission.Value;
        ViewBag.MaxScore = submission.Value.ScheduleItemTitle; // This would need to be enhanced
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grade(GradeSubmissionCommand command)
    {
        if (!ModelState.IsValid)
        {
            var submission = await _mediator.Send(new GetSubmissionByIdQuery(command.Id));
            ViewBag.Submission = submission.Value;
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while grading the submission");
            var submission = await _mediator.Send(new GetSubmissionByIdQuery(command.Id));
            ViewBag.Submission = submission.Value;
            return View(command);
        }

        TempData["Success"] = "Submission graded successfully";
        return RedirectToAction(nameof(Submissions));
    }

    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var submission = await _mediator.Send(new GetSubmissionByIdQuery(id));
        if (!submission.IsSuccess || submission.Value == null)
        {
            return NotFound("Submission not found");
        }

        return View(submission.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateSubmissionStatusCommand command)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Submissions));
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "An error occurred while updating the submission status";
        }
        else
        {
            TempData["Success"] = "Submission status updated successfully";
        }

        return RedirectToAction(nameof(Submissions));
    }
}
