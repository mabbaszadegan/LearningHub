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
public class ClassesController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<ClassesController> _logger;

    public ClassesController(
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<ClassesController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: Admin/Classes
    public async Task<IActionResult> Index(string? search, bool? isActive, int page = 1, int pageSize = 20)
    {
        var query = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Name.Contains(search) || 
                                   c.Course.Title.Contains(search) ||
                                   c.Teacher.FirstName.Contains(search) ||
                                   c.Teacher.LastName.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalClasses = await query.CountAsync();
        var classes = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new ClassManagementViewModel
        {
            Classes = classes,
            Search = search,
            IsActive = isActive,
            CurrentPage = page,
            PageSize = pageSize,
            TotalClasses = totalClasses,
            TotalPages = (int)Math.Ceiling((double)totalClasses / pageSize)
        };

        return View(model);
    }

    // GET: Admin/Classes/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var classEntity = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classEntity == null)
        {
            return NotFound();
        }

        return View(classEntity);
    }

    private async Task LogActivity(string action, string? entityType = null, object? entityId = null, string? details = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return;

        var log = new ActivityLog
        {
            UserId = currentUser.Id,
            Action = action,
            EntityType = entityType,
            EntityId = entityId?.ToString() != null ? int.TryParse(entityId.ToString(), out var id) ? id : null : null,
            Details = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
            Timestamp = DateTimeOffset.UtcNow
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
