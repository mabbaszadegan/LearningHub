namespace EduTrack.Domain.Entities;

/// <summary>
/// GroupMember entity - represents a student's membership in a group
/// </summary>
public class GroupMember
{
    public int Id { get; private set; }
    public int StudentGroupId { get; private set; }
    public string StudentId { get; private set; } = string.Empty;

    // Navigation properties
    public StudentGroup StudentGroup { get; private set; } = null!;
    public User Student { get; private set; } = null!;

    // Private constructor for EF Core
    private GroupMember() { }

    public static GroupMember Create(int studentGroupId, string studentId)
    {
        if (studentGroupId <= 0)
            throw new ArgumentException("Student Group ID must be greater than 0", nameof(studentGroupId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

        return new GroupMember
        {
            StudentGroupId = studentGroupId,
            StudentId = studentId
        };
    }
}