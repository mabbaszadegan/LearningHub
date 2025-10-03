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
    public async Task<IActionResult> Index(string? search, string? role, int page = 1, int pageSize = 20)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || 
                                   u.LastName.Contains(search) || 
                                   u.Email!.Contains(search));
        }

        if (!string.IsNullOrEmpty(role))
        {
            // Filter by role using UserManager
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = usersInRole.Select(u => u.Id).ToList();
            query = query.Where(u => userIds.Contains(u.Id));
        }

        var totalUsers = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get roles for each user
        var usersWithRoles = new List<UserWithRolesViewModel>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRolesViewModel
            {
                User = user,
                Roles = userRoles.ToList()
            });
        }

        var model = new UsersIndexViewModel
        {
            Users = usersWithRoles,
            Search = search,
            Role = role,
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
            var user = EduTrack.Domain.Entities.User.Create(
                model.FirstName,
                model.LastName,
                model.Email);
            user.UserName = model.Email;

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add user to role
                await _userManager.AddToRoleAsync(user, model.Role.ToString());
                
                // Log activity
                await LogActivity("CreateUser", "User", user.Id, $"Created user: {user.FullName} ({model.Role})");
                
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

        var userRoles = await _userManager.GetRolesAsync(user);
        var primaryRole = userRoles.FirstOrDefault() ?? "Student";
        
        var model = new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            Role = Enum.Parse<UserRole>(primaryRole),
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

            user.UpdateProfile(model.FirstName, model.LastName);
            user.Email = model.Email;
            user.UserName = model.Email;
            
            if (model.IsActive)
                user.Activate();
            else
                user.Deactivate();

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update user role
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, model.Role.ToString());
                
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

        if (user.IsActive)
            user.Deactivate();
        else
            user.Activate();
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
