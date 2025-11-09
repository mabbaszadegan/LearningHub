using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItem entity - represents a scheduled assignment or activity in a teaching plan
/// </summary>
public class ScheduleItem
{
    public int Id { get; private set; }
    public int TeachingPlanId { get; private set; }
    public int? GroupId { get; private set; }
    public int? LessonId { get; private set; }
    public ScheduleItemType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public bool IsMandatory { get; private set; }
    public DisciplineType? DisciplineHint { get; private set; }
    public string ContentJson { get; private set; } = string.Empty;
    public decimal? MaxScore { get; private set; }
    public int? SessionReportId { get; set; }
    public int CurrentStep { get; private set; } = 1;
    public bool IsCompleted { get; private set; } = false;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public TeachingPlan TeachingPlan { get; private set; } = null!;
    public StudentGroup? Group { get; private set; } // Legacy single group assignment
    public Lesson? Lesson { get; private set; }
    public TeachingSessionReport? SessionReport { get; set; }
    
    // Navigation properties for multiple assignments
    public ICollection<ScheduleItemGroupAssignment> GroupAssignments { get; set; } = new List<ScheduleItemGroupAssignment>();
    public ICollection<ScheduleItemSubChapterAssignment> SubChapterAssignments { get; set; } = new List<ScheduleItemSubChapterAssignment>();
    public ICollection<ScheduleItemStudentAssignment> StudentAssignments { get; set; } = new List<ScheduleItemStudentAssignment>();

    // Private constructor for EF Core
    private ScheduleItem() { }

    public static ScheduleItem Create(int teachingPlanId, ScheduleItemType type, string title, 
        string? description, DateTimeOffset startDate, DateTimeOffset? dueDate, bool isMandatory,
        string contentJson, decimal? maxScore = null, int? groupId = null, int? lessonId = null,
        DisciplineType? disciplineHint = null)
    {
        if (teachingPlanId <= 0)
            throw new ArgumentException("Teaching Plan ID must be greater than 0", nameof(teachingPlanId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(contentJson))
            throw new ArgumentException("Content JSON cannot be null or empty", nameof(contentJson));
        
        if (maxScore.HasValue && maxScore.Value < 0)
            throw new ArgumentException("Max score cannot be negative", nameof(maxScore));

        return new ScheduleItem
        {
            TeachingPlanId = teachingPlanId,
            GroupId = groupId,
            LessonId = lessonId,
            Type = type,
            Title = title,
            Description = description,
            StartDate = startDate,
            DueDate = dueDate,
            IsMandatory = isMandatory,
            DisciplineHint = disciplineHint,
            ContentJson = contentJson,
            MaxScore = maxScore,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDates(DateTimeOffset startDate, DateTimeOffset? dueDate)
    {
        StartDate = startDate;
        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMandatory(bool isMandatory)
    {
        IsMandatory = isMandatory;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateContent(string contentJson)
    {
        // Allow empty content but ensure it's a valid JSON string
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            ContentJson = "{}";
        }
        else
        {
            ContentJson = contentJson;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMaxScore(decimal? maxScore)
    {
        if (maxScore.HasValue && maxScore.Value < 0)
            throw new ArgumentException("Max score cannot be negative", nameof(maxScore));

        MaxScore = maxScore;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateCurrentStep(int step)
    {
        if (step < 1 || step > 4)
            throw new ArgumentException("Step must be between 1 and 4", nameof(step));

        CurrentStep = step;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
        CurrentStep = 4;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsIncomplete()
    {
        IsCompleted = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAssignment(int? groupId, int? lessonId)
    {
        GroupId = groupId;
        LessonId = lessonId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsOverdue()
    {
        return DueDate.HasValue && DateTimeOffset.UtcNow > DueDate.Value;
    }

    public bool IsUpcoming()
    {
        return DateTimeOffset.UtcNow < StartDate;
    }

    public bool IsActive()
    {
        return DateTimeOffset.UtcNow >= StartDate && (!DueDate.HasValue || DateTimeOffset.UtcNow <= DueDate.Value);
    }

    public TimeSpan? GetTimeUntilDue()
    {
        if (!DueDate.HasValue) return null;
        return DueDate.Value - DateTimeOffset.UtcNow;
    }

    public void AddGroupAssignment(ScheduleItemGroupAssignment groupAssignment)
    {
        if (groupAssignment == null)
            throw new ArgumentNullException(nameof(groupAssignment));

        if (GroupAssignments.Any(ga => ga.StudentGroupId == groupAssignment.StudentGroupId))
            throw new InvalidOperationException("Group already assigned to this schedule item");

        GroupAssignments.Add(groupAssignment);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveGroupAssignment(int studentGroupId)
    {
        var assignment = GroupAssignments.FirstOrDefault(ga => ga.StudentGroupId == studentGroupId);
        if (assignment != null)
        {
            GroupAssignments.Remove(assignment);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddSubChapterAssignment(ScheduleItemSubChapterAssignment subChapterAssignment)
    {
        if (subChapterAssignment == null)
            throw new ArgumentNullException(nameof(subChapterAssignment));

        if (SubChapterAssignments.Any(sca => sca.SubChapterId == subChapterAssignment.SubChapterId))
            throw new InvalidOperationException("SubChapter already assigned to this schedule item");

        SubChapterAssignments.Add(subChapterAssignment);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveSubChapterAssignment(int subChapterId)
    {
        var assignment = SubChapterAssignments.FirstOrDefault(sca => sca.SubChapterId == subChapterId);
        if (assignment != null)
        {
            SubChapterAssignments.Remove(assignment);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool IsAssignedToGroup(int studentGroupId)
    {
        return GroupAssignments.Any(ga => ga.StudentGroupId == studentGroupId);
    }

    public bool IsAssignedToSubChapter(int subChapterId)
    {
        return SubChapterAssignments.Any(sca => sca.SubChapterId == subChapterId);
    }

    public bool IsAssignedToAllGroups()
    {
        return !GroupAssignments.Any();
    }

    public void AddStudentAssignment(ScheduleItemStudentAssignment studentAssignment)
    {
        if (studentAssignment == null)
            throw new ArgumentNullException(nameof(studentAssignment));

        var hasConflict = StudentAssignments.Any(sa =>
            sa.StudentProfileId == studentAssignment.StudentProfileId);

        if (hasConflict)
            throw new InvalidOperationException("Student already assigned to this schedule item");

        StudentAssignments.Add(studentAssignment);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveStudentAssignment(int studentProfileId)
    {
        if (studentProfileId <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));

        var assignment = StudentAssignments.FirstOrDefault(sa =>
            sa.StudentProfileId == studentProfileId);
        if (assignment != null)
        {
            StudentAssignments.Remove(assignment);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool IsAssignedToStudentProfile(int studentProfileId)
    {
        if (studentProfileId <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));

        return StudentAssignments.Any(sa => sa.StudentProfileId == studentProfileId);
    }

    public bool IsAssignedToAllStudents()
    {
        return !StudentAssignments.Any();
    }
}