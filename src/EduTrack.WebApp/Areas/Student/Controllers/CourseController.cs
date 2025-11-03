using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Features.CourseEnrollment.Commands;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Application.Features.CourseEnrollment.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.Chapters.Queries;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize] // Temporarily remove role restriction for testing
public class CourseController : Controller
{
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<CourseController> _logger;

    public CourseController(
        IMediator mediator,
        UserManager<User> userManager,
        ILogger<CourseController> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Display student's enrolled courses
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var result = await _mediator.Send(new GetStudentCourseEnrollmentsQuery(currentUser.Id));
        
        if (result.IsSuccess)
        {
            return View(result.Value);
        }

        TempData["Error"] = result.Error;
        return View(new List<StudentCourseEnrollmentSummaryDto>());
    }

    /// <summary>
    /// Display course catalog for enrollment with comprehensive statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Catalog(int pageNumber = 1, int pageSize = 12)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var result = await _mediator.Send(new GetAvailableCoursesForEnrollmentQuery(currentUser.Id, pageNumber, pageSize));
        
        if (result.IsSuccess)
        {
            ViewBag.CurrentUserId = currentUser.Id;
            return View(result.Value);
        }

        TempData["Error"] = result.Error;
        return View(new PaginatedList<CourseDto>(new List<CourseDto>(), 0, pageNumber, pageSize));
    }

    /// <summary>
    /// Display course details
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get course details
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(id));
        if (!courseResult.IsSuccess)
        {
            TempData["Error"] = "دوره یافت نشد";
            return RedirectToAction("Catalog");
        }

        // Check if student is enrolled
        var enrollmentResult = await _mediator.Send(new GetCourseEnrollmentQuery(id, currentUser.Id));
        var isEnrolled = enrollmentResult.IsSuccess;

        // Check if student can enroll
        var canEnrollResult = await _mediator.Send(new CanEnrollInCourseQuery(id, currentUser.Id));
        var canEnroll = canEnrollResult.IsSuccess && canEnrollResult.Value;

        ViewBag.IsEnrolled = isEnrolled;
        ViewBag.CanEnroll = canEnroll;
        ViewBag.CurrentUserId = currentUser.Id;

        return View(courseResult.Value);
    }

    /// <summary>
    /// Enroll student in a course
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new EnrollInCourseCommand(courseId, currentUser.Id));
        
        if (result.IsSuccess)
        {
            TempData["Success"] = "با موفقیت در دوره ثبت‌نام شدید";
            return Json(new { success = true, message = "با موفقیت در دوره ثبت‌نام شدید" });
        }

        return Json(new { success = false, error = result.Error });
    }

    /// <summary>
    /// Unenroll student from a course
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new UnenrollFromCourseCommand(courseId, currentUser.Id));
        
        if (result.IsSuccess)
        {
            TempData["Success"] = "از دوره خارج شدید";
            return Json(new { success = true, message = "از دوره خارج شدید" });
        }

        return Json(new { success = false, error = result.Error });
    }

    /// <summary>
    /// Display course content for enrolled students
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Study(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Check if student is enrolled
        var enrollmentResult = await _mediator.Send(new GetCourseEnrollmentQuery(id, currentUser.Id));
        if (!enrollmentResult.IsSuccess)
        {
            TempData["Error"] = "شما در این دوره ثبت‌نام نکرده‌اید";
            return RedirectToAction("Details", new { id });
        }

        // Get course details
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(id));
        if (!courseResult.IsSuccess)
        {
            TempData["Error"] = "دوره یافت نشد";
            return RedirectToAction("Index");
        }

        // Get course progress
        var progressResult = await _mediator.Send(new GetStudentCourseProgressQuery(id, currentUser.Id));
        
        // Get course chapters with subchapters
        var chaptersResult = await _mediator.Send(new GetChaptersByCourseIdQuery(id));
        var chapters = chaptersResult.IsSuccess ? chaptersResult.Value : new List<ChapterDto>();
        
        // Get schedule items for the course
        var scheduleItemsResult = await _mediator.Send(new GetCourseScheduleItemsQuery(id, currentUser.Id));
        var scheduleItems = scheduleItemsResult.IsSuccess ? scheduleItemsResult.Value : new List<ScheduleItemDto>();
        
        ViewBag.Enrollment = enrollmentResult.Value;
        ViewBag.Progress = progressResult.IsSuccess ? progressResult.Value : null;
        ViewBag.Chapters = chapters;
        ViewBag.ScheduleItems = scheduleItems;

        return View(courseResult.Value);
    }

    /// <summary>
    /// Test endpoint to check if controller is working
    /// </summary>
    [HttpGet]
    public IActionResult Test()
    {
        var userInfo = new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name,
            Roles = User.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value).ToList(),
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };
        
        return Json(new { 
            success = true, 
            message = "Controller is working", 
            timestamp = DateTime.Now,
            user = userInfo
        });
    }

    /// <summary>
    /// Get available courses for AJAX loading
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAvailableCourses(int pageNumber = 1, int pageSize = 6)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found in GetAvailableCourses");
                return Json(new { success = false, error = "کاربر یافت نشد" });
            }

            _logger.LogInformation("Loading available courses for user: {UserId}", currentUser.Id);

            var result = await _mediator.Send(new GetAvailableCoursesForEnrollmentQuery(currentUser.Id, pageNumber, pageSize));
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully loaded {Count} courses", result.Value?.Items?.Count ?? 0);
                
                // If no courses found, return empty state
                if (result.Value?.Items == null || !result.Value.Items.Any())
                {
                    return PartialView("_CourseGridPartial", new PaginatedList<CourseDto>(new List<CourseDto>(), 0, 1, pageSize));
                }
                
                return PartialView("_CourseGridPartial", result.Value);
            }

            _logger.LogError("Failed to load courses: {Error}", result.Error);
            return Json(new { success = false, error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetAvailableCourses");
            return Json(new { success = false, error = "خطا در بارگذاری دوره‌ها" });
        }
    }

    /// <summary>
    /// Get student's enrolled courses for AJAX loading
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyCourses()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new GetStudentCourseEnrollmentsQuery(currentUser.Id));
        
        if (result.IsSuccess)
        {
            return PartialView("_MyCoursesPartial", result.Value!);
        }

        return Json(new { success = false, error = result.Error });
    }

    /// <summary>
    /// Get course details for modal
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCourseDetails(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // Get course details
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(id));
        if (!courseResult.IsSuccess)
        {
            return Json(new { success = false, error = "دوره یافت نشد" });
        }

        // Check if student is enrolled
        var enrollmentResult = await _mediator.Send(new GetCourseEnrollmentQuery(id, currentUser.Id));
        var isEnrolled = enrollmentResult.IsSuccess;

        // Check if student can enroll
        var canEnrollResult = await _mediator.Send(new CanEnrollInCourseQuery(id, currentUser.Id));
        var canEnroll = canEnrollResult.IsSuccess && canEnrollResult.Value;

        ViewBag.IsEnrolled = isEnrolled;
        ViewBag.CanEnroll = canEnroll;
        ViewBag.CurrentUserId = currentUser.Id;

        return PartialView("_CourseDetailsPartial", courseResult.Value);
    }

    /// <summary>
    /// Display course schedule items for enrolled students
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ScheduleItems(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Check if student is enrolled
        var enrollmentResult = await _mediator.Send(new GetCourseEnrollmentQuery(id, currentUser.Id));
        if (!enrollmentResult.IsSuccess)
        {
            TempData["Error"] = "شما در این دوره ثبت‌نام نکرده‌اید";
            return RedirectToAction("Details", new { id });
        }

        // Get course details
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(id));
        if (!courseResult.IsSuccess)
        {
            TempData["Error"] = "دوره یافت نشد";
            return RedirectToAction("Index");
        }

        // Get schedule items for the course
        var scheduleItemsResult = await _mediator.Send(new GetCourseScheduleItemsQuery(id, currentUser.Id));
        if (!scheduleItemsResult.IsSuccess)
        {
            TempData["Error"] = scheduleItemsResult.Error;
            return RedirectToAction("Study", new { id });
        }

        ViewBag.Course = courseResult.Value;
        ViewBag.Enrollment = enrollmentResult.Value;

        return View(scheduleItemsResult.Value);
    }

    /// <summary>
    /// Get course schedule items as JSON for AJAX requests with filtering and sorting
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetScheduleItems(int id, string filter = "all")
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // Check if student is enrolled
        var enrollmentResult = await _mediator.Send(new GetCourseEnrollmentQuery(id, currentUser.Id));
        if (!enrollmentResult.IsSuccess)
        {
            return Json(new { success = false, error = "شما در این دوره ثبت‌نام نکرده‌اید" });
        }

        // Get schedule items for the course
        var scheduleItemsResult = await _mediator.Send(new GetCourseScheduleItemsQuery(id, currentUser.Id));
        if (!scheduleItemsResult.IsSuccess)
        {
            return Json(new { success = false, error = scheduleItemsResult.Error });
        }

        // Apply filtering
        var filteredItems = scheduleItemsResult.Value ?? new List<ScheduleItemDto>();
        if (filter != "all" && filteredItems.Any())
        {
            filteredItems = filteredItems.Where(item => item.Type.ToString() == filter).ToList();
        }

        // Sort by due date: items with due dates first (descending), then items without due dates
        var sortedItems = filteredItems
            .OrderByDescending(item => item.DueDate.HasValue)
            .ThenByDescending(item => item.DueDate)
            .ToList();

        // Get available content types (types that have data)
        var availableTypes = new List<object>();
        if (scheduleItemsResult.Value != null)
        {
            availableTypes = scheduleItemsResult.Value
                .Where(item => item != null)
                .GroupBy(item => item.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .OrderBy(x => x.Type)
                .Cast<object>()
                .ToList();
        }

        return Json(new { 
            success = true, 
            data = sortedItems,
            totalCount = sortedItems.Count,
            availableTypes = availableTypes
        });
    }
}
