using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.WebApp.Models;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalClasses { get; set; }
    public int ActiveClasses { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalExams { get; set; }
    
    public List<User> RecentUsers { get; set; } = new();
    public List<Class> RecentClasses { get; set; } = new();
    
    public List<MonthlyData> UserRegistrationsByMonth { get; set; } = new();
    public List<MonthlyData> ClassActivityByMonth { get; set; } = new();
    public List<TopItemData> TopCourses { get; set; } = new();
    public List<ActiveUserData> ActiveUsers { get; set; } = new();
}

public class UserManagementViewModel
{
    public List<User> Users { get; set; } = new();
    public string? Search { get; set; }
    public UserRole? SelectedRole { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalUsers { get; set; }
    public int TotalPages { get; set; }
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "نام الزامی است")]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(100, ErrorMessage = "رمز عبور باید حداقل {2} کاراکتر باشد", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب نقش الزامی است")]
    [Display(Name = "نقش")]
    public UserRole Role { get; set; } = UserRole.Student;
}

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام الزامی است")]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب نقش الزامی است")]
    [Display(Name = "نقش")]
    public UserRole Role { get; set; }

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; }
}

public class CourseManagementViewModel
{
    public List<Course> Courses { get; set; } = new();
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCourses { get; set; }
    public int TotalPages { get; set; }
}

public class CreateCourseViewModel
{
    [Required(ErrorMessage = "عنوان دوره الزامی است")]
    [Display(Name = "عنوان دوره")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "ترتیب نمایش")]
    public int Order { get; set; }

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; } = true;
}

public class ClassManagementViewModel
{
    public List<Class> Classes { get; set; } = new();
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalClasses { get; set; }
    public int TotalPages { get; set; }
}

public class AnalyticsViewModel
{
    public UserStatistics UserStats { get; set; } = new();
    public CourseStatistics CourseStats { get; set; } = new();
    public ClassStatistics ClassStats { get; set; } = new();
    public ActivityStatistics ActivityStats { get; set; } = new();
    public List<TopItemData> TopUsers { get; set; } = new();
    public List<TopItemData> TopCourses { get; set; } = new();
    public List<TopItemData> TopClasses { get; set; } = new();
    public List<ActivityLog> RecentActivity { get; set; } = new();
}

public class ReportsViewModel
{
    public UserReport UserReport { get; set; } = new();
    public CourseReport CourseReport { get; set; } = new();
    public ClassReport ClassReport { get; set; } = new();
    public ActivityReport ActivityReport { get; set; } = new();
}

public class ActivityLogsViewModel
{
    public List<ActivityLog> Logs { get; set; } = new();
    public string? Search { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalLogs { get; set; }
    public int TotalPages { get; set; }
    public List<string> AvailableActions { get; set; } = new();
}

public class MonthlyData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}

public class TopItemData
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ActiveUserData
{
    public User User { get; set; } = null!;
    public int ActivityCount { get; set; }
    public DateTimeOffset LastActivity { get; set; }
}

public class ActiveSessionData
{
    public User User { get; set; } = null!;
    public DateTimeOffset LastLogin { get; set; }
    public bool IsOnline { get; set; }
}

public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int NewUsersThisMonth { get; set; }
}

public class CourseStatistics
{
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int TotalModules { get; set; }
    public int TotalLessons { get; set; }
    public int NewCoursesThisMonth { get; set; }
}

public class ClassStatistics
{
    public int TotalClasses { get; set; }
    public int ActiveClasses { get; set; }
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int NewClassesThisMonth { get; set; }
}

public class ActivityStatistics
{
    public int TotalActivities { get; set; }
    public int ActivitiesThisWeek { get; set; }
    public int ActivitiesThisMonth { get; set; }
    public int UniqueActiveUsersThisWeek { get; set; }
}

public class UserReport
{
    public int TotalUsers { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int UsersRegisteredThisMonth { get; set; }
    public int UsersWithRecentLogin { get; set; }
}

public class CourseReport
{
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int InactiveCourses { get; set; }
    public int CoursesWithClasses { get; set; }
    public int TotalEnrollmentsAcrossAllCourses { get; set; }
    public double AverageEnrollmentsPerCourse { get; set; }
}

public class ClassReport
{
    public int TotalClasses { get; set; }
    public int ActiveClasses { get; set; }
    public int InactiveClasses { get; set; }
    public int TotalEnrollments { get; set; }
    public double AverageEnrollmentsPerClass { get; set; }
    public int ClassesStartedThisMonth { get; set; }
}

public class ActivityReport
{
    public int TotalActivities { get; set; }
    public int ActivitiesThisWeek { get; set; }
    public int ActivitiesThisMonth { get; set; }
    public Dictionary<string, int> MostCommonActions { get; set; } = new();
    public int UniqueActiveUsers { get; set; }
}
