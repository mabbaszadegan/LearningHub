using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.WebApp.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CourseEnrollmentQueries = EduTrack.Application.Features.CourseEnrollment.Queries;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class MyModeController : Controller
{
    private readonly ILogger<MyModeController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;
    private readonly IStudentProfileContext _studentProfileContext;

    public MyModeController(
        ILogger<MyModeController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IStudentProfileContext studentProfileContext)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
        _studentProfileContext = studentProfileContext;
    }

    public async Task<IActionResult> Choose(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue)
        {
            TempData["Error"] = "لطفاً ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.";
            return RedirectToAction("Index", "Profile", new { area = "Student" });
        }

        // Verify course exists and user is enrolled
        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null)
        {
            return NotFound("Course not found");
        }

        // Get current enrollment
        var enrollment = await _mediator.Send(new CourseEnrollmentQueries.GetCourseEnrollmentQuery(courseId, currentUser.Id, activeProfileId));
        var currentMode = enrollment.IsSuccess ? enrollment.Value?.LearningMode : LearningMode.SelfStudy;

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Value.Title;
        ViewBag.CurrentMode = currentMode;
        var activeProfileName = await _studentProfileContext.GetActiveProfileNameAsync();
        ViewBag.ActiveProfileName = activeProfileName ?? string.Empty;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Choose(UpdateLearningModeCommand command)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue)
        {
            TempData["Error"] = "لطفاً ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.";
            return RedirectToAction("Index", "Profile", new { area = "Student" });
        }

        var commandWithContext = command with
        {
            StudentId = currentUser.Id,
            StudentProfileId = activeProfileId
        };

        if (!ModelState.IsValid)
        {
            var course = await _mediator.Send(new GetCourseByIdQuery(command.CourseId));
            var enrollment = await _mediator.Send(new CourseEnrollmentQueries.GetCourseEnrollmentQuery(command.CourseId, currentUser.Id, activeProfileId));
            
            ViewBag.CourseId = command.CourseId;
            ViewBag.CourseTitle = course.Value?.Title ?? "Unknown Course";
            ViewBag.CurrentMode = enrollment.IsSuccess ? enrollment.Value?.LearningMode : LearningMode.SelfStudy;
            return View(commandWithContext);
        }

        var result = await _mediator.Send(commandWithContext);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while updating your learning mode");
            var course = await _mediator.Send(new GetCourseByIdQuery(command.CourseId));
            var enrollment = await _mediator.Send(new CourseEnrollmentQueries.GetCourseEnrollmentQuery(command.CourseId, currentUser.Id, activeProfileId));
            
            ViewBag.CourseId = command.CourseId;
            ViewBag.CourseTitle = course.Value?.Title ?? "Unknown Course";
            ViewBag.CurrentMode = enrollment.IsSuccess ? enrollment.Value?.LearningMode : LearningMode.SelfStudy;
            return View(commandWithContext);
        }

        TempData["Success"] = $"Learning mode updated to {command.LearningMode}";
        return RedirectToAction("Index", "Course", new { area = "Student", courseId = command.CourseId });
    }

    public async Task<IActionResult> Info()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        var enrollments = await _mediator.Send(new CourseEnrollmentQueries.GetStudentCourseEnrollmentsQuery(currentUser.Id, activeProfileId));
        var enrollmentData = new List<object>();

        if (enrollments.IsSuccess)
        {
            foreach (var enrollment in enrollments.Value ?? new List<StudentCourseEnrollmentSummaryDto>())
            {
                var course = await _mediator.Send(new GetCourseByIdQuery(enrollment.CourseId));
                enrollmentData.Add(new
                {
                    CourseId = enrollment.CourseId,
                    CourseTitle = course.IsSuccess ? course.Value?.Title : "Unknown Course",
                    EnrolledAt = enrollment.EnrolledAt,
                    ProgressPercentage = enrollment.ProgressPercentage
                });
            }
        }

        ViewBag.Enrollments = enrollmentData;
        return View();
    }
}
