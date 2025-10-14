using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using AutoFixture;

namespace EduTrack.TestUtilities;

/// <summary>
/// Fluent builder for creating test data with realistic scenarios
/// </summary>
public class TestDataBuilder
{
    private readonly Fixture _fixture;

    public TestDataBuilder()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    /// <summary>
    /// Creates a user builder for fluent user creation
    /// </summary>
    public UserBuilder CreateUser()
    {
        return new UserBuilder(_fixture);
    }

    /// <summary>
    /// Creates a course builder for fluent course creation
    /// </summary>
    public CourseBuilder CreateCourse()
    {
        return new CourseBuilder(_fixture);
    }

    /// <summary>
    /// Creates a class builder for fluent class creation
    /// </summary>
    public ClassBuilder CreateClass()
    {
        return new ClassBuilder(_fixture);
    }

    /// <summary>
    /// Creates a teaching plan builder for fluent teaching plan creation
    /// </summary>
    public TeachingPlanBuilder CreateTeachingPlan()
    {
        return new TeachingPlanBuilder(_fixture);
    }

    /// <summary>
    /// Creates a schedule item builder for fluent schedule item creation
    /// </summary>
    public ScheduleItemBuilder CreateScheduleItem()
    {
        return new ScheduleItemBuilder(_fixture);
    }
}

/// <summary>
/// Fluent builder for User entities
/// </summary>
public class UserBuilder
{
    private readonly Fixture _fixture;
    private string _email = "";
    private string _firstName = "";
    private string _lastName = "";

    public UserBuilder(Fixture fixture)
    {
        _fixture = fixture;
        _email = _fixture.Create<string>() + "@test.com";
        _firstName = _fixture.Create<string>();
        _lastName = _fixture.Create<string>();
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public UserBuilder AsStudent()
    {
        _firstName = "Student";
        _lastName = _fixture.Create<string>();
        return this;
    }

    public UserBuilder AsTeacher()
    {
        _firstName = "Teacher";
        _lastName = _fixture.Create<string>();
        return this;
    }

    public UserBuilder AsAdmin()
    {
        _firstName = "Admin";
        _lastName = _fixture.Create<string>();
        return this;
    }

    public User Build()
    {
        return User.Create(_email, _firstName, _lastName);
    }
}

/// <summary>
/// Fluent builder for Course entities
/// </summary>
public class CourseBuilder
{
    private readonly Fixture _fixture;
    private string _title = "";
    private string _description = "";
    private DisciplineType _disciplineType = DisciplineType.Math;

    public CourseBuilder(Fixture fixture)
    {
        _fixture = fixture;
        _title = _fixture.Create<string>();
        _description = _fixture.Create<string>();
    }

    public CourseBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CourseBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CourseBuilder OfType(DisciplineType disciplineType)
    {
        _disciplineType = disciplineType;
        return this;
    }

    public CourseBuilder AsMathematics()
    {
        _disciplineType = DisciplineType.Math;
        _title = "Mathematics Course";
        return this;
    }

    public CourseBuilder AsProgramming()
    {
        _disciplineType = DisciplineType.Programming;
        _title = "Programming Course";
        return this;
    }

    public CourseBuilder AsLanguage()
    {
        _disciplineType = DisciplineType.Language;
        _title = "Language Course";
        return this;
    }

    public Course Build()
    {
        return Course.Create(
            _title,
            _description,
            "test-thumbnail.jpg",
            _fixture.Create<int>(),
            "test-user",
            _disciplineType
        );
    }
}

/// <summary>
/// Fluent builder for Class entities
/// </summary>
public class ClassBuilder
{
    private readonly Fixture _fixture;
    private int _courseId;
    private string _name = "";
    private string _description = "";
    private string _teacherId = "";
    private DateTimeOffset _startDate = DateTimeOffset.UtcNow;
    private DateTimeOffset? _endDate = DateTimeOffset.UtcNow.AddDays(30);

    public ClassBuilder(Fixture fixture)
    {
        _fixture = fixture;
        _name = _fixture.Create<string>();
        _description = _fixture.Create<string>();
        _teacherId = "test-teacher";
    }

    public ClassBuilder ForCourse(int courseId)
    {
        _courseId = courseId;
        return this;
    }

    public ClassBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ClassBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ClassBuilder WithTeacher(string teacherId)
    {
        _teacherId = teacherId;
        return this;
    }

    public ClassBuilder WithDates(DateTimeOffset startDate, DateTimeOffset? endDate = null)
    {
        _startDate = startDate;
        _endDate = endDate;
        return this;
    }

    public ClassBuilder AsActive()
    {
        _startDate = DateTimeOffset.UtcNow.AddDays(-7);
        _endDate = DateTimeOffset.UtcNow.AddDays(30);
        return this;
    }

    public ClassBuilder AsUpcoming()
    {
        _startDate = DateTimeOffset.UtcNow.AddDays(7);
        _endDate = DateTimeOffset.UtcNow.AddDays(37);
        return this;
    }

    public Class Build()
    {
        return Class.Create(
            _courseId,
            _name,
            _description,
            _teacherId,
            _startDate,
            _endDate
        );
    }
}

/// <summary>
/// Fluent builder for TeachingPlan entities
/// </summary>
public class TeachingPlanBuilder
{
    private readonly Fixture _fixture;
    private int _courseId;
    private string _teacherId = "";
    private string _title = "";
    private string? _description = null;
    private string? _objectives = null;

    public TeachingPlanBuilder(Fixture fixture)
    {
        _fixture = fixture;
        _teacherId = "test-teacher";
        _title = _fixture.Create<string>();
        _description = _fixture.Create<string>();
    }

    public TeachingPlanBuilder ForCourse(int courseId)
    {
        _courseId = courseId;
        return this;
    }

    public TeachingPlanBuilder WithTeacher(string teacherId)
    {
        _teacherId = teacherId;
        return this;
    }

    public TeachingPlanBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TeachingPlanBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TeachingPlanBuilder WithObjectives(string objectives)
    {
        _objectives = objectives;
        return this;
    }

    public TeachingPlan Build()
    {
        return TeachingPlan.Create(
            _courseId,
            _teacherId,
            _title,
            _description,
            _objectives
        );
    }
}

/// <summary>
/// Fluent builder for ScheduleItem entities
/// </summary>
public class ScheduleItemBuilder
{
    private readonly Fixture _fixture;
    private int _teachingPlanId;
    private ScheduleItemType _type = ScheduleItemType.Writing;
    private string _title = "";
    private string? _description = null;
    private DateTimeOffset _startDate = DateTimeOffset.UtcNow;
    private DateTimeOffset? _dueDate = DateTimeOffset.UtcNow.AddDays(7);
    private bool _isMandatory = false;
    private string _contentJson = "{}";
    private decimal? _maxScore = null;
    private int? _groupId = null;
    private int? _lessonId = null;
    private DisciplineType? _disciplineHint = null;

    public ScheduleItemBuilder(Fixture fixture)
    {
        _fixture = fixture;
        _title = _fixture.Create<string>();
        _description = _fixture.Create<string>();
    }

    public ScheduleItemBuilder ForTeachingPlan(int teachingPlanId)
    {
        _teachingPlanId = teachingPlanId;
        return this;
    }

    public ScheduleItemBuilder OfType(ScheduleItemType type)
    {
        _type = type;
        return this;
    }

    public ScheduleItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ScheduleItemBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ScheduleItemBuilder WithDates(DateTimeOffset startDate, DateTimeOffset? dueDate = null)
    {
        _startDate = startDate;
        _dueDate = dueDate;
        return this;
    }

    public ScheduleItemBuilder AsMandatory()
    {
        _isMandatory = true;
        return this;
    }

    public ScheduleItemBuilder WithMaxScore(decimal maxScore)
    {
        _maxScore = maxScore;
        return this;
    }

    public ScheduleItemBuilder AsAssignment()
    {
        _type = ScheduleItemType.Writing;
        _title = "Assignment";
        return this;
    }

    public ScheduleItemBuilder AsQuiz()
    {
        _type = ScheduleItemType.Quiz;
        _title = "Quiz";
        _maxScore = 100;
        return this;
    }

    public ScheduleItemBuilder AsHomework()
    {
        _type = ScheduleItemType.Writing;
        _title = "Homework";
        _isMandatory = true;
        return this;
    }

    public ScheduleItem Build()
    {
        return ScheduleItem.Create(
            _teachingPlanId,
            _type,
            _title,
            _description,
            _startDate,
            _dueDate,
            _isMandatory,
            _contentJson,
            _maxScore,
            _groupId,
            _lessonId,
            _disciplineHint
        );
    }
}
