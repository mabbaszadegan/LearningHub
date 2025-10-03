using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;

namespace EduTrack.Application.Features.EducationalContent.Commands;

public class DeleteEducationalContentCommandHandler : IRequestHandler<DeleteEducationalContentCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _contentRepository;
    private readonly IRepository<Domain.Entities.File> _fileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DeleteEducationalContentCommandHandler(
        IRepository<Domain.Entities.EducationalContent> contentRepository,
        IRepository<Domain.Entities.File> fileRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _contentRepository = contentRepository;
        _fileRepository = fileRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<bool>> Handle(DeleteEducationalContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (content == null)
        {
            return Result<bool>.Failure("Educational content not found");
        }

        // Handle file reference count if content has associated file
        if (content.FileId.HasValue)
        {
            var file = await _fileRepository.GetByIdAsync(content.FileId.Value, cancellationToken);
            if (file != null)
            {
                file.ReferenceCount--;
                
                if (file.ReferenceCount <= 0)
                {
                    // No more references, delete the physical file and record
                    await _fileStorageService.DeleteFileAsync(file.FilePath, cancellationToken);
                    await _fileRepository.DeleteAsync(file, cancellationToken);
                }
                else
                {
                    // Update reference count
                    await _fileRepository.UpdateAsync(file, cancellationToken);
                }
            }
        }

        await _contentRepository.DeleteAsync(content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}