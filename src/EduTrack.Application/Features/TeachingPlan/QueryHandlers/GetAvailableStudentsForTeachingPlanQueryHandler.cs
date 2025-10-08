using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetAvailableStudentsForTeachingPlanQueryHandler : IRequestHandler<GetAvailableStudentsForTeachingPlanQuery, Result<List<UserDto>>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly IRepository<EduTrack.Domain.Entities.CourseEnrollment> _courseEnrollmentRepository;

    public GetAvailableStudentsForTeachingPlanQueryHandler(
        ITeachingPlanRepository teachingPlanRepository,
        IRepository<EduTrack.Domain.Entities.CourseEnrollment> courseEnrollmentRepository)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _courseEnrollmentRepository = courseEnrollmentRepository;
    }

    public async Task<Result<List<UserDto>>> Handle(GetAvailableStudentsForTeachingPlanQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the teaching plan to find the course
            var teachingPlan = await _teachingPlanRepository.GetTeachingPlanWithAllAsync(request.TeachingPlanId, cancellationToken);
            if (teachingPlan == null)
            {
                return Result<List<UserDto>>.Failure("Teaching plan not found");
            }

            // Get all students enrolled in the course
            var enrolledStudents = await _courseEnrollmentRepository.GetAll()
                .Where(ce => ce.CourseId == teachingPlan.CourseId && ce.IsActive)
                .Select(ce => ce.Student)
                .ToListAsync(cancellationToken);

            // Get all students already in groups of this teaching plan
            var studentsInGroups = teachingPlan.Groups
                .SelectMany(g => g.Members)
                .Select(m => m.StudentId)
                .ToHashSet();

            // Filter out students who are already in groups
            var availableStudents = enrolledStudents
                .Where(s => !studentsInGroups.Contains(s.Id))
                .Select(s => new UserDto
                {
                    Id = s.Id,
                    UserName = s.UserName ?? "",
                    Email = s.Email ?? "",
                    FullName = s.FirstName + " " + s.LastName,
                    Role = UserRole.Student, // Default to Student role
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                })
                .OrderBy(s => s.FullName)
                .ToList();

            return Result<List<UserDto>>.Success(availableStudents);
        }
        catch (Exception ex)
        {
            return Result<List<UserDto>>.Failure($"Error retrieving available students: {ex.Message}");
        }
    }
}
