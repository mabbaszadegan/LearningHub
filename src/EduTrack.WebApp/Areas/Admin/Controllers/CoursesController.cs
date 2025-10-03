using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using EduTrack.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CoursesController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<CoursesController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: Admin/Courses
    public async Task<IActionResult> Index(string? search, bool? isActive, int page = 1, int pageSize = 20)
    {
        var query = _context.Courses.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) || (c.Description != null && c.Description.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalCourses = await query.CountAsync();
        var courses = await query
            .Include(c => c.Modules)
            .Include(c => c.Classes)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new CourseManagementViewModel
        {
            Courses = courses,
            Search = search,
            IsActive = isActive,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCourses = totalCourses,
            TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize)
        };

        return View(model);
    }

    // GET: Admin/Courses/Create
    public IActionResult Create()
    {
        return View(new CreateCourseViewModel());
    }

    // POST: Admin/Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel model)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            var course = Course.Create(
                model.Title,
                model.Description,
                null,
                model.Order,
                currentUser?.Id ?? "");
            
            if (model.IsActive)
                course.Activate();
            else
                course.Deactivate();

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            await LogActivity("CreateCourse", "Course", course.Id, $"Created course: {course.Title}");
            
            TempData["Success"] = $"دوره {course.Title} با موفقیت ایجاد شد";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // POST: Admin/Courses/ToggleStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
        {
            return Json(new { success = false, message = "دوره یافت نشد" });
        }

        if (course.IsActive)
            course.Deactivate();
        else
            course.Activate();
        await _context.SaveChangesAsync();

        var status = course.IsActive ? "فعال" : "غیرفعال";
        await LogActivity("ToggleCourseStatus", "Course", course.Id, $"Changed course status to {status}: {course.Title}");
        
        return Json(new { success = true, message = $"وضعیت دوره به {status} تغییر یافت", isActive = course.IsActive });
    }

    private async Task LogActivity(string action, string? entityType = null, object? entityId = null, string? details = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return;

        var log = ActivityLog.Create(
            currentUser.Id,
            action,
            entityType,
            entityId?.ToString() != null ? int.TryParse(entityId.ToString(), out var id) ? id : null : null,
            details,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            HttpContext.Request.Headers["User-Agent"].ToString());

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
