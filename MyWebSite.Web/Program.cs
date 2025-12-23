using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;
using MyWebSite.Data.Context;
using MyWebSite.Data.UnitOfWork;
using MyWebSite.Web.Validators;

var builder = WebApplication.CreateBuilder(args);

// MVC ve FluentValidation yapılandırması
// FluentValidation: Model validasyonlarını attribute'lar yerine fluent API ile yapmamızı sağlar
builder.Services.AddControllersWithViews().AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<ContactMessageValidator>();
    fv.AutomaticValidationEnabled = true;
})
.AddMvcOptions(options =>
{
    // Model binding sırasında collection boyutu limiti
    options.MaxModelBindingCollectionSize = 1000;
});

// Dosya yükleme limitleri
// FormOptions: Multipart form data (dosya yükleme) için boyut limitleri
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 31457280; // 30 MB - Toplam form verisi boyutu
    options.MultipartHeadersLengthLimit = 1024; // 1 KB - Header boyutu
    options.ValueLengthLimit = int.MaxValue; // Tek bir form field'ı için boyut limiti
    options.ValueCountLimit = 1024; // Form field sayısı limiti (çok sayıda field ile 400 hatası alınmaması için)
    options.MultipartBoundaryLengthLimit = 128; // Multipart boundary için
});

// Response compression: Ağ trafiğini azaltmak için HTTP yanıtlarını sıkıştırır
// Production'da aktif, development'ta Browser Link ile uyumsuz olduğu için kapalı
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/json", "text/css", "application/javascript" });
});

// Brotli compression: Gzip'den daha iyi sıkıştırma oranı sağlar (öncelikli)
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Gzip compression: Brotli desteklemeyen tarayıcılar için fallback
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Entity Framework DbContext yapılandırması
// Connection string: Önce environment variable'dan alınır (production'da güvenlik için),
// bulunamazsa appsettings.json'dan okunur (development için)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                           ?? builder.Configuration.GetConnectionString("DefaultConnection");
    
    options.UseSqlServer(connectionString);
});

// ASP.NET Core Identity yapılandırması
// Identity: Kullanıcı kimlik doğrulama ve yetkilendirme sistemi
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Şifre güvenlik kuralları
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Kullanıcı kuralları
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Email doğrulaması zorunlu değil

    // Account lockout: Brute force saldırılarına karşı koruma
    // 5 başarısız denemeden sonra 5 dakika hesap kilitlenir
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<ApplicationDbContext>() // Identity tabloları ApplicationDbContext'te saklanır
    .AddDefaultTokenProviders(); // Şifre sıfırlama token'ları için

// Identity Cookie güvenlik ayarları
// Bu ayarlar XSS ve CSRF saldırılarına karşı koruma sağlar
builder.Services.ConfigureApplicationCookie(options =>
{
    // HttpOnly: JavaScript'ten erişilemez, XSS saldırılarında cookie çalınmasını önler
    options.Cookie.HttpOnly = true;
    
    // Secure Policy: Production'da sadece HTTPS üzerinden cookie gönderilir
    // Development'ta HTTP'ye de izin verilir (localhost için)
    options.Cookie.SecurePolicy = builder.Environment.IsProduction() 
        ? CookieSecurePolicy.Always 
        : CookieSecurePolicy.SameAsRequest;
    
    // SameSite: CSRF saldırılarını önler
    // Lax: Cross-site GET isteklerine izin verir, POST isteklerinde koruma sağlar
    options.Cookie.SameSite = SameSiteMode.Lax;
    
    options.Cookie.Path = "/";
    
    // Cookie geçerlilik süresi ve sliding expiration
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // 60 dakika sonra otomatik çıkış
    options.SlidingExpiration = true; // Her aktivitede süre yenilenir
    
    // Identity redirect path'leri
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Home/Index";
    options.LogoutPath = "/Account/Logout";
});

// Repository Pattern: UnitOfWork implementasyonu
// Scoped lifetime: Her HTTP isteği için yeni bir instance, request bitince dispose edilir
// Bu sayede aynı DbContext instance'ı tüm repository'lerde kullanılır
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Veritabanı seed işlemi: İlk çalıştırmada admin kullanıcısı ve rolleri oluşturulur
// Production'da admin bilgileri environment variable'lardan alınır (güvenlik için)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var environment = services.GetRequiredService<IHostEnvironment>();

    var seeder = new MyWebSite.Data.Seed.DataSeeder(roleManager, userManager, configuration, environment);
    await seeder.SeedAsync();
}

// HTTP Request Pipeline yapılandırması
// Middleware'ler sırayla çalışır, bu yüzden sıralama önemlidir

// Exception handling: Environment'a göre farklı davranış
if (app.Environment.IsDevelopment())
{
    // Development: Detaylı hata sayfası (stack trace, exception detayları)
    app.UseDeveloperExceptionPage();
}
else 
{
    // Production: Genel hata sayfası (kullanıcıya detay gösterilmez, güvenlik için)
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTPS Strict Transport Security
}

// Status code sayfaları: 404, 400, 403 gibi durum kodları için özel sayfa
app.UseStatusCodePagesWithRedirects("/Home/NotFound?statusCode={0}");

// Response compression: Development'ta kapalı (Browser Link ile uyumsuz)
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}

// Security headers: Production'da eklenir (XSS, clickjacking, MIME sniffing koruması)
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        // MIME type sniffing koruması
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // Clickjacking koruması: Sayfanın iframe içinde gösterilmesini engeller
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        
        // Referrer policy: Hangi bilgilerin referrer header'ında gönderileceğini kontrol eder
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Permissions Policy: Tarayıcı özelliklerine erişimi kontrol eder (geolocation, camera, microphone devre dışı)
        context.Response.Headers.Append("Permissions-Policy", 
            "geolocation=(), microphone=(), camera=()");
        
        // Content Security Policy: XSS saldırılarını önlemek için hangi kaynakların yüklenebileceğini belirler
        // unsafe-inline ve unsafe-eval Bootstrap ve jQuery için gerekli
        var cspHeader = "default-src 'self'; " +
                        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
                        "font-src 'self' https://fonts.gstatic.com data:; " +
                        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                        "img-src 'self' data: https:; " +
                        "connect-src 'self'; " +
                        "frame-ancestors 'none';";
        context.Response.Headers.Append("Content-Security-Policy", cspHeader);
        
        await next();
    });
}

// Static files: CSS, JavaScript, image dosyalarını serve eder
app.UseStaticFiles();

// HTTPS redirection: HTTP isteklerini HTTPS'e yönlendirir
app.UseHttpsRedirection();

// Routing: URL'leri controller ve action'lara eşler
app.UseRouting();

// Authentication: Kullanıcının kimliğini doğrular (login/logout)
app.UseAuthentication();

// Authorization: Kullanıcının yetkisini kontrol eder (roles, policies)
app.UseAuthorization();

// Static assets mapping
app.MapStaticAssets();

// Area routing: Admin area için özel routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default routing: Normal controller'lar için
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
