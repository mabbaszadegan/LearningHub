using EduTrack.Domain.Entities;

namespace EduTrack.Application.Features.StudentProfiles;

internal static class StudentProfileMapping
{
    public static StudentProfileDto ToDto(StudentProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        DisplayName = profile.DisplayName,
        AvatarUrl = profile.AvatarUrl,
        DateOfBirth = profile.DateOfBirth,
        GradeLevel = profile.GradeLevel,
        Notes = profile.Notes,
        IsArchived = profile.IsArchived,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}

