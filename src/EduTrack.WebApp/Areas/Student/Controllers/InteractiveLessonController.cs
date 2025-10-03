using EduTrack.Application.Features.InteractiveLesson.Queries;
using EduTrack.Application.Features.InteractiveLesson.Commands;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class InteractiveLessonController : Controller
{
    private readonly IMediator _mediator;

    public InteractiveLessonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int classId)
    {
        var result = await _mediator.Send(new GetInteractiveLessonsByClassQuery(classId));
        
        if (result.IsSuccess)
        {
            ViewBag.ClassId = classId;
            return View(result.Value);
        }

        return NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> View(int id)
    {
        var result = await _mediator.Send(new GetInteractiveLessonByIdQuery(id));
        
        if (result.IsSuccess)
        {
            var studentId = User.Identity?.Name ?? "";
            var progressResult = await _mediator.Send(new GetStudentProgressQuery(id, studentId));
            
            ViewBag.StudentProgress = progressResult.IsSuccess ? progressResult.Value : null;
            return View(result.Value);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitAnswer(SubmitStudentAnswerCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetProgress(int interactiveLessonId)
    {
        var studentId = User.Identity?.Name ?? "";
        var result = await _mediator.Send(new GetStudentProgressQuery(interactiveLessonId, studentId));
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Value });
        }

        return Json(new { success = false, error = result.Error });
    }
}

