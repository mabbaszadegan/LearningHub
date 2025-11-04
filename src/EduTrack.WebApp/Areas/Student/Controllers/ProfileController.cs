using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.WebApp.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IRepository<Profile> _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileController(
        ILogger<ProfileController> logger,
        UserManager<User> userManager,
        IRepository<Profile> profileRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userManager = userManager;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get user profile
        var profile = await _profileRepository.GetAll()
            .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

        var viewModel = new
        {
            StudentName = currentUser.FullName,
            StudentFirstName = currentUser.FirstName,
            Email = currentUser.Email,
            UserName = currentUser.UserName,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Profile = profile != null ? new
            {
                Bio = profile.Bio,
                Avatar = profile.Avatar,
                PhoneNumber = profile.PhoneNumber,
                DateOfBirth = profile.DateOfBirth
            } : null
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string? bio, string? phoneNumber)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        try
        {
            // Update user basic info
            var newFirstName = !string.IsNullOrWhiteSpace(firstName) ? firstName : currentUser.FirstName;
            var newLastName = !string.IsNullOrWhiteSpace(lastName) ? lastName : currentUser.LastName;
            
            currentUser.UpdateProfile(newFirstName, newLastName);

            var updateResult = await _userManager.UpdateAsync(currentUser);
            if (!updateResult.Succeeded)
            {
                return Json(new { success = false, error = "خطا در به‌روزرسانی اطلاعات" });
            }

            // Get or create profile
            var profile = await _profileRepository.GetAll()
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (profile == null)
            {
                profile = Profile.Create(currentUser.Id, bio, null, phoneNumber, null);
                await _profileRepository.AddAsync(profile);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(bio))
                    profile.UpdateBio(bio);
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                    profile.UpdatePhoneNumber(phoneNumber);
                await _profileRepository.UpdateAsync(profile);
            }

            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "پروفایل با موفقیت به‌روزرسانی شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return Json(new { success = false, error = "خطا در به‌روزرسانی پروفایل" });
        }
    }
}

