namespace EduTrack.Application.Common.Models.TeachingSessions;

// SubChapter Coverage Models
public class SubChapterCoverageDataDto
{
    public int SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTimeOffset SessionDate { get; set; }
    public int TeachingPlanId { get; set; }
    public string TeachingPlanTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public List<GroupDataDto> Groups { get; set; } = new();
    public List<Courses.ChapterDto> Chapters { get; set; } = new();
    public List<SubChapterCoverageDto> ExistingCoverages { get; set; } = new();
    public List<GroupCoverageStatsDto> GroupCoverageStats { get; set; } = new();
}

public class GroupCoverageStatsDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<ChapterGroupStatsDto> ChapterStats { get; set; } = new();
}

public class ChapterGroupStatsDto
{
    public int ChapterId { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
    public int TotalCoverageCount { get; set; }
    public double AverageProgressPercentage { get; set; }
    public List<SubChapterGroupStatsDto> SubChapterStats { get; set; } = new();
}

public class SubChapterGroupStatsDto
{
    public int SubChapterId { get; set; }
    public string SubChapterTitle { get; set; } = string.Empty;
    public int CoverageCount { get; set; }
    public double AverageProgressPercentage { get; set; }
}

public class SubChapterCoverageDto
{
    public int Id { get; set; }
    public int TeachingSessionReportId { get; set; }
    public int StudentGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int SubChapterId { get; set; }
    public string SubChapterTitle { get; set; } = string.Empty;
    public string ChapterTitle { get; set; } = string.Empty;
    public bool WasPlanned { get; set; }
    public bool WasCovered { get; set; }
    public int CoveragePercentage { get; set; }
    public string? TeacherNotes { get; set; }
    public string? Challenges { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class SubChapterCoverageStepDataDto
{
    public int SessionId { get; set; }
    public List<GroupSubChapterCoverageDto> GroupCoverages { get; set; } = new();
}

public class GroupSubChapterCoverageDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<SubChapterCoverageItemDto> SubChapterCoverages { get; set; } = new();
    public string? GeneralNotes { get; set; }
    public string? Challenges { get; set; }
    public string? Recommendations { get; set; }
}

public class SubChapterCoverageItemDto
{
    public int SubChapterId { get; set; }
    public string SubChapterTitle { get; set; } = string.Empty;
    public string ChapterTitle { get; set; } = string.Empty;
    public bool WasPlanned { get; set; }
    public bool WasCovered { get; set; }
    public int CoveragePercentage { get; set; }
    public string? TeacherNotes { get; set; }
    public string? Challenges { get; set; }
}
