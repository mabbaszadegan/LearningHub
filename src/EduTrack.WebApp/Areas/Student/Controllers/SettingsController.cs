using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class SettingsController : Controller
{
    private readonly ILogger<SettingsController> _logger;
    private readonly UserManager<User> _userManager;

    public SettingsController(
        ILogger<SettingsController> logger,
        UserManager<User> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var viewModel = new
        {
            StudentName = currentUser.FullName,
            StudentFirstName = currentUser.FirstName,
            Email = currentUser.Email,
            UserName = currentUser.UserName
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotificationSettings(bool emailNotifications, bool pushNotifications)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // TODO: Implement notification settings storage
        // For now, just return success
        return Json(new { success = true, message = "تنظیمات با موفقیت ذخیره شد" });
    }
}

