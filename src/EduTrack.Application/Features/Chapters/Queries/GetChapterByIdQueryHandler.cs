using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
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
                .ThenInclude(sc => sc.EducationalContents.Where(ec => ec.IsActive))
                    .ThenInclude(ec => ec.File)
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
                UpdatedAt = subChapter.UpdatedAt,
                ContentCount = subChapter.EducationalContents.Count,
                EducationalContents = subChapter.EducationalContents.OrderBy(ec => ec.Order).Select(content => new EducationalContentDto
                {
                    Id = content.Id,
                    SubChapterId = content.SubChapterId,
                    Title = content.Title,
                    Description = content.Description,
                    Type = content.Type,
                    TextContent = content.TextContent,
                    FileId = content.FileId,
                    ExternalUrl = content.ExternalUrl,
                    IsActive = content.IsActive,
                    Order = content.Order,
                    CreatedAt = content.CreatedAt,
                    UpdatedAt = content.UpdatedAt,
                    CreatedBy = content.CreatedBy,
                    File = content.File != null ? new FileDto
                    {
                        Id = content.File.Id,
                        FileName = content.File.FileName,
                        OriginalFileName = content.File.OriginalFileName,
                        FilePath = content.File.FilePath,
                        MimeType = content.File.MimeType,
                        FileSizeBytes = content.File.FileSizeBytes,
                        MD5Hash = content.File.MD5Hash,
                        CreatedAt = content.File.CreatedAt,
                        CreatedBy = content.File.CreatedBy,
                        ReferenceCount = content.File.ReferenceCount,
                        FileUrl = $"/api/files/{content.File.FilePath}"
                    } : null
                }).ToList()
            }).ToList()
        };

        return Result<ChapterDto>.Success(chapterDto);
    }
}
