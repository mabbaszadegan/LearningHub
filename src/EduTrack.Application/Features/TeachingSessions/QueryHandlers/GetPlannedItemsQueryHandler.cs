using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetPlannedItemsQueryHandler : IRequestHandler<GetPlannedItemsQuery, Result<List<PlannedItemDto>>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;

    public GetPlannedItemsQueryHandler(ITeachingSessionReportRepository sessionReportRepository)
    {
        _sessionReportRepository = sessionReportRepository;
    }

    public async Task<Result<List<PlannedItemDto>>> Handle(GetPlannedItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var plannedItems = await _sessionReportRepository.GetPlannedItemsAsync(request.SessionId, cancellationToken);
            
            var plannedItemDtos = plannedItems.Select(p => new PlannedItemDto
            {
                StudentGroupId = p.StudentGroupId,
                PlannedObjectives = p.PlannedObjectives,
                PlannedSubTopics = ParseJsonArray(p.PlannedSubTopicsJson),
                PlannedLessons = ParseJsonArray(p.PlannedLessonsJson),
                AdditionalTopics = p.AdditionalTopics
            }).ToList();

            return Result<List<PlannedItemDto>>.Success(plannedItemDtos);
        }
        catch (Exception ex)
        {
            return Result<List<PlannedItemDto>>.Failure($"خطا در بارگذاری برنامه‌ریزی: {ex.Message}");
        }
    }

    private static List<int> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<int>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }
}
