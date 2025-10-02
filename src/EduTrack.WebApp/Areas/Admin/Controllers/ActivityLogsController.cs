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
public class ActivityLogsController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<ActivityLogsController> _logger;

    public ActivityLogsController(
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<ActivityLogsController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: Admin/ActivityLogs
    public async Task<IActionResult> Index(string? search, string? action, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 50)
    {
        var query = _context.ActivityLogs
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.User.FirstName.Contains(search) || 
                                   a.User.LastName.Contains(search) ||
                                   a.User.Email!.Contains(search) ||
                                   (a.Details != null && a.Details.Contains(search)));
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value.AddDays(1));
        }

        var totalLogs = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new ActivityLogsViewModel
        {
            Logs = logs,
            Search = search,
            Action = action,
            FromDate = fromDate,
            ToDate = toDate,
            CurrentPage = page,
            PageSize = pageSize,
            TotalLogs = totalLogs,
            TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize),
            AvailableActions = await _context.ActivityLogs.Select(a => a.Action).Distinct().ToListAsync()
        };

        return View(model);
    }
}
