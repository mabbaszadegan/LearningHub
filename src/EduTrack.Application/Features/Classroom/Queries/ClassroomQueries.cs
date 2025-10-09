using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using MediatR;

namespace EduTrack.Application.Features.Classroom.Queries;

public record GetClassesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsActive = null) : IRequest<PaginatedList<ClassDto>>;

public record GetClassByIdQuery(int Id) : IRequest<Result<ClassDto>>;

public record GetClassesByTeacherQuery(
    string TeacherId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<ClassDto>>;

public record GetEnrollmentsByClassQuery(int ClassId) : IRequest<Result<List<EnrollmentDto>>>;

public record GetEnrollmentsByStudentQuery(
    string StudentId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<EnrollmentDto>>;

public record GetClassSummaryQuery(int ClassId) : IRequest<Result<ClassSummaryDto>>;

public record EnrollmentDto
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTimeOffset EnrolledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsActive { get; set; }
}
