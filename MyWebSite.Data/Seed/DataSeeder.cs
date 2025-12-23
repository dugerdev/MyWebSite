using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MyWebSite.Core.Entities;

namespace MyWebSite.Data.Seed;

/// <summary>
/// Veritabanı seed işlemleri: Admin rolü ve kullanıcısı oluşturur
/// Production'da admin bilgileri environment variable'lardan alınır (güvenlik için)
/// </summary>
public class DataSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public DataSeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task SeedAsync()
    {
        // Admin rolü oluştur
        if(!await _roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new IdentityRole<Guid>("Admin");
            await _roleManager.CreateAsync(adminRole);
        }

        // Admin kullanıcı bilgileri: Environment variable'dan al (Production güvenliği için)
        // Development'ta varsayılan değerler kullanılır
        var adminEmail = _environment.IsProduction()
            ? _configuration["Admin:Email"] ?? throw new InvalidOperationException("Admin:Email environment variable must be set in Production")
            : _configuration["Admin:Email"] ?? "admin@admin.com";

        var adminPassword = _environment.IsProduction()
            ? _configuration["Admin:Password"] ?? throw new InvalidOperationException("Admin:Password environment variable must be set in Production")
            : _configuration["Admin:Password"] ?? "Admin123!";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        // Admin kullanıcısı yoksa oluştur
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

            var result = await _userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                // Şifre kurallarına uymuyorsa hata fırlat
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }
    }
}
