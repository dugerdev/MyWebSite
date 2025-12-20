using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;
using MyWebSite.Data.Context;
using MyWebSite.Data.UnitOfWork;
using MyWebSite.Web.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<ContactMessageValidator>();
    fv.AutomaticValidationEnabled = true;
});

// ⭐ DbContext'i Dependency Injection Container'a ekliyoruz
// AddDbContext: Entity Framework DbContext'i servis olarak kaydeder
// options: DbContext yapılandırma ayarları
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // UseSqlServer: SQL Server veritabanı kullanacağımızı belirtiyoruz
    // builder.Configuration.GetConnectionString("DefaultConnection"): 
    //    appsettings.json'daki "ConnectionStrings:DefaultConnection" değerini alır
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    //Password Options
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    //User Options 
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;

    //Lockout ayarlari (brute force saldirilarina karşı
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

})
    .AddEntityFrameworkStores<ApplicationDbContext>() //Identity Tablolarini Application Db'de tut.
    .AddDefaultTokenProviders(); // Token Provider'lari ekle (Şifre Sıfırlama İçin)

// ⭐ UnitOfWork'u Dependency Injection Container'a ekliyoruz
// AddScoped: Her HTTP isteği için yeni bir instance oluşturur
//            Aynı request içinde aynı instance kullanılır
//            Request bitince otomatik olarak dispose edilir
// IUnitOfWork: Interface (Core katmanından)
// UnitOfWork: Implementation (Data katmanından)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    var seeder = new MyWebSite.Data.Seed.DataSeeder(roleManager, userManager);
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
// ⭐ Exception Handler: Development'ta da Error sayfasını görmek için if bloğunun dışına çıkardık
app.UseExceptionHandler("/Home/Error");


app.UseStatusCodePagesWithRedirects("/Home/NotFound?statusCode={0}");

if (!app.Environment.IsDevelopment())

    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Kimlik dogrulama (Login/Logout)
app.UseAuthorization(); // Yetkilendirme (Roles, Policies)

app.MapStaticAssets();

// Area routing - Admin area için
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
