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
using System.Linq;
using Newtonsoft.Json.Serialization;

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
                if (!string.IsNullOrEmpty(blockType))
                {
                    var normalizedBlockType = blockType.Trim().Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
                    
                    if (normalizedBlockType.Contains("ordering"))
                    {
                        // Try to get OrderingBlockValidator (it supports Ordering type, but can work with any type)
                        validator = _validatorFactory.GetValidator(Domain.Enums.ScheduleItemType.Ordering);
                    }
                    else if (normalizedBlockType.Contains("multiplechoice"))
                    {
                        // Use MultipleChoice validator for any schedule item that contains MCQ blocks
                        validator = _validatorFactory.GetValidator(Domain.Enums.ScheduleItemType.MultipleChoice);
                    }
                    else if (normalizedBlockType.Contains("matching"))
                    {
                        validator = _validatorFactory.GetValidator(Domain.Enums.ScheduleItemType.Match);
                    }
                    else if (normalizedBlockType.Contains("gapfill"))
                    {
                        validator = _validatorFactory.GetValidator(Domain.Enums.ScheduleItemType.GapFill);
                    }
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
                blockContentJson: blockMetadata.BlockContentJson,
                studentProfileId: request.StudentProfileId);

            await _attemptRepository.AddAsync(attempt, cancellationToken);

            // Update or create statistics
            var statistics = await _statisticsRepository.GetByStudentAndBlockAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.BlockId,
                request.StudentProfileId,
                cancellationToken);

            if (statistics == null)
            {
                // Create new statistics
                statistics = ScheduleItemBlockStatistics.Create(
                    request.ScheduleItemId,
                    scheduleItem.Type,
                    request.BlockId,
                    request.StudentId,
                    request.StudentProfileId,
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

            if (scheduleItem.Type == Domain.Enums.ScheduleItemType.Match)
            {
                var camelSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                MatchingContent? content = null;

                try
                {
                    content = JsonConvert.DeserializeObject<MatchingContent>(contentJson, camelSettings);
                }
                catch
                {
                    try
                    {
                        content = JsonConvert.DeserializeObject<MatchingContent>(contentJson);
                    }
                    catch
                    {
                        content = null;
                    }
                }

                if (content?.Blocks != null && content.Blocks.Any())
                {
                    var block = content.Blocks.FirstOrDefault(b => string.Equals(b.Id, blockId, StringComparison.OrdinalIgnoreCase));
                    if (block != null)
                    {
                        var blockContentJson = JsonConvert.SerializeObject(block, Formatting.None);
                        return Task.FromResult<(string?, int?, string?)>((block.Instruction, block.Order, blockContentJson));
                    }
                }
                else if (content != null && (content.LeftItems?.Any() ?? false) && (content.RightItems?.Any() ?? false))
                {
                    var connections = content.Connections?.Any() == true
                        ? content.Connections
                        : content.LeftItems.OrderBy(li => li.Index)
                            .Select(li => new MatchingConnection
                            {
                                LeftIndex = li.Index,
                                RightIndex = li.Index
                            })
                            .ToList();

                    var items = new List<MatchingBlockItem>();
                    foreach (var connection in connections)
                    {
                        var left = content.LeftItems.FirstOrDefault(li => li.Index == connection.LeftIndex);
                        var right = content.RightItems.FirstOrDefault(ri => ri.Index == connection.RightIndex);
                        if (left == null || right == null)
                        {
                            continue;
                        }

                        items.Add(new MatchingBlockItem
                        {
                            Id = $"legacy-{connection.LeftIndex}",
                            Left = new MatchingBlockSide
                            {
                                Type = "text",
                                Text = left.Text
                            },
                            Right = new MatchingBlockSide
                            {
                                Type = "text",
                                Text = right.Text
                            }
                        });
                    }

                if (items.Any())
                {
                    const string instructionText = "آیتم‌های ستون چپ را با گزینه‌های ستون راست تطبیق دهید.";

                    var legacyBlock = new MatchingBlock
                    {
                        Id = "legacy",
                        Order = 0,
                        Instruction = instructionText,
                        Points = Math.Max(1, items.Count),
                        IsRequired = true,
                        Items = items
                    };

                    var blockContentJson = JsonConvert.SerializeObject(legacyBlock, Formatting.None);
                    return Task.FromResult<(string?, int?, string?)>((instructionText, 0, blockContentJson));
                }
                }
            }

            var gapFillBlock = GapFillContentParser.FindBlockById(contentJson, blockId);
            if (gapFillBlock != null)
            {
                var blockContentJson = JsonConvert.SerializeObject(gapFillBlock, Formatting.None);
                return Task.FromResult<(string?, int?, string?)>((gapFillBlock.Instruction, gapFillBlock.Order, blockContentJson));
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
                        var elements = jsonElement.EnumerateArray().ToList();

                        if (elements.Count == 0)
                        {
                            normalized[kvp.Key] = new List<object>();
                            continue;
                        }

                        if (elements.All(e => e.ValueKind == System.Text.Json.JsonValueKind.String || e.ValueKind == System.Text.Json.JsonValueKind.Null))
                        {
                            normalized[kvp.Key] = elements
                                .Select(e => e.ValueKind == System.Text.Json.JsonValueKind.Null ? string.Empty : (e.GetString() ?? string.Empty))
                                .ToList();
                            continue;
                        }

                        if (elements.All(e => e.ValueKind == System.Text.Json.JsonValueKind.Number))
                        {
                            var intValues = new List<int>();
                            var decimalValues = new List<decimal>();
                            var allInt = true;

                            foreach (var element in elements)
                            {
                                if (element.TryGetInt32(out var intValue))
                                {
                                    intValues.Add(intValue);
                                }
                                else if (element.TryGetDecimal(out var decimalValue))
                                {
                                    allInt = false;
                                    decimalValues.Add(decimalValue);
                                }
                                else
                                {
                                    allInt = false;
                                }
                            }

                            if (allInt)
                            {
                                normalized[kvp.Key] = intValues;
                            }
                            else if (decimalValues.Count > 0 && decimalValues.Count == elements.Count)
                            {
                                normalized[kvp.Key] = decimalValues;
                            }
                            else
                            {
                                normalized[kvp.Key] = elements
                                    .Select(e => System.Text.Json.JsonSerializer.Deserialize<object>(e.GetRawText()) ?? new object())
                                    .ToList();
                            }

                            continue;
                        }

                        if (elements.All(e => e.ValueKind == System.Text.Json.JsonValueKind.True || e.ValueKind == System.Text.Json.JsonValueKind.False))
                        {
                            normalized[kvp.Key] = elements
                                .Select(e => e.GetBoolean())
                                .ToList();
                            continue;
                        }

                        normalized[kvp.Key] = elements
                            .Select(e => System.Text.Json.JsonSerializer.Deserialize<object>(e.GetRawText()) ?? new object())
                            .ToList();
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
                    if (blockToken is not JObject blockObj) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    var typeValue = blockObj["type"]?.ToString() ?? string.Empty;
                    var dataToken = blockObj["data"] as JObject ?? blockObj;

                    if (!string.IsNullOrEmpty(typeValue))
                    {
                        var normalizedType = typeValue.Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();

                        if (!string.IsNullOrEmpty(currentBlockId) &&
                            string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                        {
                            if (normalizedType.Contains("ordering"))
                            {
                                return "ordering";
                            }

                            if (normalizedType.Contains("multiplechoice"))
                            {
                                return "multipleChoice";
                            }

                            if (normalizedType.Contains("matching"))
                            {
                                return "matching";
                            }

                            if (normalizedType.Contains("gapfill"))
                            {
                                return "gapFill";
                            }
                        }

                        if (normalizedType.Contains("multiplechoice"))
                        {
                            if (dataToken["questions"] is JArray questionArray)
                            {
                                foreach (var questionToken in questionArray)
                                {
                                    if (questionToken is not JObject questionObj) continue;

                                    var questionId = questionObj["id"]?.ToString();
                                    if (!string.IsNullOrEmpty(questionId) &&
                                        string.Equals(questionId, blockId, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return "multipleChoice";
                                    }
                                }
                            }
                        }

                        if (normalizedType.Contains("matching"))
                        {
                            if (dataToken["items"] is JArray matchingItemsArray)
                            {
                                foreach (var itemToken in matchingItemsArray)
                                {
                                    if (itemToken is not JObject itemObj) continue;

                                    var matchId = itemObj["id"]?.ToString();
                                    if (!string.IsNullOrEmpty(matchId) &&
                                        string.Equals(matchId, blockId, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return "matching";
                                    }
                                }
                            }
                        }

                        if (normalizedType.Contains("ordering"))
                        {
                            if (dataToken["items"] is JArray itemsArray)
                            {
                                foreach (var itemToken in itemsArray)
                                {
                                    if (itemToken is not JObject itemObj) continue;

                                    var itemId = itemObj["id"]?.ToString();
                                    if (!string.IsNullOrEmpty(itemId) &&
                                        string.Equals(itemId, blockId, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return "ordering";
                                    }
                                }
                            }
                        }

                        if (normalizedType.Contains("gapfill"))
                        {
                            if (dataToken["blanks"] is JArray blanksArray)
                            {
                                var blankIds = blanksArray.OfType<JObject>()
                                    .Select(obj => obj["id"]?.ToString())
                                    .Where(id => !string.IsNullOrWhiteSpace(id))
                                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                                if (!string.IsNullOrEmpty(currentBlockId) &&
                                    string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                                {
                                    return "gapFill";
                                }

                                if (blankIds.Contains(blockId))
                                {
                                    return "gapFill";
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(currentBlockId) &&
                        string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                    {
                        // If block has no explicit type, try to infer from data
                        if (dataToken["questions"] is JArray)
                        {
                            return "multipleChoice";
                        }

                        if (dataToken["items"] is JArray)
                        {
                            var itemsArray = dataToken["items"] as JArray;
                            var looksLikeMatching = itemsArray != null &&
                                itemsArray.OfType<JObject>().Any(obj =>
                                    obj["leftType"] != null ||
                                    obj["rightType"] != null);

                            return looksLikeMatching ? "matching" : "ordering";
                        }

                        if (dataToken["blanks"] is JArray)
                        {
                            return "gapFill";
                        }
                    }
                }
            }

            // Check in "orderingBlocks" array (for Reminder)
            if (contentObj["orderingBlocks"] is JArray orderingBlocksArray)
            {
                foreach (var blockToken in orderingBlocksArray)
                {
                    if (blockToken is not JObject blockObj) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    if (string.IsNullOrEmpty(currentBlockId) || !string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return "ordering"; // orderingBlocks always contain ordering blocks
                }
            }

            // Check in "multipleChoiceBlocks" array (Reminder, Writing, Audio, etc.)
            var multipleChoiceBlocksToken = contentObj["multipleChoiceBlocks"] 
                ?? contentObj["multiplechoiceBlocks"] 
                ?? contentObj["MultipleChoiceBlocks"];

            if (multipleChoiceBlocksToken is JArray multipleChoiceBlocksArray)
            {
                foreach (var blockToken in multipleChoiceBlocksArray)
                {
                    if (blockToken is not JObject blockObj) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    if (string.IsNullOrEmpty(currentBlockId) || !string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return "multipleChoice";
                }
            }

            var matchingBlocksToken = contentObj["matchingBlocks"]
                ?? contentObj["MatchingBlocks"];

            if (matchingBlocksToken is JArray matchingBlocksArray)
            {
                foreach (var blockToken in matchingBlocksArray)
                {
                    if (blockToken is not JObject blockObj) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    if (string.IsNullOrEmpty(currentBlockId) || !string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return "matching";
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

