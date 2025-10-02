using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using EduTrack.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        AppDbContext context,
        ILogger<AdminController> logger)
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

    #region User Management

    // GET: Admin/Users
    public async Task<IActionResult> Users(string? search, UserRole? role, int page = 1, int pageSize = 20)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || 
                                   u.LastName.Contains(search) || 
                                   u.Email!.Contains(search));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var totalUsers = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new UserManagementViewModel
        {
            Users = users,
            Search = search,
            SelectedRole = role,
            CurrentPage = page,
            PageSize = pageSize,
            TotalUsers = totalUsers,
            TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize)
        };

        return View(model);
    }

    // GET: Admin/CreateUser
    public IActionResult CreateUser()
    {
        return View(new CreateUserViewModel());
    }

    // POST: Admin/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Log activity
                await LogActivity("CreateUser", "User", user.Id, $"Created user: {user.FullName} ({user.Role})");
                
                TempData["Success"] = $"کاربر {user.FullName} با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // GET: Admin/EditUser/{id}
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            Role = user.Role,
            IsActive = user.IsActive
        };

        return View(model);
    }

    // POST: Admin/EditUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await LogActivity("UpdateUser", "User", user.Id, $"Updated user: {user.FullName}");
                TempData["Success"] = "اطلاعات کاربر با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // POST: Admin/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "کاربر یافت نشد" });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            await LogActivity("ResetPassword", "User", user.Id, $"Reset password for user: {user.FullName}");
            return Json(new { success = true, message = "رمز عبور با موفقیت تغییر یافت" });
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Json(new { success = false, message = errors });
    }

    // POST: Admin/ToggleUserStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "کاربر یافت نشد" });
        }

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var status = user.IsActive ? "فعال" : "غیرفعال";
            await LogActivity("ToggleUserStatus", "User", user.Id, $"Changed user status to {status}: {user.FullName}");
            return Json(new { success = true, message = $"وضعیت کاربر به {status} تغییر یافت", isActive = user.IsActive });
        }

        return Json(new { success = false, message = "خطا در تغییر وضعیت کاربر" });
    }

    #endregion

    #region Session Management

    // GET: Admin/Sessions
    public async Task<IActionResult> Sessions()
    {
        var activeSessions = await GetActiveUserSessions();
        return View(activeSessions);
    }

    // POST: Admin/LogoutUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "کاربر یافت نشد" });
        }

        // Update security stamp to invalidate all sessions
        await _userManager.UpdateSecurityStampAsync(user);
        
        await LogActivity("ForceLogout", "User", user.Id, $"Forced logout for user: {user.FullName}");
        
        return Json(new { success = true, message = $"کاربر {user.FullName} از سیستم خارج شد" });
    }

    #endregion

    #region Course Management

    // GET: Admin/Courses
    public async Task<IActionResult> Courses(string? search, bool? isActive, int page = 1, int pageSize = 20)
    {
        var query = _context.Courses.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) || (c.Description != null && c.Description.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalCourses = await query.CountAsync();
        var courses = await query
            .Include(c => c.Modules)
            .Include(c => c.Classes)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new CourseManagementViewModel
        {
            Courses = courses,
            Search = search,
            IsActive = isActive,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCourses = totalCourses,
            TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize)
        };

        return View(model);
    }

    // GET: Admin/CreateCourse
    public IActionResult CreateCourse()
    {
        return View(new CreateCourseViewModel());
    }

    // POST: Admin/CreateCourse
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                IsActive = model.IsActive,
                Order = model.Order,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = currentUser?.Id ?? ""
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            await LogActivity("CreateCourse", "Course", course.Id, $"Created course: {course.Title}");
            
            TempData["Success"] = $"دوره {course.Title} با موفقیت ایجاد شد";
            return RedirectToAction(nameof(Courses));
        }

        return View(model);
    }

    // POST: Admin/ToggleCourseStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCourseStatus(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
        {
            return Json(new { success = false, message = "دوره یافت نشد" });
        }

        course.IsActive = !course.IsActive;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        var status = course.IsActive ? "فعال" : "غیرفعال";
        await LogActivity("ToggleCourseStatus", "Course", course.Id, $"Changed course status to {status}: {course.Title}");
        
        return Json(new { success = true, message = $"وضعیت دوره به {status} تغییر یافت", isActive = course.IsActive });
    }

    #endregion

    #region Class Management

    // GET: Admin/Classes
    public async Task<IActionResult> Classes(string? search, bool? isActive, int page = 1, int pageSize = 20)
    {
        var query = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Name.Contains(search) || 
                                   c.Course.Title.Contains(search) ||
                                   c.Teacher.FirstName.Contains(search) ||
                                   c.Teacher.LastName.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalClasses = await query.CountAsync();
        var classes = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new ClassManagementViewModel
        {
            Classes = classes,
            Search = search,
            IsActive = isActive,
            CurrentPage = page,
            PageSize = pageSize,
            TotalClasses = totalClasses,
            TotalPages = (int)Math.Ceiling((double)totalClasses / pageSize)
        };

        return View(model);
    }

    // GET: Admin/ClassDetails/{id}
    public async Task<IActionResult> ClassDetails(int id)
    {
        var classEntity = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classEntity == null)
        {
            return NotFound();
        }

        return View(classEntity);
    }

    #endregion

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

    #region Activity Logs

    // GET: Admin/ActivityLogs
    public async Task<IActionResult> ActivityLogs(string? search, string? action, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 50)
    {
        var query = _context.ActivityLogs
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.User.FirstName.Contains(search) || 
                                   a.User.LastName.Contains(search) ||
                                   a.User.Email!.Contains(search) ||
                                   (a.Details != null && a.Details.Contains(search)));
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value.AddDays(1));
        }

        var totalLogs = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new ActivityLogsViewModel
        {
            Logs = logs,
            Search = search,
            Action = action,
            FromDate = fromDate,
            ToDate = toDate,
            CurrentPage = page,
            PageSize = pageSize,
            TotalLogs = totalLogs,
            TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize),
            AvailableActions = await _context.ActivityLogs.Select(a => a.Action).Distinct().ToListAsync()
        };

        return View(model);
    }

    #endregion

    #region Private Helper Methods

    private async Task LogActivity(string action, string? entityType = null, object? entityId = null, string? details = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return;

        var log = new ActivityLog
        {
            UserId = currentUser.Id,
            Action = action,
            EntityType = entityType,
            EntityId = entityId?.ToString() != null ? int.TryParse(entityId.ToString(), out var id) ? id : null : null,
            Details = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
            Timestamp = DateTimeOffset.UtcNow
        };

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

    private async Task<List<ActiveSessionData>> GetActiveUserSessions()
    {
        var activeUsers = await _userManager.Users
            .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value > DateTimeOffset.UtcNow.AddHours(-24))
            .OrderByDescending(u => u.LastLoginAt)
            .ToListAsync();

        return activeUsers.Select(u => new ActiveSessionData
        {
            User = u,
            LastLogin = u.LastLoginAt ?? DateTimeOffset.MinValue,
            IsOnline = u.LastLoginAt > DateTimeOffset.UtcNow.AddMinutes(-30) // Consider online if logged in within 30 minutes
        }).ToList();
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
