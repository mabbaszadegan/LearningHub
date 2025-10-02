using EduTrack.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EduTrack.WebApp.Services;

public class ClaimsTransformer : IClaimsTransformation
{
    private readonly UserManager<User> _userManager;

    public ClaimsTransformer(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
            return principal;

        // Check if role claim already exists
        if (identity.HasClaim(ClaimTypes.Role, "Admin") ||
            identity.HasClaim(ClaimTypes.Role, "Teacher") ||
            identity.HasClaim(ClaimTypes.Role, "Student"))
        {
            return principal;
        }

        // Get user from database
        var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return principal;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return principal;

        // Add role claim based on user's role
        var roleName = user.Role.ToString();
        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));

        return principal;
    }
}
