using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// CourseAccess entity - represents a student's access level to course content
/// This controls what parts of a course a student can access
/// </summary>
public class CourseAccess
{
    public int Id { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public int CourseId { get; private set; }
    public CourseAccessLevel AccessLevel { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? GrantedBy { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public User Student { get; private set; } = null!;
    public Course Course { get; private set; } = null!;

    // Private constructor for EF Core
    private CourseAccess() { }

    public static CourseAccess Create(string studentId, int courseId, CourseAccessLevel accessLevel, 
        string? grantedBy = null, DateTimeOffset? expiresAt = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));
        
        if (courseId <= 0)
            throw new ArgumentException("Course ID must be greater than 0", nameof(courseId));

        return new CourseAccess
        {
            StudentId = studentId,
            CourseId = courseId,
            AccessLevel = accessLevel,
            GrantedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true,
            GrantedBy = grantedBy,
            Notes = notes
        };
    }

    public void UpdateAccessLevel(CourseAccessLevel newLevel, string? updatedBy = null)
    {
        AccessLevel = newLevel;
        GrantedAt = DateTimeOffset.UtcNow;
        GrantedBy = updatedBy;
    }

    public void ExtendAccess(DateTimeOffset newExpiryDate, string? updatedBy = null)
    {
        if (newExpiryDate <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Expiry date must be in the future", nameof(newExpiryDate));

        ExpiresAt = newExpiryDate;
        GrantedBy = updatedBy;
    }

    public void RevokeAccess(string? revokedBy = null)
    {
        IsActive = false;
        GrantedBy = revokedBy;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow;
    }

    public bool IsValid()
    {
        return IsActive && !IsExpired();
    }

    public bool CanAccessContent()
    {
        return IsValid() && AccessLevel != CourseAccessLevel.None;
    }

    public bool CanAccessLessons()
    {
        return CanAccessContent() && AccessLevel >= CourseAccessLevel.Lessons;
    }

    public bool CanAccessExams()
    {
        return CanAccessContent() && AccessLevel >= CourseAccessLevel.Exams;
    }

    public bool CanAccessResources()
    {
        return CanAccessContent() && AccessLevel >= CourseAccessLevel.Resources;
    }

    public bool CanAccessFullCourse()
    {
        return CanAccessContent() && AccessLevel == CourseAccessLevel.Full;
    }
}
