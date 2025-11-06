using EduTrack.Application.Common.Models.TeachingPlans;

namespace EduTrack.WebApp.Areas.Student.Models;

/// <summary>
/// DTO for schedule item with study statistics
/// </summary>
public class ScheduleItemWithStats
{
    public ScheduleItemDto Item { get; set; } = null!;
    public int TotalStudyTimeSeconds { get; set; }
    public int StudySessionsCount { get; set; }
    public DateTimeOffset? LastStudyDate { get; set; }
}

