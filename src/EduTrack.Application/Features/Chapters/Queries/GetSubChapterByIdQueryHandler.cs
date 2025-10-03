using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Queries;

public class GetSubChapterByIdQueryHandler : IRequestHandler<GetSubChapterByIdQuery, Result<SubChapterDto>>
{
    private readonly IRepository<SubChapter> _subChapterRepository;

    public GetSubChapterByIdQueryHandler(IRepository<SubChapter> subChapterRepository)
    {
        _subChapterRepository = subChapterRepository;
    }

    public async Task<Result<SubChapterDto>> Handle(GetSubChapterByIdQuery request, CancellationToken cancellationToken)
    {
        var subChapter = await _subChapterRepository.GetByIdAsync(request.Id, cancellationToken);

        if (subChapter == null)
        {
            return Result<SubChapterDto>.Failure("SubChapter not found");
        }

        var subChapterDto = new SubChapterDto
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
        };

        return Result<SubChapterDto>.Success(subChapterDto);
    }
}
