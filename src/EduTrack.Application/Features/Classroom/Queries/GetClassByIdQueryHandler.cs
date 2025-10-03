using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Classroom.Queries;

public class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, Result<ClassDto>>
{
    private readonly IRepository<Class> _classRepository;

    public GetClassByIdQueryHandler(IRepository<Class> classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<Result<ClassDto>> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
    {
        var classEntity = await _classRepository.GetAll()
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (classEntity == null)
        {
            return Result<ClassDto>.Failure("Class not found");
        }

        var classDto = new ClassDto
        {
            Id = classEntity.Id,
            CourseId = classEntity.CourseId,
            Name = classEntity.Name,
            Description = classEntity.Description,
            TeacherId = classEntity.TeacherId,
            TeacherName = classEntity.Teacher.FirstName + " " + classEntity.Teacher.LastName,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            IsActive = classEntity.IsActive,
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt,
            StudentCount = classEntity.Enrollments.Count(e => e.IsActive)
        };

        return Result<ClassDto>.Success(classDto);
    }
}
