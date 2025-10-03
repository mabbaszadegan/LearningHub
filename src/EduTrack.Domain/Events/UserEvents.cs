using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Events;

/// <summary>
/// Event raised when a user is created
/// </summary>
public class UserCreatedEvent : DomainEvent
{
    public string UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public UserCreatedEvent(string userId, string email, string firstName, string lastName, int version)
        : base(version)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
}

/// <summary>
/// Event raised when a user logs in
/// </summary>
public class UserLoggedInEvent : DomainEvent
{
    public string UserId { get; }
    public DateTimeOffset LoginTime { get; }

    public UserLoggedInEvent(string userId, DateTimeOffset loginTime, int version)
        : base(version)
    {
        UserId = userId;
        LoginTime = loginTime;
    }
}

/// <summary>
/// Event raised when a course is created
/// </summary>
public class CourseCreatedEvent : DomainEvent
{
    public int CourseId { get; }
    public string Title { get; }
    public string CreatedBy { get; }

    public CourseCreatedEvent(int courseId, string title, string createdBy, int version)
        : base(version)
    {
        CourseId = courseId;
        Title = title;
        CreatedBy = createdBy;
    }
}

/// <summary>
/// Event raised when a student enrolls in a class
/// </summary>
public class StudentEnrolledEvent : DomainEvent
{
    public string StudentId { get; }
    public int ClassId { get; }
    public int CourseId { get; }

    public StudentEnrolledEvent(string studentId, int classId, int courseId, int version)
        : base(version)
    {
        StudentId = studentId;
        ClassId = classId;
        CourseId = courseId;
    }
}

/// <summary>
/// Event raised when a student completes an exam
/// </summary>
public class ExamCompletedEvent : DomainEvent
{
    public string StudentId { get; }
    public int ExamId { get; }
    public int Score { get; }
    public bool IsPassed { get; }

    public ExamCompletedEvent(string studentId, int examId, int score, bool isPassed, int version)
        : base(version)
    {
        StudentId = studentId;
        ExamId = examId;
        Score = score;
        IsPassed = isPassed;
    }
}
