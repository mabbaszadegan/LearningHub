using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Extensions;
using EduTrack.WebApp.Areas.Teacher.Views.Shared;
using EduTrack.WebApp.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using TeachingPlanDto = EduTrack.Application.Common.Models.TeachingPlans.TeachingPlanDto;

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

        var items = scheduleItems.Value?
            .Where(item => !item.SessionReportId.HasValue)
            .ToList() ?? new List<ScheduleItemDto>();

        // Get stats
        var stats = await _mediator.Send(new GetScheduleItemStatsQuery(teachingPlanId));

        ViewBag.TeachingPlanId = teachingPlanId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle ?? "دوره";
        ViewBag.CourseId = teachingPlan.Value.CourseId;
        ViewBag.Stats = stats.IsSuccess ? stats.Value : new ScheduleItemStatsDto();
        ViewBag.IsCourseScope = false;
        ViewBag.IsSessionScope = false;

        // Setup page title section
        await SetPageTitleSectionAsync(PageType.ScheduleItemsIndex, teachingPlanId);

        return View(items);
    }

    // GET: ScheduleItem/CourseLessons
    public async Task<IActionResult> CourseLessons(int courseId)
    {
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!courseResult.IsSuccess || courseResult.Value == null)
        {
            return NotFound("دوره آموزشی یافت نشد.");
        }

        var scheduleItems = await _mediator.Send(new GetScheduleItemsByCourseQuery(courseId, true));
        var items = scheduleItems.IsSuccess && scheduleItems.Value != null
            ? scheduleItems.Value
            : new List<ScheduleItemDto>();

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = courseResult.Value.Title;
        ViewBag.IsCourseScope = true;
        ViewBag.IsSessionScope = false;
        ViewBag.TeachingPlanId = (int?)null;
        ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
            .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
            .ToList();

        ViewBag.Stats = new ScheduleItemStatsDto
        {
            TotalItems = items.Count,
            ActiveItems = items.Count(i => i.Status == ScheduleItemStatus.Active),
            CompletedItems = items.Count(i => i.Status == ScheduleItemStatus.Completed),
            OverdueItems = items.Count(i => i.Status == ScheduleItemStatus.Expired)
        };

        await SetPageTitleSectionAsync(PageType.CourseDetails, courseId);

        return View("Index", items);
    }

    // GET: ScheduleItem/CreateOrEdit
    public async Task<IActionResult> CreateOrEdit(int? teachingPlanId = null, int? courseId = null, int? sessionReportId = null, int id = 0)
    {
        if (!teachingPlanId.HasValue && !courseId.HasValue && !sessionReportId.HasValue)
        {
            return BadRequest("کانتکست نامعتبر است.");
        }

        TeachingPlanDto? teachingPlan = null;
        CourseDto? course = null;
        TeachingSessionReportDto? sessionReport = null;

        if (sessionReportId.HasValue)
        {
            var sessionReportResult = await _mediator.Send(new GetTeachingSessionReportDetailsQuery(sessionReportId.Value));
            if (!sessionReportResult.IsSuccess || sessionReportResult.Value == null)
            {
                return NotFound("گزارش جلسه آموزشی یافت نشد.");
            }

            sessionReport = sessionReportResult.Value;
            teachingPlanId ??= sessionReport.TeachingPlanId;
        }

        if (teachingPlanId.HasValue)
        {
            var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId.Value));
            if (!teachingPlanResult.IsSuccess || teachingPlanResult.Value == null)
            {
                return NotFound("برنامه آموزشی یافت نشد.");
            }

            teachingPlan = teachingPlanResult.Value;
            courseId ??= teachingPlan.CourseId;
        }

        if (courseId.HasValue)
        {
            var courseResult = await _mediator.Send(new GetCourseByIdQuery(courseId.Value));
            if (!courseResult.IsSuccess || courseResult.Value == null)
            {
                return NotFound("دوره آموزشی یافت نشد.");
            }

            course = courseResult.Value;
        }

        if (!courseId.HasValue)
        {
            return BadRequest("دوره مرتبط یافت نشد.");
        }

        var isCourseScope = !teachingPlanId.HasValue && !sessionReportId.HasValue;
        var isSessionScope = sessionReportId.HasValue;

        ViewBag.TeachingPlanId = teachingPlanId;
        ViewBag.TeachingPlanTitle = teachingPlan?.Title ?? (isCourseScope ? course?.Title ?? "دوره" : "برنامه آموزشی");
        ViewBag.CourseTitle = course?.Title ?? teachingPlan?.CourseTitle ?? "دوره";
        ViewBag.CourseId = courseId;
        ViewBag.SessionReportId = sessionReportId;
        ViewBag.IsCourseScope = isCourseScope;
        ViewBag.IsSessionScope = isSessionScope;
        ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
            .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
            .ToList();

        // Check if we're in edit mode
        bool isEditMode = id > 0;
        ViewBag.IsEditMode = isEditMode;
        ViewBag.ScheduleItemId = id;

        // Setup page title section
        if (isEditMode && teachingPlanId.HasValue)
        {
            await SetPageTitleSectionAsync(PageType.ScheduleItemEdit, (teachingPlanId.Value, id));
        }
        else if (teachingPlanId.HasValue)
        {
            await SetPageTitleSectionAsync(PageType.ScheduleItemCreate, teachingPlanId.Value);
        }
        else
        {
            await SetPageTitleSectionAsync(PageType.CourseDetails, courseId.Value);
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
                TeachingPlanId = scheduleItem.Value.TeachingPlanId,
                CourseId = scheduleItem.Value.CourseId ?? courseId,
                SessionReportId = scheduleItem.Value.SessionReportId,
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
                CourseId = courseId,
                SessionReportId = sessionReportId,
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
            ViewBag.CourseId = request.CourseId;
            ViewBag.SessionReportId = request.SessionReportId;
            ViewBag.IsCourseScope = !request.TeachingPlanId.HasValue && request.CourseId.HasValue;
            ViewBag.IsSessionScope = request.SessionReportId.HasValue;
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
                CourseId = request.CourseId,
                TeachingPlanId = request.TeachingPlanId,
                SessionReportId = request.SessionReportId,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                IsMandatory = request.IsMandatory,
                ContentJson = request.ContentJson,
                MaxScore = request.MaxScore,
                GroupIds = request.GroupIds,
                SubChapterIds = request.SubChapterIds,
                StudentProfileIds = request.StudentProfileIds
            };

            var updateCommand = new UpdateScheduleItemCommand(
                updateRequest.Id,
                updateRequest.CourseId,
                updateRequest.TeachingPlanId,
                updateRequest.SessionReportId,
                updateRequest.Title,
                updateRequest.Description,
                updateRequest.StartDate,
                updateRequest.DueDate,
                updateRequest.IsMandatory,
                updateRequest.ContentJson,
                updateRequest.MaxScore,
                updateRequest.GroupIds,
                updateRequest.SubChapterIds
            );

            var result = await _mediator.Send(updateCommand);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error ?? "خطا در به‌روزرسانی آیتم آموزشی");
                ViewBag.TeachingPlanId = request.TeachingPlanId;
                ViewBag.CourseId = request.CourseId;
                ViewBag.SessionReportId = request.SessionReportId;
                ViewBag.IsCourseScope = !request.TeachingPlanId.HasValue && request.CourseId.HasValue;
                ViewBag.IsSessionScope = request.SessionReportId.HasValue;
                ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
                .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
                .ToList();
                return View("Edit", updateRequest);
            }

            if (request.TeachingPlanId.HasValue && request.TeachingPlanId.Value > 0)
            {
                return RedirectToAction(nameof(Index), new { teachingPlanId = request.TeachingPlanId.Value });
            }

            if (request.CourseId.HasValue && request.CourseId.Value > 0)
            {
                return RedirectToAction(nameof(CourseLessons), new { courseId = request.CourseId.Value });
            }

            return RedirectToAction(nameof(Index), new { teachingPlanId = 0 });
        }
        else
        {
            // Create new item
            var command = new CreateScheduleItemCommand(
                request.CourseId,
                request.TeachingPlanId,
                request.SessionReportId,
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
                request.StudentProfileIds
            );

            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error ?? "خطا در ایجاد آیتم آموزشی");
                ViewBag.TeachingPlanId = request.TeachingPlanId;
                ViewBag.CourseId = request.CourseId;
                ViewBag.SessionReportId = request.SessionReportId;
                ViewBag.IsCourseScope = !request.TeachingPlanId.HasValue && request.CourseId.HasValue;
                ViewBag.IsSessionScope = request.SessionReportId.HasValue;
                ViewBag.ScheduleItemTypes = Enum.GetValues<ScheduleItemType>()
                .Select(type => new { Value = (int)type, Text = type.GetDisplayName(), Description = type.GetDescription() })
                .ToList();
                return View(request);
            }

            if (request.TeachingPlanId.HasValue && request.TeachingPlanId.Value > 0)
            {
                return RedirectToAction(nameof(Index), new { teachingPlanId = request.TeachingPlanId.Value });
            }

            if (request.CourseId.HasValue && request.CourseId.Value > 0)
            {
                return RedirectToAction(nameof(CourseLessons), new { courseId = request.CourseId.Value });
            }

            return RedirectToAction(nameof(Index), new { teachingPlanId = 0 });
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
            ViewBag.TeachingPlanId = request.TeachingPlanId;
            ViewBag.CourseId = request.CourseId;
            ViewBag.SessionReportId = request.SessionReportId;
            ViewBag.IsCourseScope = !request.TeachingPlanId.HasValue && request.CourseId.HasValue;
            ViewBag.IsSessionScope = request.SessionReportId.HasValue;
            return View(request);
        }

        var command = new UpdateScheduleItemCommand(
            request.Id,
            request.CourseId,
            request.TeachingPlanId,
            request.SessionReportId,
            request.Title,
            request.Description,
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.ContentJson,
            request.MaxScore,
            request.GroupIds,
            request.SubChapterIds
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
        if (scheduleItem.Value?.TeachingPlanId is int redirectPlanId && redirectPlanId > 0)
        {
            return RedirectToAction(nameof(Index), new { teachingPlanId = redirectPlanId });
        }

        var redirectCourseId = scheduleItem.Value?.CourseId ?? request.CourseId;
        if (redirectCourseId.HasValue && redirectCourseId.Value > 0)
        {
            return RedirectToAction(nameof(CourseLessons), new { courseId = redirectCourseId.Value });
        }

        return RedirectToAction(nameof(Index), new { teachingPlanId = 0 });
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

        if (scheduleItem.Value.TeachingPlanId.HasValue)
        {
            return RedirectToAction(nameof(Index), new { teachingPlanId = scheduleItem.Value.TeachingPlanId.Value });
        }

        if (scheduleItem.Value.CourseId.HasValue)
        {
            return RedirectToAction(nameof(CourseLessons), new { courseId = scheduleItem.Value.CourseId.Value });
        }

        return RedirectToAction(nameof(Index), new { teachingPlanId = 0 });
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

    [HttpGet]
    public async Task<IActionResult> GetCourseScheduleItems(int courseId, bool courseScopeOnly = true)
    {
        var result = await _mediator.Send(new GetScheduleItemsByCourseQuery(courseId, courseScopeOnly));

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
        Console.WriteLine($"CreateScheduleItem request: TeachingPlanId={request.TeachingPlanId}, CourseId={request.CourseId}, Title={request.Title}, Type={request.Type}");

        var command = new CreateScheduleItemCommand(
            request.CourseId,
            request.TeachingPlanId,
            request.SessionReportId,
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
            request.SubChapterIds ?? new List<int>(),
            request.StudentProfileIds ?? new List<int>()
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
            request.CourseId,
            request.TeachingPlanId,
            request.SessionReportId,
            request.Title,
            request.Description,
            request.StartDate,
            request.DueDate,
            request.IsMandatory,
            request.ContentJson,
            request.MaxScore,
            request.GroupIds,
            request.SubChapterIds
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
            if (request == null)
            {
                _logger.LogWarning("SaveStep called with null request body. Attempting manual JSON deserialization.");

                try
                {
                    Request.EnableBuffering();
                    Request.Body.Position = 0;

                    using (var reader = new StreamReader(Request.Body, leaveOpen: true))
                    {
                        var rawBody = await reader.ReadToEndAsync();
                        _logger.LogWarning("SaveStep raw request body when initial bind failed: {RawBody}", string.IsNullOrWhiteSpace(rawBody) ? "<empty>" : rawBody);
                        Request.Body.Position = 0;
                    }

                    SaveScheduleItemStepRequest? fallbackRequest = await Request.ReadFromJsonAsync<SaveScheduleItemStepRequest>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (fallbackRequest != null)
                    {
                        request = fallbackRequest;
                    }
                    Request.Body.Position = 0;
                }
                catch (Exception deserializeEx)
                {
                    _logger.LogError(deserializeEx, "Manual deserialization of SaveScheduleItemStepRequest failed.");
                }

                if (request == null)
                {
                    return Json(new { success = false, message = "درخواست نامعتبر است." });
                }
            }

            _logger.LogInformation("SaveStep called with request: {@Request}", request);
            Console.WriteLine($"SaveStep called - Step: {request.Step}, TeachingPlanId: {request.TeachingPlanId}, CourseId: {request.CourseId}");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found in SaveStep");
                return Json(new { success = false, message = "کاربر یافت نشد." });
            }

            // Parse comma-separated IDs if they come as strings
            List<int>? groupIds = request.GroupIds;
            List<int>? subChapterIds = request.SubChapterIds;
            List<int>? studentProfileIds = request.StudentProfileIds;

            var command = new SaveScheduleItemStepCommand(
                request.Id,
            request.CourseId,
                request.TeachingPlanId,
            request.SessionReportId,
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
                studentProfileIds
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
    public IActionResult GetBlockTypeOptions(string itemType, bool showRegularBlocks, bool showQuestionBlocks, string? questionTypeBlocks = null)
    {
        try
        {
            ViewData["ItemType"] = itemType;
            ViewData["ShowRegularBlocks"] = showRegularBlocks;
            ViewData["ShowQuestionBlocks"] = showQuestionBlocks;
            
            var questionTypeBlocksList = new List<string>();
            if (!string.IsNullOrWhiteSpace(questionTypeBlocks))
            {
                questionTypeBlocksList = questionTypeBlocks.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();
            }
            ViewData["QuestionTypeBlocks"] = questionTypeBlocksList;
            
            return PartialView("_BlockTypeOptions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading block type options for item type: {ItemType}", itemType);
            return Content("<div class='alert alert-danger'>خطا در بارگذاری گزینه‌های بلاک</div>");
        }
    }

    [HttpGet]
    public IActionResult GetMcqQuestionTemplate(int questionIndex = 0)
    {
        try
        {
            ViewData["QuestionId"] = Guid.NewGuid().ToString("N");
            ViewData["QuestionIndex"] = questionIndex;
            return PartialView("_McqQuestionItem");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading MCQ question template");
            return StatusCode(StatusCodes.Status500InternalServerError, "خطا در بارگذاری قالب سوال");
        }
    }

    [HttpGet]
    public IActionResult GetMcqOptionTemplate(string? questionId = null, int optionIndex = 0, bool isSingle = true)
    {
        try
        {
            ViewData["QuestionId"] = string.IsNullOrWhiteSpace(questionId) ? Guid.NewGuid().ToString("N") : questionId;
            ViewData["OptionIndex"] = optionIndex;
            ViewData["IsSingle"] = isSingle;
            return PartialView("_McqOptionItem");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading MCQ option template for question {QuestionId}", questionId);
            return StatusCode(StatusCodes.Status500InternalServerError, "خطا در بارگذاری قالب گزینه");
        }
    }
}
