using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EduTrack.Application.Common.Models.ScheduleItems;

public static class GapFillContentParser
{
    private static readonly JsonSerializerSettings CamelCaseSettings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    public static GapFillContent FromBlocks(JArray blocksArray)
    {
        var content = new GapFillContent();

        foreach (var blockToken in blocksArray.OfType<JObject>())
        {
            var blockType = blockToken["type"]?.ToString();
            if (!string.Equals(blockType, "gapFill", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(blockType, "gapfill", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var block = ParseBlock(blockToken);
            if (block != null)
            {
                content.Blocks.Add(block);
            }
        }

        content.Blocks = content.Blocks
            .OrderBy(b => b.Order)
            .ThenBy(b => b.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        PopulateContentMetadata(content);

        return content;
    }

    public static GapFillContent FromContentJson(string contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            return new GapFillContent();
        }

        try
        {
            var contentObject = JObject.Parse(contentJson);
            if (contentObject["blocks"] is JArray blocksArray)
            {
                return FromBlocks(blocksArray);
            }
        }
        catch
        {
            // Fallback to direct deserialization
        }

        var legacyContent = JsonConvert.DeserializeObject<GapFillContent>(contentJson, CamelCaseSettings) ?? new GapFillContent();
        PopulateContentMetadata(legacyContent);
        return legacyContent;
    }

    public static GapFillBlock? FindBlockById(string? contentJson, string blockId)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            return null;
        }

        var content = FromContentJson(contentJson);
        return content.Blocks.FirstOrDefault(block =>
            string.Equals(block.Id, blockId, StringComparison.OrdinalIgnoreCase));
    }

    public static GapFillBlock? ParseBlock(JObject blockObject)
    {
        if (blockObject == null)
        {
            return null;
        }

        var data = blockObject["data"] as JObject ?? blockObject;

        var block = new GapFillBlock
        {
            Id = blockObject["id"]?.ToString() ?? Guid.NewGuid().ToString(),
            Order = blockObject["order"]?.Value<int>() ?? data["order"]?.Value<int>() ?? 0,
            Instruction = data["instruction"]?.ToString(),
            Content = data["content"]?.ToString() ?? string.Empty,
            TextContent = data["textContent"]?.ToString() ?? data["content"]?.ToString() ?? string.Empty,
            AnswerType = data["answerType"]?.ToString() ?? "exact",
            CaseSensitive = data["caseSensitive"]?.Value<bool>() ?? false,
            ShowGlobalOptions = data["showGlobalOptions"]?.Value<bool>() ??
                                data["showOptions"]?.Value<bool>() ?? false,
            Points = ParseDecimal(data["points"]) ?? 1,
            IsRequired = data["isRequired"]?.Value<bool>() ?? true
        };

        block.Media = ParseMedia(data);
        block.GlobalOptions = ParseOptions(data["globalOptions"] ?? data["options"]);
        block.Blanks = ParseBlanks(data, block.ShowGlobalOptions);

        if (string.IsNullOrWhiteSpace(block.Content) && !string.IsNullOrWhiteSpace(block.TextContent))
        {
            block.Content = block.TextContent;
        }

        if (!block.Blanks.Any() && data["gaps"] is JArray legacyGaps)
        {
            block.Blanks = ParseLegacyBlanks(legacyGaps, block.ShowGlobalOptions);
        }

        if (block.GlobalOptions.Any() && !block.ShowGlobalOptions)
        {
            block.ShowGlobalOptions = true;
        }

        block.Blanks = block.Blanks
            .OrderBy(b => b.Index)
            .ThenBy(b => b.GetIdentifier(), StringComparer.OrdinalIgnoreCase)
            .ToList();

        return block;
    }

    private static void PopulateContentMetadata(GapFillContent content)
    {
        if (content == null)
        {
            return;
        }

        if (content.Blocks == null)
        {
            content.Blocks = new List<GapFillBlock>();
        }

        if (!content.Blocks.Any())
        {
            content.EnsureBlocksFromLegacy();
        }
        else
        {
            foreach (var block in content.Blocks)
            {
                if (block.Blanks == null)
                {
                    block.Blanks = new List<GapFillBlank>();
                }

                block.Blanks = block.Blanks
                    .OrderBy(blank => blank.Index)
                    .ThenBy(blank => blank.GetIdentifier(), StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        if (!string.IsNullOrWhiteSpace(content.Text))
        {
            // keep legacy text
        }
        else if (content.Blocks.Any())
        {
            var firstBlock = content.Blocks.First();
            content.Text = firstBlock.TextContent ?? firstBlock.Content;
            content.AnswerType = firstBlock.AnswerType;
            content.CaseSensitive = firstBlock.CaseSensitive;
            content.ShowOptions = firstBlock.ShowGlobalOptions;
        }

        if (content.GlobalOptions == null || !content.GlobalOptions.Any())
        {
            var accumulated = content.Blocks
                .SelectMany(block => block.GlobalOptions ?? new List<GapFillOption>())
                .Where(option => option != null && !string.IsNullOrWhiteSpace(option.Value))
                .GroupBy(option => option.Value.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var representative = group.First();
                    representative.Value = group.Key;
                    representative.DisplayText ??= representative.Value;
                    return representative;
                })
                .ToList();

            content.GlobalOptions = accumulated;
        }
    }

    private static GapFillMedia? ParseMedia(JObject data)
    {
        var mediaType = data["mediaType"]?.ToString();
        var fileUrl = data["fileUrl"]?.ToString();
        var fileId = data["fileId"]?.ToString();

        if (string.IsNullOrWhiteSpace(mediaType) &&
            string.IsNullOrWhiteSpace(fileUrl) &&
            string.IsNullOrWhiteSpace(fileId))
        {
            return null;
        }

        return new GapFillMedia
        {
            MediaType = mediaType,
            FileUrl = fileUrl,
            FileId = fileId,
            FileName = data["fileName"]?.ToString(),
            MimeType = data["mimeType"]?.ToString(),
            FileSize = data["fileSize"]?.Value<long?>(),
            IsRecorded = data["isRecorded"]?.Value<bool?>(),
            Duration = data["duration"]?.Value<int?>(),
            Size = data["size"]?.ToString(),
            Position = data["position"]?.ToString(),
            Caption = data["caption"]?.ToString(),
            CaptionPosition = data["captionPosition"]?.ToString(),
            DisplayMode = data["displayMode"]?.ToString(),
            AttachmentMode = data["attachmentMode"]?.ToString()
        };
    }

    private static List<GapFillBlank> ParseBlanks(JObject data, bool defaultAllowGlobalOptions)
    {
        var blanks = new List<GapFillBlank>();

        if (data["blanks"] is not JArray blanksArray)
        {
            return blanks;
        }

        foreach (var blankToken in blanksArray.OfType<JObject>())
        {
            var index = blankToken["index"]?.Value<int>() ??
                        blankToken["order"]?.Value<int>() ??
                        blanks.Count + 1;

            var blank = new GapFillBlank
            {
                Id = blankToken["id"]?.ToString() ??
                     blankToken["key"]?.ToString() ??
                     $"blank{index}",
                Index = index,
                CorrectAnswer = blankToken["correctAnswer"]?.ToString() ?? string.Empty,
                Hint = blankToken["hint"]?.ToString(),
                CorrectOptionId = blankToken["correctOptionId"]?.ToString(),
                AllowManualInput = blankToken["allowManualInput"]?.Value<bool?>() ?? true,
                AllowGlobalOptions = blankToken["allowGlobalOptions"]?.Value<bool?>() ?? defaultAllowGlobalOptions,
                AllowBlankOptions = blankToken["allowBlankOptions"]?.Value<bool?>() ?? false
            };

            blank.AlternativeAnswers = ParseStringList(blankToken["alternativeAnswers"]);
            blank.AlternativeOptionIds = ParseStringList(blankToken["alternativeOptionIds"]);

            var optionsToken = blankToken["options"] ?? blankToken["suggestions"];
            blank.Options = ParseOptions(optionsToken);

            if (!blank.Options.Any() && blank.AlternativeAnswers.Any())
            {
                blank.Options = blank.AlternativeAnswers
                    .Select(answer => new GapFillOption
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = answer,
                        DisplayText = answer
                    })
                    .ToList();

                if (!blankToken.ContainsKey("allowBlankOptions"))
                {
                    blank.AllowBlankOptions = true;
                }
            }

            if (blank.Options.Any() && !blank.AllowBlankOptions)
            {
                blank.AllowBlankOptions = true;
            }

            if (string.IsNullOrWhiteSpace(blank.CorrectAnswer) && !string.IsNullOrWhiteSpace(blank.CorrectOptionId))
            {
                var matchingOption = blank.Options.FirstOrDefault(option =>
                    string.Equals(option.Id, blank.CorrectOptionId, StringComparison.OrdinalIgnoreCase));
                if (matchingOption != null)
                {
                    blank.CorrectAnswer = matchingOption.Value;
                }
            }

            blanks.Add(blank);
        }

        return blanks;
    }

    private static List<GapFillBlank> ParseLegacyBlanks(JArray legacyGaps, bool defaultAllowGlobalOptions)
    {
        var blanks = new List<GapFillBlank>();

        foreach (var gapToken in legacyGaps.OfType<JObject>())
        {
            var index = gapToken["index"]?.Value<int>() ?? blanks.Count + 1;

            var blank = new GapFillBlank
            {
                Id = $"blank{index}",
                Index = index,
                CorrectAnswer = gapToken["correctAnswer"]?.ToString() ?? string.Empty,
                Hint = gapToken["hint"]?.ToString(),
                AllowManualInput = true,
                AllowGlobalOptions = defaultAllowGlobalOptions,
                AllowBlankOptions = gapToken["alternativeAnswers"] is JArray altArray && altArray.Any()
            };

            blank.AlternativeAnswers = ParseStringList(gapToken["alternativeAnswers"]);

            if (blank.AllowBlankOptions && blank.AlternativeAnswers.Any())
            {
                blank.Options = blank.AlternativeAnswers
                    .Select(answer => new GapFillOption
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = answer,
                        DisplayText = answer
                    })
                    .ToList();
            }

            blanks.Add(blank);
        }

        return blanks;
    }

    private static List<GapFillOption> ParseOptions(JToken? token)
    {
        var options = new List<GapFillOption>();
        if (token is not JArray array || array.Count == 0)
        {
            return options;
        }

        foreach (var item in array)
        {
            if (item is JObject optionObj)
            {
                var value = optionObj["value"]?.ToString() ??
                            optionObj["text"]?.ToString() ??
                            optionObj["label"]?.ToString() ??
                            string.Empty;

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var option = new GapFillOption
                {
                    Id = optionObj["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                    Value = value.Trim(),
                    DisplayText = optionObj["displayText"]?.ToString() ??
                                  optionObj["label"]?.ToString() ??
                                  value.Trim()
                };

                options.Add(option);
            }
            else
            {
                var rawValue = item?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    continue;
                }

                options.Add(new GapFillOption
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = rawValue.Trim(),
                    DisplayText = rawValue.Trim()
                });
            }
        }

        return options;
    }

    private static List<string> ParseStringList(JToken? token)
    {
        if (token == null)
        {
            return new List<string>();
        }

        if (token is JArray array)
        {
            return array
                .Select(item => item?.ToString())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .ToList();
        }

        if (token is JToken singleToken)
        {
            var singleValue = singleToken.ToString();
            if (!string.IsNullOrWhiteSpace(singleValue))
            {
                return new List<string> { singleValue.Trim() };
            }
        }

        return new List<string>();
    }

    private static decimal? ParseDecimal(JToken? token)
    {
        if (token == null)
        {
            return null;
        }

        if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
        {
            return token.Value<decimal>();
        }

        if (token.Type == JTokenType.String &&
            decimal.TryParse(token.ToString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }
}

