using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CourseEnrollmentEntity = EduTrack.Domain.Entities.CourseEnrollment;
using EducationalContentEntity = EduTrack.Domain.Entities.EducationalContent;

namespace EduTrack.Application.Features.Courses.Queries;

/// <summary>
/// Handler for getting student course catalog with comprehensive statistics
/// </summary>
public class GetStudentCourseCatalogQueryHandler : IRequestHandler<GetStudentCourseCatalogQuery, PaginatedList<StudentCourseCatalogDto>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<CourseEnrollmentEntity> _enrollmentRepository;
    private readonly IRepository<EducationalContentEntity> _educationalContentRepository;
    private readonly IRepository<Exam> _examRepository;

    public GetStudentCourseCatalogQueryHandler(
        IRepository<Course> courseRepository,
        IRepository<CourseEnrollmentEntity> enrollmentRepository,
        IRepository<EducationalContentEntity> educationalContentRepository,
        IRepository<Exam> examRepository)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _educationalContentRepository = educationalContentRepository;
        _examRepository = examRepository;
    }

    public async Task<PaginatedList<StudentCourseCatalogDto>> Handle(GetStudentCourseCatalogQuery request, CancellationToken cancellationToken)
    {
        var query = _courseRepository.GetAll()
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .Include(c => c.Chapters)
                .ThenInclude(ch => ch.SubChapters)
            .Include(c => c.Classes)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        // Get all courses with their related data
        var courses = await query
            .OrderBy(c => c.Order)
            .ThenBy(c => c.Title)
            .ToListAsync(cancellationToken);

        // Get student's enrollments
        var studentEnrollments = await _enrollmentRepository.GetAll()
            .Where(e => e.StudentId == request.StudentId && e.IsActive)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);

        // Get educational content counts per course
        var educationalContentCounts = await _educationalContentRepository.GetAll()
            .Include(ec => ec.SubChapter)
                .ThenInclude(sc => sc.Chapter)
            .Where(ec => ec.IsActive)
            .GroupBy(ec => ec.SubChapter.Chapter.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Get exam counts per course (exams are not directly linked to courses, so we'll count all active exams)
        var totalExamCount = await _examRepository.GetAll()
            .Where(e => e.IsActive)
            .CountAsync(cancellationToken);

        // Convert to DTOs
        var courseDtos = courses.Select(c => new StudentCourseCatalogDto
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
            DisciplineType = c.DisciplineType,
            
            // Calculate statistics
            ModuleCount = c.Modules?.Count ?? 0,
            LessonCount = c.Modules?.Sum(m => m.Lessons?.Count ?? 0) ?? 0,
            ChapterCount = c.Chapters?.Count ?? 0,
            SubChapterCount = c.Chapters?.Sum(ch => ch.SubChapters?.Count ?? 0) ?? 0,
            EducationalContentCount = educationalContentCounts.FirstOrDefault(ec => ec.CourseId == c.Id)?.Count ?? 0,
            ExamCount = totalExamCount, // For now, show total exams as exams are not course-specific
            ClassCount = c.Classes?.Count ?? 0,
            
            // Enrollment status
            IsEnrolled = studentEnrollments.Contains(c.Id),
            CanEnroll = c.IsActive && !studentEnrollments.Contains(c.Id)
        }).ToList();

        return await PaginatedList<StudentCourseCatalogDto>.CreateAsync(
            courseDtos.AsQueryable(),
            request.PageNumber,
            request.PageSize);
    }
}
