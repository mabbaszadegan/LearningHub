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
        var course = await _courseRepository.GetAll()
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                    .ThenInclude(l => l.Resources)
            .Include(c => c.Chapters)
                .ThenInclude(ch => ch.SubChapters)
                    .ThenInclude(sc => sc.EducationalContents)
                        .ThenInclude(ec => ec.File)
            .Include(c => c.TeachingPlans)
                .ThenInclude(tp => tp.ScheduleItems)
                    .ThenInclude(si => si.SubChapterAssignments)
            .Include(c => c.Classes)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course == null)
        {
            return Result<CourseDto>.Failure("Course not found");
        }

        // Get teacher's full name
        var teacher = await _userRepository.GetAll()
            .FirstOrDefaultAsync(u => u.Id == course.CreatedBy, cancellationToken);
        var teacherName = teacher?.FullName ?? course.CreatedBy;

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
            ModuleCount = course.Chapters.Sum(ch => ch.SubChapters.Count), // تعداد زیرمبحث
            LessonCount = course.Chapters.Sum(ch => ch.SubChapters.Sum(sc => sc.EducationalContents.Count)), // تعداد محتوا
            ChapterCount = course.Chapters.Count, // تعداد مبحث
            ClassCount = course.Classes.Count, // تعداد کلاس
            Modules = course.Modules.Select(m => new ModuleDto
            {
                Id = m.Id,
                CourseId = m.CourseId,
                Title = m.Title,
                Description = m.Description,
                IsActive = m.IsActive,
                Order = m.Order,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                LessonCount = m.Lessons.Count,
                Lessons = m.Lessons.Select(l => new LessonDto
                {
                    Id = l.Id,
                    ModuleId = l.ModuleId,
                    Title = l.Title,
                    Content = l.Content,
                    VideoUrl = l.VideoUrl,
                    IsActive = l.IsActive,
                    Order = l.Order,
                    DurationMinutes = l.DurationMinutes,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                    Resources = l.Resources.Select(r => new ResourceDto
                    {
                        Id = r.Id,
                        LessonId = r.LessonId,
                        Title = r.Title,
                        Description = r.Description,
                        Type = r.Type,
                        FilePath = r.FilePath,
                        Url = r.Url,
                        FileSizeBytes = r.FileSizeBytes,
                        MimeType = r.MimeType,
                        IsActive = r.IsActive,
                        Order = r.Order,
                        CreatedAt = r.CreatedAt
                    }).ToList()
                }).ToList()
            }).ToList(),
            Chapters = course.Chapters.Select(c => new ChapterDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                Objective = c.Objective,
                IsActive = c.IsActive,
                Order = c.Order,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                SubChapterCount = c.SubChapters.Count,
                SubChapters = c.SubChapters.Select(sc => new SubChapterDto
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
                    ContentCount = sc.EducationalContents.Count,
                    ScheduleItemStats = course.TeachingPlans
                        .SelectMany(tp => tp.ScheduleItems)
                        .Where(si => si.SubChapterAssignments.Any(sia => sia.SubChapterId == sc.Id))
                        .GroupBy(si => si.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    EducationalContents = sc.EducationalContents.Select(ec => new EducationalContentDto
                    {
                        Id = ec.Id,
                        SubChapterId = ec.SubChapterId,
                        Title = ec.Title,
                        Description = ec.Description,
                        Type = ec.Type,
                        TextContent = ec.TextContent,
                        FileId = ec.FileId,
                        ExternalUrl = ec.ExternalUrl,
                        IsActive = ec.IsActive,
                        Order = ec.Order,
                        CreatedAt = ec.CreatedAt,
                        UpdatedAt = ec.UpdatedAt,
                        CreatedBy = ec.CreatedBy,
                        File = ec.File != null ? new FileDto
                        {
                            Id = ec.File.Id,
                            FileName = ec.File.FileName,
                            OriginalFileName = ec.File.OriginalFileName,
                            FilePath = ec.File.FilePath,
                            MimeType = ec.File.MimeType,
                            FileSizeBytes = ec.File.FileSizeBytes,
                            MD5Hash = ec.File.MD5Hash,
                            CreatedAt = ec.File.CreatedAt,
                            CreatedBy = ec.File.CreatedBy,
                            ReferenceCount = ec.File.ReferenceCount
                        } : null
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        return Result<CourseDto>.Success(courseDto);
    }
}
