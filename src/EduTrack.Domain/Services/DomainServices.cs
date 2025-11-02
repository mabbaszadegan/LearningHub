using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Domain.Services;

/// <summary>
/// Implementation of user domain service
/// </summary>
public class UserDomainService : IUserDomainService
{
    private readonly UserManager<User> _userManager;

    public UserDomainService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> CanAssignRoleAsync(User user, UserRole newRole)
    {
        if (user == null)
            return false;

        var userRoles = await _userManager.GetRolesAsync(user);
        var currentRole = userRoles.FirstOrDefault();

        // Business rule: Only admins can assign admin role
        if (newRole == UserRole.Admin && currentRole != "Admin")
            return false;

        // Business rule: Users can only be promoted, not demoted
        if (currentRole != null && (int)newRole < (int)Enum.Parse<UserRole>(currentRole))
            return false;

        return true;
    }

    public async Task<bool> CanEnrollInClassAsync(User user, Class classEntity)
    {
        if (user == null || classEntity == null)
            return false;

        var userRoles = await _userManager.GetRolesAsync(user);
        
        // Business rule: Only students can enroll in classes
        if (!userRoles.Contains("Student"))
            return false;

        // Business rule: User must be active
        if (!user.IsActive)
            return false;

        // Business rule: Class must be active
        if (!classEntity.IsActive)
            return false;

        // Business rule: Check if class has started
        if (classEntity.StartDate > DateTimeOffset.UtcNow)
            return false;

        return true;
    }

    public double CalculateOverallProgress(User user)
    {
        if (user == null || !user.Progresses.Any())
            return 0.0;

        var totalProgresses = user.Progresses.Count;
        var completedProgresses = user.Progresses.Count(p => p.Status == ProgressStatus.Done || p.Status == ProgressStatus.Mastered);

        return totalProgresses > 0 ? (double)completedProgresses / totalProgresses * 100 : 0.0;
    }
}

/// <summary>
/// Implementation of course domain service
/// </summary>
public class CourseDomainService : ICourseDomainService
{
    public bool CanActivateCourse(Course course)
    {
        if (course == null)
            return false;

        // Business rule: Course must have at least one chapter (Modules removed, using Chapters instead)
        if (!course.Chapters.Any())
            return false;

        // Business rule: Course must have at least one sub-chapter
        var hasSubChapters = course.Chapters.Any(c => c.SubChapters.Any());
        if (!hasSubChapters)
            return false;

        return true;
    }

    public double CalculateCompletionPercentage(Course course)
    {
        if (course == null || !course.Classes.Any())
            return 0.0;

        var totalEnrollments = course.Classes.SelectMany(c => c.Enrollments).Count();
        var completedEnrollments = course.Classes
            .SelectMany(c => c.Enrollments)
            .Count(e => e.CompletedAt.HasValue);

        return totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0.0;
    }

    public bool ValidatePrerequisites(Course course)
    {
        if (course == null)
            return false;

        // Business rule: Course must have valid order
        if (course.Order < 0)
            return false;

        // Business rule: Course must have valid title
        if (string.IsNullOrWhiteSpace(course.Title))
            return false;

        return true;
    }
}

/// <summary>
/// Implementation of progress domain service
/// </summary>
public class ProgressDomainService : IProgressDomainService
{
    private const int MasteryThreshold = 20;
    private const int CompletionThreshold = 10;

    public ProgressStatus CalculateProgressStatus(int correctCount, int streak)
    {
        if (streak >= MasteryThreshold)
            return ProgressStatus.Mastered;
        
        if (streak >= CompletionThreshold)
            return ProgressStatus.Done;
        
        if (correctCount > 0)
            return ProgressStatus.InProgress;
        
        return ProgressStatus.NotStarted;
    }

    public void UpdateProgressAfterExam(Progress progress, bool isPassed, int score)
    {
        if (progress == null)
            throw new ArgumentNullException(nameof(progress));

        if (isPassed)
        {
            progress.IncrementCorrectCount();
            progress.IncrementStreak();
        }
        else
        {
            progress.DecrementCorrectCount();
            progress.ResetStreak();
        }

        progress.UpdateStatus(CalculateProgressStatus(progress.CorrectCount, progress.Streak));
    }

    public bool CanRetakeExam(Progress progress)
    {
        if (progress == null)
            return false;

        // Business rule: Can retake if not mastered
        if (progress.Status == ProgressStatus.Mastered)
            return false;

        // Business rule: Must wait at least 24 hours between attempts
        var lastAttempt = progress.UpdatedAt;
        var timeSinceLastAttempt = DateTimeOffset.UtcNow - lastAttempt;
        
        return timeSinceLastAttempt.TotalHours >= 24;
    }
}
