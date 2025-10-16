using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using MediatR;

namespace EduTrack.Application.Features.Courses.Queries;

/// <summary>
/// Query to get course catalog for students with comprehensive statistics
/// </summary>
public class GetStudentCourseCatalogQuery : IRequest<PaginatedList<StudentCourseCatalogDto>>
{
    public string StudentId { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool? IsActive { get; }

    public GetStudentCourseCatalogQuery(string studentId, int pageNumber = 1, int pageSize = 12, bool? isActive = true)
    {
        StudentId = studentId;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsActive = isActive;
    }
}
