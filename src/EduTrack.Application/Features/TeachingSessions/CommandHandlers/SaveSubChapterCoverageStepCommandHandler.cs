using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using System.Text.Json;

namespace EduTrack.Application.Features.TeachingSessions.CommandHandlers;

public class SaveSubChapterCoverageStepCommandHandler : IRequestHandler<SaveSubChapterCoverageStepCommand, Result>
{
    private readonly ITeachingSessionTopicCoverageRepository _topicCoverageRepository;
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveSubChapterCoverageStepCommandHandler(
        ITeachingSessionTopicCoverageRepository topicCoverageRepository,
        ITeachingSessionReportRepository sessionReportRepository,
        IUnitOfWork unitOfWork)
    {
        _topicCoverageRepository = topicCoverageRepository;
        _sessionReportRepository = sessionReportRepository;
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
            var completionData = JsonSerializer.Serialize(request.CoverageData);
            var stepCompletions = string.IsNullOrEmpty(sessionReport.StepCompletionsJson) 
                ? new Dictionary<int, string>() 
                : JsonSerializer.Deserialize<Dictionary<int, string>>(sessionReport.StepCompletionsJson) ?? new Dictionary<int, string>();

            stepCompletions[3] = completionData; // Step 3 is subchapter coverage
            sessionReport.StepCompletionsJson = JsonSerializer.Serialize(stepCompletions);
            sessionReport.CurrentStep = 3; // All steps completed
            sessionReport.IsCompleted = true;
            sessionReport.UpdatedAt = DateTimeOffset.UtcNow;

            await _sessionReportRepository.UpdateAsync(sessionReport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"خطا در ذخیره پوشش زیرمباحث: {ex.Message}");
        }
    }
}
