using System;
using System.Linq;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.CourseEnrollment.Queries;

public class GetCourseStudentsQueryHandler : IRequestHandler<GetCourseStudentsQuery, Result<PaginatedList<CourseEnrollmentDto>>>
{
    private readonly IRepository<Domain.Entities.CourseEnrollment> _enrollmentRepository;

    public GetCourseStudentsQueryHandler(IRepository<Domain.Entities.CourseEnrollment> enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<Result<PaginatedList<CourseEnrollmentDto>>> Handle(GetCourseStudentsQuery request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0)
        {
            return Result<PaginatedList<CourseEnrollmentDto>>.Failure("Course ID must be greater than 0");
        }

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = _enrollmentRepository
            .GetAll()
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.StudentProfile)
            .Where(e => e.CourseId == request.CourseId);

        if (!request.IncludeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        if (!request.IncludeCompleted)
        {
            query = query.Where(e => e.CompletedAt == null);
        }

        query = query
            .OrderBy(e => e.Student.LastName)
            .ThenBy(e => e.Student.FirstName)
            .ThenBy(e => e.EnrolledAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var enrollments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var items = enrollments.Select(enrollment =>
        {
            var student = enrollment.Student;
            var fullName = student?.FullName;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = string.Join(' ', new[] { student?.FirstName, student?.LastName }
                    .Where(x => !string.IsNullOrWhiteSpace(x)))
                    .Trim();
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = student?.UserName ?? enrollment.StudentId;
            }

            var lastAccessedAt = enrollment.LastAccessedAt ?? enrollment.EnrolledAt;
            var duration = enrollment.CompletedAt.HasValue
                ? enrollment.CompletedAt.Value - enrollment.EnrolledAt
                : (TimeSpan?)null;

            return new CourseEnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                StudentProfileId = enrollment.StudentProfileId,
                StudentProfileName = enrollment.StudentProfile?.DisplayName,
                CourseId = enrollment.CourseId,
                StudentName = fullName,
                StudentEmail = student?.Email ?? string.Empty,
                CourseTitle = enrollment.Course?.Title ?? string.Empty,
                EnrolledAt = enrollment.EnrolledAt,
                CompletedAt = enrollment.CompletedAt,
                IsActive = enrollment.IsActive,
                LastAccessedAt = enrollment.LastAccessedAt,
                ProgressPercentage = enrollment.ProgressPercentage,
                LearningMode = enrollment.LearningMode,
                IsCompleted = enrollment.CompletedAt.HasValue,
                IsInProgress = enrollment.IsActive && !enrollment.CompletedAt.HasValue,
                Duration = duration,
                TimeSinceLastAccess = now - lastAccessedAt
            };
        }).ToList();

        var paginated = new PaginatedList<CourseEnrollmentDto>(items, totalCount, pageNumber, pageSize);

        return Result<PaginatedList<CourseEnrollmentDto>>.Success(paginated);
    }
}

