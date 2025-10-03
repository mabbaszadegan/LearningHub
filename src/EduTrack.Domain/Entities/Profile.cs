namespace EduTrack.Domain.Entities;

/// <summary>
/// Profile value object - represents user profile information
/// </summary>
public class Profile
{
    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? Avatar { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTimeOffset? DateOfBirth { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    // Private constructor for EF Core
    private Profile() { }

    public static Profile Create(string userId, string? bio = null, string? avatar = null, 
        string? phoneNumber = null, DateTimeOffset? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        return new Profile
        {
            UserId = userId,
            Bio = bio,
            Avatar = avatar,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateBio(string? bio)
    {
        Bio = bio;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAvatar(string? avatar)
    {
        Avatar = avatar;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDateOfBirth(DateTimeOffset? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
