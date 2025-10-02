using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.WebApp.Controllers;

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

    // GET: Account/Login
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        _logger.LogInformation("Login POST action called for email: {Email}", model?.Email ?? "null");
        ViewData["ReturnUrl"] = returnUrl;

        if (model == null)
        {
            _logger.LogWarning("Login model is null");
            return View();
        }

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    user.LastLoginAt = DateTimeOffset.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("User {Email} logged in.", model.Email);
                
                // Redirect to role-based dashboard after login
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out.", model.Email);
                ModelState.AddModelError(string.Empty, "This account has been locked out, please try again later.");
            }
            else
            {
                _logger.LogWarning("Login failed for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }
        else
        {
            _logger.LogWarning("Model state is invalid for login attempt. Errors: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        return View(model);
    }

    // GET: Account/Logout
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    // GET: Account/Register
    public IActionResult Register(string? role = null)
    {
        var model = new RegisterViewModel();
        
        // Set role if provided
        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            model.Role = userRole;
        }
        
        return View(model);
    }

    // POST: Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
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
                _logger.LogInformation("User {Email} created a new account with role {Role}.", model.Email, model.Role);

                // Sign in the user immediately after registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // GET: Account/AccessDenied
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "نام الزامی است")]
    [StringLength(100, ErrorMessage = "نام نباید بیش از 100 کاراکتر باشد")]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [StringLength(100, ErrorMessage = "نام خانوادگی نباید بیش از 100 کاراکتر باشد")]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(100, ErrorMessage = "رمز عبور باید حداقل {2} و حداکثر {1} کاراکتر باشد", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "تکرار رمز عبور")]
    [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب نقش الزامی است")]
    [Display(Name = "نقش")]
    public UserRole Role { get; set; } = UserRole.Student;
}
