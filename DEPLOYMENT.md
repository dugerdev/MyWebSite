# Production Deployment Guide

Bu dokümantasyon, MyWebSite projesinin production ortamına deploy edilmesi için gerekli adımları ve best practice'leri içerir.

## 📋 İçindekiler

1. [Ön Gereksinimler](#ön-gereksinimler)
2. [Database Migration Stratejisi](#database-migration-stratejisi)
3. [Environment Variables](#environment-variables)
4. [Production Deployment Adımları](#production-deployment-adımları)
5. [Post-Deployment Kontroller](#post-deployment-kontroller)
6. [Güvenlik Kontrol Listesi](#güvenlik-kontrol-listesi)

---

## 🚀 Ön Gereksinimler

### Gerekli Yazılımlar

- **.NET 9.0 SDK** (veya Runtime)
- **SQL Server** (2019 veya üzeri önerilir)
- **IIS** (Windows Server için) veya **Nginx/Kestrel** (Linux için)
- **Git** (version control için)

### Gerekli Bilgiler

- Production database connection string
- HTTPS sertifikası
- Domain adı
- Production sunucu bilgileri

---

## 🗄️ Database Migration Stratejisi

### Development Ortamında Migration Oluşturma

Yeni bir entity değişikliği yaptığınızda migration oluşturun:

```bash
# Migration oluşturma
dotnet ef migrations add MigrationName --project MyWebSite.Data --startup-project MyWebSite.Web

# Örnek:
dotnet ef migrations add AddNewFeature --project MyWebSite.Data --startup-project MyWebSite.Web
```

### Migration Adlandırma Kuralları

- **Açıklayıcı isimler kullanın**: `AddUserTable`, `UpdateProjectDescription`
- **Kısa ve öz olun**: `AddResumeEntities` ✅ | `AddResumeEntitiesAndUpdateProjectTable` ❌
- **Tarih öneki kullanmayın**: EF Core otomatik ekliyor

### Production'a Migration Uygulama

#### Yöntem 1: Entity Framework CLI ile (Önerilen)

```bash
# Production sunucusunda
cd /path/to/MyWebSite

# Tüm pending migration'ları uygula
dotnet ef database update --project MyWebSite.Data --startup-project MyWebSite.Web

# Belirli bir migration'a kadar uygula (gerekirse)
dotnet ef database update MigrationName --project MyWebSite.Data --startup-project MyWebSite.Web
```

#### Yöntem 2: SQL Script Oluşturma (Manuel Kontrol İçin)

```bash
# Migration'ları SQL script'e dönüştür
dotnet ef migrations script --project MyWebSite.Data --startup-project MyWebSite.Web --output migration.sql

# Son migration'dan sonraki değişiklikleri script'e dönüştür
dotnet ef migrations script --project MyWebSite.Data --startup-project MyWebSite.Web --from LastMigrationName --output migration.sql
```

**⚠️ ÖNEMLİ**: SQL script'i production'a uygulamadan önce:
1. Script'i gözden geçirin
2. Test ortamında test edin
3. Database backup alın

### Migration Geri Alma (Rollback)

```bash
# Bir önceki migration'a geri dön
dotnet ef database update PreviousMigrationName --project MyWebSite.Data --startup-project MyWebSite.Web

# Tüm migration'ları geri al (DİKKATLİ KULLANIN!)
dotnet ef database update 0 --project MyWebSite.Data --startup-project MyWebSite.Web
```

### Best Practices

1. **Her deployment öncesi backup alın**
   ```sql
   -- SQL Server Management Studio veya Azure Portal üzerinden
   BACKUP DATABASE MyPortfolioDb TO DISK = 'C:\Backups\MyPortfolioDb_20241220.bak'
   ```

2. **Migration'ları sırayla uygulayın**: EF Core otomatik olarak sırayı takip eder

3. **Test ortamında önce test edin**: Production'a geçmeden önce staging ortamında test edin

4. **Migration'ları source control'de tutun**: `.gitignore`'da Migrations klasörünü ignore etmeyin

5. **Data migration gerekirse ayrı script yazın**: Yapısal değişiklikler için EF Migration, veri migrasyonu için ayrı SQL script

---

## 🔐 Environment Variables

### Production Connection String

**⚠️ GÜVENLİK**: Connection string'i environment variable olarak set edin, `appsettings.Production.json` dosyasında tutmayın!

#### Windows (IIS / PowerShell)

```powershell
# Kullanıcı seviyesinde (geçici)
$env:ConnectionStrings__DefaultConnection = "Server=your-server;Database=MyPortfolioDb;User Id=sa;Password=your-password;TrustServerCertificate=True;"

# Sistem seviyesinde (kalıcı)
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=your-server;Database=MyPortfolioDb;User Id=sa;Password=your-password;TrustServerCertificate=True;", "Machine")
```

#### Linux (systemd service)

`/etc/systemd/system/mywebsite.service` dosyasına ekleyin:

```ini
[Service]
Environment="ConnectionStrings__DefaultConnection=Server=your-server;Database=MyPortfolioDb;User Id=sa;Password=your-password;TrustServerCertificate=True;"
```

#### Azure App Service

1. Azure Portal → App Service → Configuration → Application settings
2. Yeni bir setting ekleyin:
   - **Name**: `ConnectionStrings:DefaultConnection`
   - **Value**: Connection string'iniz

---

## 🚀 Production Deployment Adımları

### 1. Projeyi Build Etme

```bash
# Release modunda build
dotnet build --configuration Release --project MyWebSite.Web

# Publish (yayınlama)
dotnet publish --configuration Release --output ./publish --project MyWebSite.Web
```

### 2. Dosyaları Production Sunucusuna Kopyalama

```bash
# SCP ile (Linux)
scp -r ./publish/* user@production-server:/var/www/mywebsite/

# FTP veya Azure DevOps Pipeline kullanabilirsiniz
```

### 3. Database Migration Uygulama

```bash
# Production sunucusunda
cd /var/www/mywebsite
dotnet ef database update --project MyWebSite.Data --startup-project MyWebSite.Web
```

### 4. IIS Yapılandırması (Windows)

1. IIS Manager'ı açın
2. New Application Pool oluşturun (.NET CLR Version: No Managed Code)
3. New Website oluşturun:
   - Physical Path: `C:\inetpub\wwwroot\mywebsite\publish`
   - Binding: HTTPS (443 port, SSL sertifikası ile)
   - Application Pool: Yukarıda oluşturduğunuz pool

### 5. Nginx Yapılandırması (Linux)

`/etc/nginx/sites-available/mywebsite`:

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /path/to/certificate.crt;
    ssl_certificate_key /path/to/private.key;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 6. Systemd Service (Linux)

`/etc/systemd/system/mywebsite.service`:

```ini
[Unit]
Description=MyWebsite ASP.NET Core App
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/mywebsite/MyWebSite.Web.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment="ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;"
SyslogIdentifier=mywebsite

[Install]
WantedBy=multi-user.target
```

Service'i başlatın:

```bash
sudo systemctl enable mywebsite
sudo systemctl start mywebsite
sudo systemctl status mywebsite
```

---

## ✅ Post-Deployment Kontroller

### 1. Database Kontrolü

- [ ] Migration'lar başarıyla uygulandı mı?
- [ ] Identity tabloları oluşturuldu mu?
- [ ] Admin kullanıcısı var mı?

### 2. Application Kontrolü

- [ ] HTTPS çalışıyor mu?
- [ ] Static dosyalar (CSS, JS, images) yükleniyor mu?
- [ ] Admin paneli erişilebilir mi?
- [ ] Login işlemi çalışıyor mu?

### 3. Güvenlik Kontrolü

- [ ] Security headers gönderiliyor mu? (Browser DevTools → Network → Headers)
- [ ] Cookie HttpOnly ve Secure mi?
- [ ] Error sayfaları generic mesaj gösteriyor mu? (sensitive bilgi sızmıyor mu?)

### 4. Performans Kontrolü

- [ ] Response Compression aktif mi?
- [ ] Sayfa yükleme süreleri kabul edilebilir mi?

---

## 🔒 Güvenlik Kontrol Listesi

### Pre-Deployment

- [ ] `appsettings.Production.json` git'e commit edilmedi mi? (`.gitignore` kontrol)
- [ ] Connection string environment variable olarak set edildi mi?
- [ ] HTTPS sertifikası geçerli ve güncel mi?
- [ ] Admin kullanıcı şifresi güçlü mü?

### Post-Deployment

- [ ] Security headers aktif mi?
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Content-Security-Policy`
  - `Referrer-Policy`
- [ ] Cookie güvenlik ayarları aktif mi?
  - `HttpOnly`
  - `Secure` (Production'da)
  - `SameSite`
- [ ] File upload validasyonu çalışıyor mu?
- [ ] Error sayfaları sensitive bilgi sızdırmıyor mu?

### Ongoing

- [ ] Log dosyaları düzenli kontrol ediliyor mu?
- [ ] Database backup'ları otomatik alınıyor mu?
- [ ] Güvenlik güncellemeleri takip ediliyor mu?

---

## 📞 Sorun Giderme

### Migration Hataları

```bash
# Migration durumunu kontrol et
dotnet ef migrations list --project MyWebSite.Data --startup-project MyWebSite.Web

# Database'i migration olmadan oluştur (sadece test için)
dotnet ef database drop --project MyWebSite.Data --startup-project MyWebSite.Web
dotnet ef database update --project MyWebSite.Data --startup-project MyWebSite.Web
```

### Log Kontrolü

```bash
# Application logs (Linux)
sudo journalctl -u mywebsite -f

# IIS logs (Windows)
C:\inetpub\logs\LogFiles\W3SVC1\
```

### Performance Issues

- Response Compression aktif mi kontrol edin
- Database connection pool ayarlarını kontrol edin
- Static file caching aktif mi kontrol edin

---

## 📚 Ek Kaynaklar

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [ASP.NET Core Production Best Practices](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/production-best-practices)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)

---

**Son Güncelleme**: 2024-12-20

