using System;
using System.Threading.Tasks;
using EduTrack.Application.Features.StudentProfiles;
using EduTrack.Application.Features.StudentProfiles.Commands;
using EduTrack.Application.Features.StudentProfiles.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.WebApp.Areas.Student.Models;
using EduTrack.WebApp.Services;
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
    private readonly IMediator _mediator;
    private readonly IStudentProfileContext _studentProfileContext;

    public ProfileController(
        ILogger<ProfileController> logger,
        UserManager<User> userManager,
        IRepository<Profile> profileRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IStudentProfileContext studentProfileContext)
    {
        _logger = logger;
        _userManager = userManager;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _studentProfileContext = studentProfileContext;
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

        var studentProfiles = await _studentProfileContext.GetProfilesForCurrentUserAsync(false);
        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        var activeProfileName = await _studentProfileContext.GetActiveProfileNameAsync();

        var viewModel = new StudentProfilePageViewModel
        {
            StudentName = currentUser.FullName,
            StudentFirstName = currentUser.FirstName,
            StudentLastName = currentUser.LastName,
            Email = currentUser.Email ?? string.Empty,
            UserName = currentUser.UserName ?? string.Empty,
            AccountProfile = profile != null ? new AccountProfileInfo
            {
                Bio = profile.Bio,
                Avatar = profile.Avatar,
                PhoneNumber = profile.PhoneNumber,
                DateOfBirth = profile.DateOfBirth
            } : null,
            StudentProfiles = studentProfiles,
            ActiveStudentProfileId = activeProfileId,
            ActiveStudentProfileName = activeProfileName
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudentProfile(string displayName, string? gradeLevel, DateTimeOffset? dateOfBirth, string? notes)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new CreateStudentProfileCommand(currentUser.Id, displayName, gradeLevel, dateOfBirth, notes));

        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue && result.Value != null)
        {
            await _studentProfileContext.SetActiveProfileAsync(result.Value.Id, HttpContext.RequestAborted);
        }

        return Json(new { success = true, profile = result.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStudentProfile(int profileId, string displayName, string? gradeLevel, DateTimeOffset? dateOfBirth, string? notes)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new UpdateStudentProfileCommand(profileId, currentUser.Id, displayName, gradeLevel, dateOfBirth, notes));

        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (result.Value != null && result.Value.Id == activeProfileId)
        {
            await _studentProfileContext.SetActiveProfileAsync(result.Value.Id, HttpContext.RequestAborted);
        }

        return Json(new { success = true, profile = result.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveStudentProfile(int profileId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new ArchiveStudentProfileCommand(profileId, currentUser.Id));

        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        if (result.Value?.Id == await _studentProfileContext.GetActiveProfileIdAsync())
        {
            await _studentProfileContext.SetActiveProfileAsync(null, HttpContext.RequestAborted);
        }

        return Json(new { success = true, profile = result.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreStudentProfile(int profileId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new RestoreStudentProfileCommand(profileId, currentUser.Id));

        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, profile = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> GetStudentProfiles()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        var result = await _mediator.Send(new GetStudentProfilesQuery(currentUser.Id));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();

        return Json(new { success = true, profiles = result.Value, activeProfileId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActiveProfile(string? profileId)
    {
        int? parsedProfileId = null;
        if (!string.IsNullOrWhiteSpace(profileId) && int.TryParse(profileId, out var parsed))
        {
            parsedProfileId = parsed;
        }

        await _studentProfileContext.SetActiveProfileAsync(parsedProfileId, HttpContext.RequestAborted);

        return Json(new { success = true, reload = true });
    }
}

