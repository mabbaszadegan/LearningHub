using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.CommandHandlers;

public class SaveSessionCompletionCommandHandler : IRequestHandler<SaveSessionCompletionCommand, Result<bool>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ITeachingSessionExecutionRepository _executionRepository;
    private readonly ITeachingSessionTopicCoverageRepository _topicCoverageRepository;
    private readonly ITeachingPlanProgressRepository _progressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveSessionCompletionCommandHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ITeachingSessionExecutionRepository executionRepository,
        ITeachingSessionTopicCoverageRepository topicCoverageRepository,
        ITeachingPlanProgressRepository progressRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionReportRepository = sessionReportRepository;
        _executionRepository = executionRepository;
        _topicCoverageRepository = topicCoverageRepository;
        _progressRepository = progressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(SaveSessionCompletionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.TeachingSessionReportId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<bool>.Failure("جلسه یافت نشد.");
            }

            // Save group completions
            foreach (var groupCompletion in request.GroupCompletions)
            {
                var execution = new TeachingSessionExecution
                {
                    TeachingSessionReportId = request.TeachingSessionReportId,
                    StudentGroupId = groupCompletion.StudentGroupId,
                    AchievedObjectives = groupCompletion.AchievedObjectives,
                    AchievedSubTopicsJson = groupCompletion.AchievedSubTopicsJson,
                    AchievedLessonsJson = groupCompletion.AchievedLessonsJson,
                    AdditionalTopicsCovered = groupCompletion.AdditionalTopicsCovered,
                    UncoveredPlannedTopics = groupCompletion.UncoveredPlannedTopics,
                    UncoveredReasons = groupCompletion.UncoveredReasons,
                    GroupFeedback = groupCompletion.GroupFeedback,
                    UnderstandingLevel = groupCompletion.UnderstandingLevel,
                    ParticipationLevel = groupCompletion.ParticipationLevel,
                    Challenges = groupCompletion.Challenges,
                    NextSessionRecommendations = groupCompletion.NextSessionRecommendations,
                    CompletedAt = DateTimeOffset.UtcNow
                };

                await _executionRepository.AddAsync(execution, cancellationToken);
            }

            // Save topic coverages
            foreach (var topicCoverage in request.TopicCoverages)
            {
                var coverage = new TeachingSessionTopicCoverage
                {
                    TeachingSessionReportId = request.TeachingSessionReportId,
                    StudentGroupId = topicCoverage.StudentGroupId,
                    TopicType = topicCoverage.TopicType,
                    TopicId = topicCoverage.TopicId,
                    TopicTitle = topicCoverage.TopicTitle,
                    WasPlanned = topicCoverage.WasPlanned,
                    WasCovered = topicCoverage.WasCovered,
                    CoveragePercentage = topicCoverage.CoveragePercentage,
                    CoverageStatus = topicCoverage.CoverageStatus,
                    TeacherNotes = topicCoverage.TeacherNotes,
                    Challenges = topicCoverage.Challenges,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _topicCoverageRepository.AddAsync(coverage, cancellationToken);
            }

            // Update teaching plan progress
            await UpdateTeachingPlanProgress(request, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"خطا در ذخیره گزارش: {ex.Message}");
        }
    }

    private async Task UpdateTeachingPlanProgress(SaveSessionCompletionCommand request, CancellationToken cancellationToken)
    {
        // Get session report to get teaching plan ID
        var sessionReport = await _sessionReportRepository.GetByIdAsync(request.TeachingSessionReportId, cancellationToken);
        if (sessionReport == null) return;

        // Group topic coverages by subtopic and group
        var subtopicCoverages = request.TopicCoverages
            .Where(tc => tc.TopicType == "SubTopic" && tc.TopicId.HasValue)
            .GroupBy(tc => new { SubTopicId = tc.TopicId!.Value, tc.StudentGroupId })
            .ToList();

        foreach (var group in subtopicCoverages)
        {
            var subtopicId = group.Key.SubTopicId;
            var groupId = group.Key.StudentGroupId;
            var coverages = group.ToList();

            // Calculate overall progress
            var totalCoverage = coverages.Sum(c => c.CoveragePercentage);
            var averageCoverage = coverages.Any() ? totalCoverage / coverages.Count : 0;
            var wasCovered = coverages.Any(c => c.WasCovered);

            // Get or create progress record
            var progress = await _progressRepository.GetByPlanSubTopicAndGroupAsync(
                sessionReport.TeachingPlanId, subtopicId, groupId, cancellationToken);

            if (progress == null)
            {
                progress = new TeachingPlanProgress
                {
                    TeachingPlanId = sessionReport.TeachingPlanId,
                    SubTopicId = subtopicId,
                    StudentGroupId = groupId,
                    OverallStatus = wasCovered ? 1 : 0, // 1 = InProgress, 0 = NotStarted
                    FirstTaughtDate = wasCovered ? DateTimeOffset.UtcNow : null,
                    LastTaughtDate = wasCovered ? DateTimeOffset.UtcNow : null,
                    SessionsCount = wasCovered ? 1 : 0,
                    OverallProgressPercentage = averageCoverage,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await _progressRepository.AddAsync(progress, cancellationToken);
            }
            else
            {
                // Update existing progress
                progress.LastTaughtDate = wasCovered ? DateTimeOffset.UtcNow : progress.LastTaughtDate;
                progress.SessionsCount = wasCovered ? progress.SessionsCount + 1 : progress.SessionsCount;
                progress.OverallProgressPercentage = Math.Max(progress.OverallProgressPercentage, averageCoverage);
                progress.UpdatedAt = DateTimeOffset.UtcNow;

                // Update status based on coverage
                if (wasCovered)
                {
                    if (progress.OverallStatus == 0) // NotStarted
                    {
                        progress.OverallStatus = 1; // InProgress
                        progress.FirstTaughtDate = DateTimeOffset.UtcNow;
                    }
                    
                    if (averageCoverage >= 100)
                    {
                        progress.OverallStatus = 2; // Completed
                    }
                }

                await _progressRepository.UpdateAsync(progress, cancellationToken);
            }
        }
    }
}
