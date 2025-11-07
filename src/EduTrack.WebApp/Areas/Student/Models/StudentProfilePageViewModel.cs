using System;
using System.Collections.Generic;
using EduTrack.Application.Features.StudentProfiles;

namespace EduTrack.WebApp.Areas.Student.Models;

public class StudentProfilePageViewModel
{
    public string StudentName { get; init; } = string.Empty;
    public string StudentFirstName { get; init; } = string.Empty;
    public string StudentLastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public AccountProfileInfo? AccountProfile { get; init; }
        = default;
    public IReadOnlyList<StudentProfileDto> StudentProfiles { get; init; } = new List<StudentProfileDto>();
    public int? ActiveStudentProfileId { get; init; }
    public string? ActiveStudentProfileName { get; init; }
}

public class AccountProfileInfo
{
    public string? Bio { get; init; }
    public string? Avatar { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTimeOffset? DateOfBirth { get; init; }
}

