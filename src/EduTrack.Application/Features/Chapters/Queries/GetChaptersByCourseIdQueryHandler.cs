using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Chapters.Queries;

public class GetChaptersByCourseIdQueryHandler : IRequestHandler<GetChaptersByCourseIdQuery, Result<List<ChapterDto>>>
{
    private readonly IRepository<Chapter> _chapterRepository;

    public GetChaptersByCourseIdQueryHandler(IRepository<Chapter> chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<Result<List<ChapterDto>>> Handle(GetChaptersByCourseIdQuery request, CancellationToken cancellationToken)
    {
        var chapters = await _chapterRepository.GetAll()
            .Where(c => c.CourseId == request.CourseId && c.IsActive)
            .Include(c => c.SubChapters.Where(sc => sc.IsActive))
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        var chapterDtos = chapters.Select(chapter => new ChapterDto
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
        }).ToList();

        return Result<List<ChapterDto>>.Success(chapterDtos);
    }
}
