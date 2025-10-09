using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Courses.Queries;

public class GetModulesByCourseIdQueryHandler : IRequestHandler<GetModulesByCourseIdQuery, Result<List<ModuleDto>>>
{
    private readonly IRepository<Module> _moduleRepository;

    public GetModulesByCourseIdQueryHandler(IRepository<Module> moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    public async Task<Result<List<ModuleDto>>> Handle(GetModulesByCourseIdQuery request, CancellationToken cancellationToken)
    {
        var modules = await _moduleRepository.GetAll()
            .Include(m => m.Lessons)
            .Where(m => m.CourseId == request.CourseId)
            .OrderBy(m => m.Order)
            .ToListAsync(cancellationToken);

        var moduleDtos = modules.Select(m => new ModuleDto
        {
            Id = m.Id,
            CourseId = m.CourseId,
            Title = m.Title,
            Description = m.Description,
            IsActive = m.IsActive,
            Order = m.Order,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            LessonCount = m.Lessons.Count
        }).ToList();

        return Result<List<ModuleDto>>.Success(moduleDtos);
    }
}
