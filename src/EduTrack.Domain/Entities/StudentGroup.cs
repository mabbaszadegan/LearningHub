namespace EduTrack.Domain.Entities;

/// <summary>
/// StudentGroup entity - represents a group of students within a teaching plan
/// </summary>
public class StudentGroup
{
    private readonly List<GroupMember> _members = new();

    public int Id { get; private set; }
    public int TeachingPlanId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Navigation properties
    public TeachingPlan TeachingPlan { get; private set; } = null!;
    public IReadOnlyCollection<GroupMember> Members => _members.AsReadOnly();
    
    // Navigation properties for new entities
    public ICollection<TeachingSessionPlan> SessionPlans { get; set; } = new List<TeachingSessionPlan>();
    public ICollection<TeachingSessionExecution> SessionExecutions { get; set; } = new List<TeachingSessionExecution>();
    public ICollection<TeachingSessionTopicCoverage> TopicCoverages { get; set; } = new List<TeachingSessionTopicCoverage>();
    public ICollection<TeachingPlanProgress> PlanProgresses { get; set; } = new List<TeachingPlanProgress>();

    // Private constructor for EF Core
    private StudentGroup() { }

    public static StudentGroup Create(int teachingPlanId, string name)
    {
        if (teachingPlanId <= 0)
            throw new ArgumentException("Teaching Plan ID must be greater than 0", nameof(teachingPlanId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        return new StudentGroup
        {
            TeachingPlanId = teachingPlanId,
            Name = name
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        Name = name;
    }

    public void AddMember(GroupMember member)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));

        if (_members.Any(m => m.Id == member.Id))
            throw new InvalidOperationException("Member already exists in this group");

        _members.Add(member);
    }

    public void RemoveMember(GroupMember member)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));

        var memberToRemove = _members.FirstOrDefault(m => m.Id == member.Id);
        if (memberToRemove != null)
        {
            _members.Remove(memberToRemove);
        }
    }

    public int GetTotalMembers() => _members.Count;

    public bool HasStudent(string studentId)
    {
        return _members.Any(m => m.StudentId == studentId);
    }
}