namespace EduTrack.Application.Common.Models.ScheduleItems;

public class ScheduleItemGroupAssignmentDto
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public int StudentGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class ScheduleItemSubChapterAssignmentDto
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public int SubChapterId { get; set; }
    public string SubChapterTitle { get; set; } = string.Empty;
    public string ChapterTitle { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class ScheduleItemStudentAssignmentDto
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public int StudentProfileId { get; set; }
    public string StudentUserId { get; set; } = string.Empty;
    public string StudentDisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

