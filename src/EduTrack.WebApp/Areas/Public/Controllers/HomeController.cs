using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.Classroom.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Public.Controllers;

[Area("Public")]
public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;

    public HomeController(IMediator mediator, ILogger<HomeController> logger, UserManager<User> userManager)
    {
        _mediator = mediator;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // If user is not authenticated, show landing page
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View("Landing");
        }

        // If user is authenticated, redirect to role-based dashboard
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            
            if (userRoles.Contains("Admin"))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            else if (userRoles.Contains("Teacher"))
                return RedirectToAction("Index", "Home", new { area = "Teacher" });
            else if (userRoles.Contains("Student"))
                return RedirectToAction("Index", "Home", new { area = "Student" });
            else
                return RedirectToAction("Index", "Home", new { area = "Student" });
        }

        // Fallback to landing page
        return View("Landing");
    }

    public IActionResult Landing()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
