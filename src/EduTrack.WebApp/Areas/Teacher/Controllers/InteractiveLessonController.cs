using EduTrack.Application.Features.InteractiveLesson.Commands;
using EduTrack.Application.Features.InteractiveLesson.Queries;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using EduTrack.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using EduTrack.WebApp.Scripts;
using EduTrack.Infrastructure.Data;
using System.Security.Claims;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class InteractiveLessonController : Controller
{
    private readonly IMediator _mediator;
    private readonly IRepository<Course> _courseRepository;
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public InteractiveLessonController(IMediator mediator, IRepository<Course> courseRepository, UserManager<User> userManager, AppDbContext context)
    {
        _mediator = mediator;
        _courseRepository = courseRepository;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // نمایش لیست دوره‌ها برای انتخاب
        var courses = await _courseRepository.GetAll()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();
        
        return View("CourseSelection", courses);
    }

    [HttpGet]
    [Route("Teacher/InteractiveLesson/ByCourse/{courseId}")]
    public async Task<IActionResult> ByCourse(int courseId)
    {
        var result = await _mediator.Send(new GetInteractiveLessonsByCourseQuery(courseId));
        
        if (result.IsSuccess)
        {
            ViewBag.CourseId = courseId;
            return View("Index", result.Value);
        }

        return NotFound();
    }

    [HttpGet]
    public IActionResult Create(int courseId)
    {
        ViewBag.CourseId = courseId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInteractiveLessonCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Edit), new { id = result.Value!.Id });
        }

        ModelState.AddModelError("", result.Error ?? "خطا در ایجاد درس تعاملی");
        ViewBag.CourseId = command.CourseId;
        return View(command);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetInteractiveLessonByIdQuery(id));
        
        if (result.IsSuccess)
        {
            return View(result.Value);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddContent(AddContentToInteractiveLessonCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> AddQuestion(AddQuestionToInteractiveLessonCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveContent(int contentItemId)
    {
        var result = await _mediator.Send(new RemoveContentFromInteractiveLessonCommand(contentItemId));
        
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> ReorderContent(int interactiveLessonId, List<int> contentItemIds)
    {
        var result = await _mediator.Send(new ReorderInteractiveContentCommand(interactiveLessonId, contentItemIds));
        
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableContent(int courseId)
    {
        var result = await _mediator.Send(new GetAvailableEducationalContentQuery(courseId));
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> FixUserRoles()
    {
        try
        {
            await FixUserRolesScript.FixUserRolesAsync(_userManager, _context);
            return Json(new { success = true, message = "User roles fixed successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckUserRoles()
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, error = "User not found" });
            }

            var identityRoles = await _userManager.GetRolesAsync(currentUser);
            var claims = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            return Json(new { 
                success = true, 
                data = new {
                    UserEmail = currentUser.Email,
                    IdentityRoles = identityRoles,
                    ClaimsRoles = claims
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
}
