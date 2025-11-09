using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class AddGroupMembersCommandHandler : IRequestHandler<AddGroupMembersCommand, Result<bool>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IRepository<GroupMember> _groupMemberRepository;
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IRepository<Domain.Entities.CourseEnrollment> _courseEnrollmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddGroupMembersCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        IRepository<GroupMember> groupMemberRepository,
        IStudentProfileRepository studentProfileRepository,
        IRepository<Domain.Entities.CourseEnrollment> courseEnrollmentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _studentGroupRepository = studentGroupRepository;
        _groupMemberRepository = groupMemberRepository;
        _studentProfileRepository = studentProfileRepository;
        _courseEnrollmentRepository = courseEnrollmentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(AddGroupMembersCommand request, CancellationToken cancellationToken)
    {
        var group = await _studentGroupRepository.GetGroupWithMembersAsync(request.GroupId, cancellationToken);
        if (group == null)
        {
            return Result<bool>.Failure("Student group not found");
        }

        // Verify user has permission to manage this group
        if (group.TeachingPlan.TeacherId != _currentUserService.UserId)
        {
            return Result<bool>.Failure("You don't have permission to manage this group");
        }

        // Get all students already in groups of this teaching plan
        var studentsInOtherGroups = group.TeachingPlan.Groups
            .Where(g => g.Id != request.GroupId)
            .SelectMany(g => g.Members)
            .Select(m => m.StudentProfileId)
            .ToHashSet();

        var addedCount = 0;
        var errors = new List<string>();

        foreach (var studentProfileId in request.StudentProfileIds?.Distinct() ?? Enumerable.Empty<int>())
        {
            if (studentProfileId <= 0)
            {
                continue; // Skip if already in this group
            }

            // Check if student is already in this group
            if (group.HasStudentProfile(studentProfileId))
            {
                continue;
            }

            // Check if student is already in another group of this teaching plan
            if (studentsInOtherGroups.Contains(studentProfileId))
            {
                errors.Add($"Student is already assigned to another group in this teaching plan");
                continue;
            }

            var profile = await _studentProfileRepository.GetByIdAsync(studentProfileId, cancellationToken);
            if (profile == null)
            {
                errors.Add("Student profile not found");
                continue;
            }

            // Ensure profile is enrolled in course
            var isEnrolledInCourse = await _courseEnrollmentRepository
                .GetAll()
                .AnyAsync(
                    e => e.CourseId == group.TeachingPlan.CourseId &&
                         e.StudentProfileId == studentProfileId &&
                         e.IsActive,
                    cancellationToken);

            if (!isEnrolledInCourse)
            {
                errors.Add("Student profile is not enrolled in this course");
                continue;
            }

            var member = GroupMember.Create(request.GroupId, studentProfileId);
            await _groupMemberRepository.AddAsync(member, cancellationToken);
            addedCount++;
        }

        if (errors.Any())
        {
            return Result<bool>.Failure(string.Join("; ", errors));
        }

        if (addedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}
