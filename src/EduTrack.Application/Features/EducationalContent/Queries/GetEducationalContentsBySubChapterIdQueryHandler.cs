using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.EducationalContent.Queries;

public class GetEducationalContentsBySubChapterIdQueryHandler : IRequestHandler<GetEducationalContentsBySubChapterIdQuery, Result<List<EducationalContentDto>>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _contentRepository;

    public GetEducationalContentsBySubChapterIdQueryHandler(IRepository<Domain.Entities.EducationalContent> contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<Result<List<EducationalContentDto>>> Handle(GetEducationalContentsBySubChapterIdQuery request, CancellationToken cancellationToken)
    {
        var contents = await _contentRepository.GetAll()
            .Where(c => c.SubChapterId == request.SubChapterId && c.IsActive)
            .Include(c => c.File)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        var contentDtos = contents.Select(content => new EducationalContentDto
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
        }).ToList();

        return Result<List<EducationalContentDto>>.Success(contentDtos);
    }
}