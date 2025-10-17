using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.WebApp.Extensions;
using EduTrack.WebApp.Areas.Teacher.Views.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class CoursesController : BaseTeacherController
{
    private readonly ILogger<CoursesController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public CoursesController(
        ILogger<CoursesController> logger, 
        UserManager<User> userManager,
        IMediator mediator,
        IPageTitleSectionService pageTitleSectionService) : base(pageTitleSectionService)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get teacher's courses (including inactive ones)
        var coursesActive = await _mediator.Send(new GetCoursesQuery(1, 100, true));
        var coursesInactive = await _mediator.Send(new GetCoursesQuery(1, 100, false));
        
        var allCourses = coursesActive.Items.Concat(coursesInactive.Items)
            .Where(c => c.CreatedBy == currentUser.Id)
            .Distinct()
            .ToList();

        // Setup page title section
        await SetPageTitleSectionAsync(PageType.CoursesIndex);

        return View(allCourses);
    }

    public IActionResult Create()
    {
        // Setup page title section
        SetPageTitleSection(PageType.CourseCreate);
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseCommand command)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Public" });
            }

            // The CreatedBy is set automatically in the handler using ICurrentUserService

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "دوره با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = result.Error;
            }
        }

        return View(command);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetCourseByIdQuery(id));
        if (result.IsSuccess)
        {
            var updateCommand = new UpdateCourseCommand(
                result.Value!.Id,
                result.Value.Title,
                result.Value.Description,
                result.Value.Thumbnail,
                result.Value.IsActive,
                result.Value.Order
            );
            return View(updateCommand);
        }
        
        TempData["Error"] = "دوره مورد نظر یافت نشد";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateCourseCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "دوره با موفقیت ویرایش شد";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = result.Error;
            }
        }

        return View(command);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            var result = await _mediator.Send(new ToggleCourseActiveCommand(id));
            if (result.IsSuccess)
            {
                return Json(new { success = true, isActive = result.Value });
            }
            return Json(new { success = false, error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling course active status");
            return Json(new { success = false, error = "خطا در تغییر وضعیت" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReorderCourses([FromBody] List<int> courseIds)
    {
        try
        {
            var result = await _mediator.Send(new ReorderCoursesCommand { CourseIds = courseIds });
            if (result.IsSuccess)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering courses");
            return Json(new { success = false, error = "خطا در تغییر ترتیب" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MoveCourse(int id, string direction)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, error = "کاربر یافت نشد" });
            }

            // Get all teacher's courses
            var coursesActive = await _mediator.Send(new GetCoursesQuery(1, 100, true));
            var coursesInactive = await _mediator.Send(new GetCoursesQuery(1, 100, false));
            
            var allCourses = coursesActive.Items.Concat(coursesInactive.Items)
                .Where(c => c.CreatedBy == currentUser.Id)
                .OrderBy(c => c.Order)
                .ToList();

            var currentCourse = allCourses.FirstOrDefault(c => c.Id == id);
            if (currentCourse == null)
            {
                return Json(new { success = false, error = "دوره یافت نشد" });
            }

            var currentIndex = allCourses.IndexOf(currentCourse);
            var newIndex = direction == "up" ? currentIndex - 1 : currentIndex + 1;

            if (newIndex < 0 || newIndex >= allCourses.Count)
            {
                return Json(new { success = false, error = "امکان جابجایی وجود ندارد" });
            }

            // Swap orders
            var otherCourse = allCourses[newIndex];
            var tempOrder = currentCourse.Order;

            var updateCurrent = new UpdateCourseCommand(
                currentCourse.Id,
                currentCourse.Title,
                currentCourse.Description,
                currentCourse.Thumbnail,
                currentCourse.IsActive,
                otherCourse.Order
            );

            var updateOther = new UpdateCourseCommand(
                otherCourse.Id,
                otherCourse.Title,
                otherCourse.Description,
                otherCourse.Thumbnail,
                otherCourse.IsActive,
                tempOrder
            );

            await _mediator.Send(updateCurrent);
            await _mediator.Send(updateOther);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving course");
            return Json(new { success = false, error = "خطا در جابجایی" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetHierarchy()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        try
        {
            var result = await _mediator.Send(new GetTeacherCoursesHierarchyQuery(currentUser.Id));
            if (result.IsSuccess)
            {
                return Json(new { success = true, data = result.Value });
            }
            else
            {
                return Json(new { success = false, error = result.Error });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher courses hierarchy");
            return Json(new { success = false, error = "خطا در دریافت اطلاعات" });
        }
    }
}