using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Chapters.Queries;

public class GetSubChaptersByChapterIdQueryHandler : IRequestHandler<GetSubChaptersByChapterIdQuery, Result<List<SubChapterDto>>>
{
    private readonly IRepository<SubChapter> _subChapterRepository;

    public GetSubChaptersByChapterIdQueryHandler(IRepository<SubChapter> subChapterRepository)
    {
        _subChapterRepository = subChapterRepository;
    }

    public async Task<Result<List<SubChapterDto>>> Handle(GetSubChaptersByChapterIdQuery request, CancellationToken cancellationToken)
    {
        var subChapters = await _subChapterRepository.GetAll()
            .Where(sc => sc.ChapterId == request.ChapterId)
            .OrderBy(sc => sc.Order)
            .ToListAsync(cancellationToken);

        var subChapterDtos = subChapters.Select(subChapter => new SubChapterDto
        {
            Id = subChapter.Id,
            ChapterId = subChapter.ChapterId,
            Title = subChapter.Title,
            Description = subChapter.Description,
            Objective = subChapter.Objective,
            IsActive = subChapter.IsActive,
            Order = subChapter.Order,
            CreatedAt = subChapter.CreatedAt,
            UpdatedAt = subChapter.UpdatedAt
        }).ToList();

        return Result<List<SubChapterDto>>.Success(subChapterDtos);
    }
}
