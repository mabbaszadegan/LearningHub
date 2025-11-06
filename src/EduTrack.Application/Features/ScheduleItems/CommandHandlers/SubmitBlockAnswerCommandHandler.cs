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
using System.Text.Json;

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

            // Try to get validator based on block type first (for ordering blocks in non-ordering items)
            IBlockAnswerValidator? validator = null;
            try
            {
                var blockType = GetBlockTypeFromContent(scheduleItem.ContentJson, request.BlockId);
                if (!string.IsNullOrEmpty(blockType) && string.Equals(blockType, "ordering", StringComparison.OrdinalIgnoreCase))
                {
                    // Try to get OrderingBlockValidator (it supports Ordering type, but can work with any type)
                    validator = _validatorFactory.GetValidator(Domain.Enums.ScheduleItemType.Ordering);
                }
            }
            catch
            {
                // If block type detection fails, fall back to schedule item type
            }

            // Fall back to schedule item type validator if block type validator not found
            validator ??= _validatorFactory.GetValidator(scheduleItem.Type);

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

            // Get block metadata and full content (instruction, order, full block content, etc.)
            var blockMetadata = await GetBlockMetadataAsync(scheduleItem, request.BlockId, cancellationToken);

            // Serialize submitted and correct answers properly
            // Use validationResult.SubmittedAnswer which is already normalized by the validator
            // This ensures consistency between what was validated and what is stored
            var submittedAnswerJson = JsonConvert.SerializeObject(
                validationResult.SubmittedAnswer ?? normalizedAnswer, 
                Formatting.None);
            
            // For correct answer, use the validation result which should have proper types
            var correctAnswerJson = JsonConvert.SerializeObject(
                validationResult.CorrectAnswer ?? new Dictionary<string, object>(), 
                Formatting.None);

            // Create attempt entity
            var attempt = ScheduleItemBlockAttempt.Create(
                scheduleItemId: request.ScheduleItemId,
                scheduleItemType: scheduleItem.Type,
                blockId: request.BlockId,
                studentId: request.StudentId,
                submittedAnswerJson: submittedAnswerJson,
                correctAnswerJson: correctAnswerJson,
                isCorrect: validationResult.IsCorrect,
                pointsEarned: validationResult.PointsEarned,
                maxPoints: validationResult.MaxPoints,
                blockInstruction: blockMetadata.Instruction,
                blockOrder: blockMetadata.Order,
                blockContentJson: blockMetadata.BlockContentJson);

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

    private Task<(string? Instruction, int? Order, string? BlockContentJson)> GetBlockMetadataAsync(
        Domain.Entities.ScheduleItem scheduleItem,
        string blockId,
        CancellationToken cancellationToken)
    {
        try
        {
            var contentJson = scheduleItem.ContentJson;
            if (string.IsNullOrWhiteSpace(contentJson))
            {
                return Task.FromResult<(string?, int?, string?)>((null, null, null));
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
                        // Serialize full block content for historical preservation
                        var blockContentJson = JsonConvert.SerializeObject(block, Formatting.None);
                        return Task.FromResult<(string?, int?, string?)>((block.Instruction, block.Order, blockContentJson));
                    }
                }
                else if (blockId == "main" || blockId == "legacy")
                {
                    // For legacy format, create a block-like structure
                    if (content != null)
                    {
                        var legacyBlock = new OrderingBlock
                        {
                            Id = "main",
                            Order = 0,
                            Instruction = content.Instruction ?? string.Empty,
                            Items = content.Items ?? new List<OrderingItem>(),
                            CorrectOrder = content.CorrectOrder ?? new List<string>(),
                            AllowDragDrop = content.AllowDragDrop,
                            Direction = content.Direction,
                            ShowNumbers = content.ShowNumbers,
                            Points = content.Points,
                            IsRequired = content.IsRequired
                        };
                        var blockContentJson = JsonConvert.SerializeObject(legacyBlock, Formatting.None);
                        return Task.FromResult<(string?, int?, string?)>((content.Instruction, 0, blockContentJson));
                    }
                }
            }

            // For other types, return default
            return Task.FromResult<(string?, int?, string?)>((null, null, null));
        }
        catch
        {
            return Task.FromResult<(string?, int?, string?)>((null, null, null));
        }
    }

    private Dictionary<string, object> NormalizeSubmittedAnswer(Dictionary<string, object> submittedAnswer)
    {
        // Convert Dictionary to JSON and back to ensure proper type handling
        // Convert JTokens to actual .NET types that can be serialized
        try
        {
            var normalized = new Dictionary<string, object>();
            foreach (var kvp in submittedAnswer)
            {
                // Handle System.Text.Json JsonElement (ASP.NET Core default)
                if (kvp.Value is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        // Try to convert to List<string> first (for order arrays)
                        try
                        {
                            var stringList = jsonElement.EnumerateArray()
                                .Select(x => x.GetString() ?? x.ToString())
                                .ToList();
                            normalized[kvp.Key] = stringList;
                        }
                        catch
                        {
                            // Fallback to List<object>
                            normalized[kvp.Key] = jsonElement.EnumerateArray()
                                .Select(x => x.GetRawText())
                                .ToList();
                        }
                    }
                    else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        normalized[kvp.Key] = jsonElement.GetString() ?? string.Empty;
                    }
                    else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        if (jsonElement.TryGetInt32(out var intValue))
                            normalized[kvp.Key] = intValue;
                        else if (jsonElement.TryGetDecimal(out var decimalValue))
                            normalized[kvp.Key] = decimalValue;
                        else
                            normalized[kvp.Key] = jsonElement.GetRawText();
                    }
                    else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.True || jsonElement.ValueKind == System.Text.Json.JsonValueKind.False)
                    {
                        normalized[kvp.Key] = jsonElement.GetBoolean();
                    }
                    else
                    {
                        // For other types, serialize and deserialize
                        normalized[kvp.Key] = System.Text.Json.JsonSerializer.Deserialize<object>(jsonElement.GetRawText()) ?? new object();
                    }
                }
                // Handle Newtonsoft.Json JToken
                else if (kvp.Value is JToken jToken)
                {
                    // Convert JToken to actual .NET type
                    if (jToken.Type == JTokenType.Array)
                    {
                        // For arrays, try to convert to List<string> first (for order arrays)
                        // If that fails, convert to List<object>
                        try
                        {
                            var stringList = jToken.ToObject<List<string>>();
                            if (stringList != null)
                            {
                                normalized[kvp.Key] = stringList;
                            }
                            else
                            {
                                normalized[kvp.Key] = jToken.ToObject<List<object>>() ?? new List<object>();
                            }
                        }
                        catch
                        {
                            normalized[kvp.Key] = jToken.ToObject<List<object>>() ?? new List<object>();
                        }
                    }
                    else if (jToken.Type == JTokenType.String)
                    {
                        normalized[kvp.Key] = jToken.ToObject<string>() ?? string.Empty;
                    }
                    else if (jToken.Type == JTokenType.Integer)
                    {
                        normalized[kvp.Key] = jToken.ToObject<int>();
                    }
                    else if (jToken.Type == JTokenType.Boolean)
                    {
                        normalized[kvp.Key] = jToken.ToObject<bool>();
                    }
                    else
                    {
                        // For other types, convert to object
                        normalized[kvp.Key] = jToken.ToObject<object>() ?? new object();
                    }
                }
                else
                {
                    normalized[kvp.Key] = kvp.Value;
                }
            }
            return normalized;
        }
        catch
        {
            // If conversion fails, return original
        }

        return submittedAnswer;
    }

    private string? GetBlockTypeFromContent(string? contentJson, string blockId)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
            return null;

        try
        {
            var contentObj = JObject.Parse(contentJson);

            // Check in "blocks" array
            if (contentObj["blocks"] is JArray blocksArray)
            {
                foreach (var blockToken in blocksArray)
                {
                    var blockObj = blockToken as JObject;
                    if (blockObj == null) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    if (string.IsNullOrEmpty(currentBlockId) || !string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return blockObj["type"]?.ToString();
                }
            }

            // Check in "orderingBlocks" array (for Reminder)
            if (contentObj["orderingBlocks"] is JArray orderingBlocksArray)
            {
                foreach (var blockToken in orderingBlocksArray)
                {
                    var blockObj = blockToken as JObject;
                    if (blockObj == null) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    if (string.IsNullOrEmpty(currentBlockId) || !string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return "ordering"; // orderingBlocks always contain ordering blocks
                }
            }
        }
        catch
        {
            // If parsing fails, return null
        }

        return null;
    }
}

