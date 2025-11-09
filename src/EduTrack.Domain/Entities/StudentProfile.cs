using System.Collections.Generic;

namespace EduTrack.Domain.Entities;

/// <summary>
/// Represents an individual learner profile that belongs to a single student account (Identity user).
/// Enables families to manage multiple learners under one login.
/// </summary>
public class StudentProfile
{
    private readonly List<ScheduleItemStudentAssignment> _scheduleItemAssignments = new();
    private readonly List<ScheduleItemBlockAttempt> _blockAttempts = new();
    private readonly List<ScheduleItemBlockStatistics> _blockStatistics = new();
    private readonly List<Submission> _submissions = new();
    private readonly List<StudySession> _studySessions = new();
    private readonly List<StudentAnswer> _studentAnswers = new();
    private readonly List<CourseEnrollment> _courseEnrollments = new();
    private readonly List<GroupMember> _groupMemberships = new();

    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset? DateOfBirth { get; private set; }
    public string? GradeLevel { get; private set; }
    public string? Notes { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public IReadOnlyCollection<ScheduleItemStudentAssignment> ScheduleItemAssignments => _scheduleItemAssignments.AsReadOnly();
    public IReadOnlyCollection<ScheduleItemBlockAttempt> BlockAttempts => _blockAttempts.AsReadOnly();
    public IReadOnlyCollection<ScheduleItemBlockStatistics> BlockStatistics => _blockStatistics.AsReadOnly();
    public IReadOnlyCollection<Submission> Submissions => _submissions.AsReadOnly();
    public IReadOnlyCollection<StudySession> StudySessions => _studySessions.AsReadOnly();
    public IReadOnlyCollection<StudentAnswer> StudentAnswers => _studentAnswers.AsReadOnly();
    public IReadOnlyCollection<CourseEnrollment> CourseEnrollments => _courseEnrollments.AsReadOnly();
    public IReadOnlyCollection<GroupMember> GroupMemberships => _groupMemberships.AsReadOnly();

    // Private constructor for EF Core
    private StudentProfile() { }

    public static StudentProfile Create(string userId, string displayName, string? gradeLevel = null, DateTimeOffset? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty", nameof(displayName));

        var now = DateTimeOffset.UtcNow;

        return new StudentProfile
        {
            UserId = userId,
            DisplayName = displayName.Trim(),
            GradeLevel = gradeLevel?.Trim(),
            DateOfBirth = dateOfBirth,
            CreatedAt = now,
            UpdatedAt = now,
            IsArchived = false
        };
    }

    public void UpdateProfile(string displayName, string? gradeLevel = null, DateTimeOffset? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty", nameof(displayName));

        DisplayName = displayName.Trim();
        GradeLevel = string.IsNullOrWhiteSpace(gradeLevel) ? null : gradeLevel.Trim();
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Restore()
    {
        IsArchived = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

