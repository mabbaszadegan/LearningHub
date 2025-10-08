using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class AddGroupMembersCommandHandler : IRequestHandler<AddGroupMembersCommand, Result<bool>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IRepository<GroupMember> _groupMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddGroupMembersCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        IRepository<GroupMember> groupMemberRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _studentGroupRepository = studentGroupRepository;
        _groupMemberRepository = groupMemberRepository;
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
            .Select(m => m.StudentId)
            .ToHashSet();

        var addedCount = 0;
        var errors = new List<string>();

        // Handle both single student ID and comma-separated IDs
        var studentIds = request.StudentIds;
        if (studentIds.Count == 1 && studentIds[0].Contains(','))
        {
            studentIds = studentIds[0].Split(',').Select(s => s.Trim()).ToList();
        }

        foreach (var studentId in studentIds)
        {
            // Check if student is already in this group
            if (group.HasStudent(studentId))
            {
                continue; // Skip if already in this group
            }

            // Check if student is already in another group of this teaching plan
            if (studentsInOtherGroups.Contains(studentId))
            {
                errors.Add($"Student is already assigned to another group in this teaching plan");
                continue;
            }

            var member = GroupMember.Create(request.GroupId, studentId);
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
