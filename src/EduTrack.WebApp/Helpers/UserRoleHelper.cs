using EduTrack.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.WebApp.Helpers;

public static class UserRoleHelper
{
    public static async Task<string> GetUserRoleAsync(UserManager<User> userManager, User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? "Student";
    }
    
    public static async Task<List<string>> GetUserRolesAsync(UserManager<User> userManager, User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.ToList();
    }
}
