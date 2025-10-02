using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using EduTrack.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: Admin/Users
    public async Task<IActionResult> Index(string? search, UserRole? role, int page = 1, int pageSize = 20)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || 
                                   u.LastName.Contains(search) || 
                                   u.Email!.Contains(search));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var totalUsers = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new UserManagementViewModel
        {
            Users = users,
            Search = search,
            SelectedRole = role,
            CurrentPage = page,
            PageSize = pageSize,
            TotalUsers = totalUsers,
            TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize)
        };

        return View(model);
    }

    // GET: Admin/Users/Create
    public IActionResult Create()
    {
        return View(new CreateUserViewModel());
    }

    // POST: Admin/Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Log activity
                await LogActivity("CreateUser", "User", user.Id, $"Created user: {user.FullName} ({user.Role})");
                
                TempData["Success"] = $"کاربر {user.FullName} با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // GET: Admin/Users/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            Role = user.Role,
            IsActive = user.IsActive
        };

        return View(model);
    }

    // POST: Admin/Users/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await LogActivity("UpdateUser", "User", user.Id, $"Updated user: {user.FullName}");
                TempData["Success"] = "اطلاعات کاربر با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // POST: Admin/Users/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "کاربر یافت نشد" });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            await LogActivity("ResetPassword", "User", user.Id, $"Reset password for user: {user.FullName}");
            return Json(new { success = true, message = "رمز عبور با موفقیت تغییر یافت" });
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Json(new { success = false, message = errors });
    }

    // POST: Admin/Users/ToggleStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "کاربر یافت نشد" });
        }

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var status = user.IsActive ? "فعال" : "غیرفعال";
            await LogActivity("ToggleUserStatus", "User", user.Id, $"Changed user status to {status}: {user.FullName}");
            return Json(new { success = true, message = $"وضعیت کاربر به {status} تغییر یافت", isActive = user.IsActive });
        }

        return Json(new { success = false, message = "خطا در تغییر وضعیت کاربر" });
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
