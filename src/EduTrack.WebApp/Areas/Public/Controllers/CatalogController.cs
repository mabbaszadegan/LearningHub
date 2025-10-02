using EduTrack.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Public.Controllers;

[Area("Public")]
public class CatalogController : Controller
{
    private readonly ILogger<CatalogController> _logger;
    private readonly UserManager<User> _userManager;

    public CatalogController(
        ILogger<CatalogController> logger, 
        UserManager<User> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Course(int id)
    {
        return View();
    }

    public IActionResult Classes()
    {
        return View();
    }

    public IActionResult Class(int id)
    {
        return View();
    }
}