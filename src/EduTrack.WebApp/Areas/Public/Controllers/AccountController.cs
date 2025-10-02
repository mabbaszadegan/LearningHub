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
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                
                // Update last login time
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    user.LastLoginAt = DateTimeOffset.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
                
                return RedirectToLocal(returnUrl);
            }
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "حساب کاربری شما قفل شده است.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "ایمیل یا رمز عبور اشتباه است.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

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
                Role = UserRole.Student, // Default role for public registration
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home", new { area = "Student" });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
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
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}