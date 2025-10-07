using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class ReportingController : Controller
{
    private readonly ILogger<ReportingController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ReportingController(
        ILogger<ReportingController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> GroupProgress(int planId, int? groupId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify teaching plan exists and user has access
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(planId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("Teaching plan not found");
        }

        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to view reports for this teaching plan");
        }

        var groupProgress = await _mediator.Send(new GetGroupProgressQuery(planId, groupId));
        if (!groupProgress.IsSuccess)
        {
            return View(new List<GroupProgressDto>());
        }

        ViewBag.TeachingPlanId = planId;
        ViewBag.TeachingPlanTitle = teachingPlan.Value.Title;
        ViewBag.CourseTitle = teachingPlan.Value.CourseTitle;
        ViewBag.SelectedGroupId = groupId;

        // Get groups for filter dropdown
        var groups = await _mediator.Send(new GetStudentGroupsByTeachingPlanQuery(planId));
        ViewBag.Groups = groups.IsSuccess ? groups.Value : new List<StudentGroupDto>();

        return View(groupProgress.Value);
    }

    public async Task<IActionResult> ExportGroupProgress(int planId, int? groupId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Verify teaching plan exists and user has access
        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(planId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
        {
            return NotFound("Teaching plan not found");
        }

        if (teachingPlan.Value.TeacherId != currentUser.Id)
        {
            return Forbid("You don't have permission to export reports for this teaching plan");
        }

        var groupProgress = await _mediator.Send(new GetGroupProgressQuery(planId, groupId));
        if (!groupProgress.IsSuccess)
        {
            return NotFound("No data to export");
        }

        var csv = GenerateGroupProgressCsv(groupProgress.Value ?? new List<GroupProgressDto>(), teachingPlan.Value ?? new TeachingPlanDto());
        var fileName = $"GroupProgress_{teachingPlan.Value?.Title ?? "Unknown"}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    public async Task<IActionResult> StudentProgress(string studentId, int? courseId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var submissions = await _mediator.Send(new GetSubmissionsByStudentQuery(studentId));
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

        ViewBag.StudentId = studentId;
        ViewBag.CourseId = courseId;
        return View(studentSubmissions);
    }

    public async Task<IActionResult> OverdueSubmissions()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var overdueItems = await _mediator.Send(new GetOverdueScheduleItemsQuery());
        if (!overdueItems.IsSuccess)
        {
            return View(new List<ScheduleItemDto>());
        }

        // Get submissions for overdue items
        var overdueSubmissions = new List<SubmissionDto>();
        foreach (var item in overdueItems.Value ?? new List<ScheduleItemDto>())
        {
            var submissions = await _mediator.Send(new GetSubmissionsByScheduleItemQuery(item.Id));
            if (submissions.IsSuccess)
            {
                // Filter for incomplete submissions
                var incompleteSubmissions = submissions.Value?.Where(s => !s.IsPassing).ToList() ?? new List<SubmissionDto>();
                overdueSubmissions.AddRange(incompleteSubmissions);
            }
        }

        ViewBag.TotalOverdueItems = overdueItems.Value?.Count ?? 0;
        ViewBag.TotalOverdueSubmissions = overdueSubmissions.Count;
        return View(overdueSubmissions);
    }

    private static string GenerateGroupProgressCsv(List<GroupProgressDto> groupProgress, TeachingPlanDto teachingPlan)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine($"Teaching Plan: {teachingPlan.Title}");
        csv.AppendLine($"Course: {teachingPlan.CourseTitle}");
        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        
        // Group summary
        csv.AppendLine("Group Summary");
        csv.AppendLine("Group Name,Total Students,Total Items,Completed Submissions,Overdue Submissions,Completion %,Average Score");
        
        foreach (var group in groupProgress)
        {
            csv.AppendLine($"{group.GroupName},{group.TotalStudents},{group.TotalScheduleItems},{group.CompletedSubmissions},{group.OverdueSubmissions},{group.CompletionPercentage:F1}%,{group.AverageScore:F1}");
        }
        
        csv.AppendLine();
        
        // Student details
        csv.AppendLine("Student Details");
        csv.AppendLine("Group,Student Name,Completed Submissions,Total Submissions,Completion %,Average Score,Overdue Count,Last Activity");
        
        foreach (var group in groupProgress)
        {
            foreach (var student in group.StudentProgress)
            {
                csv.AppendLine($"{group.GroupName},{student.StudentName},{student.CompletedSubmissions},{student.TotalSubmissions},{student.CompletionPercentage:F1}%,{student.AverageScore:F1},{student.OverdueCount},{student.LastActivity:yyyy-MM-dd HH:mm:ss}");
            }
        }
        
        return csv.ToString();
    }
}
