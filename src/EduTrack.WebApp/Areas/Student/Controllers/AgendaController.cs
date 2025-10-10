using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EduTrack.Application.Common.Models.TeachingPlans;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class AgendaController : Controller
{
    private readonly ILogger<AgendaController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public AgendaController(
        ILogger<AgendaController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<IActionResult> MyAgenda(int? courseId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var agenda = await _mediator.Send(new GetStudentAgendaQuery(currentUser.Id, courseId));
        if (!agenda.IsSuccess)
        {
            return View(new StudentAgendaDto());
        }

        ViewBag.CourseId = courseId;
        return View(agenda.Value);
    }

    public async Task<IActionResult> Item(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get schedule item
        var scheduleItem = await _mediator.Send(new GetScheduleItemByIdQuery(id));
        if (!scheduleItem.IsSuccess || scheduleItem.Value == null)
        {
            return NotFound("Schedule item not found");
        }

        // Get or create submission
        var submission = await _mediator.Send(new GetSubmissionByStudentAndItemQuery(currentUser.Id, id));
        if (!submission.IsSuccess)
        {
            // Create a new submission if it doesn't exist
            var newSubmission = Submission.Create(id, currentUser.Id, "{}");
            // This would need to be saved to the database
        }

        ViewBag.ScheduleItem = scheduleItem.Value;
        ViewBag.Submission = submission.IsSuccess ? submission.Value : null;
        ViewBag.ContentData = ParseContentJson(scheduleItem.Value.ContentJson, scheduleItem.Value.Type);
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitWorkCommand command)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Item), new { id = command.ScheduleItemId });
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "An error occurred while submitting your work";
        }
        else
        {
            TempData["Success"] = "Your work has been submitted successfully";
        }

        return RedirectToAction(nameof(Item), new { id = command.ScheduleItemId });
    }

    public async Task<IActionResult> Start(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get or create submission and start it
        var submission = await _mediator.Send(new GetSubmissionByStudentAndItemQuery(currentUser.Id, id));
        if (!submission.IsSuccess)
        {
            // Create new submission
            var newSubmission = Submission.Create(id, currentUser.Id, "{}");
            // This would need to be saved and started
        }
        else
        {
            // Start existing submission
            var updateResult = await _mediator.Send(new UpdateSubmissionStatusCommand(submission.Value?.Id ?? 0, SubmissionStatus.InProgress));
            if (!updateResult.IsSuccess)
            {
                TempData["Error"] = "Failed to start the assignment";
            }
        }

        return RedirectToAction(nameof(Item), new { id });
    }

    private static object? ParseContentJson(string contentJson, ScheduleItemType type)
    {
        try
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            
            return type switch
            {
                ScheduleItemType.Reminder => JsonConvert.DeserializeObject<ReminderContent>(contentJson, jsonSettings),
                ScheduleItemType.Writing => JsonConvert.DeserializeObject<WritingContent>(contentJson, jsonSettings),
                ScheduleItemType.Audio => JsonConvert.DeserializeObject<AudioContent>(contentJson, jsonSettings),
                ScheduleItemType.GapFill => JsonConvert.DeserializeObject<GapFillContent>(contentJson, jsonSettings),
                ScheduleItemType.MultipleChoice => JsonConvert.DeserializeObject<MultipleChoiceContent>(contentJson, jsonSettings),
                ScheduleItemType.Match => JsonConvert.DeserializeObject<MatchContent>(contentJson, jsonSettings),
                ScheduleItemType.ErrorFinding => JsonConvert.DeserializeObject<ErrorFindingContent>(contentJson, jsonSettings),
                ScheduleItemType.CodeExercise => JsonConvert.DeserializeObject<CodeExerciseContent>(contentJson, jsonSettings),
                ScheduleItemType.Quiz => JsonConvert.DeserializeObject<QuizContent>(contentJson, jsonSettings),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}

// Content type classes for JSON deserialization
public class ReminderContent
{
    public string Text { get; set; } = string.Empty;
    public List<LinkItem> Links { get; set; } = new();
}

public class WritingContent
{
    public string Prompt { get; set; } = string.Empty;
    public int MaxWords { get; set; }
    public string Rubric { get; set; } = string.Empty;
}

public class AudioContent
{
    public string Mode { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public List<AudioQuestion> Questions { get; set; } = new();
}

public class GapFillContent
{
    public string Text { get; set; } = string.Empty;
    public List<string> Answers { get; set; } = new();
}

public class MultipleChoiceContent
{
    public string Stem { get; set; } = string.Empty;
    public List<ChoiceItem> Choices { get; set; } = new();
}

public class MatchContent
{
    public List<MatchPair> Pairs { get; set; } = new();
}

public class ErrorFindingContent
{
    public string Text { get; set; } = string.Empty;
    public string Expected { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class CodeExerciseContent
{
    public string Language { get; set; } = string.Empty;
    public string StarterCode { get; set; } = string.Empty;
    public List<TestCase> Tests { get; set; } = new();
}

public class QuizContent
{
    public int TimeLimitMin { get; set; }
    public List<QuizItem> Items { get; set; } = new();
}

// Supporting classes
public class LinkItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class AudioQuestion
{
    public string Question { get; set; } = string.Empty;
    public int MaxChars { get; set; }
}

public class ChoiceItem
{
    public string Text { get; set; } = string.Empty;
    public bool Correct { get; set; }
}

public class MatchPair
{
    public string Left { get; set; } = string.Empty;
    public string Right { get; set; } = string.Empty;
}

public class TestCase
{
    public string Input { get; set; } = string.Empty;
    public string Expected { get; set; } = string.Empty;
}

public class QuizItem
{
    public string QType { get; set; } = string.Empty;
    public string Stem { get; set; } = string.Empty;
    public List<ChoiceItem> Choices { get; set; } = new();
    public List<string> Answers { get; set; } = new();
    public int Points { get; set; }
}
