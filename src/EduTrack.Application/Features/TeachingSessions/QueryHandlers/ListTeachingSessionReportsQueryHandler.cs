using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class ListTeachingSessionReportsQueryHandler : IRequestHandler<ListTeachingSessionReportsQuery, Result<List<TeachingSessionReportDto>>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly ICurrentUserService _currentUserService;

    public ListTeachingSessionReportsQueryHandler(
        ITeachingPlanRepository teachingPlanRepository,
        ICurrentUserService currentUserService)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<TeachingSessionReportDto>>> Handle(ListTeachingSessionReportsQuery request, CancellationToken cancellationToken)
    {
        var teacherId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(teacherId))
        {
            return Result<List<TeachingSessionReportDto>>.Failure("User is not authenticated.");
        }

        var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
        if (teachingPlan == null)
        {
            return Result<List<TeachingSessionReportDto>>.Failure($"Teaching Plan with ID {request.TeachingPlanId} not found.");
        }

        if (teachingPlan.TeacherId != teacherId)
        {
            return Result<List<TeachingSessionReportDto>>.Failure("You don't have permission to view session reports for this teaching plan.");
        }

        // This would need to be implemented with a proper repository
        // For now, return empty list
        var sessionReports = new List<TeachingSessionReportDto>();

        return Result<List<TeachingSessionReportDto>>.Success(sessionReports);
    }
}
