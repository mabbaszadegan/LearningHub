using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using MediatR;

namespace EduTrack.Application.Features.Courses.Queries;

public record GetCoursesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsActive = null) : IRequest<PaginatedList<CourseDto>>;

public record GetCourseByIdQuery(int Id) : IRequest<Result<CourseDto>>;

// Module-related queries removed - Module entity removed

public record GetLessonByIdQuery(int Id) : IRequest<Result<LessonDto>>;

public record GetResourcesByLessonIdQuery(int LessonId) : IRequest<Result<List<ResourceDto>>>;

public record GetResourceByIdQuery(int Id) : IRequest<Result<ResourceDto>>;
