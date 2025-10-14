using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Files;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Files.Queries;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, Result<FileDto>>
{
    private readonly IFileRepository _fileRepository;

    public GetFileByIdQueryHandler(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<Result<FileDto>> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _fileRepository.GetByIdAsync(request.Id, cancellationToken);
            if (file == null)
            {
                return Result<FileDto>.Failure("فایل یافت نشد.");
            }

            var dto = new FileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                OriginalFileName = file.OriginalFileName,
                FilePath = file.FilePath,
                MimeType = file.MimeType,
                FileSizeBytes = file.FileSizeBytes,
                MD5Hash = file.MD5Hash,
                CreatedAt = file.CreatedAt,
                CreatedBy = file.CreatedBy,
                ReferenceCount = file.ReferenceCount
            };

            return Result<FileDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<FileDto>.Failure($"خطا در دریافت فایل: {ex.Message}");
        }
    }
}
