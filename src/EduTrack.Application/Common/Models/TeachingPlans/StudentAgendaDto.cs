using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.TeachingPlans;

public class StudentAgendaDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int? StudentProfileId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public LearningMode LearningMode { get; set; }
    public List<ScheduleItemDto> UpcomingItems { get; set; } = new();
    public List<ScheduleItemDto> InProgressItems { get; set; } = new();
    public List<ScheduleItemDto> OverdueItems { get; set; } = new();
    public List<ScheduleItemDto> CompletedItems { get; set; } = new();
    public int TotalItems { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
    public double CompletionPercentage { get; set; }
}
