using System;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.TeachingSessions;

public class TeachingSessionAssignmentDto
{
    public int ScheduleItemId { get; set; }
    public int? TeachingPlanId { get; set; }
    public int? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ScheduleItemType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public ScheduleItemStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsMandatory { get; set; }
    public decimal? MaxScore { get; set; }
    public int CurrentStep { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOverdue { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public IReadOnlyList<string> AssignedGroups { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> AssignedStudents { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> AssignedSubChapters { get; set; } = Array.Empty<string>();
}

