using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Core.Entities;

namespace MyWebSite.Data.Context;

/// <summary>
/// ApplicationDbContext: Veritabanı bağlantısı ve Entity konfigürasyonları
/// IdentityDbContext: Identity yönetimi için gerekli tabloları da içerir
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<ResumeItem> ResumeItems { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<AboutMe> AboutMe { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ResumeItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CompanyOrInstitution).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(200); // Nullable (DateTime? olduğu için)
            // StartDate ve EndDate nullable: Henüz başlamamış veya devam eden pozisyonlar için
            entity.Property(e => e.Description).IsRequired(); // MaxLength yok: Uzun açıklamalara izin verilir
            entity.Property(e => e.DisplayOrder).IsRequired();

            // Enum'u veritabanında int olarak sakla
            entity.Property(e => e.Type).HasConversion<int>();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.DisplayOrder).IsRequired();

            // Enum'u veritabanında int olarak sakla
            entity.Property(e => e.Category).HasConversion<int>();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.Technologies).HasMaxLength(1000);
            entity.Property(e => e.GitHubUrl).HasMaxLength(1000);
            entity.Property(e => e.LiveUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<AboutMe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShortDescription).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullDescription).IsRequired();
            entity.Property(e => e.TwitterUrl).HasMaxLength(500);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(500);
            entity.Property(e => e.GitHubUrl).HasMaxLength(500);
        });

        // Global query filter: Tüm sorgularda IsDeleted=false olan kayıtlar otomatik filtrelenir
        // Bu sayede soft delete yapılan kayıtlar sorgu sonuçlarına dahil edilmez
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContactMessage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ResumeItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Skill>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AboutMe>().HasQueryFilter(e => !e.IsDeleted);
    }
}
