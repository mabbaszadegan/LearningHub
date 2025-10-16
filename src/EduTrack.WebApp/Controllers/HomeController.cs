using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Redirect to Public area
        return RedirectToAction("Index", "Home", new { area = "Public" });
    }




}