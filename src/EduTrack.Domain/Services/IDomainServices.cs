using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Services;

/// <summary>
/// Domain service for user-related business logic
/// </summary>
public interface IUserDomainService
{
    /// <summary>
    /// Validates if a user can be assigned to a specific role
    /// </summary>
    Task<bool> CanAssignRoleAsync(User user, UserRole newRole);
    
    /// <summary>
    /// Validates if a user can be enrolled in a class
    /// </summary>
    Task<bool> CanEnrollInClassAsync(User user, Class classEntity);
    
    /// <summary>
    /// Calculates user's overall progress percentage
    /// </summary>
    double CalculateOverallProgress(User user);
}

/// <summary>
/// Domain service for course-related business logic
/// </summary>
public interface ICourseDomainService
{
    /// <summary>
    /// Validates if a course can be activated
    /// </summary>
    bool CanActivateCourse(Course course);
    
    /// <summary>
    /// Calculates course completion percentage
    /// </summary>
    double CalculateCompletionPercentage(Course course);
    
    /// <summary>
    /// Validates course prerequisites
    /// </summary>
    bool ValidatePrerequisites(Course course);
}

/// <summary>
/// Domain service for progress tracking business logic
/// </summary>
public interface IProgressDomainService
{
    /// <summary>
    /// Calculates progress status based on streak and correct count
    /// </summary>
    ProgressStatus CalculateProgressStatus(int correctCount, int streak);
    
    /// <summary>
    /// Updates progress based on exam result
    /// </summary>
    void UpdateProgressAfterExam(Progress progress, bool isPassed, int score);
    
    /// <summary>
    /// Validates if student can retake exam
    /// </summary>
    bool CanRetakeExam(Progress progress);
}
