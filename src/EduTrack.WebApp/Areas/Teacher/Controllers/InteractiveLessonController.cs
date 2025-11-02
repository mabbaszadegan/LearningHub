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
using EduTrack.Domain.Enums;

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
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return NotFound();

        var result = await _mediator.Send(new GetInteractiveLessonsByCourseQuery(courseId));
        
        if (result.IsSuccess)
        {
            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;
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

    // AddContent method removed - AddContentToInteractiveLessonCommand removed (EducationalContent entity removed)
    // Use AddQuestion instead

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
    // GetAvailableContent method removed - GetAvailableEducationalContentQuery removed (EducationalContent entity removed)

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

    // Enhanced Interactive Lesson Methods with Stages
    [HttpGet]
    [Route("Teacher/InteractiveLesson/CreateWithStages/{courseId}")]
    public async Task<IActionResult> CreateWithStages(int courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return NotFound();

        // Get sub-chapters for the course
        var subChapters = await _context.SubChapters
            .Include(sc => sc.Chapter)
            // EducationalContents removed - EducationalContent entity removed
            .Where(sc => sc.Chapter.CourseId == courseId && sc.IsActive)
            .OrderBy(sc => sc.Chapter.Order)
            .ThenBy(sc => sc.Order)
            .ToListAsync();

        ViewBag.Course = course;
        ViewBag.SubChapters = subChapters;
        ViewBag.StageTypes = Enum.GetValues<InteractiveLessonStageType>();
        ViewBag.ArrangementTypes = Enum.GetValues<ContentArrangementType>();

        return View();
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/CreateWithStages")]
    public async Task<IActionResult> CreateWithStages(CreateInteractiveLessonWithStagesCommand command)
    {
        if (!ModelState.IsValid)
        {
            if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
            {
                return Json(new { success = false, error = "اطلاعات وارد شده صحیح نیست" });
            }
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
            {
                return Json(new { success = true, data = new { id = result.Value!.Id } });
            }
            return RedirectToAction("Details", new { id = result.Value!.Id });
        }

        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        {
            return Json(new { success = false, error = result.Error });
        }
        return BadRequest(result.Error);
    }

    [HttpGet]
    [Route("Teacher/InteractiveLesson/Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _mediator.Send(new GetInteractiveLessonWithStagesQuery { Id = id });
        if (!result.IsSuccess)
            return NotFound();

        return View(result.Value);
    }

    [HttpGet]
    [Route("Teacher/InteractiveLesson/ManageStages/{interactiveLessonId}")]
    public async Task<IActionResult> ManageStages(int interactiveLessonId)
    {
        var interactiveLesson = await _context.InteractiveLessons
            .Include(il => il.Course)
            .FirstOrDefaultAsync(il => il.Id == interactiveLessonId);

        if (interactiveLesson == null)
            return NotFound();

        var stages = await _mediator.Send(new GetInteractiveLessonStagesQuery { InteractiveLessonId = interactiveLessonId });
        
        ViewBag.InteractiveLesson = interactiveLesson;
        ViewBag.StageTypes = Enum.GetValues<InteractiveLessonStageType>();
        ViewBag.ArrangementTypes = Enum.GetValues<ContentArrangementType>();

        return View(stages.Value);
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/CreateStage")]
    public async Task<IActionResult> CreateStage(CreateInteractiveLessonStageCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/UpdateStage")]
    public async Task<IActionResult> UpdateStage(UpdateInteractiveLessonStageCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/DeleteStage")]
    public async Task<IActionResult> DeleteStage(DeleteInteractiveLessonStageCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpGet]
    [Route("Teacher/InteractiveLesson/ManageSubChapters/{interactiveLessonId}")]
    public async Task<IActionResult> ManageSubChapters(int interactiveLessonId)
    {
        var interactiveLesson = await _context.InteractiveLessons
            .Include(il => il.Course)
            .FirstOrDefaultAsync(il => il.Id == interactiveLessonId);

        if (interactiveLesson == null)
            return NotFound();

        var subChapters = await _mediator.Send(new GetInteractiveLessonSubChaptersQuery { InteractiveLessonId = interactiveLessonId });
        
        // Get available sub-chapters for the course
        var availableSubChapters = await _context.SubChapters
            .Include(sc => sc.Chapter)
            .Where(sc => sc.Chapter.CourseId == interactiveLesson.CourseId && sc.IsActive)
            .OrderBy(sc => sc.Chapter.Order)
            .ThenBy(sc => sc.Order)
            .ToListAsync();

        ViewBag.InteractiveLesson = interactiveLesson;
        ViewBag.AvailableSubChapters = availableSubChapters;

        return View(subChapters.Value);
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/AddSubChapter")]
    public async Task<IActionResult> AddSubChapter(AddSubChapterToInteractiveLessonCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/RemoveSubChapter")]
    public async Task<IActionResult> RemoveSubChapter(RemoveSubChapterFromInteractiveLessonCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpGet]
    [Route("Teacher/InteractiveLesson/ManageStageContent/{stageId}")]
    public async Task<IActionResult> ManageStageContent(int stageId)
    {
        var stage = await _context.InteractiveLessonStages
            .Include(s => s.InteractiveLesson)
                .ThenInclude(il => il.Course)
            .Include(s => s.InteractiveLesson)
                .ThenInclude(il => il.SubChapters.Where(sc => sc.IsActive))
                    .ThenInclude(sc => sc.SubChapter)
            .FirstOrDefaultAsync(s => s.Id == stageId);

        if (stage == null)
            return NotFound();

        var contentItems = await _mediator.Send(new GetStageContentItemsQuery { InteractiveLessonStageId = stageId });
        
        ViewBag.Stage = stage;
        ViewBag.ContentItems = contentItems.Value;

        return View();
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/AddStageContentItem")]
    public async Task<IActionResult> AddStageContentItem(AddStageContentItemCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpPost]
    [Route("Teacher/InteractiveLesson/RemoveStageContentItem")]
    public async Task<IActionResult> RemoveStageContentItem(RemoveStageContentItemCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = result.Error });
    }
}
