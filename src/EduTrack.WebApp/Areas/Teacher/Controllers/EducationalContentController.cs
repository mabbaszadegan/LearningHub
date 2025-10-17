using EduTrack.Application.Features.EducationalContent.Commands;
using EduTrack.Application.Features.EducationalContent.Queries;
using EduTrack.Application.Features.Chapters.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class EducationalContentController : BaseTeacherController
{
    private readonly ILogger<EducationalContentController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public EducationalContentController(
        ILogger<EducationalContentController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IPageTitleSectionService pageTitleSectionService) : base(pageTitleSectionService)
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
        ViewBag.ChapterId = subChapterResult.Value.ChapterId;
        ViewBag.CourseTitle = courseResult.Value?.Title;
        ViewBag.CourseId = chapterResult.Value.CourseId;

        // Setup page title section
        await SetPageTitleSectionAsync(PageType.EducationalContentIndex, subChapterId);

        return View(contentsResult.Value);
    }


    public async Task<IActionResult> Create(int subChapterId, int id)
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

        // If id is provided, load existing content for editing
        if (id > 0)
        {
            var contentResult = await _mediator.Send(new GetEducationalContentByIdQuery(id));
            if (!contentResult.IsSuccess || contentResult.Value == null)
            {
                return NotFound();
            }

            var content = contentResult.Value;

            // Verify content belongs to the subchapter
            if (content.SubChapterId != subChapterId)
            {
                return NotFound();
            }

            ViewBag.SubChapterId = subChapterId;
            ViewBag.SubChapterTitle = subChapterResult.Value.Title;
            ViewBag.ChapterTitle = chapterResult.Value.Title;
            ViewBag.ChapterId = subChapterResult.Value.ChapterId;
            ViewBag.CourseTitle = courseResult.Value?.Title;

            var command = new CreateEducationalContentCommand(
                content.SubChapterId,
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
            ViewBag.IsEdit = true;
            ViewBag.ContentId = content.Id;

            return View("Edit", command);
        }
        else
        {
            // Create new content
            ViewBag.SubChapterId = subChapterId;
            ViewBag.SubChapterTitle = subChapterResult.Value.Title;
            ViewBag.ChapterTitle = chapterResult.Value.Title;
            ViewBag.ChapterId = subChapterResult.Value.ChapterId;
            ViewBag.CourseTitle = courseResult.Value?.Title;

            return View(new CreateEducationalContentCommand(subChapterId, string.Empty, string.Empty, EduTrack.Domain.Enums.EducationalContentType.Text, string.Empty, null, string.Empty, true, 0));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEducationalContentCommand command, int subChapterId, int id)
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

        if (ModelState.IsValid)
        {
            if (id > 0)
            {
                // Update existing content
                var updateCommand = new UpdateEducationalContentCommand(
                    id,
                    command.Title,
                    command.Description,
                    command.Type,
                    command.TextContent,
                    command.File,
                    command.ExternalUrl,
                    command.IsActive,
                    command.Order);

                var result = await _mediator.Send(updateCommand);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "محتوا با موفقیت ویرایش شد.";
                    return RedirectToAction("Index", new { subChapterId = subChapterId });
                }
                else
                {
                    ModelState.AddModelError("", result.Error ?? "خطایی در ویرایش محتوا رخ داد.");
                }
            }
            else
            {
                // Create new content
                var result = await _mediator.Send(command);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "محتوا با موفقیت ایجاد شد.";
                    return RedirectToAction("Index", new { subChapterId = subChapterId });
                }
                else
                {
                    ModelState.AddModelError("", result.Error ?? "خطایی در ایجاد محتوا رخ داد.");
                }
            }
        }

        ViewBag.SubChapterId = subChapterId;
        ViewBag.SubChapterTitle = subChapterResult.Value.Title;
        ViewBag.ChapterTitle = chapterResult.Value.Title;
        ViewBag.ChapterId = subChapterResult.Value.ChapterId;
        ViewBag.CourseTitle = courseResult.Value?.Title;

        return View(id > 0 ? "Edit" : "Create", command);
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
        ViewBag.ChapterId = subChapterResult.Value.ChapterId;
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
        ViewBag.ChapterId = subChapterResult.Value.ChapterId;
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

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // Get content to verify ownership
        var contentResult = await _mediator.Send(new GetEducationalContentByIdQuery(id));
        if (!contentResult.IsSuccess || contentResult.Value == null)
        {
            return Json(new { success = false, error = "محتوا یافت نشد" });
        }

        var content = contentResult.Value;

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(content.SubChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return Json(new { success = false, error = "زیرمبحث یافت نشد" });
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return Json(new { success = false, error = "مبحث یافت نشد" });
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return Json(new { success = false, error = "دسترسی غیرمجاز" });
        }

        var result = await _mediator.Send(new ToggleEducationalContentActiveCommand(id));
        if (result.IsSuccess)
        {
            return Json(new { success = true, isActive = result.Value });
        }
        else
        {
            return Json(new { success = false, error = result.Error ?? "خطا در تغییر وضعیت" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReorderContents([FromBody] List<int> contentIds)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        if (contentIds == null || !contentIds.Any())
        {
            return Json(new { success = false, error = "لیست محتوا خالی است" });
        }

        // Verify ownership of first content to ensure teacher has access
        var firstContentResult = await _mediator.Send(new GetEducationalContentByIdQuery(contentIds.First()));
        if (!firstContentResult.IsSuccess || firstContentResult.Value == null)
        {
            return Json(new { success = false, error = "محتوا یافت نشد" });
        }

        var firstContent = firstContentResult.Value;

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(firstContent.SubChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return Json(new { success = false, error = "زیرمبحث یافت نشد" });
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return Json(new { success = false, error = "مبحث یافت نشد" });
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return Json(new { success = false, error = "دسترسی غیرمجاز" });
        }

        var result = await _mediator.Send(new ReorderEducationalContentsCommand { ContentIds = contentIds });
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false, error = result.Error ?? "خطا در تغییر ترتیب" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MoveContent(int id, string direction)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // Get content to verify ownership
        var contentResult = await _mediator.Send(new GetEducationalContentByIdQuery(id));
        if (!contentResult.IsSuccess || contentResult.Value == null)
        {
            return Json(new { success = false, error = "محتوا یافت نشد" });
        }

        var content = contentResult.Value;

        // Verify subchapter and ownership
        var subChapterResult = await _mediator.Send(new GetSubChapterByIdQuery(content.SubChapterId));
        if (!subChapterResult.IsSuccess || subChapterResult.Value == null)
        {
            return Json(new { success = false, error = "زیرمبحث یافت نشد" });
        }

        // Get chapter to verify course ownership
        var chapterResult = await _mediator.Send(new GetChapterByIdQuery(subChapterResult.Value.ChapterId));
        if (!chapterResult.IsSuccess || chapterResult.Value == null)
        {
            return Json(new { success = false, error = "مبحث یافت نشد" });
        }

        // Verify course belongs to teacher
        var courseResult = await _mediator.Send(new GetCourseByIdQuery(chapterResult.Value.CourseId));
        if (!courseResult.IsSuccess || courseResult.Value?.CreatedBy != currentUser.Id)
        {
            return Json(new { success = false, error = "دسترسی غیرمجاز" });
        }

        // Get all contents in the same subchapter
        var allContentsResult = await _mediator.Send(new GetEducationalContentsBySubChapterIdQuery(content.SubChapterId));
        if (!allContentsResult.IsSuccess || allContentsResult.Value == null)
        {
            return Json(new { success = false, error = "خطا در دریافت لیست محتوا" });
        }

        var allContents = allContentsResult.Value.OrderBy(c => c.Order).ToList();
        var currentIndex = allContents.FindIndex(c => c.Id == id);

        if (currentIndex == -1)
        {
            return Json(new { success = false, error = "محتوا در لیست یافت نشد" });
        }

        int newIndex;
        if (direction == "up" && currentIndex > 0)
        {
            newIndex = currentIndex - 1;
        }
        else if (direction == "down" && currentIndex < allContents.Count - 1)
        {
            newIndex = currentIndex + 1;
        }
        else
        {
            return Json(new { success = false, error = "امکان جابجایی وجود ندارد" });
        }

        // Swap orders
        var currentContent = allContents[currentIndex];
        var targetContent = allContents[newIndex];

        var contentIds = allContents.Select(c => c.Id).ToList();
        contentIds[currentIndex] = targetContent.Id;
        contentIds[newIndex] = currentContent.Id;

        var result = await _mediator.Send(new ReorderEducationalContentsCommand { ContentIds = contentIds });
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false, error = result.Error ?? "خطا در جابجایی" });
        }
    }
}
