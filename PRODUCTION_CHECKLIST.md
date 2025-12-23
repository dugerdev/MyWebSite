# Production Checklist - MyWebSite Project

Bu dokümanda projenin production ortamına hazır olması için eksik olan yapılar ve yapılması gereken iyileştirmeler listelenmektedir.

## ✅ Tamamlanan Yapılar

1. **Error Handling**: Custom error pages (Error.cshtml, NotFound.cshtml) mevcut
2. **HTTPS Redirection**: `app.UseHttpsRedirection()` mevcut
3. **HSTS**: Production'da HSTS aktif (`app.UseHsts()`)
4. **Identity Security**: Password requirements ve lockout ayarları yapılmış
5. **Anti-Forgery Tokens**: Form validation için kullanılıyor
6. **Authorization**: Admin area için `[Authorize(Roles = "Admin")]` kullanılıyor

---

## ❌ Eksik Yapılar ve Düzeltmeler

### 🔴 KRİTİK (Mutlaka Yapılmalı)

#### 1. **Security Headers Eksik**
**Durum**: Security headers (X-Content-Type-Options, X-Frame-Options, vb.) eksik
**Risk**: XSS, Clickjacking gibi saldırılara karşı koruma yok
**Çözüm**: `Program.cs`'de production için security headers middleware eklenmeli

**Örnek Kod:**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });
}
```

#### 2. **Developer Exception Page Kontrolü**
**Durum**: Development ve Production ortamları için exception handling ayrımı eksik
**Risk**: Production'da detaylı hata mesajları görünebilir
**Çözüm**: `Program.cs`'de environment kontrolü ile Developer Exception Page sadece Development'ta aktif olmalı

**Mevcut Kod:**
```csharp
app.UseExceptionHandler("/Home/Error"); // Her zaman aktif
```

**Olması Gereken:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

#### 3. **Connection String Güvenliği**
**Durum**: Connection string `appsettings.json`'da hardcoded
**Risk**: Hassas veritabanı bilgileri kaynak kodda görünür
**Çözüm**: 
- Production connection string'i environment variables veya Azure Key Vault'tan alınmalı
- `appsettings.json`'dan connection string kaldırılmalı veya placeholder olmalı

**Örnek:**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "" // Production'da environment variable kullanılacak
  }
}
```

```bash
# Production ortamında
setx ASPNETCORE_ConnectionStrings__DefaultConnection "Server=..."
```

#### 4. **Cookie/Session Güvenlik Ayarları**
**Durum**: Identity cookie ayarları yapılandırılmamış
**Risk**: Cookie hijacking, CSRF saldırılarına açık
**Çözüm**: `Program.cs`'de Identity ayarlarına cookie options eklenmeli

**Örnek Kod:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Production için
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
});
```

#### 5. **File Upload Validasyonu**
**Durum**: File upload için size ve type kontrolü yok
**Risk**: Büyük dosya upload saldırıları, zararlı dosya yükleme
**Çözüm**: `ProjectsController`'da file upload validasyonu eklenmeli

**Eksikler:**
- Maximum file size kontrolü (örn: 5MB)
- Allowed file types kontrolü (örn: jpg, png, jpeg, gif)
- File content validation (sadece extension değil, file header kontrolü)

---

### 🟡 ÖNEMLİ (Yapılması Önerilir)

#### 6. **appsettings.Production.json Eksik**
**Durum**: Production için ayrı configuration dosyası yok
**Çözüm**: `appsettings.Production.json` dosyası oluşturulmalı

**Örnek İçerik:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "AllowedHosts": "yourdomain.com"
}
```

#### 7. **Response Compression Eksik**
**Durum**: Response compression middleware yok
**Etki**: Performans optimizasyonu eksik
**Çözüm**: `Program.cs`'de compression middleware eklenmeli

**Kod:**
```csharp
builder.Services.AddResponseCompression();

// Pipeline'da
app.UseResponseCompression();
```

#### 8. **Production Logging Yapılandırması**
**Durum**: Production için file logging veya external logging provider yok
**Çözüm**: 
- Serilog veya NLog gibi bir logging framework eklenebilir
- Ya da built-in file logger kullanılabilir

**Örnek (Serilog):**
```csharp
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});
```

#### 9. **Database Migration Stratejisi**
**Durum**: Migration'ların nasıl uygulanacağı belirsiz
**Çözüm**: 
- Otomatik migration (development için)
- Manuel migration uygulama (production için önerilir)
- Dokümantasyon hazırlanmalı

**Örnek (Development için otomatik):**
```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        dbContext.Database.Migrate();
    }
}
```

**NOT**: Production'da migration'ları manuel olarak `dotnet ef database update` ile uygulamak daha güvenlidir.

#### 10. **.gitignore Güncellemesi**
**Durum**: `appsettings.Production.json` için .gitignore kontrolü gerekli
**Not**: Eğer `appsettings.Production.json` production-specific değerler içermeyecekse (sadece default değerler), git'e eklenebilir. Ancak hassas bilgiler içeriyorsa gitignore'a eklenmeli.

---

### 🟢 OPSIYONEL (Performans/İyileştirme)

#### 11. **Rate Limiting**
**Durum**: API endpoint'leri için rate limiting yok
**Not**: Eğer public API endpoint'leri varsa rate limiting eklenebilir

#### 12. **CORS Yapılandırması**
**Durum**: CORS ayarları yok
**Not**: Eğer frontend ayrı bir uygulama ise CORS ayarları gerekebilir. Şu an monolith yapı olduğu için gerekli görünmüyor.

#### 13. **Caching Stratejisi**
**Durum**: Response caching yok
**Not**: Performans için önemli sayfalar için caching eklenebilir (örn: Home/Index için featured projects)

---

## 📋 Yapılacaklar Öncelik Sırası

1. ✅ Security Headers ekle
2. ✅ Developer Exception Page kontrolü
3. ✅ Cookie/Session güvenlik ayarları
4. ✅ File upload validasyonu
5. ✅ Connection String güvenliği
6. ✅ appsettings.Production.json oluştur
7. ✅ Response Compression ekle
8. ✅ Production logging yapılandırması
9. ✅ Database migration stratejisi
10. ✅ .gitignore kontrolü

---

## 🔍 Test Edilmesi Gerekenler

1. **Security Headers Test**: Browser DevTools > Network > Headers sekmesinde security headers'ların gönderildiğini kontrol et
2. **Error Handling Test**: Production modunda hata oluştur ve kullanıcı dostu error page'in göründüğünü kontrol et
3. **Cookie Security Test**: Browser DevTools > Application > Cookies'de cookie'lerin HttpOnly ve Secure flag'lerini kontrol et
4. **File Upload Test**: 
   - 5MB'dan büyük dosya yüklemeyi dene (reject edilmeli)
   - .exe, .bat gibi zararlı dosya uzantılarını dene (reject edilmeli)
5. **HTTPS Test**: HTTP isteklerinin HTTPS'e yönlendirildiğini kontrol et
6. **Connection String Test**: Environment variable'dan connection string'in okunduğunu kontrol et

---

## 📚 Kaynaklar

- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Production Best Practices](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/production-checklist)

---

**Son Güncelleme**: 2025-01-20
**Proje**: MyWebSite
**Versiyon**: 1.0.0



