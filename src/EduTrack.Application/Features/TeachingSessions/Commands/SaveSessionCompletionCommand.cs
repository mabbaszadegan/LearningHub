using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Commands;

public record SaveSessionCompletionCommand(
    int TeachingSessionReportId,
    List<GroupCompletionData> GroupCompletions,
    List<TopicCoverageData> TopicCoverages) : IRequest<Result<bool>>;

public record GroupCompletionData(
    int StudentGroupId,
    string? AchievedObjectives,
    string? AchievedSubTopicsJson,
    string? AchievedLessonsJson,
    string? AdditionalTopicsCovered,
    string? UncoveredPlannedTopics,
    string? UncoveredReasons,
    string? GroupFeedback,
    int UnderstandingLevel,
    int ParticipationLevel,
    string? Challenges,
    string? NextSessionRecommendations);

public record TopicCoverageData(
    int StudentGroupId,
    string TopicType,
    int? TopicId,
    string? TopicTitle,
    bool WasPlanned,
    bool WasCovered,
    int CoveragePercentage,
    int CoverageStatus,
    string? TeacherNotes,
    string? Challenges);
