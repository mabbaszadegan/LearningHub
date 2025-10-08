using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class StudentGroupController : Controller
{
    private readonly ILogger<StudentGroupController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public StudentGroupController(
        ILogger<StudentGroupController> logger,
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

        // Verify teaching plan exists and user has access
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(planId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("Teaching plan not found");
        }

        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to access this teaching plan");
        }

        var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(planId));
        if (!groups.IsSuccess)
        {
            return View(new List<StudentGroupDto>());
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        return View(groups.Value);
    }

    public async Task<IActionResult> Create(int planId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify teaching plan exists and user has access
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(planId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("Teaching plan not found");
        }

        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to create groups for this teaching plan");
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentGroupCommand command)
    {
        if (!ModelState.IsValid)
        {
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(command.TeachingPlanId));
            ViewBag.TeachingPlanId = command.TeachingPlanId;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while creating the group");
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(command.TeachingPlanId));
            ViewBag.TeachingPlanId = command.TeachingPlanId;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            return View(command);
        }

        return RedirectToAction(nameof(Index), new { planId = command.TeachingPlanId });
    }

    public async Task<IActionResult> ManageMembers(int groupId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var group = await _mediator.Send(new GetStudentGroupWithMembersQuery(groupId));
        if (!group.IsSuccess || group.Value == null)
        {
            return NotFound("Group not found");
        }

        // Verify user has access to this group's teaching plan
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(group.Value.TeachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null || teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to manage this group");
        }

        // Get all students enrolled in the course
        var courseStudents = await _mediator.Send(new GetCourseStudentsQuery(teachingPlan.Value.CourseId));
        var availableStudents = courseStudents.IsSuccess ? courseStudents.Value : new List<UserDto>();

        ViewBag.GroupId = groupId;
        ViewBag.GroupName = group.Value.Name;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        ViewBag.AvailableStudents = availableStudents;
        return View(group.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMembers(AddGroupMembersCommand command)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(ManageMembers), new { groupId = command.GroupId });
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "An error occurred while adding members to the group";
        }
        else
        {
            TempData["Success"] = "Members added successfully";
        }

        return RedirectToAction(nameof(ManageMembers), new { groupId = command.GroupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(RemoveGroupMemberCommand command)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(ManageMembers), new { groupId = command.GroupId });
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "An error occurred while removing the member from the group";
        }
        else
        {
            TempData["Success"] = "Member removed successfully";
        }

        return RedirectToAction(nameof(ManageMembers), new { groupId = command.GroupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var group = await _mediator.Send(new GetStudentGroupByIdQuery(id));
        if (!group.IsSuccess || group.Value == null)
        {
            return NotFound("Group not found");
        }

        // Verify user has access to this group's teaching plan
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(group.Value.TeachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null || teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to delete this group");
        }

        var result = await _mediator.Send(new DeleteStudentGroupCommand(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "An error occurred while deleting the group";
        }
        else
        {
            TempData["Success"] = "Group deleted successfully";
        }

        return RedirectToAction(nameof(Index), new { planId = group.Value.TeachingPlanId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var group = await _mediator.Send(new GetStudentGroupByIdQuery(id));
        if (!group.IsSuccess || group.Value == null)
        {
            return NotFound("Group not found");
        }

        // Verify user has access to this group's teaching plan
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(group.Value.TeachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null || teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to edit this group");
        }

        var command = new UpdateStudentGroupCommand(
            group.Value.Id,
            group.Value.Name);

        ViewBag.TeachingPlanId = group.Value.TeachingPlanId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateStudentGroupCommand command)
    {
        if (!ModelState.IsValid)
        {
            var group = await _mediator.Send(new GetStudentGroupByIdQuery(command.Id));
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(group.Value?.TeachingPlanId ?? 0));
            ViewBag.TeachingPlanId = group.Value?.TeachingPlanId ?? 0;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while updating the group");
            var group = await _mediator.Send(new GetStudentGroupByIdQuery(command.Id));
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(group.Value?.TeachingPlanId ?? 0));
            ViewBag.TeachingPlanId = group.Value?.TeachingPlanId ?? 0;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            return View(command);
        }

        return RedirectToAction(nameof(Index), new { planId = result.Value?.TeachingPlanId ?? 0 });
    }
}
