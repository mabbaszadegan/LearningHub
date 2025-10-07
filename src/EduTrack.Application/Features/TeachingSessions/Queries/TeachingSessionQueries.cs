using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Queries;

public record ListTeachingSessionReportsQuery(
    int TeachingPlanId,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<List<TeachingSessionReportDto>>>;

public record GetTeachingSessionReportDetailsQuery(int ReportId) : IRequest<Result<TeachingSessionReportDto>>;
