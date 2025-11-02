using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Chapters.Queries;

public class GetChapterByIdQueryHandler : IRequestHandler<GetChapterByIdQuery, Result<ChapterDto>>
{
    private readonly IRepository<Chapter> _chapterRepository;

    public GetChapterByIdQueryHandler(IRepository<Chapter> chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<Result<ChapterDto>> Handle(GetChapterByIdQuery request, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetAll()
            .Include(c => c.SubChapters.Where(sc => sc.IsActive))
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (chapter == null)
        {
            return Result<ChapterDto>.Failure("Chapter not found");
        }

        var chapterDto = new ChapterDto
        {
            Id = chapter.Id,
            CourseId = chapter.CourseId,
            Title = chapter.Title,
            Description = chapter.Description,
            Objective = chapter.Objective,
            IsActive = chapter.IsActive,
            Order = chapter.Order,
            CreatedAt = chapter.CreatedAt,
            UpdatedAt = chapter.UpdatedAt,
            SubChapterCount = chapter.SubChapters.Count,
            SubChapters = chapter.SubChapters.OrderBy(sc => sc.Order).Select(subChapter => new SubChapterDto
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
            }).ToList()
        };

        return Result<ChapterDto>.Success(chapterDto);
    }
}
