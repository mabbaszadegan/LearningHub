using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.CourseEnrollment.Queries;
using EduTrack.Application.Common.Models.Exams;
using EduTrack.Application.Common.Models.Statistics;
using EduTrack.Application.Features.Statistics.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.WebApp.Areas.Student.Models;
using EduTrack.WebApp.Services;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class StatisticsController : Controller
{
    private readonly ILogger<StatisticsController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;
    private readonly IStudentProfileContext _studentProfileContext;

    public StatisticsController(
        ILogger<StatisticsController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IStudentProfileContext studentProfileContext)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
        _studentProfileContext = studentProfileContext;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();

        // Get progress statistics
        var progressResult = await _mediator.Send(new GetProgressByStudentQuery(currentUser.Id, 1, 100));
        var progressData = progressResult.Items?.ToList() ?? new List<ProgressDto>();

        // Get student statistics
        var studentStatsResult = await _mediator.Send(new GetStudentStatsQuery(currentUser.Id));

        // Get enrolled courses
        var activeProfileName = await _studentProfileContext.GetActiveProfileNameAsync();
        var enrolledCoursesResult = await _mediator.Send(new GetStudentCourseEnrollmentsQuery(currentUser.Id, activeProfileId));
        var enrolledCourses = enrolledCoursesResult.IsSuccess && enrolledCoursesResult.Value != null 
            ? enrolledCoursesResult.Value 
            : new List<Application.Features.CourseEnrollment.DTOs.StudentCourseEnrollmentSummaryDto>();

        var learningStatsResult = await _mediator.Send(new GetStudentLearningStatisticsQuery(currentUser.Id, activeProfileId));
        LearningStatisticsDto learningStatistics;

        if (learningStatsResult.IsSuccess && learningStatsResult.Value != null)
        {
            learningStatistics = learningStatsResult.Value;
        }
        else
        {
            _logger.LogWarning("Failed to load learning statistics for student {StudentId}: {Error}", currentUser.Id, learningStatsResult.Error);
            learningStatistics = new LearningStatisticsDto();
        }

        // Calculate statistics
        var totalProgress = progressData.Count;
        var completedProgress = progressData.Count(p => p.Status == Domain.Enums.ProgressStatus.Done);
        var inProgress = progressData.Count(p => p.Status == Domain.Enums.ProgressStatus.InProgress);
        var notStarted = progressData.Count(p => p.Status == Domain.Enums.ProgressStatus.NotStarted);
        var completionPercentage = totalProgress > 0 ? (double)completedProgress / totalProgress * 100 : 0;

        var viewModel = new StudentStatisticsViewModel
        {
            StudentName = currentUser.FullName,
            StudentFirstName = currentUser.FirstName,
            ActiveStudentProfileId = activeProfileId,
            ActiveStudentProfileName = activeProfileName,
            StudentStatistics = studentStatsResult.IsSuccess ? studentStatsResult.Value : null,
            EnrolledCourses = enrolledCourses,
            ProgressStats = new ProgressOverviewViewModel
            {
                Total = totalProgress,
                Completed = completedProgress,
                InProgress = inProgress,
                NotStarted = notStarted,
                CompletionPercentage = completionPercentage
            },
            LearningStatistics = learningStatistics
        };

        return View(viewModel);
    }
}

