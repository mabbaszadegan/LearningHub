using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Courses.Queries;

public class GetTeacherCoursesHierarchyQueryHandler : IRequestHandler<GetTeacherCoursesHierarchyQuery, Result<List<TeacherCourseHierarchyDto>>>
{
    private readonly IRepository<Course> _courseRepository;

    public GetTeacherCoursesHierarchyQueryHandler(IRepository<Course> courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Result<List<TeacherCourseHierarchyDto>>> Handle(GetTeacherCoursesHierarchyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var courses = await _courseRepository.GetAll()
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.SubChapters)
                .Where(c => c.CreatedBy == request.TeacherId && c.IsActive)
                .OrderBy(c => c.Order)
                .ThenBy(c => c.Title)
                .ToListAsync(cancellationToken);

            var courseHierarchy = courses.Select(course => new TeacherCourseHierarchyDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                IsActive = course.IsActive,
                Order = course.Order,
                ChapterCount = course.Chapters.Count,
                Chapters = course.Chapters
                    .OrderBy(ch => ch.Order)
                    .ThenBy(ch => ch.Title)
                    .Select(chapter => new TeacherChapterHierarchyDto
                    {
                        Id = chapter.Id,
                        CourseId = chapter.CourseId,
                        Title = chapter.Title,
                        Description = chapter.Description,
                        IsActive = chapter.IsActive,
                        Order = chapter.Order,
                        SubChapterCount = chapter.SubChapters.Count,
                        SubChapters = chapter.SubChapters
                            .OrderBy(sc => sc.Order)
                            .ThenBy(sc => sc.Title)
                            .Select(subChapter => new TeacherSubChapterHierarchyDto
                            {
                                Id = subChapter.Id,
                                ChapterId = subChapter.ChapterId,
                                Title = subChapter.Title,
                                Description = subChapter.Description,
                                IsActive = subChapter.IsActive,
                                Order = subChapter.Order
                            }).ToList()
                    }).ToList()
            }).ToList();

            return Result<List<TeacherCourseHierarchyDto>>.Success(courseHierarchy);
        }
        catch (Exception ex)
        {
            return Result<List<TeacherCourseHierarchyDto>>.Failure($"خطا در دریافت اطلاعات دوره‌ها: {ex.Message}");
        }
    }
}
