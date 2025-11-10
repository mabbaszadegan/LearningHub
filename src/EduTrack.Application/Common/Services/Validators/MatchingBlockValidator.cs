using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Linq;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EduTrack.Application.Common.Services.Validators;

public class MatchingBlockValidator : IBlockAnswerValidator
{
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public MatchingBlockValidator(IScheduleItemRepository scheduleItemRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
    }

    public ScheduleItemType SupportedType => ScheduleItemType.Match;

    public async Task<BlockValidationResult> ValidateAnswerAsync(
        int scheduleItemId,
        string blockId,
        Dictionary<string, object> submittedAnswer,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blockId))
        {
            throw new ArgumentException("Block ID is required", nameof(blockId));
        }

        var scheduleItem = await _scheduleItemRepository.GetByIdAsync(scheduleItemId, cancellationToken);
        if (scheduleItem == null)
        {
            throw new ArgumentException("Schedule item not found", nameof(scheduleItemId));
        }

        var matchingBlock = ResolveMatchingBlock(scheduleItem, blockId);
        if (matchingBlock == null)
        {
            throw new ArgumentException($"Matching block with ID '{blockId}' not found", nameof(blockId));
        }

        var matches = NormalizeSubmittedMatches(submittedAnswer);
        if (matches.Count == 0)
        {
            throw new ArgumentException("Submitted answer must contain at least one match", nameof(submittedAnswer));
        }

        var evaluation = EvaluateMatches(matchingBlock, matches);

        return new BlockValidationResult
        {
            IsCorrect = evaluation.IsCorrect,
            PointsEarned = evaluation.PointsEarned,
            MaxPoints = evaluation.MaxPoints,
            CorrectAnswer = BuildCorrectAnswerPayload(matchingBlock),
            SubmittedAnswer = BuildSubmittedAnswerPayload(matches),
            Feedback = evaluation.Feedback,
            DetailedFeedback = evaluation.DetailedFeedback
        };
    }

    private MatchingBlock? ResolveMatchingBlock(Domain.Entities.ScheduleItem scheduleItem, string blockId)
    {
        if (string.IsNullOrWhiteSpace(scheduleItem.ContentJson))
        {
            return null;
        }

        try
        {
            var contentObj = JObject.Parse(scheduleItem.ContentJson);

            if (contentObj["blocks"] is JArray blocksArray)
            {
                foreach (var blockToken in blocksArray.OfType<JObject>())
                {
                    var typeValue = blockToken["type"]?.ToString();
                    if (!string.Equals(typeValue, "matching", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var order = blockToken["order"]?.Value<int>() ?? 0;
                    var currentBlockId = blockToken["id"]?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(currentBlockId) &&
                        string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                    {
                        var parsedBlock = ConvertJObjectToMatchingBlock(blockToken, order);
                        if (parsedBlock != null)
                        {
                            return parsedBlock;
                        }
                    }

                    // If blockId refers to a nested item identifier, fall back to the container block
                    var dataObj = blockToken["data"] as JObject;
                    if (dataObj?["items"] is JArray dataItems &&
                        dataItems.OfType<JObject>().Any(item => string.Equals(item["id"]?.ToString(), blockId, StringComparison.OrdinalIgnoreCase)))
                    {
                        var parsedBlock = ConvertJObjectToMatchingBlock(blockToken, order);
                        if (parsedBlock != null)
                        {
                            return parsedBlock;
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors and fall back to other approaches
        }

        var camelSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        MatchingContent? matchingContent = null;

        try
        {
            matchingContent = JsonConvert.DeserializeObject<MatchingContent>(scheduleItem.ContentJson, camelSettings);
        }
        catch
        {
            // ignore
        }

        if (matchingContent == null)
        {
            try
            {
                matchingContent = JsonConvert.DeserializeObject<MatchingContent>(scheduleItem.ContentJson);
            }
            catch
            {
                matchingContent = null;
            }
        }

        if (matchingContent != null)
        {
            if (matchingContent.Blocks != null && matchingContent.Blocks.Any())
            {
                var block = matchingContent.Blocks.FirstOrDefault(b =>
                    string.Equals(b.Id, blockId, StringComparison.OrdinalIgnoreCase) ||
                    (string.Equals(blockId, "main", StringComparison.OrdinalIgnoreCase) && string.Equals(b.Id, "legacy", StringComparison.OrdinalIgnoreCase)));

                if (block != null)
                {
                    return block;
                }
            }

            var legacyBlock = ConvertLegacyContentToBlock(matchingContent);
            if (legacyBlock != null &&
                (string.Equals(legacyBlock.Id, blockId, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(blockId, "legacy", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(blockId, "main", StringComparison.OrdinalIgnoreCase)))
            {
                return legacyBlock;
            }
        }

        return null;
    }

    private MatchingBlock? ConvertJObjectToMatchingBlock(JObject blockObj, int order)
    {
        var data = blockObj["data"] as JObject;
        if (data == null)
        {
            return null;
        }

        var matchingBlock = new MatchingBlock
        {
            Id = blockObj["id"]?.ToString() ?? Guid.NewGuid().ToString(),
            Order = order,
            Instruction = data["instruction"]?.ToString(),
            IsRequired = data["isRequired"]?.Value<bool>() ?? true,
            Points = ParseDecimal(data["points"])
        };

        if (matchingBlock.Points <= 0)
        {
            matchingBlock.Points = 0;
        }

        matchingBlock.Items = ParseItemsFromData(data);
        if (!matchingBlock.Items.Any())
        {
            return null;
        }

        if (matchingBlock.Points <= 0)
        {
            matchingBlock.Points = Math.Max(1, matchingBlock.Items.Count);
        }

        return matchingBlock;
    }

    private MatchingBlock? ConvertLegacyContentToBlock(MatchingContent content)
    {
        if (!(content.LeftItems?.Any() ?? false) || !(content.RightItems?.Any() ?? false))
        {
            return null;
        }

        var items = new List<MatchingBlockItem>();
        var connections = content.Connections?.Any() == true
            ? content.Connections
            : content.LeftItems.OrderBy(li => li.Index).Select(li => new MatchingConnection
            {
                LeftIndex = li.Index,
                RightIndex = li.Index
            }).ToList();

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

        if (!items.Any())
        {
            return null;
        }

        return new MatchingBlock
        {
            Id = "legacy",
            Order = 0,
            IsRequired = true,
            Points = Math.Max(1, items.Count),
            Items = items
        };
    }

    private List<MatchingBlockItem> ParseItemsFromData(JObject data)
    {
        var items = new List<MatchingBlockItem>();

        if (data["items"] is JArray itemsArray && itemsArray.Count > 0)
        {
            foreach (var token in itemsArray.OfType<JObject>())
            {
                var pairId = token["id"]?.ToString() ?? Guid.NewGuid().ToString();
                var leftSide = ParseSide(token, "left");
                var rightSide = ParseSide(token, "right");

                if (leftSide != null && rightSide != null)
                {
                    items.Add(new MatchingBlockItem
                    {
                        Id = pairId,
                        Left = leftSide,
                        Right = rightSide
                    });
                }
            }
        }
        else
        {
            items.AddRange(ParseLegacyItemsFromData(data));
        }

        return items;
    }

    private IEnumerable<MatchingBlockItem> ParseLegacyItemsFromData(JObject data)
    {
        var results = new List<MatchingBlockItem>();

        if (data["leftItems"] is not JArray leftItems ||
            data["rightItems"] is not JArray rightItems)
        {
            return results;
        }

        var connections = data["connections"] as JArray ?? new JArray();

        var leftLookup = leftItems
            .OfType<JObject>()
            .Select(obj => new
            {
                Index = obj["Index"]?.Value<int>() ?? obj["index"]?.Value<int>() ?? 0,
                Text = obj["Text"]?.ToString() ?? obj["text"]?.ToString() ?? string.Empty
            })
            .ToDictionary(x => x.Index, x => x.Text);

        var rightLookup = rightItems
            .OfType<JObject>()
            .Select(obj => new
            {
                Index = obj["Index"]?.Value<int>() ?? obj["index"]?.Value<int>() ?? 0,
                Text = obj["Text"]?.ToString() ?? obj["text"]?.ToString() ?? string.Empty
            })
            .ToDictionary(x => x.Index, x => x.Text);

        if (!connections.Any())
        {
            connections = new JArray(leftLookup.Keys.Select(idx => new JObject
            {
                ["LeftIndex"] = idx,
                ["RightIndex"] = idx
            }));
        }

        foreach (var token in connections.OfType<JObject>())
        {
            var leftIndex = token["LeftIndex"]?.Value<int>() ?? token["leftIndex"]?.Value<int>() ?? -1;
            var rightIndex = token["RightIndex"]?.Value<int>() ?? token["rightIndex"]?.Value<int>() ?? -1;

            if (!leftLookup.TryGetValue(leftIndex, out var leftText) ||
                !rightLookup.TryGetValue(rightIndex, out var rightText))
            {
                continue;
            }

            results.Add(new MatchingBlockItem
            {
                Id = $"legacy-{leftIndex}",
                Left = new MatchingBlockSide
                {
                    Type = "text",
                    Text = leftText
                },
                Right = new MatchingBlockSide
                {
                    Type = "text",
                    Text = rightText
                }
            });
        }

        return results;
    }

    private MatchingBlockSide? ParseSide(JObject itemObj, string prefix)
    {
        var type = itemObj[$"{prefix}Type"]?.ToString() ?? "text";
        var side = new MatchingBlockSide
        {
            Type = type
        };

        var textToken = itemObj[$"{prefix}Text"] ?? itemObj[$"{prefix}Value"] ?? itemObj[$"{prefix}Content"];
        if (textToken != null && textToken.Type != JTokenType.Null)
        {
            side.Text = textToken.ToString();
        }

        var fileIdToken = itemObj[$"{prefix}FileId"] ?? itemObj[$"{prefix}FileID"];
        if (fileIdToken != null && fileIdToken.Type != JTokenType.Null)
        {
            side.FileId = fileIdToken.Type switch
            {
                JTokenType.Integer => fileIdToken.Value<int>().ToString(),
                JTokenType.Float => fileIdToken.Value<float>().ToString(CultureInfo.InvariantCulture),
                _ => fileIdToken.ToString()
            };
        }

        var fileNameToken = itemObj[$"{prefix}FileName"];
        if (fileNameToken != null && fileNameToken.Type != JTokenType.Null)
        {
            side.FileName = fileNameToken.ToString();
        }

        var fileUrlToken = itemObj[$"{prefix}FileUrl"] ?? itemObj[$"{prefix}Url"];
        if (fileUrlToken != null && fileUrlToken.Type != JTokenType.Null)
        {
            side.FileUrl = fileUrlToken.ToString();
        }

        var mimeToken = itemObj[$"{prefix}MimeType"] ?? itemObj[$"{prefix}Mime"];
        if (mimeToken != null && mimeToken.Type != JTokenType.Null)
        {
            side.MimeType = mimeToken.ToString();
        }

        var isRecordedToken = itemObj[$"{prefix}IsRecorded"];
        if (isRecordedToken != null && isRecordedToken.Type != JTokenType.Null)
        {
            side.IsRecorded = isRecordedToken.Type == JTokenType.Boolean
                ? isRecordedToken.Value<bool>()
                : bool.TryParse(isRecordedToken.ToString(), out var flag) && flag;
        }

        var durationToken = itemObj[$"{prefix}Duration"];
        if (durationToken != null && durationToken.Type != JTokenType.Null)
        {
            if (durationToken.Type == JTokenType.Integer)
            {
                side.Duration = durationToken.Value<int>();
            }
            else if (int.TryParse(durationToken.ToString(), out var duration))
            {
                side.Duration = duration;
            }
        }

        return side;
    }

    private decimal ParseDecimal(JToken? token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return 0;
        }

        if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
        {
            return token.Value<decimal>();
        }

        if (token.Type == JTokenType.String &&
            decimal.TryParse(token.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return 0;
    }

    private List<SubmittedMatch> NormalizeSubmittedMatches(Dictionary<string, object> submittedAnswer)
    {
        if (!submittedAnswer.TryGetValue("matches", out var matchesValue))
        {
            throw new ArgumentException("Submitted answer must contain 'matches' data.", nameof(submittedAnswer));
        }

        var matches = new List<SubmittedMatch>();

        if (matchesValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in jsonElement.EnumerateArray())
            {
                var parsed = ParseMatchEntry(element);
                if (parsed != null)
                {
                    matches.Add(parsed);
                }
            }
        }
        else if (matchesValue is JArray jArray)
        {
            foreach (var token in jArray)
            {
                var parsed = ParseMatchEntry(token);
                if (parsed != null)
                {
                    matches.Add(parsed);
                }
            }
        }
        else if (matchesValue is IEnumerable<object> enumerable)
        {
            foreach (var entry in enumerable)
            {
                var parsed = ParseMatchEntry(entry);
                if (parsed != null)
                {
                    matches.Add(parsed);
                }
            }
        }
        else
        {
            var parsed = ParseMatchEntry(matchesValue);
            if (parsed != null)
            {
                matches.Add(parsed);
            }
        }

        return matches
            .Where(m => !string.IsNullOrWhiteSpace(m.LeftItemId))
            .OrderBy(m => m.OrderIndex)
            .ToList();
    }

    private SubmittedMatch? ParseMatchEntry(object? entry)
    {
        if (entry == null)
        {
            return null;
        }

        if (entry is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var left = jsonElement.TryGetProperty("leftItemId", out var leftProperty)
                ? leftProperty.GetString() ?? string.Empty
                : string.Empty;

            var selected = jsonElement.TryGetProperty("selectedPairId", out var selectedProperty)
                ? selectedProperty.GetString() ?? string.Empty
                : string.Empty;

            var order = jsonElement.TryGetProperty("orderIndex", out var orderProperty) && orderProperty.ValueKind == JsonValueKind.Number
                ? orderProperty.GetInt32()
                : int.MaxValue;

            return new SubmittedMatch
            {
                LeftItemId = left.Trim(),
                SelectedPairId = selected.Trim(),
                OrderIndex = order
            };
        }

        if (entry is JObject jObject)
        {
            var left = jObject["leftItemId"]?.ToString() ?? string.Empty;
            var selected = jObject["selectedPairId"]?.ToString() ?? string.Empty;
            var order = jObject["orderIndex"]?.Value<int?>() ?? int.MaxValue;

            return new SubmittedMatch
            {
                LeftItemId = left.Trim(),
                SelectedPairId = selected.Trim(),
                OrderIndex = order
            };
        }

        if (entry is IDictionary<string, object> dictionary)
        {
            var left = dictionary.TryGetValue("leftItemId", out var leftObj) ? leftObj?.ToString() ?? string.Empty : string.Empty;
            var selected = dictionary.TryGetValue("selectedPairId", out var selectedObj) ? selectedObj?.ToString() ?? string.Empty : string.Empty;
            var order = dictionary.TryGetValue("orderIndex", out var orderObj) && int.TryParse(orderObj?.ToString(), out var idx) ? idx : int.MaxValue;

            return new SubmittedMatch
            {
                LeftItemId = left.Trim(),
                SelectedPairId = selected.Trim(),
                OrderIndex = order
            };
        }

        return null;
    }

    private EvaluationResult EvaluateMatches(MatchingBlock block, List<SubmittedMatch> matches)
    {
        var items = block.Items ?? new List<MatchingBlockItem>();
        if (items.Count == 0)
        {
            return new EvaluationResult
            {
                IsCorrect = true,
                PointsEarned = block.Points > 0 ? block.Points : 1,
                MaxPoints = block.Points > 0 ? block.Points : 1,
                Feedback = "پاسخی برای این بلاک ثبت نشده است.",
                DetailedFeedback = new Dictionary<string, object>()
            };
        }

        var matchesByLeftId = matches
            .Where(m => !string.IsNullOrWhiteSpace(m.LeftItemId))
            .GroupBy(m => m.LeftItemId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var matchesByIndex = matches
            .Where(m => m.OrderIndex >= 0)
            .GroupBy(m => m.OrderIndex)
            .ToDictionary(g => g.Key, g => g.First());

        var maxPoints = block.Points > 0 ? block.Points : Math.Max(1, items.Count);
        var perItemPoints = maxPoints / Math.Max(1, items.Count);
        var correctCount = 0;

        var pairDetails = new List<Dictionary<string, object>>();

        for (var i = 0; i < items.Count; i++)
        {
            var blockItem = items[i];
            SubmittedMatch? submitted = null;

            if (matchesByLeftId.TryGetValue(blockItem.Id, out var byId))
            {
                submitted = byId;
            }
            else if (matchesByIndex.TryGetValue(i, out var byIndex))
            {
                submitted = byIndex;
            }

            var selectedPairId = submitted?.SelectedPairId ?? string.Empty;
            var isCorrect = !string.IsNullOrWhiteSpace(selectedPairId) &&
                            string.Equals(selectedPairId, blockItem.Id, StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                correctCount++;
            }

            pairDetails.Add(new Dictionary<string, object>
            {
                ["leftItemId"] = blockItem.Id,
                ["selectedPairId"] = selectedPairId,
                ["correctPairId"] = blockItem.Id,
                ["isCorrect"] = isCorrect
            });
        }

        var pointsEarned = Math.Round(perItemPoints * correctCount, 2, MidpointRounding.AwayFromZero);
        var isFullyCorrect = correctCount == items.Count;

        var feedback = isFullyCorrect
            ? "عالی! همه تطبیق‌ها صحیح هستند."
            : "برخی از تطبیق‌ها نیاز به بررسی مجدد دارند.";

        return new EvaluationResult
        {
            IsCorrect = isFullyCorrect,
            PointsEarned = pointsEarned,
            MaxPoints = maxPoints,
            Feedback = feedback,
            DetailedFeedback = new Dictionary<string, object>
            {
                ["pairs"] = pairDetails
            }
        };
    }

    private Dictionary<string, object> BuildCorrectAnswerPayload(MatchingBlock block)
    {
        var pairs = block.Items
            .Select(item => (object)new Dictionary<string, object>
            {
                ["leftItemId"] = item.Id,
                ["correctPairId"] = item.Id
            })
            .ToList();

        return new Dictionary<string, object>
        {
            ["matches"] = pairs
        };
    }

    private Dictionary<string, object> BuildSubmittedAnswerPayload(List<SubmittedMatch> matches)
    {
        var pairs = matches
            .Select(match => (object)new Dictionary<string, object>
            {
                ["leftItemId"] = match.LeftItemId,
                ["selectedPairId"] = match.SelectedPairId,
                ["orderIndex"] = match.OrderIndex
            })
            .ToList();

        return new Dictionary<string, object>
        {
            ["matches"] = pairs
        };
    }

    private sealed class SubmittedMatch
    {
        public string LeftItemId { get; set; } = string.Empty;
        public string SelectedPairId { get; set; } = string.Empty;
        public int OrderIndex { get; set; } = int.MaxValue;
    }

    private sealed class EvaluationResult
    {
        public bool IsCorrect { get; set; }
        public decimal PointsEarned { get; set; }
        public decimal MaxPoints { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public Dictionary<string, object> DetailedFeedback { get; set; } = new();
    }
}

