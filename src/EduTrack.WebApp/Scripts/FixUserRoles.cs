using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EduTrack.Infrastructure.Data;

namespace EduTrack.WebApp.Scripts;

public static class FixUserRolesScript
{
    public static async Task FixUserRolesAsync(UserManager<User> userManager, AppDbContext context)
    {
        // Get all users
        var users = await userManager.Users.ToListAsync();
        
        foreach (var user in users)
        {
            // Get current roles
            var currentRoles = await userManager.GetRolesAsync(user);
            
            // Remove all roles
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            
            // Determine correct role based on email pattern (simple logic)
            string correctRole;
            if (user.Email?.Contains("admin") == true)
                correctRole = "Admin";
            else if (user.Email?.Contains("teacher") == true)
                correctRole = "Teacher";
            else
                correctRole = "Student";
            
            await userManager.AddToRoleAsync(user, correctRole);
            
            Console.WriteLine($"Fixed user {user.Email}: Removed roles [{string.Join(", ", currentRoles)}], Added role [{correctRole}]");
        }
        
        await context.SaveChangesAsync();
        Console.WriteLine("User roles fixed successfully!");
    }
}
