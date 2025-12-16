using MyWebSite.Core.Entities;
using System.Linq.Expressions;

namespace MyWebSite.Core.Interfaces;

/// <summary>
/// Generic Repository Pattern için interface
/// Tüm repository'ler için ortak CRUD işlemlerini tanımlar
/// </summary>
/// <typeparam name="T">Entity tipi (BaseEntity'den türemiş olmalı)</typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Tüm kayıtları getirir (silinmemiş olanlar)
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Belirli bir koşula göre kayıtları getirir
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// ID'ye göre tek bir kayıt getirir (Guid kullanıyoruz)
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Yeni bir kayıt ekler
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Mevcut bir kaydı günceller
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Bir kaydı siler (soft delete - IsDeleted = true yapar)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Veritabanından kalıcı olarak siler (hard delete)
    /// </summary>
    Task DeletePermanentlyAsync(Guid id);

    /// <summary>
    /// Belirli bir koşulu sağlayan kayıt var mı kontrol eder
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
