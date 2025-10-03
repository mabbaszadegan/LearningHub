using EduTrack.Application.Features.Chapters.Commands;
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
public class ChaptersController : Controller
{
    private readonly ILogger<ChaptersController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ChaptersController(
        ILogger<ChaptersController> logger, 
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        var chaptersResult = await _mediator.Send(new GetChaptersByCourseIdQuery(courseId));
        if (!chaptersResult.IsSuccess)
        {
            return NotFound();
        }

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;
        
        return View(chaptersResult.Value);
    }

    public async Task<IActionResult> Create(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(new CreateChapterCommand(courseId, string.Empty, string.Empty, string.Empty, 0));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateChapterCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(command.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "مبحث با موفقیت ایجاد شد.";
                return RedirectToAction("Index", new { courseId = command.CourseId });
            }
            else
            {
                ModelState.AddModelError("", result.Error ?? "خطایی در ایجاد مبحث رخ داد.");
            }
        }

        ViewBag.CourseId = command.CourseId;
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

        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(id));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        var command = new UpdateChapterCommand(
            chapter.Id,
            chapter.Title,
            chapter.Description,
            chapter.Objective,
            chapter.IsActive,
            chapter.Order);

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateChapterCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get chapter to verify ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(command.Id));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "مبحث با موفقیت ویرایش شد.";
                return RedirectToAction("Index", new { courseId = chapter.CourseId });
            }
            else
            {
                ModelState.AddModelError("", result.Error ?? "خطایی در ویرایش مبحث رخ داد.");
            }
        }

        ViewBag.CourseId = chapter.CourseId;
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

        // Get chapter to verify ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(id));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        var result = await _mediator.Send(new DeleteChapterCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "مبحث با موفقیت حذف شد.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error ?? "خطایی در حذف مبحث رخ داد.";
        }

        return RedirectToAction("Index", new { courseId = chapter.CourseId });
    }

    // SubChapter Actions
    public async Task<IActionResult> CreateSubChapter(int chapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(chapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        ViewBag.ChapterId = chapterId;
        ViewBag.ChapterTitle = chapter.Title;
        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(new CreateSubChapterCommand(chapterId, string.Empty, string.Empty, string.Empty, 0));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubChapter(CreateSubChapterCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(command.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "زیرمبحث با موفقیت ایجاد شد.";
                return RedirectToAction("Index", new { courseId = chapter.CourseId });
            }
            else
            {
                ModelState.AddModelError("", result.Error ?? "خطایی در ایجاد زیرمبحث رخ داد.");
            }
        }

        ViewBag.ChapterId = command.ChapterId;
        ViewBag.ChapterTitle = chapter.Title;
        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(command);
    }

    public async Task<IActionResult> EditSubChapter(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(id));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        var subChapter = subChapterResult.Value;

        // Get chapter and verify ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapter.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        ViewBag.ChapterId = subChapter.ChapterId;
        ViewBag.ChapterTitle = chapter.Title;
        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        var command = new UpdateSubChapterCommand(
            subChapter.Id,
            subChapter.Title,
            subChapter.Description,
            subChapter.Objective,
            subChapter.IsActive,
            subChapter.Order);

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubChapter(UpdateSubChapterCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get subchapter to verify ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(command.Id));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        var subChapter = subChapterResult.Value;

        // Get chapter and verify ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapter.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "زیرمبحث با موفقیت ویرایش شد.";
                return RedirectToAction("Index", new { courseId = chapter.CourseId });
            }
            else
            {
                ModelState.AddModelError("", result.Error ?? "خطایی در ویرایش زیرمبحث رخ داد.");
            }
        }

        ViewBag.ChapterId = subChapter.ChapterId;
        ViewBag.ChapterTitle = chapter.Title;
        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubChapter(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get subchapter to verify ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(id));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return NotFound();
        }

        var subChapter = subChapterResult.Value;

        // Get chapter and verify ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapter.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        var result = await _mediator.Send(new DeleteSubChapterCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "زیرمبحث با موفقیت حذف شد.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error ?? "خطایی در حذف زیرمبحث رخ داد.";
        }

        return RedirectToAction("Index", new { courseId = chapter.CourseId });
    }

    // SubChapter Management Page
    public async Task<IActionResult> ManageSubChapters(int chapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(chapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        // Get subchapters for this chapter
        var subChaptersResult = await _mediator.Send(new GetSubChaptersByChapterIdQuery(chapterId));
        if (!subChaptersResult.IsSuccess)
        {
            return NotFound();
        }

        ViewBag.ChapterId = chapterId;
        ViewBag.ChapterTitle = chapter.Title;
        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(subChaptersResult.Value);
    }

    // API endpoint to get chapter info from subChapterId
    [HttpGet]
    public async Task<IActionResult> GetChapterInfoBySubChapter(int subChapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        try
        {
            var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(subChapterId));
            if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
            {
                return Json(new { success = false, error = "زیرمبحث یافت نشد" });
            }

            var subChapter = subChapterResult.Value;
            
            // Get chapter info
            var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapter.ChapterId));
            if (!chapterResult.IsSuccess || chapterResult.Value == null)
            {
                return Json(new { success = false, error = "مبحث یافت نشد" });
            }

            var chapter = chapterResult.Value;

            // Verify ownership
            var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
            if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
            {
                return Json(new { success = false, error = "دسترسی غیرمجاز" });
            }

            return Json(new { 
                success = true, 
                data = new {
                    chapterId = chapter.Id,
                    courseId = chapter.CourseId,
                    chapterTitle = chapter.Title,
                    courseTitle = courseResult.Value?.Title
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chapter info for subChapter {SubChapterId}", subChapterId);
            return Json(new { success = false, error = "خطا در دریافت اطلاعات" });
        }
    }

    // API endpoint to get course info from chapterId
    [HttpGet]
    public async Task<IActionResult> GetCourseInfoByChapter(int chapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        try
        {
            var chapterResult = await _mediator.Send(new GetChapterByIdQuery(chapterId));
            if (!chapterResult.IsSuccess || chapterResult.Value == null)
            {
                return Json(new { success = false, error = "مبحث یافت نشد" });
            }

            var chapter = chapterResult.Value;

            // Get course info
            var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
            if (!courseResult.IsSuccess || courseResult.Value == null)
            {
                return Json(new { success = false, error = "دوره یافت نشد" });
            }

            var course = courseResult.Value;

            // Verify ownership
            if (course.CreatedBy != currentUser.Id)
            {
                return Json(new { success = false, error = "دسترسی غیرمجاز" });
            }

            return Json(new { 
                success = true, 
                data = new {
                    courseId = course.Id,
                    courseTitle = course.Title,
                    chapterTitle = chapter.Title
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course info for chapter {ChapterId}", chapterId);
            return Json(new { success = false, error = "خطا در دریافت اطلاعات" });
        }
    }
}
