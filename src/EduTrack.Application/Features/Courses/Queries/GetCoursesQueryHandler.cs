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
        var baseQuery = _courseRepository.GetAll();

        if (request.IsActive.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.IsActive == request.IsActive.Value);
        }

        var query = baseQuery
            .Include(c => c.Chapters)
                .ThenInclude(ch => ch.SubChapters)
            .Include(c => c.TeachingPlans)
                .ThenInclude(tp => tp.ScheduleItems)
            .Include(c => c.Enrollments)
            .OrderBy(c => c.Order)
            .ThenBy(c => c.Title);

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
                CreatedByName = c.CreatedBy, // Will be updated with actual teacher name if needed
                DisciplineType = c.DisciplineType,
                ChapterCount = c.Chapters.Count, // تعداد مبحث
                // ModuleCount and LessonCount removed - Modules removed
                // ModuleCount = c.Chapters.Sum(ch => ch.SubChapters.Count), // تعداد زیرمبحث
                // LessonCount = c.TeachingPlans.Sum(tp => tp.ScheduleItems.Count), // تعداد محتوا (ScheduleItems)
                ClassCount = c.Enrollments.Count(e => e.IsActive) // تعداد دانش‌آموز (تعداد ثبت‌نام‌های فعال)
            });

        return await PaginatedList<CourseDto>.CreateAsync(
            coursesQuery,
            request.PageNumber,
            request.PageSize);
    }
}
