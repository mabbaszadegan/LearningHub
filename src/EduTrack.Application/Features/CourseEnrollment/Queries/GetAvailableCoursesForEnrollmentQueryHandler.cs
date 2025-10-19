using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.CourseEnrollment.Queries;

/// <summary>
/// Handler for GetAvailableCoursesForEnrollmentQuery
/// </summary>
public class GetAvailableCoursesForEnrollmentQueryHandler : IRequestHandler<GetAvailableCoursesForEnrollmentQuery, Result<PaginatedList<CourseDto>>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;

    public GetAvailableCoursesForEnrollmentQueryHandler(
        IRepository<Course> courseRepository,
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<Result<PaginatedList<CourseDto>>> Handle(GetAvailableCoursesForEnrollmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get courses that student is already enrolled in
            var enrolledCourseIds = await _enrollmentRepository.GetAll()
                .Where(e => e.StudentId == request.StudentId && e.IsActive)
                .Select(e => e.CourseId)
                .ToListAsync(cancellationToken);

            // Get all active courses that student is not enrolled in
            var coursesQuery = _courseRepository.GetAll()
                .Where(c => c.IsActive && !enrolledCourseIds.Contains(c.Id))
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.SubChapters)
                .Include(c => c.Classes)
                .Include(c => c.TeachingPlans)
                    .ThenInclude(tp => tp.ScheduleItems);

            // Get total count
            var totalCount = await coursesQuery.CountAsync(cancellationToken);

            // Apply pagination
            var courses = await coursesQuery
                .OrderBy(c => c.Order)
                .ThenBy(c => c.Title)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Convert to DTOs
            var courseDtos = courses.Select(c => new CourseDto
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
                ModuleCount = c.Chapters?.Sum(ch => ch.SubChapters?.Count ?? 0) ?? 0, // تعداد زیرمبحث
                LessonCount = c.TeachingPlans?.Sum(tp => tp.ScheduleItems?.Count ?? 0) ?? 0, // تعداد محتوا (ScheduleItems)
                ChapterCount = c.Chapters?.Count ?? 0, // تعداد مبحث
                ClassCount = c.Classes?.Count ?? 0, // تعداد کلاس
                Modules = c.Modules?.Select(m => new ModuleDto
                {
                    Id = m.Id,
                    CourseId = m.CourseId,
                    Title = m.Title,
                    Description = m.Description,
                    IsActive = m.IsActive,
                    Order = m.Order,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    LessonCount = m.Lessons?.Count ?? 0,
                    Lessons = m.Lessons?.Select(l => new LessonDto
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
                        Resources = new List<ResourceDto>()
                    }).ToList() ?? new List<LessonDto>()
                }).ToList() ?? new List<ModuleDto>()
            }).ToList();

            var paginatedResult = new PaginatedList<CourseDto>(
                courseDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<CourseDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<CourseDto>>.Failure($"خطا در بارگذاری دوره‌ها: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for GetCourseEnrollmentQuery
/// </summary>
public class GetCourseEnrollmentQueryHandler : IRequestHandler<GetCourseEnrollmentQuery, Result<CourseEnrollmentDto>>
{
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<User> _userRepository;

    public GetCourseEnrollmentQueryHandler(
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository,
        IRepository<Course> courseRepository,
        IRepository<User> userRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<CourseEnrollmentDto>> Handle(GetCourseEnrollmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var enrollment = await _enrollmentRepository.GetAll()
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e =>
                                        e.CourseId == request.CourseId &&
                                        e.StudentId == request.StudentId &&
                                        e.IsActive,
                                        cancellationToken);

            if (enrollment == null)
            {
                return Result<CourseEnrollmentDto>.Failure("Student is not enrolled in this course");
            }

            var enrollmentDto = new CourseEnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                StudentName = $"{enrollment.Student?.FirstName} {enrollment.Student?.LastName}".Trim(),
                CourseTitle = enrollment.Course?.Title ?? "",
                EnrolledAt = enrollment.EnrolledAt,
                CompletedAt = enrollment.CompletedAt,
                IsActive = enrollment.IsActive,
                LastAccessedAt = enrollment.LastAccessedAt,
                ProgressPercentage = 0, // TODO: Calculate actual progress
                IsCompleted = enrollment.IsCompleted,
                Duration = enrollment.CompletedAt.HasValue ? enrollment.CompletedAt.Value - enrollment.EnrolledAt : null,
                TimeSinceLastAccess = enrollment.LastAccessedAt.HasValue ? DateTimeOffset.Now - enrollment.LastAccessedAt.Value : TimeSpan.Zero
            };

            return Result<CourseEnrollmentDto>.Success(enrollmentDto);
        }
        catch (Exception ex)
        {
            return Result<CourseEnrollmentDto>.Failure($"خطا در بارگذاری اطلاعات ثبت‌نام: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for CanEnrollInCourseQuery
/// </summary>
public class CanEnrollInCourseQueryHandler : IRequestHandler<CanEnrollInCourseQuery, Result<bool>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;

    public CanEnrollInCourseQueryHandler(
        IRepository<Course> courseRepository,
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<Result<bool>> Handle(CanEnrollInCourseQuery request, CancellationToken cancellationToken)
    {
        // Check if course exists and is active
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            return Result<bool>.Success(false);
        }

        // Check if student has any enrollment record (active or inactive)
        // Once a student unenrolls, they cannot re-enroll to maintain restriction
        var existingEnrollment = await _enrollmentRepository.GetAll()
            .FirstOrDefaultAsync(e => e.CourseId == request.CourseId && e.StudentId == request.StudentId && e.IsActive, cancellationToken);

        if (existingEnrollment != null)
        {
            return Result<bool>.Success(false);
        }

        return Result<bool>.Success(true);
    }
}

/// <summary>
/// Handler for GetStudentCourseProgressQuery
/// </summary>
public class GetStudentCourseProgressQueryHandler : IRequestHandler<GetStudentCourseProgressQuery, Result<StudentCourseProgressDto>>
{
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<Domain.Entities.Progress> _progressRepository;

    public GetStudentCourseProgressQueryHandler(
        IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository,
        IRepository<Course> courseRepository,
        IRepository<Domain.Entities.Progress> progressRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _progressRepository = progressRepository;
    }

    public async Task<Result<StudentCourseProgressDto>> Handle(GetStudentCourseProgressQuery request, CancellationToken cancellationToken)
    {
        // Check if student is enrolled
        var enrollment = await _enrollmentRepository.GetAll()
            .Include(e => e.Course)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.CourseId == request.CourseId && e.StudentId == request.StudentId, cancellationToken);

        if (enrollment == null)
        {
            return Result<StudentCourseProgressDto>.Failure("Student is not enrolled in this course");
        }

        // Get course with modules and lessons
        var course = await _courseRepository.GetAll()
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            return Result<StudentCourseProgressDto>.Failure("Course not found");
        }

        // Get student's progress for lessons in this course
        var lessonIds = course.Modules?.SelectMany(m => m.Lessons?.Select(l => l.Id) ?? new List<int>()) ?? new List<int>();

        var progresses = await _progressRepository.GetAll()
            .Where(p => p.StudentId == request.StudentId && lessonIds.Contains(p.LessonId ?? 0))
            .ToListAsync(cancellationToken);

        var completedLessons = progresses.Count(p => p.IsCompleted);
        var totalLessons = lessonIds.Count();

        var progressPercentage = totalLessons > 0 ? (int)((double)completedLessons / totalLessons * 100) : 0;

        var result = new StudentCourseProgressDto
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            StudentId = request.StudentId,
            StudentName = enrollment.Student?.FirstName + " " + enrollment.Student?.LastName,
            ProgressPercentage = progressPercentage,
            EnrolledAt = enrollment.EnrolledAt,
            LastAccessedAt = enrollment.LastAccessedAt,
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            TotalExams = 0, // TODO: Implement exam progress
            CompletedExams = 0,
            AverageScore = 0, // TODO: Calculate average score
            LessonProgress = new List<LessonProgressDto>(),
            ExamProgress = new List<ExamProgressDto>()
        };

        return Result<StudentCourseProgressDto>.Success(result);
    }
}
