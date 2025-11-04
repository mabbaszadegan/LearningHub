using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.CourseEnrollment.Queries;
using EduTrack.Application.Common.Models.Exams;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class StatisticsController : Controller
{
    private readonly ILogger<StatisticsController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public StatisticsController(
        ILogger<StatisticsController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get progress statistics
        var progressResult = await _mediator.Send(new GetProgressByStudentQuery(currentUser.Id, 1, 100));
        var progressData = progressResult.Items?.ToList() ?? new List<ProgressDto>();

        // Get student statistics
        var studentStatsResult = await _mediator.Send(new GetStudentStatsQuery(currentUser.Id));
        
        // Get enrolled courses
        var enrolledCoursesResult = await _mediator.Send(new GetStudentCourseEnrollmentsQuery(currentUser.Id));
        var enrolledCourses = enrolledCoursesResult.IsSuccess && enrolledCoursesResult.Value != null 
            ? enrolledCoursesResult.Value 
            : new List<Application.Features.CourseEnrollment.DTOs.StudentCourseEnrollmentSummaryDto>();

        // Calculate statistics
        var totalProgress = progressData.Count;
        var completedProgress = progressData.Count(p => p.Status == Domain.Enums.ProgressStatus.Done);
        var inProgress = progressData.Count(p => p.Status == Domain.Enums.ProgressStatus.InProgress);
        var notStarted = progressData.Count(p => p.Status == Domain.Enums.ProgressStatus.NotStarted);
        var completionPercentage = totalProgress > 0 ? (double)completedProgress / totalProgress * 100 : 0;

        var viewModel = new
        {
            StudentName = currentUser.FullName,
            StudentFirstName = currentUser.FirstName,
            ProgressData = progressData,
            StudentStatistics = studentStatsResult.IsSuccess ? studentStatsResult.Value : null,
            EnrolledCourses = enrolledCourses,
            ProgressStats = new
            {
                Total = totalProgress,
                Completed = completedProgress,
                InProgress = inProgress,
                NotStarted = notStarted,
                CompletionPercentage = completionPercentage
            }
        };

        return View(viewModel);
    }
}

