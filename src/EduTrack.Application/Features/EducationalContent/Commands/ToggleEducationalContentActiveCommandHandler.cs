using EduTrack.Application.Common.Models;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.EducationalContent.Commands;

public class ToggleEducationalContentActiveCommandHandler : IRequestHandler<ToggleEducationalContentActiveCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _contentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleEducationalContentActiveCommandHandler(
        IRepository<Domain.Entities.EducationalContent> contentRepository,
        IUnitOfWork unitOfWork)
    {
        _contentRepository = contentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ToggleEducationalContentActiveCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (content == null)
        {
            return Result<bool>.Failure("Educational content not found");
        }

        // Toggle the active status
        if (content.IsActive)
        {
            content.Deactivate();
        }
        else
        {
            content.Activate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(content.IsActive);
    }
}

public class ReorderEducationalContentsCommandHandler : IRequestHandler<ReorderEducationalContentsCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _contentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReorderEducationalContentsCommandHandler(
        IRepository<Domain.Entities.EducationalContent> contentRepository,
        IUnitOfWork unitOfWork)
    {
        _contentRepository = contentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ReorderEducationalContentsCommand request, CancellationToken cancellationToken)
    {
        var contents = await _contentRepository.GetAll()
            .Where(c => request.ContentIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (contents.Count != request.ContentIds.Count)
        {
            return Result<bool>.Failure("Some educational contents not found");
        }

        // Update order based on the provided sequence
        for (int i = 0; i < request.ContentIds.Count; i++)
        {
            var content = contents.First(c => c.Id == request.ContentIds[i]);
            content.UpdateOrder(i + 1);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
