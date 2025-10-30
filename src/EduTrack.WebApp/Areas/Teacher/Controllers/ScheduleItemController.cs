using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Extensions;
using EduTrack.WebApp.Extensions;
using EduTrack.WebApp.Areas.Teacher.Views.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class ScheduleItemController : BaseTeacherController
{
    private readonly ILogger<ScheduleItemController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ScheduleItemController(
        ILogger<ScheduleItemController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IPageTitleSectionService pageTitleSectionService) : base(pageTitleSectionService)
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

        // Setup page title section
        await SetPageTitleSectionAsync(PageType.ScheduleItemsIndex, teachingPlanId);

        return View(scheduleItems.Value);
    }

    // GET: ScheduleItem/CreateOrEdit
    public async Task<IActionResult> CreateOrEdit(int teachingPlanId, int id = 0)
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
        ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
            .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
            .ToList();

        // Check if we're in edit mode
        bool isEditMode = id > 0;
        ViewBag.IsEditMode = isEditMode;
        ViewBag.ScheduleItemId = id;

        // Setup page title section
        if (isEditMode)
        {
            await SetPageTitleSectionAsync(PageType.ScheduleItemEdit, (teachingPlanId, id));
        }
        else
        {
            await SetPageTitleSectionAsync(PageType.ScheduleItemCreate, teachingPlanId);
        }

        CreateScheduleItemRequest model;

        if (isEditMode)
        {
            // Load existing item for editing
            var scheduleItem = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
            if (!scheduleItem.IsSuccess || scheduleItem.Value == null)
            {
                return NotFound("آیتم آموزشی یافت نشد.");
            }

            model = new CreateScheduleItemRequest
            {
                TeachingPlanId = teachingPlanId,
                Title = scheduleItem.Value.Title,
                Description = scheduleItem.Value.Description,
                StartDate = scheduleItem.Value.StartDate,
                DueDate = scheduleItem.Value.DueDate,
                IsMandatory = scheduleItem.Value.IsMandatory,
                ContentJson = scheduleItem.Value.ContentJson,
                MaxScore = scheduleItem.Value.MaxScore,
                Type = scheduleItem.Value.Type,
                DisciplineHint = scheduleItem.Value.DisciplineHint
            };
        }
        else
        {
            // Create a new model instance for the view
            model = new CreateScheduleItemRequest
            {
                TeachingPlanId = teachingPlanId,
                ContentJson = string.Empty
            };
        }

        return View(model);
    }

    // POST: ScheduleItem/CreateOrEdit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit(CreateScheduleItemRequest request, int id = 0)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TeachingPlanId = request.TeachingPlanId;
            ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
                .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
                .ToList();
            return View(id > 0 ? "Edit" : "CreateOrEdit", request);
        }

        if (id > 0)
        {
            // Update existing item
            var updateRequest = new UpdateScheduleItemRequest
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                IsMandatory = request.IsMandatory,
                ContentJson = request.ContentJson,
                MaxScore = request.MaxScore
            };

            var updateCommand = new UpdateScheduleItemCommand(
                updateRequest.Id,
                updateRequest.Title,
                updateRequest.Description,
                updateRequest.StartDate,
                updateRequest.DueDate,
                updateRequest.IsMandatory,
                updateRequest.ContentJson,
                updateRequest.MaxScore
            );

            var result = await _mediator.Send(updateCommand);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error ?? "خطا در به‌روزرسانی آیتم آموزشی");
                ViewBag.TeachingPlanId = request.TeachingPlanId;
                ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
                .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
                .ToList();
                return View("Edit", updateRequest);
            }

            return RedirectToAction(nameof(Index), new { teachingPlanId = request.TeachingPlanId });
        }
        else
        {
            // Create new item
            var command = new CreateScheduleItemCommand(
                request.TeachingPlanId,
                request.GroupId,
                request.Type,
                request.Title,
                request.Description,
                request.StartDate,
                request.DueDate,
                request.IsMandatory,
                request.DisciplineHint,
                request.ContentJson,
                request.MaxScore,
                request.GroupIds,
                request.SubChapterIds,
                request.StudentIds
            );

            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error ?? "خطا در ایجاد آیتم آموزشی");
                ViewBag.TeachingPlanId = request.TeachingPlanId;
                ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
                .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
                .ToList();
                return View(request);
            }

            return RedirectToAction(nameof(Index), new { teachingPlanId = request.TeachingPlanId });
        }
    }

    // GET: ScheduleItem/Edit
    public async Task<IActionResult> Edit(int id)
    {
        var scheduleItem = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
        if (!scheduleItem.IsSuccess || scheduleItem.Value == null)
        {
            return NotFound("آیتم آموزشی یافت نشد.");
        }
        // Redirect all edit traffic to the unified CreateOrEdit view to ensure consistent UI (form-navigation, steps, etc.)
        return RedirectToAction(nameof(CreateOrEdit), new { teachingPlanId = scheduleItem.Value.TeachingPlanId, id });
    }

    // POST: ScheduleItem/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateScheduleItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
                .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
                .ToList();
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
            ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
                .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
                .ToList();
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

        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, message = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetScheduleItem(int id)
    {
        var result = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));

        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> CreateScheduleItem([FromBody] CreateScheduleItemRequest request)
    {
        if (request == null)
        {
            return Json(new { success = false, message = "درخواست نامعتبر است" });
        }

        // Log the request for debugging
        Console.WriteLine($"CreateScheduleItem request: TeachingPlanId={request.TeachingPlanId}, Title={request.Title}, Type={request.Type}");

        var command = new CreateScheduleItemCommand(
            request.TeachingPlanId,
            request.GroupId ?? null,
            request.Type,
            request.Title ?? "",
            request.Description ?? "",
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.DisciplineHint ?? null,
            request.ContentJson ?? "{}",
            request.MaxScore,
            request.GroupIds ?? new List<int>(),
            request.SubChapterIds ?? new List<int>()
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Json(new { success = true, id = result.Value });
        }

        return Json(new { success = false, message = result.Error });
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

        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteScheduleItem(int id)
    {
        var result = await _mediator.Send(new DeleteScheduleItemCommand(id));

        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, message = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetStats(int teachingPlanId)
    {
        var result = await _mediator.Send(new GetScheduleItemStatsQuery(teachingPlanId));

        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, message = result.Error });
    }

    // POST: ScheduleItem/SaveStep
    [HttpPost]
    public async Task<IActionResult> SaveStep([FromBody] SaveScheduleItemStepRequest request)
    {
        try
        {
            _logger.LogInformation("SaveStep called with request: {@Request}", request);
            Console.WriteLine($"SaveStep called - Step: {request.Step}, TeachingPlanId: {request.TeachingPlanId}");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found in SaveStep");
                return Json(new { success = false, message = "کاربر یافت نشد." });
            }

            // Parse comma-separated IDs if they come as strings
            List<int>? groupIds = request.GroupIds;
            List<int>? subChapterIds = request.SubChapterIds;
            List<string>? studentIds = request.StudentIds;

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
                request.PersianStartDate,
                request.PersianDueDate,
                request.StartTime,
                request.DueTime,
                groupIds,
                subChapterIds,
                studentIds
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

    // GET: ScheduleItem/GetById/{id}
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            _logger.LogInformation("GetById called with ID: {Id}", id);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found in GetById");
                return Json(new { success = false, message = "کاربر یافت نشد." });
            }

            var query = new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                _logger.LogInformation("GetById successful for ID: {Id}", id);
                return Json(new { success = true, data = result.Value });
            }
            else
            {
                _logger.LogError("GetById failed: {Error}", result.Error);
                return Json(new { success = false, message = result.Error });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetById");
            return Json(new { success = false, message = $"خطا در دریافت آیتم: {ex.Message}" });
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


    private static List<int> ParseCommaSeparatedIds(string? commaSeparatedIds)
    {
        if (string.IsNullOrWhiteSpace(commaSeparatedIds))
            return new List<int>();

        return commaSeparatedIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id.Trim(), out var parsedId) ? parsedId : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }

    // POST: ScheduleItem/GetBlockTypeOptions
    [HttpPost]
    public IActionResult GetBlockTypeOptions(string itemType, bool showRegularBlocks, bool showQuestionBlocks)
    {
        try
        {
            ViewData["ItemType"] = itemType;
            ViewData["ShowRegularBlocks"] = showRegularBlocks;
            ViewData["ShowQuestionBlocks"] = showQuestionBlocks;
            
            return PartialView("_BlockTypeOptions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading block type options for item type: {ItemType}", itemType);
            return Content("<div class='alert alert-danger'>خطا در بارگذاری گزینه‌های بلاک</div>");
        }
    }
}
