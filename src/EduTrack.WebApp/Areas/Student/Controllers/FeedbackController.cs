using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Application.Common.Models.TeachingPlans;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class FeedbackController : Controller
{
    private readonly ILogger<FeedbackController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public FeedbackController(
        ILogger<FeedbackController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> MySubmissions(int? courseId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var submissions = await _mediator.Send(new GetSubmissionsByStudentQuery(currentUser.Id));
        if (!submissions.IsSuccess)
        {
            return View(new List<SubmissionDto>());
        }

        var studentSubmissions = submissions.Value?.ToList() ?? new List<SubmissionDto>();

        // Filter by course if specified
        if (courseId.HasValue)
        {
            // This would need to be enhanced to filter by course
            // For now, we'll show all submissions
        }

        // Group by status
        var gradedSubmissions = studentSubmissions.Where(s => s.IsPassing).ToList();
        var pendingSubmissions = studentSubmissions.Where(s => s.Status == Domain.Enums.SubmissionStatus.Submitted || s.Status == Domain.Enums.SubmissionStatus.Reviewed).ToList();
        var inProgressSubmissions = studentSubmissions.Where(s => s.Status == Domain.Enums.SubmissionStatus.InProgress).ToList();

        ViewBag.CourseId = courseId;
        ViewBag.GradedSubmissions = gradedSubmissions;
        ViewBag.PendingSubmissions = pendingSubmissions;
        ViewBag.InProgressSubmissions = inProgressSubmissions;
        ViewBag.TotalSubmissions = studentSubmissions.Count;
        ViewBag.AverageScore = studentSubmissions.Where(s => s.PercentageScore.HasValue).Any() 
            ? studentSubmissions.Where(s => s.PercentageScore.HasValue).Average(s => s.PercentageScore!.Value) 
            : 0;

        return View(studentSubmissions);
    }

    public async Task<IActionResult> SubmissionDetails(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var submission = await _mediator.Send(new GetSubmissionByIdQuery(id));
        if (!submission.IsSuccess || submission.Value == null)
        {
            return NotFound("Submission not found");
        }

        // Verify the submission belongs to the current user
        if (submission.Value.StudentId != currentUser.Id)
        {
            return Forbid("You don't have permission to view this submission");
        }

        return View(submission.Value);
    }

    public async Task<IActionResult> Progress(int? courseId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var submissions = await _mediator.Send(new GetSubmissionsByStudentQuery(currentUser.Id));
        if (!submissions.IsSuccess)
        {
            return View(new List<SubmissionDto>());
        }

        var studentSubmissions = submissions.Value?.ToList() ?? new List<SubmissionDto>();

        // Filter by course if specified
        if (courseId.HasValue)
        {
            // This would need to be enhanced to filter by course
        }

        // Calculate progress statistics
        var totalSubmissions = studentSubmissions.Count;
        var completedSubmissions = studentSubmissions.Count(s => s.IsPassing);
        var pendingSubmissions = studentSubmissions.Count(s => s.Status == Domain.Enums.SubmissionStatus.Submitted || s.Status == Domain.Enums.SubmissionStatus.Reviewed);
        var overdueSubmissions = studentSubmissions.Count(s => s.IsOverdue); // This would need to be calculated

        var averageScore = studentSubmissions.Where(s => s.PercentageScore.HasValue).Any() 
            ? studentSubmissions.Where(s => s.PercentageScore.HasValue).Average(s => s.PercentageScore!.Value) 
            : 0;

        var completionPercentage = totalSubmissions > 0 ? (double)completedSubmissions / totalSubmissions * 100 : 0;

        // Group by course for detailed view
        var submissionsByCourse = studentSubmissions
            .GroupBy(s => s.ScheduleItemTitle) // This would need to be enhanced to group by actual course
            .Select(g => new
            {
                CourseName = g.Key,
                TotalItems = g.Count(),
                CompletedItems = g.Count(s => s.IsPassing),
                AverageScore = g.Where(s => s.PercentageScore.HasValue).Any() 
                    ? g.Where(s => s.PercentageScore.HasValue).Average(s => s.PercentageScore!.Value) 
                    : 0,
                Submissions = g.ToList()
            })
            .ToList();

        ViewBag.CourseId = courseId;
        ViewBag.TotalSubmissions = totalSubmissions;
        ViewBag.CompletedSubmissions = completedSubmissions;
        ViewBag.PendingSubmissions = pendingSubmissions;
        ViewBag.OverdueSubmissions = overdueSubmissions;
        ViewBag.AverageScore = averageScore;
        ViewBag.CompletionPercentage = completionPercentage;
        ViewBag.SubmissionsByCourse = submissionsByCourse;

        return View(studentSubmissions);
    }

    public async Task<IActionResult> DownloadFeedback(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var submission = await _mediator.Send(new GetSubmissionByIdQuery(id));
        if (!submission.IsSuccess || submission.Value == null)
        {
            return NotFound("Submission not found");
        }

        // Verify the submission belongs to the current user
        if (submission.Value.StudentId != currentUser.Id)
        {
            return Forbid("You don't have permission to download this feedback");
        }

        // Generate feedback document
        var feedbackContent = GenerateFeedbackDocument(submission.Value);
        var fileName = $"Feedback_{submission.Value.ScheduleItemTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        
        return File(System.Text.Encoding.UTF8.GetBytes(feedbackContent), "text/plain", fileName);
    }

    private static string GenerateFeedbackDocument(SubmissionDto submission)
    {
        var document = new System.Text.StringBuilder();
        
        document.AppendLine($"Assignment: {submission.ScheduleItemTitle}");
        document.AppendLine($"Student: {submission.StudentName}");
        document.AppendLine($"Submitted: {submission.SubmittedAt:yyyy-MM-dd HH:mm:ss}");
        document.AppendLine($"Status: {submission.Status}");
        
        if (submission.Grade.HasValue)
        {
            document.AppendLine($"Grade: {submission.Grade:F1}");
        }
        
        if (submission.PercentageScore.HasValue)
        {
            document.AppendLine($"Score: {submission.PercentageScore:F1}%");
        }
        
        document.AppendLine($"Passing: {(submission.IsPassing ? "Yes" : "No")}");
        document.AppendLine();
        
        if (!string.IsNullOrEmpty(submission.FeedbackText))
        {
            document.AppendLine("Teacher Feedback:");
            document.AppendLine(submission.FeedbackText);
            document.AppendLine();
        }
        
        if (!string.IsNullOrEmpty(submission.TeacherName))
        {
            document.AppendLine($"Graded by: {submission.TeacherName}");
        }
        
        if (submission.ReviewedAt.HasValue)
        {
            document.AppendLine($"Reviewed: {submission.ReviewedAt:yyyy-MM-dd HH:mm:ss}");
        }
        
        return document.ToString();
    }
}
