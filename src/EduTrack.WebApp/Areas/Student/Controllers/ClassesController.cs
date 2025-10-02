using EduTrack.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class ClassesController : Controller
{
    private readonly ILogger<ClassesController> _logger;
    private readonly UserManager<User> _userManager;

    public ClassesController(
        ILogger<ClassesController> logger, 
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

        return View();
    }
}