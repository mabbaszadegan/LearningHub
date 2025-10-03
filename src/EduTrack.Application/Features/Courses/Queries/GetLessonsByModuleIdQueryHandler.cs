using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Courses.Queries;

public class GetLessonsByModuleIdQueryHandler : IRequestHandler<GetLessonsByModuleIdQuery, Result<List<LessonDto>>>
{
    private readonly IRepository<Lesson> _lessonRepository;

    public GetLessonsByModuleIdQueryHandler(IRepository<Lesson> lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<Result<List<LessonDto>>> Handle(GetLessonsByModuleIdQuery request, CancellationToken cancellationToken)
    {
        var lessons = await _lessonRepository.GetAll()
            .Include(l => l.Resources)
            .Where(l => l.ModuleId == request.ModuleId)
            .OrderBy(l => l.Order)
            .ToListAsync(cancellationToken);

        var lessonDtos = lessons.Select(l => new LessonDto
        {
            Id = l.Id,
            ModuleId = l.ModuleId,
            Title = l.Title,
            Content = l.Content,
            VideoUrl = l.VideoUrl,
            IsActive = l.IsActive,
            Order = l.Order,
            DurationMinutes = l.DurationMinutes,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            Resources = l.Resources.Select(r => new ResourceDto
            {
                Id = r.Id,
                LessonId = r.LessonId,
                Title = r.Title,
                Description = r.Description,
                Type = r.Type,
                FilePath = r.FilePath,
                Url = r.Url,
                FileSizeBytes = r.FileSizeBytes,
                MimeType = r.MimeType,
                IsActive = r.IsActive,
                Order = r.Order,
                CreatedAt = r.CreatedAt
            }).ToList()
        }).ToList();

        return Result<List<LessonDto>>.Success(lessonDtos);
    }
}
