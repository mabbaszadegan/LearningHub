namespace EduTrack.Domain.Entities;

/// <summary>
/// Class entity - represents a class instance of a course
/// </summary>
public class Class
{
    private readonly List<Enrollment> _enrollments = new();

    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string TeacherId { get; private set; } = string.Empty;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public Course Course { get; private set; } = null!;
    public User Teacher { get; private set; } = null!;
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    // Private constructor for EF Core
    private Class() { }

    public static Class Create(int courseId, string name, string? description, 
        string teacherId, DateTimeOffset startDate, DateTimeOffset? endDate = null)
    {
        if (courseId <= 0)
            throw new ArgumentException("Course ID must be greater than 0", nameof(courseId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new ArgumentException("Teacher ID cannot be null or empty", nameof(teacherId));
        
        if (endDate.HasValue && endDate.Value <= startDate)
            throw new ArgumentException("End date must be after start date");

        return new Class
        {
            CourseId = courseId,
            Name = name,
            Description = description,
            TeacherId = teacherId,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStartDate(DateTimeOffset startDate)
    {
        if (EndDate.HasValue && EndDate.Value <= startDate)
            throw new ArgumentException("Start date must be before end date");

        StartDate = startDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateEndDate(DateTimeOffset? endDate)
    {
        if (endDate.HasValue && endDate.Value <= StartDate)
            throw new ArgumentException("End date must be after start date");

        EndDate = endDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void EnrollStudent(User student)
    {
        if (student == null)
            throw new ArgumentNullException(nameof(student));

        if (_enrollments.Any(e => e.StudentId == student.Id))
            throw new InvalidOperationException("Student is already enrolled in this class");

        var enrollment = Enrollment.Create(Id, student.Id);
        _enrollments.Add(enrollment);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UnenrollStudent(User student)
    {
        if (student == null)
            throw new ArgumentNullException(nameof(student));

        var enrollment = _enrollments.FirstOrDefault(e => e.StudentId == student.Id);
        if (enrollment != null)
        {
            _enrollments.Remove(enrollment);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool IsStudentEnrolled(string studentId)
    {
        return _enrollments.Any(e => e.StudentId == studentId && e.IsActive);
    }

    public int GetEnrolledStudentCount()
    {
        return _enrollments.Count(e => e.IsActive);
    }

    public bool IsCompleted()
    {
        return EndDate.HasValue && EndDate.Value <= DateTimeOffset.UtcNow;
    }

    public TimeSpan? GetDuration()
    {
        if (!EndDate.HasValue)
            return null;

        return EndDate.Value - StartDate;
    }
}
