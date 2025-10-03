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

        // Get user from database
        var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return principal;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return principal;

        // Get user roles from Identity system
        var userRoles = await _userManager.GetRolesAsync(user);
        
        // Remove existing role claims
        var roleClaims = identity.FindAll(ClaimTypes.Role).ToList();
        foreach (var claim in roleClaims)
        {
            identity.RemoveClaim(claim);
        }

        // Add all user roles as claims
        foreach (var role in userRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        // Also add the role from the User entity as a fallback
        var entityRole = user.Role.ToString();
        if (!userRoles.Contains(entityRole))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, entityRole));
        }

        return principal;
    }
}
