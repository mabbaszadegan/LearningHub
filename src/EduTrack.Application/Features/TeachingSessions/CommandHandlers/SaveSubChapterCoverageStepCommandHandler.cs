using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EduTrack.Application.Features.TeachingSessions.CommandHandlers;

public class SaveSubChapterCoverageStepCommandHandler : IRequestHandler<SaveSubChapterCoverageStepCommand, Result>
{
    private readonly ITeachingSessionTopicCoverageRepository _topicCoverageRepository;
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ITeachingPlanProgressRepository _teachingPlanProgressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveSubChapterCoverageStepCommandHandler(
        ITeachingSessionTopicCoverageRepository topicCoverageRepository,
        ITeachingSessionReportRepository sessionReportRepository,
        ITeachingPlanProgressRepository teachingPlanProgressRepository,
        IUnitOfWork unitOfWork)
    {
        _topicCoverageRepository = topicCoverageRepository;
        _sessionReportRepository = sessionReportRepository;
        _teachingPlanProgressRepository = teachingPlanProgressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SaveSubChapterCoverageStepCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result.Failure("جلسه یافت نشد.");
            }

            // Get existing subchapter coverage records for this session
            var existingCoverages = await _topicCoverageRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            var subChapterCoverages = existingCoverages.Where(ec => ec.TopicType == "SubTopic").ToList();

            // Collect all group IDs from the current request
            var requestGroupIds = request.CoverageData.GroupCoverages.Select(gc => gc.GroupId).ToHashSet();

            // Delete only subchapter coverages for groups that are in the current request
            var coveragesToDelete = subChapterCoverages.Where(c => requestGroupIds.Contains(c.StudentGroupId)).ToList();
            foreach (var coverage in coveragesToDelete)
            {
                await _topicCoverageRepository.DeleteAsync(coverage, cancellationToken);
            }

            // Add new subchapter coverage records only for subchapters that have data
            foreach (var groupCoverage in request.CoverageData.GroupCoverages)
            {
                foreach (var subChapterCoverage in groupCoverage.SubChapterCoverages)
                {
                    // Only save if user has provided data (checked, has percentage, status, or notes)
                    if (subChapterCoverage.WasCovered ||
                        subChapterCoverage.CoveragePercentage > 0 ||
                        subChapterCoverage.CoverageStatus > 0 ||
                        !string.IsNullOrWhiteSpace(subChapterCoverage.TeacherNotes) ||
                        !string.IsNullOrWhiteSpace(subChapterCoverage.Challenges))
                    {
                        var coverage = new TeachingSessionTopicCoverage
                        {
                            TeachingSessionReportId = request.SessionId,
                            StudentGroupId = groupCoverage.GroupId,
                            TopicType = "SubTopic",
                            TopicId = subChapterCoverage.SubChapterId,
                            TopicTitle = subChapterCoverage.SubChapterTitle,
                            WasPlanned = subChapterCoverage.WasPlanned,
                            WasCovered = subChapterCoverage.WasCovered,
                            CoveragePercentage = subChapterCoverage.CoveragePercentage,
                            CoverageStatus = subChapterCoverage.CoverageStatus,
                            TeacherNotes = subChapterCoverage.TeacherNotes,
                            Challenges = subChapterCoverage.Challenges,
                            CreatedAt = DateTimeOffset.UtcNow
                        };

                        await _topicCoverageRepository.AddAsync(coverage, cancellationToken);
                    }
                }
            }

            // Update session report
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var completionData = JsonConvert.SerializeObject(request.CoverageData, jsonSettings);
            var stepCompletions = string.IsNullOrEmpty(sessionReport.StepCompletionsJson)
                ? new Dictionary<int, string>()
                : JsonConvert.DeserializeObject<Dictionary<int, string>>(sessionReport.StepCompletionsJson) ?? new Dictionary<int, string>();

            stepCompletions[3] = completionData; // Step 3 is subchapter coverage
            sessionReport.StepCompletionsJson = JsonConvert.SerializeObject(stepCompletions, jsonSettings);
            sessionReport.CurrentStep = 3; // All steps completed
            sessionReport.IsCompleted = true;
            sessionReport.UpdatedAt = DateTimeOffset.UtcNow;

            await _sessionReportRepository.UpdateAsync(sessionReport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update TeachingPlanProgresses for each covered subchapter
            await UpdateTeachingPlanProgresses(sessionReport.TeachingPlanId, request.CoverageData, cancellationToken);


            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"خطا در ذخیره پوشش زیرمباحث: {ex.Message}");
        }
    }

    private async Task UpdateTeachingPlanProgresses(int teachingPlanId, SubChapterCoverageStepDataDto coverageData, CancellationToken cancellationToken)
    {
        foreach (var groupCoverage in coverageData.GroupCoverages)
        {
            foreach (var subChapterCoverage in groupCoverage.SubChapterCoverages)
            {
                // Only update progress for subchapters that were actually covered
                if (subChapterCoverage.WasCovered ||
                    subChapterCoverage.CoveragePercentage > 0 ||
                    subChapterCoverage.CoverageStatus > 0)
                {
                    // Get or create TeachingPlanProgress record
                    var progress = await _teachingPlanProgressRepository.GetByPlanSubTopicAndGroupAsync(
                        teachingPlanId,
                        subChapterCoverage.SubChapterId,
                        groupCoverage.GroupId,
                        cancellationToken);

                    if (progress == null)
                    {
                        // Create new progress record
                        progress = new TeachingPlanProgress
                        {
                            TeachingPlanId = teachingPlanId,
                            SubTopicId = subChapterCoverage.SubChapterId,
                            StudentGroupId = groupCoverage.GroupId,
                            OverallStatus = CalculateOverallStatus(subChapterCoverage.CoverageStatus),
                            FirstTaughtDate = DateTimeOffset.UtcNow,
                            LastTaughtDate = DateTimeOffset.UtcNow,
                            SessionsCount = 1,
                            OverallProgressPercentage = subChapterCoverage.CoveragePercentage,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        };

                        await _teachingPlanProgressRepository.AddAsync(progress, cancellationToken);
                    }
                    else
                    {
                        // Update existing progress record
                        progress.LastTaughtDate = DateTimeOffset.UtcNow;
                        progress.SessionsCount = await CalculateSessionsCount(teachingPlanId, subChapterCoverage.SubChapterId, groupCoverage.GroupId, cancellationToken);
                        progress.OverallStatus = CalculateOverallStatus(subChapterCoverage.CoverageStatus);
                        progress.OverallProgressPercentage = Math.Max(progress.OverallProgressPercentage, subChapterCoverage.CoveragePercentage);
                        progress.UpdatedAt = DateTimeOffset.UtcNow;

                        await _teachingPlanProgressRepository.UpdateAsync(progress, cancellationToken);
                    }
                }
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> CalculateSessionsCount(int teachingPlanId, int subTopicId, int groupId, CancellationToken cancellationToken)
    {
        // Get all session reports for this teaching plan
        var sessionReports = await _sessionReportRepository.GetByPlanIdAsync(teachingPlanId, cancellationToken);

        int sessionsCount = 0;

        foreach (var sessionReport in sessionReports)
        {
            // Get coverages for this session
            var sessionCoverages = await _topicCoverageRepository.GetBySessionIdAsync(sessionReport.Id, cancellationToken);

            // Check if this subtopic was covered for this group in this session
            var hasCoverage = sessionCoverages.Any(c =>
                c.TopicType == "SubTopic" &&
                c.TopicId == subTopicId &&
                c.StudentGroupId == groupId &&
                (c.WasCovered || c.CoveragePercentage > 0 || c.CoverageStatus > 0));

            if (hasCoverage)
            {
                sessionsCount++;
            }
        }

        return sessionsCount;
    }

    private int CalculateOverallStatus(int coverageStatus)
    {
        // Map coverage status to overall status
        return coverageStatus switch
        {
            0 => 0, // NotStarted
            1 => 1, // InProgress
            2 => 2, // Completed
            3 => 3, // NeedsReview
            _ => 1   // Default to InProgress
        };
    }
}
