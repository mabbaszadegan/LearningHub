namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItemSubChapterAssignment entity - represents assignment of a schedule item to specific subchapters
/// </summary>
public class ScheduleItemSubChapterAssignment
{
    public int Id { get; private set; }
    public int ScheduleItemId { get; private set; }
    public int SubChapterId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public SubChapter SubChapter { get; private set; } = null!;

    // Private constructor for EF Core
    private ScheduleItemSubChapterAssignment() { }

    public static ScheduleItemSubChapterAssignment Create(int scheduleItemId, int subChapterId)
    {
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (subChapterId <= 0)
            throw new ArgumentException("SubChapter ID must be greater than 0", nameof(subChapterId));

        return new ScheduleItemSubChapterAssignment
        {
            ScheduleItemId = scheduleItemId,
            SubChapterId = subChapterId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
