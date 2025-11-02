using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Courses.Queries;

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, Result<CourseDto>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<User> _userRepository;

    public GetCourseByIdQueryHandler(IRepository<Course> courseRepository, IRepository<User> userRepository)
    {
        _courseRepository = courseRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<CourseDto>> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        // Step 1: Get basic course information (fast query)
        var course = await _courseRepository.GetAll()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course == null)
        {
            return Result<CourseDto>.Failure("Course not found");
        }

        // Step 2: Get teacher's full name (separate fast query)
        var teacher = await _userRepository.GetAll()
            .FirstOrDefaultAsync(u => u.Id == course.CreatedBy, cancellationToken);
        var teacherName = teacher?.FullName ?? course.CreatedBy;

        // Step 4: Get chapters with subchapters (optimized query)
        var chapters = await _courseRepository.GetAll()
            .Where(c => c.Id == request.Id)
            .SelectMany(c => c.Chapters)
            .Include(ch => ch.SubChapters.Where(sc => sc.IsActive))
            .OrderBy(ch => ch.Order)
            .ToListAsync(cancellationToken);

        // Step 5: Get teaching plans with schedule items (optimized query)
        var teachingPlans = await _courseRepository.GetAll()
            .Where(c => c.Id == request.Id)
            .SelectMany(c => c.TeachingPlans)
            .Include(tp => tp.ScheduleItems)
                .ThenInclude(si => si.SubChapterAssignments)
            .ToListAsync(cancellationToken);

       
        // EducationalContent removed - no longer needed

        // Step 6: Get student count (enrollments)
        var studentCount = await _courseRepository.GetAll()
            .Where(c => c.Id == request.Id)
            .SelectMany(c => c.Enrollments)
            .CountAsync(e => e.IsActive, cancellationToken);

        // Step 8: Build DTO efficiently
        var courseDto = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Thumbnail = course.Thumbnail,
            IsActive = course.IsActive,
            Order = course.Order,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            CreatedBy = course.CreatedBy,
            CreatedByName = teacherName,
            DisciplineType = course.DisciplineType,
            ChapterCount = chapters.Count,
            // ModuleCount and LessonCount removed - Modules removed, using Chapters/SubChapters instead
            StudentCount = studentCount,
            Chapters = chapters.Select(ch => new ChapterDto
            {
                Id = ch.Id,
                CourseId = ch.CourseId,
                Title = ch.Title,
                Description = ch.Description,
                Objective = ch.Objective,
                IsActive = ch.IsActive,
                Order = ch.Order,
                CreatedAt = ch.CreatedAt,
                UpdatedAt = ch.UpdatedAt,
                SubChapterCount = ch.SubChapters.Count,
                SubChapters = ch.SubChapters.Select(sc => new SubChapterDto
                {
                    Id = sc.Id,
                    ChapterId = sc.ChapterId,
                    Title = sc.Title,
                    Description = sc.Description,
                    Objective = sc.Objective,
                    IsActive = sc.IsActive,
                    Order = sc.Order,
                    CreatedAt = sc.CreatedAt,
                    UpdatedAt = sc.UpdatedAt,
                    // ContentCount and EducationalContents removed - EducationalContent entity removed
                    ScheduleItemStats = teachingPlans
                        .SelectMany(tp => tp.ScheduleItems)
                        .Where(si => si.SubChapterAssignments.Any(sia => sia.SubChapterId == sc.Id))
                        .GroupBy(si => si.Type)
                        .ToDictionary(g => g.Key, g => g.Count())
                }).ToList()
            }).ToList()
        };

        return Result<CourseDto>.Success(courseDto);
    }
}
