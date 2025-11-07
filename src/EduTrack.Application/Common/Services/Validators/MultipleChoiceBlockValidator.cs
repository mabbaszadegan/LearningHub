using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EduTrack.Application.Common.Services.Validators;

/// <summary>
/// Validator for MultipleChoice block answers
/// </summary>
public class MultipleChoiceBlockValidator : IBlockAnswerValidator
{
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public ScheduleItemType SupportedType => ScheduleItemType.MultipleChoice;

    public MultipleChoiceBlockValidator(IScheduleItemRepository scheduleItemRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
    }

    public async Task<BlockValidationResult> ValidateAnswerAsync(
        int scheduleItemId,
        string blockId,
        Dictionary<string, object> submittedAnswer,
        CancellationToken cancellationToken = default)
    {
        // Get schedule item
        var scheduleItem = await _scheduleItemRepository.GetByIdAsync(scheduleItemId, cancellationToken);
        if (scheduleItem == null)
        {
            throw new ArgumentException("Schedule item not found", nameof(scheduleItemId));
        }

        if (string.IsNullOrWhiteSpace(scheduleItem.ContentJson))
        {
            throw new InvalidOperationException("Schedule item content is empty");
        }

        var blockContext = ResolveBlockContext(scheduleItem, blockId);
        if (blockContext == null)
        {
            throw new ArgumentException($"Multiple choice block with ID '{blockId}' was not found or is not supported.", nameof(blockId));
        }

        if (!submittedAnswer.TryGetValue("selectedOptions", out var selectedOptionsValue))
        {
            throw new ArgumentException("Submitted answer must contain 'selectedOptions' field", nameof(submittedAnswer));
        }

        var selectedOptions = ExtractSelectedOptions(selectedOptionsValue);
        if (selectedOptions.Count == 0)
        {
            throw new ArgumentException("Submitted answer must contain at least one selected option.", nameof(submittedAnswer));
        }

        var answerType = string.Equals(blockContext.AnswerType, "multiple", StringComparison.OrdinalIgnoreCase)
            ? "multiple"
            : "single";

        var correctAnswers = blockContext.CorrectAnswers ?? new List<int>();
        if (correctAnswers.Count == 0 && blockContext.Options.Any())
        {
            correctAnswers = blockContext.Options
                .Where(o => o.IsCorrect)
                .Select(o => o.Index)
                .Distinct()
                .ToList();
        }

        var isCorrect = answerType == "multiple"
            ? CompareMultipleChoice(selectedOptions, correctAnswers)
            : CompareSingleChoice(selectedOptions, correctAnswers);

        var maxPoints = blockContext.Points > 0
            ? blockContext.Points
            : DetermineDefaultPoints(answerType, correctAnswers, blockContext.Options);

        var pointsEarned = isCorrect ? maxPoints : 0m;

        var result = new BlockValidationResult
        {
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            MaxPoints = maxPoints,
            CorrectAnswer = new Dictionary<string, object>
            {
                { "selectedOptions", correctAnswers }
            },
            SubmittedAnswer = new Dictionary<string, object>
            {
                { "selectedOptions", selectedOptions }
            },
            Feedback = isCorrect
                ? "عالی! پاسخ شما صحیح است."
                : "متأسفانه پاسخ شما صحیح نیست. لطفاً دوباره تلاش کنید.",
            DetailedFeedback = new Dictionary<string, object>
            {
                { "submittedOptions", selectedOptions },
                { "correctOptions", correctAnswers },
                { "answerType", answerType },
                { "question", blockContext.Question }
            }
        };

        return result;
    }

    private MultipleChoiceBlockContext? ResolveBlockContext(Domain.Entities.ScheduleItem scheduleItem, string blockId)
    {
        var contentJson = scheduleItem.ContentJson ?? string.Empty;

        // First try typed MultipleChoiceContent (for dedicated MCQ items)
        if (scheduleItem.Type == ScheduleItemType.MultipleChoice)
        {
            var context = TryResolveFromMultipleChoiceContent(contentJson, blockId);
            if (context != null)
            {
                return context;
            }
        }

        try
        {
            var contentObj = JObject.Parse(contentJson);

            // explicit multipleChoiceBlocks collection
            var mcqBlocksToken = contentObj["multipleChoiceBlocks"]
                ?? contentObj["multiplechoiceBlocks"]
                ?? contentObj["MultipleChoiceBlocks"];

            if (mcqBlocksToken is JArray mcqBlocksArray)
            {
                foreach (var blockToken in mcqBlocksArray)
                {
                    if (blockToken is not JObject blockObj) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    if (!IsMatchingBlockId(currentBlockId, blockId))
                        continue;

                    var block = blockObj.ToObject<MultipleChoiceBlock>();
                    if (block != null)
                    {
                        if (!string.IsNullOrWhiteSpace(currentBlockId))
                        {
                            block.Id = currentBlockId;
                        }
                        return ConvertBlockToContext(block);
                    }
                }
            }

            // generic blocks array with type metadata
            if (contentObj["blocks"] is JArray blocksArray)
            {
                foreach (var blockToken in blocksArray)
                {
                    if (blockToken is not JObject blockObj) continue;

                    var currentBlockId = blockObj["id"]?.ToString();
                    if (!IsMatchingBlockId(currentBlockId, blockId))
                        continue;

                    var typeValue = blockObj["type"]?.ToString() ?? string.Empty;
                    if (!IsMultipleChoiceType(typeValue))
                        continue;

                    var dataToken = blockObj["data"] as JObject ?? blockObj;
                    var block = dataToken.ToObject<MultipleChoiceBlock>() ?? new MultipleChoiceBlock();

                    if (!string.IsNullOrWhiteSpace(currentBlockId))
                    {
                        block.Id = currentBlockId;
                    }

                    block.Order = blockObj["order"]?.Value<int>() ?? block.Order;
                    block.Question = dataToken["question"]?.ToString() ?? block.Question;

                    if (block.Options == null || block.Options.Count == 0)
                    {
                        block.Options = dataToken["options"]?.ToObject<List<MultipleChoiceOption>>() ?? new List<MultipleChoiceOption>();
                    }

                    if ((block.CorrectAnswers == null || block.CorrectAnswers.Count == 0) &&
                        dataToken["correctAnswers"] is JArray correctArray)
                    {
                        block.CorrectAnswers = correctArray
                            .Select(x => x?.Value<int>() ?? 0)
                            .Where(i => i >= 0)
                            .Distinct()
                            .ToList();
                    }

                    if (block.Points <= 0 && blockObj["points"] != null)
                    {
                        if (decimal.TryParse(blockObj["points"]!.ToString(), out var pts))
                        {
                            block.Points = pts;
                        }
                    }

                    return ConvertBlockToContext(block);
                }
            }

            // Fallback for legacy single-question MCQ content (blockId might be "main")
            if (scheduleItem.Type == ScheduleItemType.MultipleChoice)
            {
                return TryResolveLegacyMultipleChoice(contentObj, blockId);
            }
        }
        catch
        {
            // Fallback to typed parsing if JObject parsing fails
            if (scheduleItem.Type == ScheduleItemType.MultipleChoice)
            {
                return TryResolveFromMultipleChoiceContent(contentJson, blockId, ignoreBlockMatch: true);
            }
        }

        return null;
    }

    private MultipleChoiceBlockContext? TryResolveFromMultipleChoiceContent(
        string contentJson,
        string blockId,
        bool ignoreBlockMatch = false)
    {
        MultipleChoiceContent? content;
        try
        {
            content = JsonConvert.DeserializeObject<MultipleChoiceContent>(contentJson);
        }
        catch
        {
            return null;
        }

        if (content == null)
        {
            return null;
        }

        if (content.Blocks != null && content.Blocks.Any())
        {
            MultipleChoiceBlock? block = null;

            if (!string.IsNullOrWhiteSpace(blockId) && !ignoreBlockMatch)
            {
                block = content.Blocks
                    .FirstOrDefault(b => string.Equals(b.Id, blockId, StringComparison.OrdinalIgnoreCase));
            }

            if (block == null && (ignoreBlockMatch || string.Equals(blockId, "main", StringComparison.OrdinalIgnoreCase)))
            {
                block = content.Blocks.OrderBy(b => b.Order).FirstOrDefault();
            }

            if (block != null)
            {
                return ConvertBlockToContext(block);
            }
        }

        // Legacy format: single question with "main" block
        if (string.IsNullOrWhiteSpace(blockId) && !ignoreBlockMatch)
        {
            blockId = "main";
        }

        if (string.Equals(blockId, "main", StringComparison.OrdinalIgnoreCase) || ignoreBlockMatch)
        {
            var options = content.Options ?? new List<MultipleChoiceOption>();

            var correctAnswers = content.CorrectAnswers?.ToList() ?? new List<int>();
            if (correctAnswers.Count == 0)
            {
                correctAnswers = options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Index)
                    .Distinct()
                    .ToList();
            }

            var answerType = string.IsNullOrWhiteSpace(content.AnswerType)
                ? "single"
                : content.AnswerType;

            var points = DetermineDefaultPoints(answerType, correctAnswers, options);

            return new MultipleChoiceBlockContext
            {
                BlockId = blockId,
                Question = content.Question ?? string.Empty,
                Options = options,
                AnswerType = answerType,
                CorrectAnswers = correctAnswers,
                Points = points
            };
        }

        return null;
    }

    private MultipleChoiceBlockContext? TryResolveLegacyMultipleChoice(JObject contentObj, string blockId)
    {
        var question = contentObj["question"]?.ToString() ?? string.Empty;
        var answerType = contentObj["answerType"]?.ToString() ?? "single";

        var optionsToken = contentObj["options"] as JArray;
        var options = optionsToken != null
            ? optionsToken.ToObject<List<MultipleChoiceOption>>() ?? new List<MultipleChoiceOption>()
            : new List<MultipleChoiceOption>();

        var correctAnswersToken = contentObj["correctAnswers"];
        var correctAnswers = correctAnswersToken != null
            ? correctAnswersToken.ToObject<List<int>>() ?? new List<int>()
            : options.Where(o => o.IsCorrect).Select(o => o.Index).Distinct().ToList();

        var points = DetermineDefaultPoints(answerType, correctAnswers, options);

        if (string.IsNullOrWhiteSpace(blockId))
        {
            blockId = "main";
        }

        return new MultipleChoiceBlockContext
        {
            BlockId = blockId,
            Question = question,
            Options = options,
            AnswerType = answerType,
            CorrectAnswers = correctAnswers,
            Points = points
        };
    }

    private MultipleChoiceBlockContext ConvertBlockToContext(MultipleChoiceBlock block)
    {
        var options = block.Options ?? new List<MultipleChoiceOption>();

        var correctAnswers = block.CorrectAnswers != null && block.CorrectAnswers.Count > 0
            ? block.CorrectAnswers.Distinct().ToList()
            : options.Where(o => o.IsCorrect).Select(o => o.Index).Distinct().ToList();

        var answerType = string.IsNullOrWhiteSpace(block.AnswerType)
            ? "single"
            : block.AnswerType;

        var points = block.Points > 0
            ? block.Points
            : DetermineDefaultPoints(answerType, correctAnswers, options);

        return new MultipleChoiceBlockContext
        {
            BlockId = string.IsNullOrWhiteSpace(block.Id) ? Guid.NewGuid().ToString() : block.Id,
            Question = block.Question ?? string.Empty,
            Options = options,
            AnswerType = answerType,
            CorrectAnswers = correctAnswers,
            Points = points
        };
    }

    private List<int> ExtractSelectedOptions(object selectedOptionsValue)
    {
        if (selectedOptionsValue == null)
        {
            return new List<int>();
        }

        switch (selectedOptionsValue)
        {
            case IEnumerable<int> ints:
                return ints.Distinct().ToList();
            case IEnumerable<long> longs:
                return longs.Select(l => Convert.ToInt32(l)).Distinct().ToList();
            case IEnumerable<object> objects when selectedOptionsValue is not JArray:
                return objects.Select(Convert.ToInt32).Distinct().ToList();
            case JArray jArray:
                return jArray
                    .Select(token => token?.ToObject<int>() ?? Convert.ToInt32(token?.ToString()))
                    .Distinct()
                    .ToList();
            case JToken jToken when jToken.Type == JTokenType.Array && jToken is not JArray:
                return jToken
                    .Select(token => token?.ToObject<int>() ?? Convert.ToInt32(token?.ToString()))
                    .Distinct()
                    .ToList();
            case JsonElement jsonElement:
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<int>();
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var number))
                        {
                            list.Add(number);
                        }
                        else if (item.ValueKind == JsonValueKind.String && int.TryParse(item.GetString(), out var parsed))
                        {
                            list.Add(parsed);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid value in selectedOptions array.", nameof(selectedOptionsValue));
                        }
                    }
                    return list.Distinct().ToList();
                }

                if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var singleNumber))
                {
                    return new List<int> { singleNumber };
                }

                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    return ParseSelectedOptionsFromString(jsonElement.GetString() ?? string.Empty);
                }

                break;
            case string str:
                return ParseSelectedOptionsFromString(str);
            default:
                if (selectedOptionsValue is IConvertible convertible)
                {
                    return new List<int> { Convert.ToInt32(convertible) };
                }
                break;
        }

        throw new ArgumentException("Invalid format for selectedOptions", nameof(selectedOptionsValue));
    }

    private List<int> ParseSelectedOptionsFromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<int>();
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<int>>(trimmed);
                if (list != null)
                {
                    return list.Distinct().ToList();
                }
            }
            catch
            {
                // fall back to manual parsing
            }
        }

        if (trimmed.Contains(','))
        {
            return trimmed
                .Split(',')
                .Select(segment => Convert.ToInt32(segment.Trim()))
                .Distinct()
                .ToList();
        }

        if (int.TryParse(trimmed, out var single))
        {
            return new List<int> { single };
        }

        throw new ArgumentException("Invalid string format for selectedOptions", nameof(value));
    }

    private decimal DetermineDefaultPoints(
        string answerType,
        List<int> correctAnswers,
        List<MultipleChoiceOption> options)
    {
        if (string.Equals(answerType, "multiple", StringComparison.OrdinalIgnoreCase))
        {
            var positiveCount = correctAnswers.Count > 0
                ? correctAnswers.Count
                : options.Count(o => o.IsCorrect);

            return Math.Max(1, positiveCount);
        }

        return 1m;
    }

    private bool CompareSingleChoice(List<int> submitted, List<int> correct)
    {
        if (correct.Count == 0)
        {
            return false;
        }

        if (submitted.Count != 1)
        {
            return false;
        }

        return correct.Contains(submitted[0]);
    }

    private bool CompareMultipleChoice(List<int> submitted, List<int> correct)
    {
        if (correct.Count == 0)
        {
            return false;
        }

        if (submitted.Count != correct.Count)
        {
            return false;
        }

        var submittedSet = new HashSet<int>(submitted);
        var correctSet = new HashSet<int>(correct);

        return submittedSet.SetEquals(correctSet);
    }

    private bool IsMatchingBlockId(string? candidateId, string requestedId)
    {
        if (string.IsNullOrWhiteSpace(requestedId))
        {
            return false;
        }

        return string.Equals(candidateId ?? string.Empty, requestedId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsMultipleChoiceType(string typeValue)
    {
        if (string.IsNullOrWhiteSpace(typeValue))
        {
            return false;
        }

        var normalized = typeValue.Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        return normalized.Contains("multiplechoice");
    }

    private class MultipleChoiceBlockContext
    {
        public string BlockId { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<MultipleChoiceOption> Options { get; set; } = new();
        public string AnswerType { get; set; } = "single";
        public List<int> CorrectAnswers { get; set; } = new();
        public decimal Points { get; set; } = 1;
    }
}

