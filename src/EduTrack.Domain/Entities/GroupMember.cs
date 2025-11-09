namespace EduTrack.Domain.Entities;

/// <summary>
/// GroupMember entity - represents a student profile's membership in a group
/// </summary>
public class GroupMember
{
    public int Id { get; private set; }
    public int StudentGroupId { get; private set; }
    public int StudentProfileId { get; private set; }

    // Navigation properties
    public StudentGroup StudentGroup { get; private set; } = null!;
    public StudentProfile StudentProfile { get; private set; } = null!;

    // Convenience accessor
    public string StudentId => StudentProfile?.UserId ?? string.Empty;

    // Private constructor for EF Core
    private GroupMember() { }

    public static GroupMember Create(int studentGroupId, int studentProfileId)
    {
        if (studentGroupId <= 0)
            throw new ArgumentException("Student Group ID must be greater than 0", nameof(studentGroupId));

        if (studentProfileId <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));

        return new GroupMember
        {
            StudentGroupId = studentGroupId,
            StudentProfileId = studentProfileId
        };
    }
}