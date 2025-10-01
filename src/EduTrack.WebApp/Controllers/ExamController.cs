using EduTrack.Application.Features.Exams.Commands;
using EduTrack.Application.Features.Exams.Queries;
using EduTrack.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

public class ExamController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExamController> _logger;

    public ExamController(IMediator mediator, ILogger<ExamController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // GET: Exams
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var exams = await _mediator.Send(new GetExamsQuery(pageNumber, pageSize, true));
        return View(exams);
    }

    // GET: Exams/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var result = await _mediator.Send(new GetExamByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // GET: Exams/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Exams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateExamCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Exam created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }

    // GET: Exams/Start/5
    public async Task<IActionResult> Start(int id)
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "You must be logged in to start an exam.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _mediator.Send(new StartExamCommand(id, userId));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Take), new { id = result.Value!.Id });
    }

    // GET: Exams/Take/5
    public async Task<IActionResult> Take(int id)
    {
        var result = await _mediator.Send(new GetAttemptByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var attempt = result.Value!;
        if (attempt.SubmittedAt.HasValue)
        {
            TempData["Error"] = "This exam has already been submitted.";
            return RedirectToAction(nameof(Index));
        }

        return View(attempt);
    }

    // POST: Exams/Submit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, List<AnswerSubmissionDto> answers)
    {
        var command = new SubmitExamCommand(id, answers);
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            TempData["Success"] = "Exam submitted successfully.";
            return RedirectToAction(nameof(Review), new { id = result.Value!.Id });
        }
        
        TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Take), new { id });
    }

    // GET: Exams/Review/5
    public async Task<IActionResult> Review(int id)
    {
        var result = await _mediator.Send(new GetAttemptByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // GET: Questions
    public async Task<IActionResult> Questions(int pageNumber = 1, int pageSize = 10)
    {
        var questions = await _mediator.Send(new GetQuestionsQuery(pageNumber, pageSize, true));
        return View(questions);
    }

    // GET: Questions/Create
    public IActionResult CreateQuestion()
    {
        return View();
    }

    // POST: Questions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(CreateQuestionCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Question created successfully.";
                return RedirectToAction(nameof(Questions));
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }
}
