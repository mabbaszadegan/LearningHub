using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using EduTrack.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AppDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        AppDbContext context,
        ILogger<HomeController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    // GET: Admin Dashboard
    public async Task<IActionResult> Index()
    {
        var dashboardData = new AdminDashboardViewModel
        {
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalTeachers = await _userManager.Users.CountAsync(u => u.Role == UserRole.Teacher),
            TotalStudents = await _userManager.Users.CountAsync(u => u.Role == UserRole.Student),
            TotalCourses = await _context.Courses.CountAsync(),
            TotalClasses = await _context.Classes.CountAsync(),
            ActiveClasses = await _context.Classes.CountAsync(c => c.IsActive),
            TotalEnrollments = await _context.Enrollments.CountAsync(),
            TotalExams = await _context.Exams.CountAsync(),
            
            // Recent activity
            RecentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync(),
                
            RecentClasses = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync(),
                
            // Analytics data
            UserRegistrationsByMonth = await GetUserRegistrationsByMonth(),
            ClassActivityByMonth = await GetClassActivityByMonth(),
            TopCourses = await GetTopCourses(),
            ActiveUsers = await GetActiveUsers()
        };

        return View(dashboardData);
    }

    #region Analytics & Reports

    // GET: Admin/Analytics
    public async Task<IActionResult> Analytics()
    {
        var model = new AnalyticsViewModel
        {
            UserStats = await GetUserStatistics(),
            CourseStats = await GetCourseStatistics(),
            ClassStats = await GetClassStatistics(),
            ActivityStats = await GetActivityStatistics(),
            TopUsers = await GetTopActiveUsers(),
            TopCourses = await GetTopCourses(),
            TopClasses = await GetTopClasses(),
            RecentActivity = await GetRecentActivity()
        };

        return View(model);
    }

    // GET: Admin/Reports
    public async Task<IActionResult> Reports()
    {
        var model = new ReportsViewModel
        {
            UserReport = await GenerateUserReport(),
            CourseReport = await GenerateCourseReport(),
            ClassReport = await GenerateClassReport(),
            ActivityReport = await GenerateActivityReport()
        };

        return View(model);
    }

    #endregion

    #region Private Helper Methods

    private async Task LogActivity(string action, string? entityType = null, object? entityId = null, string? details = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return;

        var log = ActivityLog.Create(
            currentUser.Id,
            action,
            entityType,
            entityId?.ToString() != null ? int.TryParse(entityId.ToString(), out var id) ? id : null : null,
            details,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            HttpContext.Request.Headers["User-Agent"].ToString());

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private async Task<List<MonthlyData>> GetUserRegistrationsByMonth()
    {
        var sixMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-6);
        
        return await _userManager.Users
            .Where(u => u.CreatedAt >= sixMonthsAgo)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new MonthlyData
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(d => d.Year)
            .ThenBy(d => d.Month)
            .ToListAsync();
    }

    private async Task<List<MonthlyData>> GetClassActivityByMonth()
    {
        var sixMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-6);
        
        return await _context.Classes
            .Where(c => c.CreatedAt >= sixMonthsAgo)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new MonthlyData
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(d => d.Year)
            .ThenBy(d => d.Month)
            .ToListAsync();
    }

    private async Task<List<TopItemData>> GetTopCourses()
    {
        return await _context.Courses
            .Include(c => c.Classes)
                .ThenInclude(cl => cl.Enrollments)
            .Select(c => new TopItemData
            {
                Name = c.Title,
                Count = c.Classes.SelectMany(cl => cl.Enrollments).Count()
            })
            .OrderByDescending(t => t.Count)
            .Take(10)
            .ToListAsync();
    }

    private async Task<List<ActiveUserData>> GetActiveUsers()
    {
        var oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        
        return await _context.ActivityLogs
            .Where(a => a.Timestamp >= oneWeekAgo)
            .Include(a => a.User)
            .GroupBy(a => a.User)
            .Select(g => new ActiveUserData
            {
                User = g.Key,
                ActivityCount = g.Count(),
                LastActivity = g.Max(a => a.Timestamp)
            })
            .OrderByDescending(a => a.ActivityCount)
            .Take(10)
            .ToListAsync();
    }

    private async Task<UserStatistics> GetUserStatistics()
    {
        return new UserStatistics
        {
            TotalUsers = await _userManager.Users.CountAsync(),
            ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive),
            TotalAdmins = await _userManager.Users.CountAsync(u => u.Role == UserRole.Admin),
            TotalTeachers = await _userManager.Users.CountAsync(u => u.Role == UserRole.Teacher),
            TotalStudents = await _userManager.Users.CountAsync(u => u.Role == UserRole.Student),
            NewUsersThisMonth = await _userManager.Users.CountAsync(u => u.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30))
        };
    }

    private async Task<CourseStatistics> GetCourseStatistics()
    {
        return new CourseStatistics
        {
            TotalCourses = await _context.Courses.CountAsync(),
            ActiveCourses = await _context.Courses.CountAsync(c => c.IsActive),
            TotalModules = await _context.Modules.CountAsync(),
            TotalLessons = await _context.Lessons.CountAsync(),
            NewCoursesThisMonth = await _context.Courses.CountAsync(c => c.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30))
        };
    }

    private async Task<ClassStatistics> GetClassStatistics()
    {
        return new ClassStatistics
        {
            TotalClasses = await _context.Classes.CountAsync(),
            ActiveClasses = await _context.Classes.CountAsync(c => c.IsActive),
            TotalEnrollments = await _context.Enrollments.CountAsync(),
            ActiveEnrollments = await _context.Enrollments.CountAsync(e => e.IsActive),
            NewClassesThisMonth = await _context.Classes.CountAsync(c => c.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30))
        };
    }

    private async Task<ActivityStatistics> GetActivityStatistics()
    {
        var oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var oneMonthAgo = DateTimeOffset.UtcNow.AddDays(-30);

        return new ActivityStatistics
        {
            TotalActivities = await _context.ActivityLogs.CountAsync(),
            ActivitiesThisWeek = await _context.ActivityLogs.CountAsync(a => a.Timestamp >= oneWeekAgo),
            ActivitiesThisMonth = await _context.ActivityLogs.CountAsync(a => a.Timestamp >= oneMonthAgo),
            UniqueActiveUsersThisWeek = await _context.ActivityLogs
                .Where(a => a.Timestamp >= oneWeekAgo)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync()
        };
    }

    private async Task<List<TopItemData>> GetTopActiveUsers()
    {
        var oneMonthAgo = DateTimeOffset.UtcNow.AddDays(-30);
        
        return await _context.ActivityLogs
            .Where(a => a.Timestamp >= oneMonthAgo)
            .Include(a => a.User)
            .GroupBy(a => a.User)
            .Select(g => new TopItemData
            {
                Name = g.Key.FullName,
                Count = g.Count()
            })
            .OrderByDescending(t => t.Count)
            .Take(10)
            .ToListAsync();
    }

    private async Task<List<TopItemData>> GetTopClasses()
    {
        return await _context.Classes
            .Include(c => c.Enrollments)
            .Select(c => new TopItemData
            {
                Name = c.Name,
                Count = c.Enrollments.Count
            })
            .OrderByDescending(t => t.Count)
            .Take(10)
            .ToListAsync();
    }

    private async Task<List<ActivityLog>> GetRecentActivity()
    {
        return await _context.ActivityLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(20)
            .ToListAsync();
    }

    private async Task<UserReport> GenerateUserReport()
    {
        var users = await _userManager.Users.ToListAsync();
        
        return new UserReport
        {
            TotalUsers = users.Count,
            UsersByRole = users.GroupBy(u => u.Role).ToDictionary(g => g.Key.ToString(), g => g.Count()),
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive),
            UsersRegisteredThisMonth = users.Count(u => u.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30)),
            UsersWithRecentLogin = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value >= DateTimeOffset.UtcNow.AddDays(-7))
        };
    }

    private async Task<CourseReport> GenerateCourseReport()
    {
        var courses = await _context.Courses.Include(c => c.Classes).ThenInclude(cl => cl.Enrollments).ToListAsync();
        
        return new CourseReport
        {
            TotalCourses = courses.Count,
            ActiveCourses = courses.Count(c => c.IsActive),
            InactiveCourses = courses.Count(c => !c.IsActive),
            CoursesWithClasses = courses.Count(c => c.Classes.Any()),
            TotalEnrollmentsAcrossAllCourses = courses.SelectMany(c => c.Classes).SelectMany(cl => cl.Enrollments).Count(),
            AverageEnrollmentsPerCourse = courses.Any() ? courses.Average(c => c.Classes.SelectMany(cl => cl.Enrollments).Count()) : 0
        };
    }

    private async Task<ClassReport> GenerateClassReport()
    {
        var classes = await _context.Classes.Include(c => c.Enrollments).ToListAsync();
        
        return new ClassReport
        {
            TotalClasses = classes.Count,
            ActiveClasses = classes.Count(c => c.IsActive),
            InactiveClasses = classes.Count(c => !c.IsActive),
            TotalEnrollments = classes.SelectMany(c => c.Enrollments).Count(),
            AverageEnrollmentsPerClass = classes.Any() ? classes.Average(c => c.Enrollments.Count) : 0,
            ClassesStartedThisMonth = classes.Count(c => c.StartDate >= DateTimeOffset.UtcNow.AddDays(-30))
        };
    }

    private async Task<ActivityReport> GenerateActivityReport()
    {
        var activities = await _context.ActivityLogs.ToListAsync();
        var oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var oneMonthAgo = DateTimeOffset.UtcNow.AddDays(-30);
        
        return new ActivityReport
        {
            TotalActivities = activities.Count,
            ActivitiesThisWeek = activities.Count(a => a.Timestamp >= oneWeekAgo),
            ActivitiesThisMonth = activities.Count(a => a.Timestamp >= oneMonthAgo),
            MostCommonActions = activities.GroupBy(a => a.Action).OrderByDescending(g => g.Count()).Take(10).ToDictionary(g => g.Key, g => g.Count()),
            UniqueActiveUsers = activities.Select(a => a.UserId).Distinct().Count()
        };
    }

    #endregion
}
