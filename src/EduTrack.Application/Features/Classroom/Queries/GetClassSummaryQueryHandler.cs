using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Classroom.Queries;

public class GetClassSummaryQueryHandler : IRequestHandler<GetClassSummaryQuery, Result<ClassSummaryDto>>
{
    private readonly IRepository<Class> _classRepository;

    public GetClassSummaryQueryHandler(IRepository<Class> classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<Result<ClassSummaryDto>> Handle(GetClassSummaryQuery request, CancellationToken cancellationToken)
    {
        var classEntity = await _classRepository.GetAll()
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

        if (classEntity == null)
        {
            return Result<ClassSummaryDto>.Failure("Class not found");
        }

        var activeEnrollments = classEntity.Enrollments.Where(e => e.IsActive).ToList();
        var completedEnrollments = activeEnrollments.Where(e => e.CompletedAt.HasValue).ToList();

        var summary = new ClassSummaryDto
        {
            ClassId = classEntity.Id,
            ClassName = classEntity.Name,
            CourseTitle = classEntity.Course.Title,
            TeacherName = classEntity.Teacher.FirstName + " " + classEntity.Teacher.LastName,
            TotalStudents = activeEnrollments.Count,
            CompletedStudents = completedEnrollments.Count,
            CompletionPercentage = activeEnrollments.Count > 0 
                ? (double)completedEnrollments.Count / activeEnrollments.Count * 100 
                : 0,
            AverageScore = 0, // This would need to be calculated from exam results
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate
        };

        return Result<ClassSummaryDto>.Success(summary);
    }
}
