using Microsoft.AspNetCore.Identity;

namespace MyWebSite.Core.Entities;

/// <summary>
/// Identity kullanıcı entity'si
/// IdentityUser<Guid>: Guid tipinde Id kullanır (mevcut BaseEntity ile uyumlu)
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Kullanıcının adı
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Kullanıcının soyadı
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Kullanıcının oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Kullanıcının son güncellenme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
