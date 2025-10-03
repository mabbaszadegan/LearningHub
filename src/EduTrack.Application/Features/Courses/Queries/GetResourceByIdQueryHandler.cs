using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Courses.Queries;

public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, Result<ResourceDto>>
{
    private readonly IRepository<Resource> _resourceRepository;

    public GetResourceByIdQueryHandler(IRepository<Resource> resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<Result<ResourceDto>> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null)
        {
            return Result<ResourceDto>.Failure("Resource not found");
        }

        var resourceDto = new ResourceDto
        {
            Id = resource.Id,
            LessonId = resource.LessonId,
            Title = resource.Title,
            Description = resource.Description,
            Type = resource.Type,
            FilePath = resource.FilePath,
            Url = resource.Url,
            FileSizeBytes = resource.FileSizeBytes,
            MimeType = resource.MimeType,
            IsActive = resource.IsActive,
            Order = resource.Order,
            CreatedAt = resource.CreatedAt
        };

        return Result<ResourceDto>.Success(resourceDto);
    }
}
