using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EduTrack.Application.Common.Services.Validators;

public class GapFillBlockValidator : IBlockAnswerValidator
{
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public GapFillBlockValidator(IScheduleItemRepository scheduleItemRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
    }

    public ScheduleItemType SupportedType => ScheduleItemType.GapFill;

    public async Task<BlockValidationResult> ValidateAnswerAsync(
        int scheduleItemId,
        string blockId,
        Dictionary<string, object> submittedAnswer,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blockId))
        {
            throw new ArgumentException("Block identifier is required.", nameof(blockId));
        }

        var scheduleItem = await _scheduleItemRepository.GetByIdAsync(scheduleItemId, cancellationToken);
        if (scheduleItem == null)
        {
            throw new ArgumentException("Schedule item not found.", nameof(scheduleItemId));
        }

        if (string.IsNullOrWhiteSpace(scheduleItem.ContentJson))
        {
            throw new InvalidOperationException("Schedule item does not contain any content.");
        }

        var block = GapFillContentParser.FindBlockById(scheduleItem.ContentJson, blockId);
        if (block == null)
        {
            throw new ArgumentException($"Gap fill block with id '{blockId}' was not found.", nameof(blockId));
        }

        var submissions = ExtractSubmittedBlanks(submittedAnswer);
        if (!submissions.Any())
        {
            throw new ArgumentException("Submitted answer does not contain any blanks.", nameof(submittedAnswer));
        }

        var evaluationResults = EvaluateBlock(block, submissions);
        var isFullyCorrect = evaluationResults.All(result => result.IsCorrect);

        var result = new BlockValidationResult
        {
            IsCorrect = isFullyCorrect,
            PointsEarned = isFullyCorrect ? block.Points : 0,
            MaxPoints = block.Points,
            CorrectAnswer = BuildCorrectAnswer(block),
            SubmittedAnswer = BuildSubmittedAnswer(evaluationResults),
            Feedback = isFullyCorrect
                ? "عالی! پاسخ شما صحیح است."
                : "برخی از پاسخ‌ها صحیح نیست. لطفاً دوباره تلاش کنید.",
            DetailedFeedback = BuildDetailedFeedback(evaluationResults, block)
        };

        return result;
    }

    private static List<BlankEvaluation> EvaluateBlock(GapFillBlock block, List<SubmittedBlank> submissions)
    {
        var evaluations = new List<BlankEvaluation>();
        var globalOptionLookup = block.GlobalOptions?.ToDictionary(
            option => option.Id,
            option => option,
            StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, GapFillOption>(StringComparer.OrdinalIgnoreCase);

        foreach (var blank in block.Blanks)
        {
            var evaluation = new BlankEvaluation(blank);

            var submitted = submissions.FirstOrDefault(submission =>
                string.Equals(submission.BlankId, blank.GetIdentifier(), StringComparison.OrdinalIgnoreCase) ||
                submission.Index == blank.Index);

            if (submitted != null)
            {
                evaluation.SubmittedOptionId = submitted.OptionId;
                evaluation.SubmittedValue = submitted.Value;
            }

            if (string.IsNullOrWhiteSpace(evaluation.SubmittedValue) &&
                string.IsNullOrWhiteSpace(evaluation.SubmittedOptionId))
            {
                evaluations.Add(evaluation);
                continue;
            }

            var optionValue = ResolveOptionValue(blank, evaluation.SubmittedOptionId, globalOptionLookup);
            var comparisonValue = optionValue ?? evaluation.SubmittedValue ?? string.Empty;

            if (string.IsNullOrWhiteSpace(optionValue) && !blank.AllowManualInput)
            {
                evaluation.IsCorrect = false;
                evaluations.Add(evaluation);
                continue;
            }

            evaluation.IsCorrect = Matches(blank, comparisonValue, block.AnswerType, block.CaseSensitive);

            if (!evaluation.IsCorrect && blank.AlternativeAnswers != null && blank.AlternativeAnswers.Any())
            {
                evaluation.IsCorrect = blank.AlternativeAnswers
                    .Any(alternative => Matches(alternative, comparisonValue, block.AnswerType, block.CaseSensitive));
            }

            if (!evaluation.IsCorrect && blank.CorrectOptionId != null &&
                evaluation.SubmittedOptionId != null)
            {
                evaluation.IsCorrect = string.Equals(
                    blank.CorrectOptionId,
                    evaluation.SubmittedOptionId,
                    StringComparison.OrdinalIgnoreCase);
            }

            if (!evaluation.IsCorrect && blank.AlternativeOptionIds != null &&
                blank.AlternativeOptionIds.Any() &&
                evaluation.SubmittedOptionId != null)
            {
                evaluation.IsCorrect = blank.AlternativeOptionIds
                    .Any(id => string.Equals(id, evaluation.SubmittedOptionId, StringComparison.OrdinalIgnoreCase));
            }

            evaluations.Add(evaluation);
        }

        return evaluations;
    }

    private static string? ResolveOptionValue(
        GapFillBlank blank,
        string? optionId,
        IReadOnlyDictionary<string, GapFillOption> globalOptions)
    {
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return null;
        }

        var blankOption = blank.Options?
            .FirstOrDefault(option => string.Equals(option.Id, optionId, StringComparison.OrdinalIgnoreCase));
        if (blankOption != null)
        {
            return blankOption.Value;
        }

        if (globalOptions.TryGetValue(optionId, out var globalOption))
        {
            return globalOption.Value;
        }

        return null;
    }

    private static bool Matches(
        GapFillBlank blank,
        string submittedValue,
        string? answerType,
        bool caseSensitive)
    {
        return Matches(blank.CorrectAnswer, submittedValue, answerType, caseSensitive);
    }

    private static bool Matches(
        string correctValue,
        string submittedValue,
        string? answerType,
        bool caseSensitive)
    {
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var normalizedAnswerType = answerType?.Trim().ToLowerInvariant() ?? "exact";

        var normalizedCorrect = correctValue?.Trim() ?? string.Empty;
        var normalizedSubmitted = submittedValue?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedCorrect))
        {
            return string.IsNullOrWhiteSpace(normalizedSubmitted);
        }

        switch (normalizedAnswerType)
        {
            case "keyword":
                return normalizedSubmitted.IndexOf(normalizedCorrect, comparison) >= 0;

            case "similar":
                return string.Equals(
                    NormalizeWhitespace(normalizedCorrect),
                    NormalizeWhitespace(normalizedSubmitted),
                    comparison);

            default:
                return string.Equals(normalizedCorrect, normalizedSubmitted, comparison);
        }
    }

    private static string NormalizeWhitespace(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var previousIsWhitespace = false;

        foreach (var character in value.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousIsWhitespace)
                {
                    builder.Append(' ');
                    previousIsWhitespace = true;
                }
            }
            else
            {
                builder.Append(character);
                previousIsWhitespace = false;
            }
        }

        return builder.ToString();
    }

    private static Dictionary<string, object> BuildCorrectAnswer(GapFillBlock block)
    {
        var blanks = block.Blanks.Select(blank => new Dictionary<string, object?>
        {
            { "blankId", blank.GetIdentifier() },
            { "index", blank.Index },
            { "correctAnswer", blank.CorrectAnswer },
            { "alternativeAnswers", blank.AlternativeAnswers ?? new List<string>() },
            { "correctOptionId", blank.CorrectOptionId },
            { "alternativeOptionIds", blank.AlternativeOptionIds ?? new List<string>() },
            {
                "options",
                (blank.Options ?? new List<GapFillOption>())
                    .Select(option => new
                    {
                        option.Id,
                        option.Value,
                        option.DisplayText
                    }).ToList()
            }
        }).ToList();

        var payload = new Dictionary<string, object>
        {
            { "answerType", block.AnswerType },
            { "caseSensitive", block.CaseSensitive },
            { "blanks", blanks }
        };

        if (block.ShowGlobalOptions && block.GlobalOptions != null && block.GlobalOptions.Any())
        {
            payload["globalOptions"] = block.GlobalOptions
                .Select(option => new
                {
                    option.Id,
                    option.Value,
                    option.DisplayText
                })
                .ToList();
        }

        return payload;
    }

    private static Dictionary<string, object> BuildSubmittedAnswer(IEnumerable<BlankEvaluation> evaluations)
    {
        var blanks = evaluations.Select(evaluation => new Dictionary<string, object?>
        {
            { "blankId", evaluation.Blank.GetIdentifier() },
            { "index", evaluation.Blank.Index },
            { "value", evaluation.SubmittedValue },
            { "optionId", evaluation.SubmittedOptionId }
        }).ToList();

        return new Dictionary<string, object>
        {
            { "blanks", blanks }
        };
    }

    private static Dictionary<string, object> BuildDetailedFeedback(
        IEnumerable<BlankEvaluation> evaluations,
        GapFillBlock block)
    {
        var perBlank = evaluations.Select(evaluation => new Dictionary<string, object?>
        {
            { "blankId", evaluation.Blank.GetIdentifier() },
            { "index", evaluation.Blank.Index },
            { "isCorrect", evaluation.IsCorrect },
            { "submittedValue", evaluation.SubmittedValue },
            { "submittedOptionId", evaluation.SubmittedOptionId },
            { "allowManual", evaluation.Blank.AllowManualInput },
            { "allowGlobalOptions", evaluation.Blank.AllowGlobalOptions },
            { "allowBlankOptions", evaluation.Blank.AllowBlankOptions }
        }).ToList();

        return new Dictionary<string, object>
        {
            { "blanks", perBlank },
            { "answerType", block.AnswerType },
            { "caseSensitive", block.CaseSensitive }
        };
    }

    private static List<SubmittedBlank> ExtractSubmittedBlanks(Dictionary<string, object> submittedAnswer)
    {
        if (submittedAnswer == null)
        {
            return new List<SubmittedBlank>();
        }

        if (submittedAnswer.TryGetValue("blanks", out var blanksValue))
        {
            var blanksToken = ConvertToJToken(blanksValue);
            if (blanksToken is JArray blanksArray)
            {
                return blanksArray
                    .OfType<JObject>()
                    .Select(ParseSubmittedBlank)
                    .Where(blank => blank != null)
                    .Cast<SubmittedBlank>()
                    .ToList();
            }
        }

        // Fallback: look for flat dictionary entries (e.g., blank1, blank2)
        var fallback = submittedAnswer
            .Where(pair => pair.Key.StartsWith("blank", StringComparison.OrdinalIgnoreCase))
            .Select(pair => new SubmittedBlank
            {
                BlankId = pair.Key,
                Value = pair.Value?.ToString()
            })
            .ToList();

        return fallback;
    }

    private static SubmittedBlank? ParseSubmittedBlank(JObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        var blank = new SubmittedBlank
        {
            BlankId = obj["blankId"]?.ToString() ??
                      obj["id"]?.ToString() ??
                      obj["key"]?.ToString(),
            OptionId = obj["optionId"]?.ToString() ??
                       obj["selectedOptionId"]?.ToString(),
            Value = obj["value"]?.ToString() ??
                    obj["text"]?.ToString() ??
                    obj["input"]?.ToString()
        };

        if (blank.BlankId == null && obj["index"] != null)
        {
            blank.Index = obj["index"]!.Value<int>();
            blank.BlankId = $"blank{Math.Max(1, blank.Index)}";
        }
        else if (int.TryParse(blank.BlankId, out var numericId))
        {
            blank.Index = numericId;
        }
        else if (blank.BlankId != null)
        {
            var digits = new string(blank.BlankId
                .Where(char.IsDigit)
                .ToArray());

            if (int.TryParse(digits, out var parsedIndex))
            {
                blank.Index = parsedIndex;
            }
        }

        if (string.IsNullOrWhiteSpace(blank.BlankId) && blank.Index > 0)
        {
            blank.BlankId = $"blank{blank.Index}";
        }

        return blank;
    }

    private static JToken? ConvertToJToken(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is JToken token)
        {
            return token;
        }

        if (value is JsonElement jsonElement)
        {
            return JToken.Parse(jsonElement.GetRawText());
        }

        if (value is string stringValue &&
            !string.IsNullOrWhiteSpace(stringValue) &&
            (stringValue.TrimStart().StartsWith("[") || stringValue.TrimStart().StartsWith("{")))
        {
            try
            {
                return JToken.Parse(stringValue);
            }
            catch
            {
                // ignore parsing errors and fall back to FromObject
            }
        }

        try
        {
            return JToken.FromObject(value);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private sealed class SubmittedBlank
    {
        public string? BlankId { get; set; }

        public int Index { get; set; }

        public string? Value { get; set; }

        public string? OptionId { get; set; }
    }

    private sealed class BlankEvaluation
    {
        public BlankEvaluation(GapFillBlank blank)
        {
            Blank = blank;
        }

        public GapFillBlank Blank { get; }

        public string? SubmittedValue { get; set; }

        public string? SubmittedOptionId { get; set; }

        public bool IsCorrect { get; set; }
    }
}

