using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Courses.Queries;

public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, PaginatedList<CourseDto>>
{
    private readonly IRepository<Course> _courseRepository;

    public GetCoursesQueryHandler(IRepository<Course> courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<PaginatedList<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var query = _courseRepository.GetAll();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        query = query.OrderBy(c => c.Order).ThenBy(c => c.Title);

        var coursesQuery = query
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Thumbnail = c.Thumbnail,
                IsActive = c.IsActive,
                Order = c.Order,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy,
                ModuleCount = 0, // Will be calculated separately if needed
                LessonCount = 0  // Will be calculated separately if needed
            });

        return await PaginatedList<CourseDto>.CreateAsync(
            coursesQuery,
            request.PageNumber,
            request.PageSize);
    }
}
