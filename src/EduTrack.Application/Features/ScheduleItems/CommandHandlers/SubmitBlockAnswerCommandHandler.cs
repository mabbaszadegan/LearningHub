using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Common.Services.Validators;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

/// <summary>
/// Handler for submitting and validating block answers
/// </summary>
public class SubmitBlockAnswerCommandHandler : IRequestHandler<SubmitBlockAnswerCommand, Result<BlockAnswerResultDto>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IScheduleItemBlockAttemptRepository _attemptRepository;
    private readonly IScheduleItemBlockStatisticsRepository _statisticsRepository;
    private readonly BlockAnswerValidatorFactory _validatorFactory;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitBlockAnswerCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        IScheduleItemBlockAttemptRepository attemptRepository,
        IScheduleItemBlockStatisticsRepository statisticsRepository,
        BlockAnswerValidatorFactory validatorFactory,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _attemptRepository = attemptRepository;
        _statisticsRepository = statisticsRepository;
        _validatorFactory = validatorFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BlockAnswerResultDto>> Handle(SubmitBlockAnswerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get schedule item
            var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.ScheduleItemId, cancellationToken);
            if (scheduleItem == null)
            {
                return Result<BlockAnswerResultDto>.Failure("Schedule item not found");
            }

            // Get validator for this schedule item type
            var validator = _validatorFactory.GetValidator(scheduleItem.Type);

            // Convert Dictionary to proper format for validator (handle JSON deserialization)
            // When JSON is deserialized to Dictionary<string, object>, arrays become List<object>
            // We need to convert it back to a format that can be properly handled
            var normalizedAnswer = NormalizeSubmittedAnswer(request.SubmittedAnswer);

            // Validate the answer
            var validationResult = await validator.ValidateAnswerAsync(
                request.ScheduleItemId,
                request.BlockId,
                normalizedAnswer,
                cancellationToken);

            // Get block metadata (instruction, order, etc.)
            var blockMetadata = await GetBlockMetadataAsync(scheduleItem, request.BlockId, cancellationToken);

            // Create attempt entity
            var attempt = ScheduleItemBlockAttempt.Create(
                scheduleItemId: request.ScheduleItemId,
                scheduleItemType: scheduleItem.Type,
                blockId: request.BlockId,
                studentId: request.StudentId,
                submittedAnswerJson: JsonConvert.SerializeObject(request.SubmittedAnswer),
                correctAnswerJson: JsonConvert.SerializeObject(validationResult.CorrectAnswer ?? new Dictionary<string, object>()),
                isCorrect: validationResult.IsCorrect,
                pointsEarned: validationResult.PointsEarned,
                maxPoints: validationResult.MaxPoints,
                blockInstruction: blockMetadata.Instruction,
                blockOrder: blockMetadata.Order);

            await _attemptRepository.AddAsync(attempt, cancellationToken);

            // Update or create statistics
            var statistics = await _statisticsRepository.GetByStudentAndBlockAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.BlockId,
                cancellationToken);

            if (statistics == null)
            {
                // Create new statistics
                statistics = ScheduleItemBlockStatistics.Create(
                    request.ScheduleItemId,
                    scheduleItem.Type,
                    request.BlockId,
                    request.StudentId,
                    blockMetadata.Instruction,
                    blockMetadata.Order);

                statistics.RecordAttempt(validationResult.IsCorrect, attempt.AttemptedAt);
                statistics.UpdateMetadata(blockMetadata.Instruction, blockMetadata.Order);

                await _statisticsRepository.AddAsync(statistics, cancellationToken);
            }
            else
            {
                // Update existing statistics
                // Entity is already tracked by EF Core (from GetByStudentAndBlockAsync with Include)
                // So we just need to modify it and SaveChanges will detect the changes
                statistics.RecordAttempt(validationResult.IsCorrect, attempt.AttemptedAt);
                statistics.UpdateMetadata(blockMetadata.Instruction, blockMetadata.Order);
                // No need to call UpdateAsync - entity is already tracked
            }

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return result
            var result = new BlockAnswerResultDto
            {
                BlockId = request.BlockId,
                IsCorrect = validationResult.IsCorrect,
                PointsEarned = validationResult.PointsEarned,
                MaxPoints = validationResult.MaxPoints,
                CorrectAnswer = validationResult.CorrectAnswer,
                SubmittedAnswer = validationResult.SubmittedAnswer,
                Feedback = validationResult.Feedback,
                DetailedFeedback = validationResult.DetailedFeedback
            };

            return Result<BlockAnswerResultDto>.Success(result);
        }
        catch (NotSupportedException ex)
        {
            return Result<BlockAnswerResultDto>.Failure($"Answer validation not supported: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<BlockAnswerResultDto>.Failure($"Error submitting answer: {ex.Message}");
        }
    }

    private Task<(string? Instruction, int? Order)> GetBlockMetadataAsync(
        Domain.Entities.ScheduleItem scheduleItem,
        string blockId,
        CancellationToken cancellationToken)
    {
        try
        {
            var contentJson = scheduleItem.ContentJson;
            if (string.IsNullOrWhiteSpace(contentJson))
            {
                return Task.FromResult<(string?, int?)>((null, null));
            }

            // Try to parse as OrderingContent
            if (scheduleItem.Type == Domain.Enums.ScheduleItemType.Ordering)
            {
                var content = JsonConvert.DeserializeObject<OrderingContent>(contentJson);
                if (content?.Blocks != null && content.Blocks.Any())
                {
                    var block = content.Blocks.FirstOrDefault(b => b.Id == blockId);
                    if (block != null)
                    {
                        return Task.FromResult<(string?, int?)>((block.Instruction, block.Order));
                    }
                }
                else if (blockId == "main" || blockId == "legacy")
                {
                    return Task.FromResult<(string?, int?)>((content?.Instruction, 0));
                }
            }

            // For other types, return default
            return Task.FromResult<(string?, int?)>((null, null));
        }
        catch
        {
            return Task.FromResult<(string?, int?)>((null, null));
        }
    }

    private Dictionary<string, object> NormalizeSubmittedAnswer(Dictionary<string, object> submittedAnswer)
    {
        // Convert Dictionary to JSON and back to JObject to ensure proper type handling
        // This ensures arrays are properly deserialized as JArray
        try
        {
            var jsonString = JsonConvert.SerializeObject(submittedAnswer);
            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);
            if (jObject != null)
            {
                // Convert JObject back to Dictionary, but keep JTokens for arrays
                var normalized = new Dictionary<string, object>();
                foreach (var prop in jObject.Properties())
                {
                    normalized[prop.Name] = prop.Value;
                }
                return normalized;
            }
        }
        catch
        {
            // If conversion fails, return original
        }

        return submittedAnswer;
    }
}

