using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Common.Models.TeachingPlans;

namespace EduTrack.Application.Common.Models.TeachingSessions;

public class SessionCompletionDataDto
{
    public int SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTimeOffset SessionDate { get; set; }
    public int TeachingPlanId { get; set; }
    public string TeachingPlanTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public bool HasPlan { get; set; }
    public List<GroupDataDto> Groups { get; set; } = new();
    public List<SubTopicDto> AvailableSubTopics { get; set; } = new();
    public List<LessonDto> AvailableLessons { get; set; } = new();
    public List<PlannedItemDto>? PlannedItems { get; set; }
    public SessionCompletionProgressDto? CompletionProgress { get; set; }
}
