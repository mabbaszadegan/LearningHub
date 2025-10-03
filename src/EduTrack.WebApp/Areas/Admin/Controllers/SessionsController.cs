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
public class SessionsController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<SessionsController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: Admin/Sessions
    public async Task<IActionResult> Index()
    {
        var activeSessions = await GetActiveUserSessions();
        return View(activeSessions);
    }

    // POST: Admin/Sessions/LogoutUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "کاربر یافت نشد" });
        }

        // Update security stamp to invalidate all sessions
        await _userManager.UpdateSecurityStampAsync(user);
        
        await LogActivity("ForceLogout", "User", user.Id, $"Forced logout for user: {user.FullName}");
        
        return Json(new { success = true, message = $"کاربر {user.FullName} از سیستم خارج شد" });
    }

    private async Task<List<ActiveSessionData>> GetActiveUserSessions()
    {
        var activeUsers = await _userManager.Users
            .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value > DateTimeOffset.UtcNow.AddHours(-24))
            .OrderByDescending(u => u.LastLoginAt)
            .ToListAsync();

        return activeUsers.Select(u => new ActiveSessionData
        {
            User = u,
            LastLogin = u.LastLoginAt ?? DateTimeOffset.MinValue,
            IsOnline = u.LastLoginAt > DateTimeOffset.UtcNow.AddMinutes(-30) // Consider online if logged in within 30 minutes
        }).ToList();
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
