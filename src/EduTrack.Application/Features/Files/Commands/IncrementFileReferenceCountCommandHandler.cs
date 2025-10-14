using EduTrack.Application.Common.Models;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Files.Commands;

public class IncrementFileReferenceCountCommandHandler : IRequestHandler<IncrementFileReferenceCountCommand, Result>
{
    private readonly IFileRepository _fileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IncrementFileReferenceCountCommandHandler(
        IFileRepository fileRepository,
        IUnitOfWork unitOfWork)
    {
        _fileRepository = fileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(IncrementFileReferenceCountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
            if (file == null)
            {
                return Result.Failure("فایل یافت نشد.");
            }

            file.IncrementReferenceCount();
            await _fileRepository.UpdateAsync(file, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"خطا در به‌روزرسانی فایل: {ex.Message}");
        }
    }
}
