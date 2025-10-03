using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Commands;

public class DeleteSubChapterCommandHandler : IRequestHandler<DeleteSubChapterCommand, Result<bool>>
{
    private readonly IRepository<SubChapter> _subChapterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSubChapterCommandHandler(
        IRepository<SubChapter> subChapterRepository,
        IUnitOfWork unitOfWork)
    {
        _subChapterRepository = subChapterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteSubChapterCommand request, CancellationToken cancellationToken)
    {
        var subChapter = await _subChapterRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subChapter == null)
        {
            return Result<bool>.Failure("SubChapter not found");
        }

        await _subChapterRepository.DeleteAsync(subChapter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
