using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Repositories;
using MediatR;
using System.Text.Json;

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
                new { Number = 3, Name = "topic-coverage", Title = "پوشش موضوعات", Description = "ثبت موضوعات پوشش داده شده برای هر گروه" }
            };

            // Parse step completions
            var stepCompletions = string.IsNullOrEmpty(sessionReport.StepCompletionsJson) 
                ? new Dictionary<int, string>() 
                : JsonSerializer.Deserialize<Dictionary<int, string>>(sessionReport.StepCompletionsJson) ?? new Dictionary<int, string>();

            foreach (var step in steps)
            {
                var isCompleted = stepCompletions.ContainsKey(step.Number);
                var isCurrent = step.Number == progress.CurrentStep;

                progress.Steps.Add(new StepCompletionDto
                {
                    StepNumber = step.Number,
                    StepName = step.Name,
                    StepTitle = step.Title,
                    StepDescription = step.Description,
                    IsCompleted = isCompleted,
                    IsCurrent = isCurrent,
                    CompletedAt = isCompleted ? DateTimeOffset.UtcNow : null,
                    CompletionData = isCompleted ? stepCompletions[step.Number] : null
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

            // Parse step completions
            var stepCompletions = string.IsNullOrEmpty(sessionReport.StepCompletionsJson) 
                ? new Dictionary<int, string>() 
                : JsonSerializer.Deserialize<Dictionary<int, string>>(sessionReport.StepCompletionsJson) ?? new Dictionary<int, string>();

            var stepData = new StepCompletionDataDto
            {
                SessionId = request.SessionId,
                StepNumber = request.StepNumber,
                StepName = GetStepName(request.StepNumber),
                CompletionData = stepCompletions.ContainsKey(request.StepNumber) ? stepCompletions[request.StepNumber] : string.Empty,
                IsCompleted = stepCompletions.ContainsKey(request.StepNumber)
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
            3 => "topic-coverage",
            _ => "unknown"
        };
    }
}
