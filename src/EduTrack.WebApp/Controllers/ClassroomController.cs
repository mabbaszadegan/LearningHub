using EduTrack.Application.Features.Classroom.Commands;
using EduTrack.Application.Features.Classroom.Queries;
using EduTrack.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

public class ClassroomController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClassroomController> _logger;

    public ClassroomController(IMediator mediator, ILogger<ClassroomController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // GET: Classes
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var classes = await _mediator.Send(new GetClassesQuery(pageNumber, pageSize, true));
        return View(classes);
    }

    // GET: Classes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // GET: Classes/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Classes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClassCommand command)
    {
        if (ModelState.IsValid)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Class created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = result.Error;
        }
        return View(command);
    }

    // GET: Classes/Enroll/5
    public async Task<IActionResult> Enroll(int id)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // POST: Classes/Enroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnrollStudent(int classId, string studentId)
    {
        var command = new EnrollStudentCommand(classId, studentId);
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            TempData["Success"] = "Student enrolled successfully.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }
        
        return RedirectToAction(nameof(Details), new { id = classId });
    }

    // POST: Classes/Unenroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnenrollStudent(int classId, string studentId)
    {
        var command = new UnenrollStudentCommand(classId, studentId);
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            TempData["Success"] = "Student unenrolled successfully.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }
        
        return RedirectToAction(nameof(Details), new { id = classId });
    }

    // GET: Classes/Summary/5
    public async Task<IActionResult> Summary(int id)
    {
        var result = await _mediator.Send(new GetClassSummaryQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // GET: Classes/Export/5
    public async Task<IActionResult> Export(int id)
    {
        var result = await _mediator.Send(new GetClassSummaryQuery(id));
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var summary = result.Value!;
        var csv = GenerateClassSummaryCsv(summary);
        
        var fileName = $"class_{id}_summary_{DateTime.Now:yyyyMMdd}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    private static string GenerateClassSummaryCsv(ClassSummaryDto summary)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Class Name,Course Title,Teacher,Total Students,Completed Students,Completion %,Average Score,Start Date,End Date");
        csv.AppendLine($"{summary.ClassName},{summary.CourseTitle},{summary.TeacherName},{summary.TotalStudents},{summary.CompletedStudents},{summary.CompletionPercentage:F2}%,{summary.AverageScore:F2},{summary.StartDate:yyyy-MM-dd},{summary.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        return csv.ToString();
    }
}
