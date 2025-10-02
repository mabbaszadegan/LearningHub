using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Classroom.Queries;

public class GetEnrollmentsByStudentQueryHandler : IRequestHandler<GetEnrollmentsByStudentQuery, PaginatedList<EnrollmentDto>>
{
    private readonly IRepository<Enrollment> _enrollmentRepository;

    public GetEnrollmentsByStudentQueryHandler(IRepository<Enrollment> enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<PaginatedList<EnrollmentDto>> Handle(GetEnrollmentsByStudentQuery request, CancellationToken cancellationToken)
    {
        var query = _enrollmentRepository.GetAll()
            .Where(e => e.StudentId == request.StudentId)
            .Include(e => e.Class)
                .ThenInclude(c => c.Course)
            .Include(e => e.Student)
            .OrderByDescending(e => e.EnrolledAt);

        var enrollmentsQuery = query
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                ClassId = e.ClassId,
                ClassName = e.Class.Name,
                CourseTitle = e.Class.Course.Title,
                StudentId = e.StudentId,
                StudentName = e.Student.FullName,
                EnrolledAt = e.EnrolledAt,
                CompletedAt = e.CompletedAt,
                IsActive = e.IsActive
            });

        return await PaginatedList<EnrollmentDto>.CreateAsync(
            enrollmentsQuery,
            request.PageNumber,
            request.PageSize);
    }
}
