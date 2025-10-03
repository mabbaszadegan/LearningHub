using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Public.Controllers;

[Area("Public")]
public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (ModelState.IsValid)
        {
            // Try to find user by username or email
            var user = await _userManager.FindByNameAsync(model.Email) ?? 
                      await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                
                // Update last login time
                user.UpdateLastLogin();
                await _userManager.UpdateAsync(user);
                
                // Get user roles from Identity system
                var userRoles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User {UserName} has roles: {Roles}", user.UserName, string.Join(", ", userRoles));
                
                // Redirect based on user role - check both entity role and Identity roles
                var primaryRole = user.Role.ToString();
                if (userRoles.Contains("Admin") || primaryRole == "Admin")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else if (userRoles.Contains("Teacher") || primaryRole == "Teacher")
                {
                    return RedirectToAction("Index", "Home", new { area = "Teacher" });
                }
                else if (userRoles.Contains("Student") || primaryRole == "Student")
                {
                    return RedirectToAction("Index", "Home", new { area = "Student" });
                }
                else
                {
                    // Default fallback
                    return RedirectToAction("Index", "Home", new { area = "Student" });
                }
            }
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "حساب کاربری شما قفل شده است.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? role = "Student")
    {
        var model = new RegisterViewModel
        {
            Role = role ?? "Student"
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // For students, email is optional, so remove email validation errors
        if (model.Role == "Student" && string.IsNullOrEmpty(model.Email))
        {
            ModelState.Remove(nameof(model.Email));
        }

        if (ModelState.IsValid)
        {
            // Check username uniqueness
            if (!string.IsNullOrEmpty(model.Username))
            {
                var existingUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.Username), "این نام کاربری قبلاً استفاده شده است.");
                    return View(model);
                }
            }

            // For teachers, email is required
            if (model.Role == "Teacher" && string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "برای ثبت نام به عنوان استاد، ایمیل الزامی است.");
                return View(model);
            }

            // For students, email is optional
            var user = EduTrack.Domain.Entities.User.Create(
                model.FirstName,
                model.LastName,
                model.Email ?? $"{model.Username}@edutrack.local", // Default email for students
                model.Role == "Teacher" ? UserRole.Teacher : UserRole.Student);
            user.UserName = model.Username;

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                
                // Add user to the appropriate role
                var roleName = user.Role.ToString();
                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("User {UserName} added to role {Role}", user.UserName, roleName);
                }
                else
                {
                    _logger.LogWarning("Failed to add user {UserName} to role {Role}: {Errors}", 
                        user.UserName, roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                // Redirect based on role
                return user.Role switch
                {
                    UserRole.Teacher => RedirectToAction("Index", "Home", new { area = "Teacher" }),
                    UserRole.Student => RedirectToAction("Index", "Home", new { area = "Student" }),
                    _ => RedirectToAction("Index", "Home", new { area = "Student" })
                };
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home", new { area = "Public" });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home", new { area = "Public" });
        }
    }
}

public class LoginViewModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Role { get; set; } = "Student";
}