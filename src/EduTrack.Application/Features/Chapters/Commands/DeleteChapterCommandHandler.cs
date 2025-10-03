using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Commands;

public class DeleteChapterCommandHandler : IRequestHandler<DeleteChapterCommand, Result<bool>>
{
    private readonly IRepository<Chapter> _chapterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteChapterCommandHandler(
        IRepository<Chapter> chapterRepository,
        IUnitOfWork unitOfWork)
    {
        _chapterRepository = chapterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteChapterCommand request, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetByIdAsync(request.Id, cancellationToken);
        if (chapter == null)
        {
            return Result<bool>.Failure("Chapter not found");
        }

        await _chapterRepository.DeleteAsync(chapter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
