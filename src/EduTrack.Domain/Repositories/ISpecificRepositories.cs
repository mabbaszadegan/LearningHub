using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Repositories;

/// <summary>
/// Specific repository for User entity with domain-specific queries
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByClassIdAsync(int classId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific repository for Course entity with domain-specific queries
/// </summary>
public interface ICourseRepository : IRepository<Course>
{
    Task<IEnumerable<Course>> GetActiveCoursesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetCoursesByCreatorAsync(string createdBy, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetCoursesByOrderAsync(CancellationToken cancellationToken = default);
    Task<Course?> GetCourseWithModulesAsync(int courseId, CancellationToken cancellationToken = default);
    Task<Course?> GetCourseWithChaptersAsync(int courseId, CancellationToken cancellationToken = default);
    Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific repository for Class entity with domain-specific queries
/// </summary>
public interface IClassRepository : IRepository<Class>
{
    Task<IEnumerable<Class>> GetClassesByTeacherAsync(string teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Class>> GetClassesByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Class>> GetActiveClassesAsync(CancellationToken cancellationToken = default);
    Task<Class?> GetClassWithEnrollmentsAsync(int classId, CancellationToken cancellationToken = default);
    Task<bool> IsStudentEnrolledAsync(int classId, string studentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific repository for Progress entity with domain-specific queries
/// </summary>
public interface IProgressRepository : IRepository<Progress>
{
    Task<IEnumerable<Progress>> GetProgressByStudentAsync(string studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Progress>> GetProgressByLessonAsync(int lessonId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Progress>> GetProgressByExamAsync(int examId, CancellationToken cancellationToken = default);
    Task<Progress?> GetStudentProgressForLessonAsync(string studentId, int lessonId, CancellationToken cancellationToken = default);
    Task<Progress?> GetStudentProgressForExamAsync(string studentId, int examId, CancellationToken cancellationToken = default);
    Task<double> GetStudentOverallProgressAsync(string studentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific repository for Question entity with domain-specific queries
/// </summary>
public interface IQuestionRepository : IRepository<Question>
{
    Task<IEnumerable<Question>> GetQuestionsByTypeAsync(QuestionType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Question>> GetActiveQuestionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Question>> GetQuestionsByCreatorAsync(string createdBy, CancellationToken cancellationToken = default);
    Task<Question?> GetQuestionWithChoicesAsync(int questionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific repository for Exam entity with domain-specific queries
/// </summary>
public interface IExamRepository : IRepository<Exam>
{
    Task<IEnumerable<Exam>> GetActiveExamsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Exam>> GetExamsByCreatorAsync(string createdBy, CancellationToken cancellationToken = default);
    Task<Exam?> GetExamWithQuestionsAsync(int examId, CancellationToken cancellationToken = default);
    Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific repository for Chapter entity with domain-specific queries
/// </summary>
public interface IChapterRepository : IRepository<Chapter>
{
    Task<IEnumerable<Chapter>> GetChaptersByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chapter>> GetActiveChaptersAsync(CancellationToken cancellationToken = default);
    Task<Chapter?> GetChapterWithSubChaptersAsync(int chapterId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific repository for EducationalContent entity with domain-specific queries
/// </summary>
public interface IEducationalContentRepository : IRepository<EducationalContent>
{
    Task<IEnumerable<EducationalContent>> GetContentBySubChapterAsync(int subChapterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EducationalContent>> GetContentByTypeAsync(EducationalContentType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<EducationalContent>> GetActiveContentAsync(CancellationToken cancellationToken = default);
}
