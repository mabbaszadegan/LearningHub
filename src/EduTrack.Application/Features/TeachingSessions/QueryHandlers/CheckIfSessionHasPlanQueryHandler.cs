using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class CheckIfSessionHasPlanQueryHandler : IRequestHandler<CheckIfSessionHasPlanQuery, Result<bool>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;

    public CheckIfSessionHasPlanQueryHandler(ITeachingSessionReportRepository sessionReportRepository)
    {
        _sessionReportRepository = sessionReportRepository;
    }

    public async Task<Result<bool>> Handle(CheckIfSessionHasPlanQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var hasPlan = await _sessionReportRepository.HasPlanningDataAsync(request.SessionId, cancellationToken);
            return Result<bool>.Success(hasPlan);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"خطا در بررسی برنامه‌ریزی: {ex.Message}");
        }
    }
}
