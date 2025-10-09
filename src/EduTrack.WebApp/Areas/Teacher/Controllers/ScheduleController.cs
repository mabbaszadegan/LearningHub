using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.Courses;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class ScheduleController : Controller
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ScheduleController(
        ILogger<ScheduleController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int planId, int? groupId = null)
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

        var scheduleItems = await _mediator.Send(new GetScheduleItemsByTeachingPlanQuery(planId));
        if (!scheduleItems.IsSuccess)
        {
            return View(new List<ScheduleItemDto>());
        }

        // Filter by group if specified
        var items = scheduleItems.Value;
        if (groupId.HasValue)
        {
            items = items?.Where(si => si.GroupId == groupId.Value).ToList() ?? new List<ScheduleItemDto>();
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        ViewBag.SelectedGroupId = groupId;
        
        // Get groups for filter dropdown
        var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(planId));
        ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();

        return View(items);
    }

    public async Task<IActionResult> Create(int planId, int? groupId = null)
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
            return Forbid("You don't have permission to create schedule items for this teaching plan");
        }

        // Get course to determine discipline type for filtering exercise types
        var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value.CourseId));
        var disciplineType = course.IsSuccess ? course.Value?.DisciplineType : null;

        // Get groups for assignment dropdown
        var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(planId));
        
        // Get lessons for linking
        var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery(teachingPlan.Value.CourseId));

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        ViewBag.SelectedGroupId = groupId;
        ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();
        ViewBag.Lessons = lessons.IsSuccess ? lessons.Value : new List<LessonDto>();
        ViewBag.DisciplineType = disciplineType;
        ViewBag.ScheduleItemTypes = GetScheduleItemTypes(disciplineType);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateScheduleItemCommand command)
    {
        if (!ModelState.IsValid)
        {
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(command.TeachingPlanId));
            var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value?.CourseId ?? 0));
            var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(command.TeachingPlanId));
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery(teachingPlan.Value?.CourseId ?? 0));

            ViewBag.TeachingPlanId = command.TeachingPlanId;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();
            ViewBag.Lessons = lessons.IsSuccess ? lessons.Value : new List<LessonDto>();
            ViewBag.DisciplineType = course.Value?.DisciplineType;
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes(course.Value?.DisciplineType);
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while creating the schedule item");
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(command.TeachingPlanId));
            var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value?.CourseId ?? 0));
            var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(command.TeachingPlanId));
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery(teachingPlan.Value?.CourseId ?? 0));

            ViewBag.TeachingPlanId = command.TeachingPlanId;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();
            ViewBag.Lessons = lessons.IsSuccess ? lessons.Value : new List<LessonDto>();
            ViewBag.DisciplineType = course.Value?.DisciplineType;
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes(course.Value?.DisciplineType);
            return View(command);
        }

        return RedirectToAction(nameof(Index), new { planId = command.TeachingPlanId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var scheduleItem = await _mediator.Send(new GetScheduleItemByIdQuery(id));
        if (!scheduleItem.IsSuccess || scheduleItem.Value == null)
        {
            return NotFound("Schedule item not found");
        }

        // Verify user has access to this schedule item's teaching plan
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.Value.TeachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null || teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to edit this schedule item");
        }

        var command = new UpdateScheduleItemCommand(
            scheduleItem.Value.Id,
            scheduleItem.Value.Title,
            scheduleItem.Value.Description,
            scheduleItem.Value.StartDate,
            scheduleItem.Value.DueDate,
            scheduleItem.Value.IsMandatory,
            scheduleItem.Value.ContentJson,
            scheduleItem.Value.MaxScore,
            scheduleItem.Value.GroupId,
            scheduleItem.Value.LessonId);

        // Get course to determine discipline type
        var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value.CourseId));
        var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(scheduleItem.Value.TeachingPlanId));
        var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery(teachingPlan.Value.CourseId));

        ViewBag.TeachingPlanId = scheduleItem.Value.TeachingPlanId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();
        ViewBag.Lessons = lessons.IsSuccess ? lessons.Value : new List<LessonDto>();
        ViewBag.DisciplineType = course.Value?.DisciplineType;
        ViewBag.ScheduleItemTypes = GetScheduleItemTypes(course.Value?.DisciplineType);
        ViewBag.CurrentType = scheduleItem.Value.Type;

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateScheduleItemCommand command)
    {
        if (!ModelState.IsValid)
        {
            var scheduleItem = await _mediator.Send(new GetScheduleItemByIdQuery(command.Id));
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.Value?.TeachingPlanId ?? 0));
            var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value?.CourseId ?? 0));
            var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(scheduleItem.Value?.TeachingPlanId ?? 0));
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery(teachingPlan.Value?.CourseId ?? 0));

            ViewBag.TeachingPlanId = scheduleItem.Value?.TeachingPlanId ?? 0;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();
            ViewBag.Lessons = lessons.IsSuccess ? lessons.Value : new List<LessonDto>();
            ViewBag.DisciplineType = course.Value?.DisciplineType;
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes(course.Value?.DisciplineType);
            ViewBag.CurrentType = scheduleItem.Value?.Type;
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while updating the schedule item");
            var scheduleItem = await _mediator.Send(new GetScheduleItemByIdQuery(command.Id));
            var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.Value?.TeachingPlanId ?? 0));
            var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value?.CourseId ?? 0));
            var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(scheduleItem.Value?.TeachingPlanId ?? 0));
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery(teachingPlan.Value?.CourseId ?? 0));

            ViewBag.TeachingPlanId = scheduleItem.Value?.TeachingPlanId ?? 0;
            ViewBag.TeachingPlanTitle = teachingPlan.Value?.Title ?? "Unknown Plan";
            ViewBag.CourseTitle = teachingPlan.Value?.CourseTitle ?? "Unknown Course";
            ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();
            ViewBag.Lessons = lessons.IsSuccess ? lessons.Value : new List<LessonDto>();
            ViewBag.DisciplineType = course.Value?.DisciplineType;
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes(course.Value?.DisciplineType);
            ViewBag.CurrentType = scheduleItem.Value?.Type;
            return View(command);
        }

        return RedirectToAction(nameof(Index), new { planId = result.Value?.TeachingPlanId ?? 0 });
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

        var scheduleItem = await _mediator.Send(new GetScheduleItemByIdQuery(id));
        if (!scheduleItem.IsSuccess || scheduleItem.Value == null)
        {
            return NotFound("Schedule item not found");
        }

        // Verify user has access to this schedule item's teaching plan
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.Value.TeachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null || teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to delete this schedule item");
        }

        var result = await _mediator.Send(new DeleteScheduleItemCommand(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "An error occurred while deleting the schedule item";
        }
        else
        {
            TempData["Success"] = "Schedule item deleted successfully";
        }

        return RedirectToAction(nameof(Index), new { planId = scheduleItem.Value.TeachingPlanId });
    }

    private static List<ScheduleItemType> GetScheduleItemTypes(DisciplineType? disciplineType)
    {
        var allTypes = Enum.GetValues<ScheduleItemType>().ToList();
        
        if (!disciplineType.HasValue)
        {
            return allTypes;
        }

        // Filter by discipline type
        return disciplineType.Value switch
        {
            DisciplineType.Language => new List<ScheduleItemType>
            {
                ScheduleItemType.Reminder,
                ScheduleItemType.Writing,
                ScheduleItemType.Audio,
                ScheduleItemType.GapFill,
                ScheduleItemType.MultipleChoice,
                ScheduleItemType.Match,
                ScheduleItemType.ErrorFinding,
                ScheduleItemType.Quiz
            },
            DisciplineType.Math => new List<ScheduleItemType>
            {
                ScheduleItemType.Reminder,
                ScheduleItemType.MultipleChoice,
                ScheduleItemType.GapFill,
                ScheduleItemType.Quiz
            },
            DisciplineType.Programming => new List<ScheduleItemType>
            {
                ScheduleItemType.Reminder,
                ScheduleItemType.CodeExercise,
                ScheduleItemType.MultipleChoice,
                ScheduleItemType.Quiz
            },
            DisciplineType.Science => new List<ScheduleItemType>
            {
                ScheduleItemType.Reminder,
                ScheduleItemType.MultipleChoice,
                ScheduleItemType.Match,
                ScheduleItemType.Quiz
            },
            _ => allTypes
        };
    }
}
