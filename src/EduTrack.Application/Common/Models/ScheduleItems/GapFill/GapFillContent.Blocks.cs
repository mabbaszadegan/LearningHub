using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EduTrack.Application.Common.Models.ScheduleItems;

public partial class GapFillContent
{
    private static readonly Regex BlankTokenRegex = new(@"\[\[blank(\d+)\]\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public List<GapFillBlock> Blocks { get; set; } = new();

    public List<GapFillOption> GlobalOptions { get; set; } = new();

    public void EnsureBlocksFromLegacy()
    {
        if (Blocks != null && Blocks.Any())
        {
            NormalizeBlockState();
            return;
        }

        if (string.IsNullOrWhiteSpace(Text) || Gaps == null || !Gaps.Any())
        {
            Blocks = new List<GapFillBlock>();
            return;
        }

        var orderedGaps = Gaps
            .OrderBy(gap => gap.Index)
            .ToList();

        var block = new GapFillBlock
        {
            Id = "legacy",
            Order = 0,
            Content = BuildContentWithTokens(Text, orderedGaps),
            TextContent = Text,
            AnswerType = AnswerType,
            CaseSensitive = CaseSensitive,
            ShowGlobalOptions = ShowOptions,
            Points = 1,
            IsRequired = true
        };

        foreach (var gap in orderedGaps)
        {
            var blank = new GapFillBlank
            {
                Id = $"blank{Math.Max(1, gap.Index)}",
                Index = Math.Max(1, gap.Index),
                CorrectAnswer = gap.CorrectAnswer ?? string.Empty,
                AlternativeAnswers = gap.AlternativeAnswers?.Where(a => !string.IsNullOrWhiteSpace(a))
                    .Select(a => a.Trim())
                    .ToList() ?? new List<string>(),
                Hint = gap.Hint,
                AllowManualInput = true,
                AllowGlobalOptions = ShowOptions,
                AllowBlankOptions = gap.AlternativeAnswers != null && gap.AlternativeAnswers.Any()
            };

            if (blank.AllowBlankOptions && gap.AlternativeAnswers != null)
            {
                blank.Options = gap.AlternativeAnswers
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Select(a => new GapFillOption
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = a.Trim(),
                        DisplayText = a.Trim()
                    })
                    .ToList();
            }

            block.Blanks.Add(blank);
        }

        Blocks = new List<GapFillBlock> { block };

        GlobalOptions = ShowOptions
            ? new List<GapFillOption>()
            : new List<GapFillOption>();

        NormalizeBlockState();
    }

    private void NormalizeBlockState()
    {
        if (Blocks == null)
        {
            Blocks = new List<GapFillBlock>();
            return;
        }

        foreach (var block in Blocks)
        {
            if (block.Blanks == null)
            {
                block.Blanks = new List<GapFillBlank>();
            }

            block.Blanks = block.Blanks
                .OrderBy(blank => blank.Index)
                .ThenBy(blank => blank.GetIdentifier(), StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var blank in block.Blanks)
            {
                if (blank.Options == null)
                {
                    blank.Options = new List<GapFillOption>();
                }

                blank.Options = blank.Options
                    .Where(option => option != null && !string.IsNullOrWhiteSpace(option.Value))
                    .Select(option =>
                    {
                        option.Value = option.Value.Trim();
                        option.DisplayText ??= option.Value;
                        if (string.IsNullOrWhiteSpace(option.Id))
                        {
                            option.Id = Guid.NewGuid().ToString();
                        }

                        return option;
                    })
                    .ToList();

                if (string.IsNullOrWhiteSpace(blank.Id))
                {
                    blank.Id = $"blank{Math.Max(1, blank.Index)}";
                }
            }
        }

        if (!Blocks.Any())
        {
            return;
        }

        if (GlobalOptions == null)
        {
            GlobalOptions = new List<GapFillOption>();
        }

        if (!GlobalOptions.Any())
        {
            var aggregated = Blocks
                .SelectMany(block => block.GlobalOptions ?? new List<GapFillOption>())
                .Where(option => option != null && !string.IsNullOrWhiteSpace(option.Value))
                .Select(option =>
                {
                    option.Value = option.Value.Trim();
                    option.DisplayText ??= option.Value;
                    if (string.IsNullOrWhiteSpace(option.Id))
                    {
                        option.Id = Guid.NewGuid().ToString();
                    }

                    return option;
                })
                .GroupBy(option => option.Value, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            GlobalOptions = aggregated;
        }
        else
        {
            GlobalOptions = GlobalOptions
                .Where(option => !string.IsNullOrWhiteSpace(option.Value))
                .Select(option =>
                {
                    option.Value = option.Value.Trim();
                    option.DisplayText ??= option.Value;
                    if (string.IsNullOrWhiteSpace(option.Id))
                    {
                        option.Id = Guid.NewGuid().ToString();
                    }

                    return option;
                })
                .GroupBy(option => option.Value, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }
    }

    private static string BuildContentWithTokens(string text, IEnumerable<GapFillGap> gaps)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var content = text;
        foreach (var gap in gaps)
        {
            var index = Math.Max(1, gap.Index);
            var legacyPlaceholder = $"{{{index}}}";
            var token = $"[[blank{index}]]";
            content = content.Replace(legacyPlaceholder, token, StringComparison.Ordinal);
        }

        if (!BlankTokenRegex.IsMatch(content))
        {
            var ordered = gaps.OrderBy(g => g.Index).ToList();
            foreach (var gap in ordered)
            {
                var token = $"[[blank{Math.Max(1, gap.Index)}]]";
                if (!content.Contains(token, StringComparison.Ordinal))
                {
                    content += $" {token}";
                }
            }
        }

        return content;
    }
}

