using EduTrack.Application.Features.Chapters.Commands;
using EduTrack.Application.Features.Chapters.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class SubChaptersController : BaseTeacherController
{
    private readonly ILogger<SubChaptersController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public SubChaptersController(
        ILogger<SubChaptersController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IPageTitleSectionService pageTitleSectionService) : base(pageTitleSectionService)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int chapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify chapter and course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(chapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapter.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return NotFound();
        }

        var subChaptersResult = await _mediator.Send(new GetSubChaptersByChapterIdQuery(chapterId));
        if (!subChaptersResult.IsSuccess)
        {
            return NotFound();
        }

        ViewBag.ChapterId = chapterId;
        ViewBag.ChapterTitle = chapter.Title;
        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        // Setup page title section
        await SetPageTitleSectionAsync(PageType.SubChaptersIndex, chapterId);

        return View(subChaptersResult.Value);
    }

    public async Task<IActionResult> Create(int chapterId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify chapter and course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(chapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;
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
    public async Task<IActionResult> Create(CreateSubChapterCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify chapter and course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(command.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;
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
                return RedirectToAction("Index", new { chapterId = command.ChapterId });
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

    public async Task<IActionResult> Edit(int id)
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
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapter.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;
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
    public async Task<IActionResult> Edit(UpdateSubChapterCommand command)
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
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapter.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return NotFound();
        }

        var chapter = chapterResult.Value;
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
                TempData["SuccessMessage"] = "زیرمبحث با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Index", new { chapterId = subChapter.ChapterId });
            }
            else
            {
                ModelState.AddModelError("", result.Error ?? "خطایی در به‌روزرسانی زیرمبحث رخ داد.");
            }
        }

        ViewBag.ChapterId = subChapter.ChapterId;
        ViewBag.ChapterTitle = chapter.Title;
        ViewBag.CourseId = chapter.CourseId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(command);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(id));
            if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
            {
                return Json(new { success = false, error = "زیرمبحث یافت نشد" });
            }

            var subChapter = subChapterResult.Value;
            var updateCommand = new UpdateSubChapterCommand(
                subChapter.Id,
                subChapter.Title,
                subChapter.Description,
                subChapter.Objective,
                !subChapter.IsActive,
                subChapter.Order
            );

            var result = await _mediator.Send(updateCommand);
            if (result.IsSuccess)
            {
                return Json(new { success = true, isActive = !subChapter.IsActive });
            }

            return Json(new { success = false, error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling subchapter active status");
            return Json(new { success = false, error = "خطا در تغییر وضعیت" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReorderSubChapters([FromBody] List<int> subChapterIds)
    {
        try
        {
            if (subChapterIds == null || !subChapterIds.Any())
            {
                return Json(new { success = false, error = "لیست زیرمباحث خالی است" });
            }

            foreach (var subChapterId in subChapterIds)
            {
                var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(subChapterId));
                if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
                {
                    continue;
                }

                var subChapter = subChapterResult.Value;
                var newOrder = subChapterIds.IndexOf(subChapterId);

                var updateCommand = new UpdateSubChapterCommand(
                    subChapter.Id,
                    subChapter.Title,
                    subChapter.Description,
                    subChapter.Objective,
                    subChapter.IsActive,
                    newOrder
                );

                await _mediator.Send(updateCommand);
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering subchapters");
            return Json(new { success = false, error = "خطا در تغییر ترتیب" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MoveSubChapter(int id, string direction)
    {
        try
        {
            var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(id));
            if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
            {
                return Json(new { success = false, error = "زیرمبحث یافت نشد" });
            }

            var currentSubChapter = subChapterResult.Value;

            // Get all subchapters for the same chapter
            var allSubChaptersResult = await _mediator.Send(new GetSubChaptersByChapterIdQuery(currentSubChapter.ChapterId));
            if (!allSubChaptersResult.IsSuccess || allSubChaptersResult.Value == null)
            {
                return Json(new { success = false, error = "خطا در دریافت زیرمباحث" });
            }

            var allSubChapters = allSubChaptersResult.Value.OrderBy(c => c.Order).ToList();
            var currentIndex = allSubChapters.FindIndex(c => c.Id == id);

            if (currentIndex == -1)
            {
                return Json(new { success = false, error = "زیرمبحث یافت نشد" });
            }

            var newIndex = direction == "up" ? currentIndex - 1 : currentIndex + 1;

            if (newIndex < 0 || newIndex >= allSubChapters.Count)
            {
                return Json(new { success = false, error = "امکان جابجایی وجود ندارد" });
            }

            // Swap orders
            var otherSubChapter = allSubChapters[newIndex];
            var tempOrder = currentSubChapter.Order;

            var updateCurrent = new UpdateSubChapterCommand(
                currentSubChapter.Id,
                currentSubChapter.Title,
                currentSubChapter.Description,
                currentSubChapter.Objective,
                currentSubChapter.IsActive,
                otherSubChapter.Order
            );

            var updateOther = new UpdateSubChapterCommand(
                otherSubChapter.Id,
                otherSubChapter.Title,
                otherSubChapter.Description,
                otherSubChapter.Objective,
                otherSubChapter.IsActive,
                tempOrder
            );

            await _mediator.Send(updateCurrent);
            await _mediator.Send(updateOther);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving subchapter");
            return Json(new { success = false, error = "خطا در جابجایی" });
        }
    }
}
