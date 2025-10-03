using EduTrack.Application.Features.EducationalContent.Commands;
using EduTrack.Application.Features.EducationalContent.Queries;
using EduTrack.Application.Features.Chapters.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class EducationalContentController : Controller
{
    private readonly ILogger<EducationalContentController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public EducationalContentController(
        ILogger<EducationalContentController> logger, 
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int subChapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get subchapter and verify ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(subChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        var contentsResult = await _mediator.Send(new GetEducationalContentsBySubChapterIdQuery(subChapterId));
        if (!contentsResult.IsSuccess)
        {
            return NotFound();
        }

        ViewBag.SubChapterId = subChapterId;
        ViewBag.SubChapterTitle = subChapterResult.Value.Title;
        ViewBag.ChapterTitle = chapterResult.Value.Title;
        ViewBag.CourseTitle = courseResult.Value?.Title;
        ViewBag.CourseId = chapterResult.Value.CourseId;
        
        return View(contentsResult.Value);
    }

    public async Task<IActionResult> Create(int subChapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(subChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        ViewBag.SubChapterId = subChapterId;
        ViewBag.SubChapterTitle = subChapterResult.Value.Title;
        ViewBag.ChapterTitle = chapterResult.Value.Title;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(new CreateEducationalContentCommand(subChapterId, string.Empty, string.Empty, EduTrack.Domain.Enums.EducationalContentType.Text, string.Empty, null, string.Empty, 0));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEducationalContentCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(command.SubChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "محتوا با موفقیت ایجاد شد.";
                return RedirectToAction("Index", new { subChapterId = command.SubChapterId });
            }
            else
            {
                ModelState.AddModelError("", result.Error ?? "خطایی در ایجاد محتوا رخ داد.");
            }
        }

        ViewBag.SubChapterId = command.SubChapterId;
        ViewBag.SubChapterTitle = subChapterResult.Value.Title;
        ViewBag.ChapterTitle = chapterResult.Value.Title;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(command);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var contentResult = await _mediator.Send(new GetEducationalContentByIdQuery(id));
        if (!contentResult.IsSuccess || contentResult.Value == null)
        {
            return NotFound();
        }

        var content = contentResult.Value;

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(content.SubChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        ViewBag.SubChapterId = content.SubChapterId;
        ViewBag.SubChapterTitle = subChapterResult.Value.Title;
        ViewBag.ChapterTitle = chapterResult.Value.Title;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        var command = new UpdateEducationalContentCommand(
            content.Id,
            content.Title,
            content.Description,
            content.Type,
            content.TextContent,
            null, // File will be handled separately
            content.ExternalUrl,
            content.IsActive,
            content.Order);

        // Pass existing file information to view
        ViewBag.ExistingFile = content.File;

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateEducationalContentCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get content to verify ownership
        var contentResult = await _mediator.Send(new GetEducationalContentByIdQuery(command.Id));
        if (!contentResult.IsSuccess || contentResult.Value == null)
        {
            return NotFound();
        }

        var content = contentResult.Value;

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(content.SubChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "محتوا با موفقیت ویرایش شد.";
                return RedirectToAction("Index", new { subChapterId = content.SubChapterId });
            }
            else
            {
                ModelState.AddModelError("", result.Error ?? "خطایی در ویرایش محتوا رخ داد.");
            }
        }

        ViewBag.SubChapterId = content.SubChapterId;
        ViewBag.SubChapterTitle = subChapterResult.Value.Title;
        ViewBag.ChapterTitle = chapterResult.Value.Title;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get content to verify ownership
        var contentResult = await _mediator.Send(new GetEducationalContentByIdQuery(id));
        if (!contentResult.IsSuccess || contentResult.Value == null)
        {
            return NotFound();
        }

        var content = contentResult.Value;

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(content.SubChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        var result = await _mediator.Send(new DeleteEducationalContentCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "محتوا با موفقیت حذف شد.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error ?? "خطایی در حذف محتوا رخ داد.";
        }

        return RedirectToAction("Index", new { subChapterId = content.SubChapterId });
    }
}
