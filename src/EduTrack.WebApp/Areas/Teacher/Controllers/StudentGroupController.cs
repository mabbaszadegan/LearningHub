using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Application.Common.Models.Users;
using EduTrack.Application.Common.Models.TeachingPlans;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class StudentGroupController : BaseTeacherController
{
    private readonly ILogger<StudentGroupController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public StudentGroupController(
        ILogger<StudentGroupController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IPageTitleSectionService pageTitleSectionService) : base(pageTitleSectionService)
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
        ViewBag.CourseId = teachingPlan.Value.CourseId;
        
        // Setup page title section
        await SetPageTitleSectionAsync(PageType.StudentGroupsIndex, planId);
        
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
        ViewBag.CourseId = teachingPlan.Value.CourseId;
        
        // Setup page title section
        await SetPageTitleSectionAsync(PageType.StudentGroupCreate, planId);
        
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

        // Get available students (enrolled but not in any group of this teaching plan)
        var availableStudentsQuery = await _mediator.Send(new GetAvailableStudentsForTeachingPlanQuery(teachingPlan.Value.Id));
        var availableStudents = availableStudentsQuery.IsSuccess ? availableStudentsQuery.Value : new List<UserDto>();

        ViewBag.GroupId = groupId;
        ViewBag.GroupName = group.Value.Name;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        ViewBag.CourseId = teachingPlan.Value.CourseId;
        ViewBag.AvailableStudents = availableStudents;
        
        // Setup page title section
        await SetPageTitleSectionAsync(PageType.StudentGroupManageMembers, groupId);
        
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
    public async Task<IActionResult> RemoveMemberAjax(RemoveGroupMemberCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "کاربر احراز هویت نشده" });
        }

        // Verify the group exists and user has permission
        var group = await _mediator.Send(new GetStudentGroupByIdQuery(command.GroupId));
        if (!group.IsSuccess || group.Value == null)
        {
            return Json(new { success = false, message = "گروه یافت نشد" });
        }

        // Get the teaching plan to verify teacher permissions
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(group.Value.TeachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return Json(new { success = false, message = "پلن آموزشی یافت نشد" });
        }

        // Verify the current user is the teacher of this teaching plan
        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Json(new { success = false, message = "شما اجازه حذف دانش‌آموز از این گروه را ندارید" });
        }

        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "دانش‌آموز با موفقیت حذف شد" });
        }
        
        return Json(new { success = false, message = result.Error ?? "خطا در حذف دانش‌آموز" });
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
        ViewBag.CourseId = teachingPlan.Value.CourseId;
        
        // Setup page title section
        await SetPageTitleSectionAsync(PageType.StudentGroupEdit, id);
        
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TransferMember(TransferGroupMemberCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "کاربر احراز هویت نشده" });
        }

        // Verify both groups exist and user has permission
        var fromGroup = await _mediator.Send(new GetStudentGroupByIdQuery(command.FromGroupId));
        if (!fromGroup.IsSuccess || fromGroup.Value == null)
        {
            return Json(new { success = false, message = "گروه مبدأ یافت نشد" });
        }

        var toGroup = await _mediator.Send(new GetStudentGroupByIdQuery(command.ToGroupId));
        if (!toGroup.IsSuccess || toGroup.Value == null)
        {
            return Json(new { success = false, message = "گروه مقصد یافت نشد" });
        }

        // Verify both groups belong to the same teaching plan
        if (fromGroup.Value.TeachingPlanId != toGroup.Value.TeachingPlanId)
        {
            return Json(new { success = false, message = "گروه‌ها باید متعلق به یک پلن آموزشی باشند" });
        }

        // Get the teaching plan to verify teacher permissions
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(fromGroup.Value.TeachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return Json(new { success = false, message = "پلن آموزشی یافت نشد" });
        }

        // Verify the current user is the teacher of this teaching plan
        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Json(new { success = false, message = "شما اجازه انتقال دانش‌آموز بین این گروه‌ها را ندارید" });
        }

        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "دانش‌آموز با موفقیت منتقل شد" });
        }
        
        return Json(new { success = false, message = result.Error ?? "خطا در انتقال دانش‌آموز" });
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents(int groupId)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { error = "User not found" });
            }

            // Get group with members
            var groupWithMembers = await _mediator.Send(new GetStudentGroupWithMembersQuery(groupId));
            if (!groupWithMembers.IsSuccess)
            {
                return Json(new { error = "Failed to load group members" });
            }

            var students = groupWithMembers.Value?.Members?.Select(member => new
            {
                id = member.StudentId, // This is already a string
                firstName = member.StudentName.Split(' ').FirstOrDefault() ?? "",
                lastName = member.StudentName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                studentId = member.StudentId,
                groupId = groupId,
                groupName = groupWithMembers.Value?.Name ?? ""
            }).Cast<object>().ToList() ?? new List<object>();

            return Json(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students for group {GroupId}", groupId);
            return Json(new { error = "An error occurred while loading students" });
        }
    }
}
