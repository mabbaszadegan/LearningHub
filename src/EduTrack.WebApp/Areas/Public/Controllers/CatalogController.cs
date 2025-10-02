using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Public.Controllers;

[Area("Public")]
public class CatalogController : Controller
{
    private readonly ILogger<CatalogController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public CatalogController(
        ILogger<CatalogController> logger, 
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var courses = await _mediator.Send(new GetCoursesQuery(pageNumber, pageSize, true));
            return View(courses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading courses for catalog");
            TempData["Error"] = "خطا در بارگذاری دوره‌ها";
            return View(new EduTrack.Application.Common.Models.PaginatedList<EduTrack.Application.Common.Models.CourseDto>(new List<EduTrack.Application.Common.Models.CourseDto>(), 0, pageNumber, pageSize));
        }
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