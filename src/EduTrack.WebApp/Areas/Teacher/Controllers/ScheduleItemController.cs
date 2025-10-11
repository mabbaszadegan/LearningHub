using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
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

    // GET: ScheduleItem/Index
    public async Task<IActionResult> Index(int teachingPlanId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get teaching plan details for navigation
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("برنامه آموزشی یافت نشد.");
        }

        // Get schedule items for the teaching plan
        var scheduleItems = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemsByTeachingPlanQuery(teachingPlanId));
        if (!scheduleItems.IsSuccess)
        {
            return View(new List<ScheduleItemDto>());
        }

        // Get stats
        var stats = await _mediator.Send(new GetScheduleItemStatsQuery(teachingPlanId));
        
        ViewBag.TeachingPlanId = teachingPlanId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle ?? "دوره";
        ViewBag.CourseId = teachingPlan.Value.CourseId;
        ViewBag.Stats = stats.IsSuccess ? stats.Value : new ScheduleItemStatsDto();
        
        return View(scheduleItems.Value);
    }

    // GET: ScheduleItem/Create
    public async Task<IActionResult> Create(int teachingPlanId)
    {
        // Get teaching plan details for navigation
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("برنامه آموزشی یافت نشد.");
        }

        ViewBag.TeachingPlanId = teachingPlanId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle ?? "دوره";
        ViewBag.CourseId = teachingPlan.Value.CourseId;
        ViewBag.ScheduleItemTypes = GetScheduleItemTypes();
        return View();
    }

    // POST: ScheduleItem/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateScheduleItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TeachingPlanId = request.TeachingPlanId;
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes();
            return View(request);
        }

        var command = new CreateScheduleItemCommand(
            request.TeachingPlanId,
            request.GroupId,
            request.LessonId,
            request.Type,
            request.Title,
            request.Description,
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.DisciplineHint,
            request.ContentJson,
            request.MaxScore
        );

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "خطا در ایجاد آیتم آموزشی");
            ViewBag.TeachingPlanId = request.TeachingPlanId;
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes();
            return View(request);
        }

        return RedirectToAction(nameof(Index), new { teachingPlanId = request.TeachingPlanId });
    }

    // GET: ScheduleItem/Edit
    public async Task<IActionResult> Edit(int id)
    {
        var scheduleItem = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
        if (!scheduleItem.IsSuccess || scheduleItem.Value == null)
        {
            return NotFound("آیتم آموزشی یافت نشد.");
        }

        var request = new UpdateScheduleItemRequest
        {
            Id = scheduleItem.Value.Id,
            Title = scheduleItem.Value.Title,
            Description = scheduleItem.Value.Description,
            StartDate = scheduleItem.Value.StartDate,
            DueDate = scheduleItem.Value.DueDate,
            IsMandatory = scheduleItem.Value.IsMandatory,
            ContentJson = scheduleItem.Value.ContentJson,
            MaxScore = scheduleItem.Value.MaxScore
        };

        ViewBag.ScheduleItemTypes = GetScheduleItemTypes();
        return View(request);
    }

    // POST: ScheduleItem/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateScheduleItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes();
            return View(request);
        }

        var command = new UpdateScheduleItemCommand(
            request.Id,
            request.Title,
            request.Description,
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.ContentJson,
            request.MaxScore
        );

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "خطا در به‌روزرسانی آیتم آموزشی");
            ViewBag.ScheduleItemTypes = GetScheduleItemTypes();
            return View(request);
        }

        // Get the teaching plan ID from the updated item
        var scheduleItem = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(request.Id));
        return RedirectToAction(nameof(Index), new { teachingPlanId = scheduleItem.Value?.TeachingPlanId ?? 0 });
    }

    // POST: ScheduleItem/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var scheduleItem = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
        if (!scheduleItem.IsSuccess || scheduleItem.Value == null)
        {
            return NotFound("آیتم آموزشی یافت نشد.");
        }

        var result = await _mediator.Send(new DeleteScheduleItemCommand(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "خطا در حذف آیتم آموزشی";
        }

        return RedirectToAction(nameof(Index), new { teachingPlanId = scheduleItem.Value.TeachingPlanId });
    }

    // API Endpoints for AJAX calls
    [HttpGet]
    public async Task<IActionResult> GetScheduleItems(int teachingPlanId)
    {
        var result = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemsByTeachingPlanQuery(teachingPlanId));
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value }, jsonSettings);
        }

        return Json(new { success = false, message = result.Error }, jsonSettings);
    }

    [HttpGet]
    public async Task<IActionResult> GetScheduleItem(int id)
    {
        var result = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value }, jsonSettings);
        }

        return Json(new { success = false, message = result.Error }, jsonSettings);
    }

    [HttpPost]
    public async Task<IActionResult> CreateScheduleItem([FromBody] CreateScheduleItemRequest request)
    {
        var command = new CreateScheduleItemCommand(
            request.TeachingPlanId,
            request.GroupId,
            request.LessonId,
            request.Type,
            request.Title,
            request.Description,
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.DisciplineHint,
            request.ContentJson,
            request.MaxScore
        );

        var result = await _mediator.Send(command);
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (result.IsSuccess)
        {
            return Json(new { success = true, id = result.Value }, jsonSettings);
        }

        return Json(new { success = false, message = result.Error }, jsonSettings);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateScheduleItem([FromBody] UpdateScheduleItemRequest request)
    {
        var command = new UpdateScheduleItemCommand(
            request.Id,
            request.Title,
            request.Description,
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.ContentJson,
            request.MaxScore
        );

        var result = await _mediator.Send(command);
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (result.IsSuccess)
        {
            return Json(new { success = true }, jsonSettings);
        }

        return Json(new { success = false, message = result.Error }, jsonSettings);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteScheduleItem(int id)
    {
        var result = await _mediator.Send(new DeleteScheduleItemCommand(id));
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (result.IsSuccess)
        {
            return Json(new { success = true }, jsonSettings);
        }

        return Json(new { success = false, message = result.Error }, jsonSettings);
    }

    [HttpGet]
    public async Task<IActionResult> GetStats(int teachingPlanId)
    {
        var result = await _mediator.Send(new GetScheduleItemStatsQuery(teachingPlanId));
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value }, jsonSettings);
        }

        return Json(new { success = false, message = result.Error }, jsonSettings);
    }

    // POST: ScheduleItem/SaveStep
    [HttpPost]
    public async Task<IActionResult> SaveStep([FromBody] SaveScheduleItemStepRequest request)
    {
        try
        {
            _logger.LogInformation("SaveStep called with request: {@Request}", request);
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found in SaveStep");
                return Json(new { success = false, message = "کاربر یافت نشد." });
            }

        var command = new SaveScheduleItemStepCommand(
            request.Id,
            request.TeachingPlanId,
            request.Step,
            request.Type,
            request.Title,
            request.Description,
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.ContentJson,
            request.MaxScore,
            request.GroupId,
            request.LessonId
        );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("SaveStep successful, ID: {Id}", result.Value);
                return Json(new { success = true, id = result.Value, message = "مرحله با موفقیت ذخیره شد." });
            }
            else
            {
                _logger.LogError("SaveStep failed: {Error}", result.Error);
                return Json(new { success = false, message = result.Error });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in SaveStep");
            return Json(new { success = false, message = $"خطا در ذخیره مرحله: {ex.Message}" });
        }
    }

    // POST: ScheduleItem/Complete
    [HttpPost]
    public async Task<IActionResult> Complete([FromBody] CompleteScheduleItemRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "کاربر یافت نشد." });
        }

        var command = new CompleteScheduleItemCommand(request.Id);
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "آیتم آموزشی با موفقیت تکمیل شد." });
        }
        else
        {
            return Json(new { success = false, message = result.Error });
        }
    }

    private static List<object> GetScheduleItemTypes()
    {
        return new List<object>
        {
            new { Value = (int)ScheduleItemType.Reminder, Text = "یادآوری" },
            new { Value = (int)ScheduleItemType.Writing, Text = "نوشتاری" },
            new { Value = (int)ScheduleItemType.Audio, Text = "صوتی" },
            new { Value = (int)ScheduleItemType.GapFill, Text = "پر کردن جای خالی" },
            new { Value = (int)ScheduleItemType.MultipleChoice, Text = "چند گزینه‌ای" },
            new { Value = (int)ScheduleItemType.Match, Text = "تطبیق" },
            new { Value = (int)ScheduleItemType.ErrorFinding, Text = "پیدا کردن خطا" },
            new { Value = (int)ScheduleItemType.CodeExercise, Text = "تمرین کد" },
            new { Value = (int)ScheduleItemType.Quiz, Text = "کویز" }
        };
    }
}
