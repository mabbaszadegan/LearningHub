using EduTrack.Domain.Enums;
using EduTrack.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Domain.Entities;

/// <summary>
/// User aggregate root - represents a user in the system
/// </summary>
public class User : IdentityUser
{
    private readonly List<Class> _classes = new();
    private readonly List<Enrollment> _enrollments = new();
    private readonly List<CourseEnrollment> _courseEnrollments = new();
    private readonly List<CourseAccess> _courseAccesses = new();
    private readonly List<Attempt> _attempts = new();
    private readonly List<Progress> _progresses = new();
    private readonly List<ActivityLog> _activityLogs = new();
    private readonly List<TeachingPlan> _teachingPlans = new();
    private readonly List<Submission> _submissions = new();

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public Profile? Profile { get; private set; }
    public IReadOnlyCollection<Class> Classes => _classes.AsReadOnly();
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();
    public IReadOnlyCollection<CourseEnrollment> CourseEnrollments => _courseEnrollments.AsReadOnly();
    public IReadOnlyCollection<CourseAccess> CourseAccesses => _courseAccesses.AsReadOnly();
    public IReadOnlyCollection<Attempt> Attempts => _attempts.AsReadOnly();
    public IReadOnlyCollection<Progress> Progresses => _progresses.AsReadOnly();
    public IReadOnlyCollection<ActivityLog> ActivityLogs => _activityLogs.AsReadOnly();
    public IReadOnlyCollection<TeachingPlan> TeachingPlans => _teachingPlans.AsReadOnly();
    public IReadOnlyCollection<Submission> Submissions => _submissions.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Private constructor for EF Core
    private User() { }

    public static User Create(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AssignToClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        if (_classes.Any(c => c.Id == classEntity.Id))
            throw new InvalidOperationException("User is already assigned to this class");

        _classes.Add(classEntity);
    }

    public void RemoveFromClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        var classToRemove = _classes.FirstOrDefault(c => c.Id == classEntity.Id);
        if (classToRemove != null)
        {
            _classes.Remove(classToRemove);
        }
    }

    public void EnrollInClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        if (_enrollments.Any(e => e.ClassId == classEntity.Id))
            throw new InvalidOperationException("User is already enrolled in this class");

        var enrollment = Enrollment.Create(classEntity.Id, Id);
        _enrollments.Add(enrollment);
    }

    public void UnenrollFromClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        var enrollment = _enrollments.FirstOrDefault(e => e.ClassId == classEntity.Id);
        if (enrollment != null)
        {
            _enrollments.Remove(enrollment);
        }
    }

    public void AddAttempt(Attempt attempt)
    {
        if (attempt == null)
            throw new ArgumentNullException(nameof(attempt));

        if (_attempts.Any(a => a.Id == attempt.Id))
            throw new InvalidOperationException("Attempt already exists for this user");

        _attempts.Add(attempt);
    }

    public void AddProgress(Progress progress)
    {
        if (progress == null)
            throw new ArgumentNullException(nameof(progress));

        if (_progresses.Any(p => p.Id == progress.Id))
            throw new InvalidOperationException("Progress already exists for this user");

        _progresses.Add(progress);
    }

    public void LogActivity(string action, string? entityType = null, int? entityId = null, 
        string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        var activityLog = ActivityLog.Create(Id, action, entityType, entityId, details, ipAddress, userAgent);
        _activityLogs.Add(activityLog);
    }

    // Course Enrollment Methods
    public void EnrollInCourse(Course course)
    {
        if (course == null)
            throw new ArgumentNullException(nameof(course));

        if (_courseEnrollments.Any(e => e.CourseId == course.Id))
            throw new InvalidOperationException("User is already enrolled in this course");

        var enrollment = CourseEnrollment.Create(Id, course.Id);
        _courseEnrollments.Add(enrollment);
    }

    public void UnenrollFromCourse(Course course)
    {
        if (course == null)
            throw new ArgumentNullException(nameof(course));

        var enrollment = _courseEnrollments.FirstOrDefault(e => e.CourseId == course.Id);
        if (enrollment != null)
        {
            _courseEnrollments.Remove(enrollment);
        }
    }

    public void GrantCourseAccess(Course course, CourseAccessLevel accessLevel, string? grantedBy = null, 
        DateTimeOffset? expiresAt = null, string? notes = null)
    {
        if (course == null)
            throw new ArgumentNullException(nameof(course));

        if (_courseAccesses.Any(a => a.CourseId == course.Id))
            throw new InvalidOperationException("User already has access to this course");

        var access = CourseAccess.Create(Id, course.Id, accessLevel, grantedBy, expiresAt, notes);
        _courseAccesses.Add(access);
    }

    public void RevokeCourseAccess(Course course, string? revokedBy = null)
    {
        if (course == null)
            throw new ArgumentNullException(nameof(course));

        var access = _courseAccesses.FirstOrDefault(a => a.CourseId == course.Id);
        if (access != null)
        {
            access.RevokeAccess(revokedBy);
        }
    }

    public bool IsEnrolledInCourse(int courseId)
    {
        return _courseEnrollments.Any(e => e.CourseId == courseId && e.IsActive);
    }

    public bool HasCourseAccess(int courseId)
    {
        return _courseAccesses.Any(a => a.CourseId == courseId && a.IsValid());
    }

    public CourseEnrollment? GetCourseEnrollment(int courseId)
    {
        return _courseEnrollments.FirstOrDefault(e => e.CourseId == courseId && e.IsActive);
    }

    public CourseAccess? GetCourseAccess(int courseId)
    {
        return _courseAccesses.FirstOrDefault(a => a.CourseId == courseId && a.IsValid());
    }

    public int GetTotalCourseEnrollments()
    {
        return _courseEnrollments.Count(e => e.IsActive);
    }

    public int GetTotalCourseAccesses()
    {
        return _courseAccesses.Count(a => a.IsValid());
    }

    public void AddTeachingPlan(TeachingPlan teachingPlan)
    {
        if (teachingPlan == null)
            throw new ArgumentNullException(nameof(teachingPlan));

        if (_teachingPlans.Any(tp => tp.Id == teachingPlan.Id))
            throw new InvalidOperationException("Teaching plan already exists for this user");

        _teachingPlans.Add(teachingPlan);
    }

    public void AddSubmission(Submission submission)
    {
        if (submission == null)
            throw new ArgumentNullException(nameof(submission));

        if (_submissions.Any(s => s.Id == submission.Id))
            throw new InvalidOperationException("Submission already exists for this user");

        _submissions.Add(submission);
    }
}
