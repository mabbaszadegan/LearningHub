using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Files.Commands;

public class CreateFileCommandHandler : IRequestHandler<CreateFileCommand, Result<int>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFileCommandHandler(
        IFileRepository fileRepository,
        IUnitOfWork unitOfWork)
    {
        _fileRepository = fileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = Domain.Entities.File.Create(
                request.FileName,
                request.OriginalFileName,
                request.FilePath,
                request.MimeType,
                request.FileSizeBytes,
                request.MD5Hash,
                "system" // TODO: Get current user ID
            );

            await _fileRepository.AddAsync(file, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(file.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"خطا در ایجاد فایل: {ex.Message}");
        }
    }
}
