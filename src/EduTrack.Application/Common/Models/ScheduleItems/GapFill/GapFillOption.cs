using System;

namespace EduTrack.Application.Common.Models.ScheduleItems;

public class GapFillOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Value { get; set; } = string.Empty;

    public string? DisplayText { get; set; }
}

