using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// Course aggregate root - represents a course in the system
/// </summary>
public class Course
{
    private readonly List<Module> _modules = new();
    private readonly List<Chapter> _chapters = new();
    private readonly List<Class> _classes = new();
    private readonly List<CourseEnrollment> _enrollments = new();
    private readonly List<CourseAccess> _accesses = new();
    private readonly List<TeachingPlan> _teachingPlans = new();

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Thumbnail { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DisciplineType DisciplineType { get; private set; } = DisciplineType.Other;

    // Navigation properties
    public IReadOnlyCollection<Module> Modules => _modules.AsReadOnly();
    public IReadOnlyCollection<Chapter> Chapters => _chapters.AsReadOnly();
    public IReadOnlyCollection<Class> Classes => _classes.AsReadOnly();
    public IReadOnlyCollection<CourseEnrollment> Enrollments => _enrollments.AsReadOnly();
    public IReadOnlyCollection<CourseAccess> Accesses => _accesses.AsReadOnly();
    public IReadOnlyCollection<TeachingPlan> TeachingPlans => _teachingPlans.AsReadOnly();

    // Private constructor for EF Core
    private Course() { }

    public static Course Create(string title, string? description, string? thumbnail, 
        int order, string createdBy, DisciplineType disciplineType = DisciplineType.Other)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new Course
        {
            Title = title,
            Description = description,
            Thumbnail = thumbnail,
            Order = order,
            CreatedBy = createdBy,
            DisciplineType = disciplineType,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
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

    public void UpdateThumbnail(string? thumbnail)
    {
        Thumbnail = thumbnail;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        Order = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDisciplineType(DisciplineType disciplineType)
    {
        DisciplineType = disciplineType;
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

    public void AddModule(Module module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        if (_modules.Any(m => m.Id == module.Id))
            throw new InvalidOperationException("Module already exists in this course");

        _modules.Add(module);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveModule(Module module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        var moduleToRemove = _modules.FirstOrDefault(m => m.Id == module.Id);
        if (moduleToRemove != null)
        {
            _modules.Remove(moduleToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddChapter(Chapter chapter)
    {
        if (chapter == null)
            throw new ArgumentNullException(nameof(chapter));

        if (_chapters.Any(c => c.Id == chapter.Id))
            throw new InvalidOperationException("Chapter already exists in this course");

        _chapters.Add(chapter);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveChapter(Chapter chapter)
    {
        if (chapter == null)
            throw new ArgumentNullException(nameof(chapter));

        var chapterToRemove = _chapters.FirstOrDefault(c => c.Id == chapter.Id);
        if (chapterToRemove != null)
        {
            _chapters.Remove(chapterToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        if (_classes.Any(c => c.Id == classEntity.Id))
            throw new InvalidOperationException("Class already exists for this course");

        _classes.Add(classEntity);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        var classToRemove = _classes.FirstOrDefault(c => c.Id == classEntity.Id);
        if (classToRemove != null)
        {
            _classes.Remove(classToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalModules()
    {
        return _modules.Count;
    }

    public int GetTotalLessons()
    {
        return _modules.Sum(m => m.Lessons.Count);
    }

    public int GetTotalChapters()
    {
        return _chapters.Count;
    }

    public int GetTotalClasses()
    {
        return _classes.Count;
    }

    public int GetTotalEnrollments()
    {
        return _enrollments.Count(e => e.IsActive);
    }

    public int GetTotalAccesses()
    {
        return _accesses.Count(a => a.IsActive);
    }

    public int GetTotalTeachingPlans()
    {
        return _teachingPlans.Count;
    }

    public void AddEnrollment(CourseEnrollment enrollment)
    {
        if (enrollment == null)
            throw new ArgumentNullException(nameof(enrollment));

        if (_enrollments.Any(e => e.StudentId == enrollment.StudentId))
            throw new InvalidOperationException("Student is already enrolled in this course");

        _enrollments.Add(enrollment);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveEnrollment(CourseEnrollment enrollment)
    {
        if (enrollment == null)
            throw new ArgumentNullException(nameof(enrollment));

        var enrollmentToRemove = _enrollments.FirstOrDefault(e => e.Id == enrollment.Id);
        if (enrollmentToRemove != null)
        {
            _enrollments.Remove(enrollmentToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddAccess(CourseAccess access)
    {
        if (access == null)
            throw new ArgumentNullException(nameof(access));

        if (_accesses.Any(a => a.StudentId == access.StudentId))
            throw new InvalidOperationException("Student already has access to this course");

        _accesses.Add(access);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveAccess(CourseAccess access)
    {
        if (access == null)
            throw new ArgumentNullException(nameof(access));

        var accessToRemove = _accesses.FirstOrDefault(a => a.Id == access.Id);
        if (accessToRemove != null)
        {
            _accesses.Remove(accessToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddTeachingPlan(TeachingPlan teachingPlan)
    {
        if (teachingPlan == null)
            throw new ArgumentNullException(nameof(teachingPlan));

        if (_teachingPlans.Any(tp => tp.Id == teachingPlan.Id))
            throw new InvalidOperationException("Teaching plan already exists for this course");

        _teachingPlans.Add(teachingPlan);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveTeachingPlan(TeachingPlan teachingPlan)
    {
        if (teachingPlan == null)
            throw new ArgumentNullException(nameof(teachingPlan));

        var planToRemove = _teachingPlans.FirstOrDefault(tp => tp.Id == teachingPlan.Id);
        if (planToRemove != null)
        {
            _teachingPlans.Remove(planToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool IsStudentEnrolled(string studentId)
    {
        return _enrollments.Any(e => e.StudentId == studentId && e.IsActive);
    }

    public bool HasStudentAccess(string studentId)
    {
        return _accesses.Any(a => a.StudentId == studentId && a.IsValid());
    }

    public CourseEnrollment? GetStudentEnrollment(string studentId)
    {
        return _enrollments.FirstOrDefault(e => e.StudentId == studentId && e.IsActive);
    }

    public CourseAccess? GetStudentAccess(string studentId)
    {
        return _accesses.FirstOrDefault(a => a.StudentId == studentId && a.IsValid());
    }
}
