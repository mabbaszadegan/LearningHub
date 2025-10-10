using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Repositories;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetSessionCompletionProgressQueryHandler : IRequestHandler<GetSessionCompletionProgressQuery, Result<SessionCompletionProgressDto>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;

    public GetSessionCompletionProgressQueryHandler(ITeachingSessionReportRepository sessionReportRepository)
    {
        _sessionReportRepository = sessionReportRepository;
    }

    public async Task<Result<SessionCompletionProgressDto>> Handle(GetSessionCompletionProgressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<SessionCompletionProgressDto>.Failure("جلسه یافت نشد.");
            }

            var progress = new SessionCompletionProgressDto
            {
                SessionId = request.SessionId,
                CurrentStep = sessionReport.CurrentStep,
                IsCompleted = sessionReport.IsCompleted,
                Steps = new List<StepCompletionDto>()
            };

            // Define steps
            var steps = new[]
            {
                new { Number = 1, Name = "attendance", Title = "حضور و غیاب", Description = "ثبت حضور و غیاب دانش‌آموزان هر گروه" },
                new { Number = 2, Name = "feedback", Title = "بازخورد گروه‌ها", Description = "ارائه بازخورد برای هر گروه" },
                new { Number = 3, Name = "subchapter-coverage", Title = "پوشش زیرمباحث", Description = "ثبت پوشش زیرمباحث برای هر گروه" }
            };

            // Check step completion status based on related tables
            foreach (var step in steps)
            {
                var isCompleted = false;
                var isCurrent = step.Number == progress.CurrentStep;
                DateTimeOffset? completedAt = null;

                switch (step.Number)
                {
                    case 1: // Attendance step
                        isCompleted = sessionReport.Attendance.Any();
                        completedAt = sessionReport.Attendance.Any() ? DateTimeOffset.UtcNow : null;
                        break;
                    case 2: // Feedback step
                        isCompleted = sessionReport.Executions.Any();
                        completedAt = sessionReport.Executions.Any() ? sessionReport.Executions.Min(e => e.CompletedAt) : null;
                        break;
                    case 3: // Subchapter coverage step
                        isCompleted = sessionReport.TopicCoverages.Any();
                        completedAt = sessionReport.TopicCoverages.Any() ? sessionReport.TopicCoverages.Min(tc => tc.CreatedAt) : null;
                        break;
                }

                progress.Steps.Add(new StepCompletionDto
                {
                    StepNumber = step.Number,
                    StepName = step.Name,
                    StepTitle = step.Title,
                    StepDescription = step.Description,
                    IsCompleted = isCompleted,
                    IsCurrent = isCurrent,
                    CompletedAt = completedAt,
                    CompletionData = null // No longer needed since we query tables directly
                });
            }

            return Result<SessionCompletionProgressDto>.Success(progress);
        }
        catch (Exception ex)
        {
            return Result<SessionCompletionProgressDto>.Failure($"خطا در دریافت پیشرفت تکمیل: {ex.Message}");
        }
    }
}

public class GetStepCompletionDataQueryHandler : IRequestHandler<GetStepCompletionDataQuery, Result<StepCompletionDataDto>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;

    public GetStepCompletionDataQueryHandler(ITeachingSessionReportRepository sessionReportRepository)
    {
        _sessionReportRepository = sessionReportRepository;
    }

    public async Task<Result<StepCompletionDataDto>> Handle(GetStepCompletionDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<StepCompletionDataDto>.Failure("جلسه یافت نشد.");
            }

            // Check step completion based on related tables
            var isCompleted = false;
            switch (request.StepNumber)
            {
                case 1: // Attendance step
                    isCompleted = sessionReport.Attendance.Any();
                    break;
                case 2: // Feedback step
                    isCompleted = sessionReport.Executions.Any();
                    break;
                case 3: // Subchapter coverage step
                    isCompleted = sessionReport.TopicCoverages.Any();
                    break;
            }

            var stepData = new StepCompletionDataDto
            {
                SessionId = request.SessionId,
                StepNumber = request.StepNumber,
                StepName = GetStepName(request.StepNumber),
                CompletionData = string.Empty, // No longer needed since we query tables directly
                IsCompleted = isCompleted
            };

            return Result<StepCompletionDataDto>.Success(stepData);
        }
        catch (Exception ex)
        {
            return Result<StepCompletionDataDto>.Failure($"خطا در دریافت داده مرحله: {ex.Message}");
        }
    }

    private static string GetStepName(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "attendance",
            2 => "feedback",
            3 => "subchapter-coverage",
            _ => "unknown"
        };
    }
}
