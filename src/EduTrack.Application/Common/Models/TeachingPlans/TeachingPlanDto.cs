namespace EduTrack.Application.Common.Models.TeachingPlans;

public class TeachingPlanDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Objectives { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int GroupCount { get; set; }
    public int ScheduleItemCount { get; set; }
    public int TotalStudents { get; set; }
    public List<StudentGroupDto> Groups { get; set; } = new();
    public List<ScheduleItemDto> ScheduleItems { get; set; } = new();
}
