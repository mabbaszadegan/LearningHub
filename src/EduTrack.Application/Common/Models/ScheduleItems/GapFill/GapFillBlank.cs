using System;
using System.Collections.Generic;

namespace EduTrack.Application.Common.Models.ScheduleItems;

public class GapFillBlank
{
    public string Id { get; set; } = string.Empty;

    public int Index { get; set; }

    public string CorrectAnswer { get; set; } = string.Empty;

    public List<string> AlternativeAnswers { get; set; } = new();

    public string? CorrectOptionId { get; set; }

    public List<string> AlternativeOptionIds { get; set; } = new();

    public string? Hint { get; set; }

    public bool AllowManualInput { get; set; } = true;

    public bool AllowGlobalOptions { get; set; }

    public bool AllowBlankOptions { get; set; }

    public List<GapFillOption> Options { get; set; } = new();

    public string GetIdentifier()
    {
        if (!string.IsNullOrWhiteSpace(Id))
        {
            return Id;
        }

        return $"blank{Math.Max(1, Index)}";
    }
}

