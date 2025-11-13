using System;
using System.Collections.Generic;
using System.Linq;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Extensions;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

public class GetScheduleItemsBySessionReportQueryHandler
    : IRequestHandler<GetScheduleItemsBySessionReportQuery, Result<List<ScheduleItemDto>>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetScheduleItemsBySessionReportQueryHandler(
        IScheduleItemRepository scheduleItemRepository,
        ITeachingSessionReportRepository sessionReportRepository,
        IStudentGroupRepository studentGroupRepository,
        ICurrentUserService currentUserService)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _sessionReportRepository = sessionReportRepository;
        _studentGroupRepository = studentGroupRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ScheduleItemDto>>> Handle(
        GetScheduleItemsBySessionReportQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(teacherId))
        {
            return Result<List<ScheduleItemDto>>.Failure("کاربر احراز هویت نشده است.");
        }

        var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionReportId, cancellationToken);
        if (sessionReport == null)
        {
            return Result<List<ScheduleItemDto>>.Failure("گزارش جلسه یافت نشد.");
        }

        if (!string.Equals(sessionReport.CreatedByTeacherId, teacherId, StringComparison.OrdinalIgnoreCase))
        {
            return Result<List<ScheduleItemDto>>.Failure("دسترسی به این گزارش جلسه مجاز نیست.");
        }

        var scheduleItems = await _scheduleItemRepository
            .GetScheduleItemsByTeachingPlanAsync(sessionReport.TeachingPlanId, cancellationToken);

        var groups = (await _studentGroupRepository
                .GetGroupsByTeachingPlanAsync(sessionReport.TeachingPlanId, cancellationToken))
            .ToDictionary(g => g.Id, g => g.Name);

        var sessionItems = scheduleItems
            .Where(item => item.SessionReportId == sessionReport.Id)
            .ToList();

        var dtoList = sessionItems
            .Select(item => MapToDto(item, groups))
            .OrderBy(item => item.StartDate)
            .ToList();

        return Result<List<ScheduleItemDto>>.Success(dtoList);
    }

    private static ScheduleItemDto MapToDto(
        ScheduleItem item,
        IReadOnlyDictionary<int, string> groups)
    {
        var status = GetStatus(item);

        var groupName = string.Empty;
        if (item.GroupId.HasValue && groups.TryGetValue(item.GroupId.Value, out var primaryGroupName))
        {
            groupName = primaryGroupName;
        }

        var studentAssignments = item.StudentAssignments
            .Select(sa => new ScheduleItemStudentAssignmentDto
            {
                Id = sa.Id,
                ScheduleItemId = sa.ScheduleItemId,
                StudentProfileId = sa.StudentProfileId,
                StudentUserId = sa.StudentProfile?.UserId ?? string.Empty,
                StudentDisplayName = GetStudentDisplayName(sa),
                CreatedAt = sa.CreatedAt
            })
            .ToList();

        return new ScheduleItemDto
        {
            Id = item.Id,
            TeachingPlanId = item.TeachingPlanId,
            CourseId = item.CourseId ?? item.TeachingPlan?.CourseId,
            GroupId = item.GroupId,
            GroupName = groupName,
            GroupIds = item.GroupAssignments.Select(ga => ga.StudentGroupId).ToList(),
            SubChapterIds = item.SubChapterAssignments.Select(sca => sca.SubChapterId).ToList(),
            StudentAssignments = studentAssignments,
            StudentProfileIds = item.StudentAssignments.Select(sa => sa.StudentProfileId).ToList(),
            Type = item.Type,
            TypeName = item.Type.GetDisplayName(),
            Title = item.Title,
            Description = item.Description,
            StartDate = item.StartDate,
            DueDate = item.DueDate,
            PersianStartDate = string.Empty,
            PersianDueDate = string.Empty,
            IsMandatory = item.IsMandatory,
            ContentJson = item.ContentJson,
            MaxScore = item.MaxScore,
            SessionReportId = item.SessionReportId,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            Status = status,
            StatusText = GetStatusText(status),
            CurrentStep = item.CurrentStep,
            IsCompleted = item.IsCompleted
        };
    }

    private static string GetStudentDisplayName(ScheduleItemStudentAssignment assignment)
    {
        var profile = assignment.StudentProfile;
        if (profile == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            return profile.DisplayName;
        }

        var user = profile.User;
        if (user == null)
        {
            return string.Empty;
        }

        var parts = new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (parts.Length > 0)
        {
            return string.Join(' ', parts);
        }

        return user.UserName ?? string.Empty;
    }

    private static ScheduleItemStatus GetStatus(ScheduleItem item)
    {
        var now = DateTimeOffset.UtcNow;

        if (item.IsCompleted)
        {
            return ScheduleItemStatus.Completed;
        }

        if (item.DueDate.HasValue && now > item.DueDate.Value)
        {
            return ScheduleItemStatus.Expired;
        }

        if (now >= item.StartDate && (!item.DueDate.HasValue || now <= item.DueDate.Value))
        {
            return ScheduleItemStatus.Active;
        }

        if (now < item.StartDate)
        {
            return ScheduleItemStatus.Published;
        }

        return ScheduleItemStatus.Draft;
    }

    private static string GetStatusText(ScheduleItemStatus status)
    {
        return status switch
        {
            ScheduleItemStatus.Draft => "پیش‌نویس",
            ScheduleItemStatus.Published => "منتشر شده",
            ScheduleItemStatus.Active => "فعال",
            ScheduleItemStatus.Completed => "تکمیل شده",
            ScheduleItemStatus.Expired => "منقضی شده",
            _ => "نامشخص"
        };
    }
}

