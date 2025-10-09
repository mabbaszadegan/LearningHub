using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Exams;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Progress.Queries;

public class GetProgressByStudentQueryHandler : IRequestHandler<GetProgressByStudentQuery, PaginatedList<ProgressDto>>
{
    private readonly IRepository<Domain.Entities.Progress> _progressRepository;

    public GetProgressByStudentQueryHandler(IRepository<Domain.Entities.Progress> progressRepository)
    {
        _progressRepository = progressRepository;
    }

    public async Task<PaginatedList<ProgressDto>> Handle(GetProgressByStudentQuery request, CancellationToken cancellationToken)
    {
        var query = _progressRepository.GetAll()
            .Where(p => p.StudentId == request.StudentId)
            .Include(p => p.Student)
            .Include(p => p.Lesson)
            .Include(p => p.Exam);

        // Get all data first, then sort and paginate in memory
        var allProgress = await query
            .Select(p => new ProgressDto
            {
                Id = p.Id,
                StudentId = p.StudentId,
                StudentName = p.Student.FullName,
                LessonId = p.LessonId,
                LessonTitle = p.Lesson != null ? p.Lesson.Title : null,
                ExamId = p.ExamId,
                ExamTitle = p.Exam != null ? p.Exam.Title : null,
                Status = p.Status,
                CorrectCount = p.CorrectCount,
                Streak = p.Streak,
                StartedAt = p.StartedAt,
                CompletedAt = p.CompletedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // Sort by update date (newest first)
        var sortedProgress = allProgress
            .OrderByDescending(p => p.UpdatedAt.DateTime)
            .ToList();

        // Manual pagination
        var totalCount = sortedProgress.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var items = sortedProgress
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<ProgressDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
