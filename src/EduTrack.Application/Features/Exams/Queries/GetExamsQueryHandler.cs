using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Exams.Queries;

public class GetExamsQueryHandler : IRequestHandler<GetExamsQuery, PaginatedList<ExamDto>>
{
    private readonly IRepository<Exam> _examRepository;

    public GetExamsQueryHandler(IRepository<Exam> examRepository)
    {
        _examRepository = examRepository;
    }

    public async Task<PaginatedList<ExamDto>> Handle(GetExamsQuery request, CancellationToken cancellationToken)
    {
        var query = _examRepository.GetAll();

        if (request.IsActive.HasValue)
        {
            query = query.Where(e => e.IsActive == request.IsActive.Value);
        }

        // Get all data first, then sort and paginate in memory
        var allExams = await query
            .Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                DurationMinutes = e.DurationMinutes,
                PassingScore = e.PassingScore,
                ShowSolutions = e.ShowSolutions,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                CreatedBy = e.CreatedBy,
                QuestionCount = e.ExamQuestions.Count()
            })
            .ToListAsync(cancellationToken);

        // Sort by creation date (newest first)
        var sortedExams = allExams
            .OrderByDescending(e => e.CreatedAt.DateTime)
            .ToList();

        // Manual pagination
        var totalCount = sortedExams.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var items = sortedExams
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<ExamDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
