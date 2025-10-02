using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Classroom.Queries;

public class GetClassesQueryHandler : IRequestHandler<GetClassesQuery, PaginatedList<ClassDto>>
{
    private readonly IRepository<Class> _classRepository;

    public GetClassesQueryHandler(IRepository<Class> classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<PaginatedList<ClassDto>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        var query = _classRepository.GetAll();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        query = query.OrderBy(c => c.Name);

        var classesQuery = query
            .Select(c => new ClassDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                Name = c.Name,
                Description = c.Description,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher.FirstName + " " + c.Teacher.LastName,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                StudentCount = c.Enrollments.Count(e => e.IsActive)
            });

        return await PaginatedList<ClassDto>.CreateAsync(
            classesQuery,
            request.PageNumber,
            request.PageSize);
    }
}
