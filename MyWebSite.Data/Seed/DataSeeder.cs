using Microsoft.AspNetCore.Identity;
using MyWebSite.Core.Entities;

namespace MyWebSite.Data.Seed;

public class DataSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DataSeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager; 
    }

    public async Task SeedAsync()
    {
        if(!await _roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new IdentityRole<Guid>("Admin");
            await _roleManager.CreateAsync(adminRole);
        }

        var adminEmail = "admin@admin.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if(adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.Now,
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
