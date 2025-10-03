using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Progress.Queries;

public class GetProgressByLessonQueryHandler : IRequestHandler<GetProgressByLessonQuery, Result<ProgressDto>>
{
    private readonly IRepository<EduTrack.Domain.Entities.Progress> _progressRepository;
    private readonly IUserService _userService;

    public GetProgressByLessonQueryHandler(
        IRepository<EduTrack.Domain.Entities.Progress> progressRepository,
        IUserService userService)
    {
        _progressRepository = progressRepository;
        _userService = userService;
    }

    public async Task<Result<ProgressDto>> Handle(GetProgressByLessonQuery request, CancellationToken cancellationToken)
    {
        var student = await _userService.GetUserByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Result<ProgressDto>.Failure("Student not found");
        }

        var progress = await _progressRepository.GetAll()
            .Include(p => p.Lesson)
            .FirstOrDefaultAsync(p => p.StudentId == request.StudentId && 
                                    p.LessonId == request.LessonId, cancellationToken);

        if (progress == null)
        {
            return Result<ProgressDto>.Failure("Progress not found for this lesson");
        }

        var progressDto = new ProgressDto
        {
            Id = progress.Id,
            StudentId = progress.StudentId,
            StudentName = student.FullName,
            LessonId = progress.LessonId,
            LessonTitle = progress.Lesson?.Title,
            ExamId = progress.ExamId,
            ExamTitle = null, // Would need to include Exam if needed
            Status = progress.Status,
            CorrectCount = progress.CorrectCount,
            Streak = progress.Streak,
            StartedAt = progress.StartedAt,
            CompletedAt = progress.CompletedAt,
            UpdatedAt = progress.UpdatedAt
        };

        return Result<ProgressDto>.Success(progressDto);
    }
}
