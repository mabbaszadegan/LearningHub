using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class MyModeController : Controller
{
    private readonly ILogger<MyModeController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public MyModeController(
        ILogger<MyModeController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Choose(int courseId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify course exists and user is enrolled
        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null)
        {
            return NotFound("Course not found");
        }

        // Get current enrollment
        var enrollment = await _mediator.Send(new GetCourseEnrollmentByStudentAndCourseQuery(currentUser.Id, courseId));
        var currentMode = enrollment.IsSuccess ? enrollment.Value?.LearningMode : LearningMode.SelfStudy;

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Value.Title;
        ViewBag.CurrentMode = currentMode;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Choose(UpdateLearningModeCommand command)
    {
        if (!ModelState.IsValid)
        {
            var course = await _mediator.Send(new GetCourseByIdQuery(command.CourseId));
            var enrollment = await _mediator.Send(new GetCourseEnrollmentByStudentAndCourseQuery(User.Identity?.Name ?? "", command.CourseId));
            
            ViewBag.CourseId = command.CourseId;
            ViewBag.CourseTitle = course.Value?.Title ?? "Unknown Course";
            ViewBag.CurrentMode = enrollment.IsSuccess ? enrollment.Value?.LearningMode : LearningMode.SelfStudy;
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "An error occurred while updating your learning mode");
            var course = await _mediator.Send(new GetCourseByIdQuery(command.CourseId));
            var enrollment = await _mediator.Send(new GetCourseEnrollmentByStudentAndCourseQuery(User.Identity?.Name ?? "", command.CourseId));
            
            ViewBag.CourseId = command.CourseId;
            ViewBag.CourseTitle = course.Value?.Title ?? "Unknown Course";
            ViewBag.CurrentMode = enrollment.IsSuccess ? enrollment.Value?.LearningMode : LearningMode.SelfStudy;
            return View(command);
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

        // Get user's course enrollments with learning modes
        var enrollments = await _mediator.Send(new GetStudentCourseEnrollmentsQuery(currentUser.Id));
        var enrollmentData = new List<object>();

        if (enrollments.IsSuccess)
        {
            foreach (var enrollment in enrollments.Value ?? new List<CourseEnrollmentDto>())
            {
                var course = await _mediator.Send(new GetCourseByIdQuery(enrollment.CourseId));
                enrollmentData.Add(new
                {
                    CourseId = enrollment.CourseId,
                    CourseTitle = course.IsSuccess ? course.Value?.Title : "Unknown Course",
                    LearningMode = enrollment.LearningMode,
                    EnrolledAt = enrollment.EnrolledAt,
                    ProgressPercentage = enrollment.ProgressPercentage
                });
            }
        }

        ViewBag.Enrollments = enrollmentData;
        return View();
    }
}
