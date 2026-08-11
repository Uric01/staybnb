using Microsoft.AspNetCore.Identity;
using Staybnb.Models;

namespace Staybnb.Data;

public class RoleSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        // Seed roles
        var roles = new[] { "Guest", "Host", "Admin", "SuperAdmin" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed SuperAdmin user
        var superAdminEmail = "superadmin@staybnb.com";
        var superAdminUser = await _userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            superAdminUser = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(superAdminUser, "SuperAdmin@123!");
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }
    }
}
