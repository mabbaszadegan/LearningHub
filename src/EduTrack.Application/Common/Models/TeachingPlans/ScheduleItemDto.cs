using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.TeachingPlans;

public enum ScheduleItemStatus
{
    Draft = 0,
    Published = 1,
    Active = 2,
    Completed = 3,
    Expired = 4
}

public class ScheduleItemDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public int? GroupId { get; set; } // Legacy single group assignment
    public string? GroupName { get; set; }
    public int? LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public ScheduleItemType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsMandatory { get; set; }
    public DisciplineType? DisciplineHint { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public decimal? MaxScore { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsUpcoming { get; set; }
    public bool IsActive { get; set; }
    public TimeSpan? TimeUntilDue { get; set; }
    public int SubmissionCount { get; set; }
    public int CompletedSubmissions { get; set; }
    
    // New properties for multiple assignments
    public List<ScheduleItemGroupAssignmentDto> GroupAssignments { get; set; } = new();
    public List<ScheduleItemSubChapterAssignmentDto> SubChapterAssignments { get; set; } = new();
    public List<ScheduleItemStudentAssignmentDto> StudentAssignments { get; set; } = new();
    public List<int> StudentProfileIds { get; set; } = new();
    public bool IsAssignedToAllGroups { get; set; }
    
    // Additional properties for status and step tracking
    public int CurrentStep { get; set; }
    public ScheduleItemStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
}
