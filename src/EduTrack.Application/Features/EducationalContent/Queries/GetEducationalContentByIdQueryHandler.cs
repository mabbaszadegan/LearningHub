using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.EducationalContent.Queries;

public class GetEducationalContentByIdQueryHandler : IRequestHandler<GetEducationalContentByIdQuery, Result<EducationalContentDto>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _contentRepository;

    public GetEducationalContentByIdQueryHandler(IRepository<Domain.Entities.EducationalContent> contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<Result<EducationalContentDto>> Handle(GetEducationalContentByIdQuery request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetAll()
            .Include(c => c.File)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (content == null)
        {
            return Result<EducationalContentDto>.Failure("Educational content not found");
        }

        var contentDto = new EducationalContentDto
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
        };

        return Result<EducationalContentDto>.Success(contentDto);
    }
}