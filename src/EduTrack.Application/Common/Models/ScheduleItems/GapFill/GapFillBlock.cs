using System;
using System.Collections.Generic;

namespace EduTrack.Application.Common.Models.ScheduleItems;

public class GapFillBlock
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int Order { get; set; }

    public string? Instruction { get; set; }

    public string Content { get; set; } = string.Empty;

    public string TextContent { get; set; } = string.Empty;

    public string AnswerType { get; set; } = "exact";

    public bool CaseSensitive { get; set; }

    public bool ShowGlobalOptions { get; set; }

    public decimal Points { get; set; } = 1;

    public bool IsRequired { get; set; } = true;

    public GapFillMedia? Media { get; set; }

    public List<GapFillOption> GlobalOptions { get; set; } = new();

    public List<GapFillBlank> Blanks { get; set; } = new();
}

