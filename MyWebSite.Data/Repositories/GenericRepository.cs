using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;
using MyWebSite.Data.Context;

namespace MyWebSite.Data.Repositories;

/// <summary>
/// GenericRepository implementasyonu - Tüm entity'ler için ortak CRUD işlemleri
/// </summary>
/// <typeparam name="T">BaseEntity'den türemiş entity tipi</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    // DbContext: Veritabanı işlemlerini yapmak için
    protected readonly ApplicationDbContext _context;
    
    // DbSet: Bu repository'nin çalışacağı tablo
    protected readonly DbSet<T> _dbSet;

    // Constructor: Repository oluşturulurken DbContext'i alır
    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        // Set<T>() ile T tipine göre DbSet'i alıyoruz
        _dbSet = context.Set<T>();
    }

    // GetAllAsync: Tüm kayıtları getirir (IsDeleted=false olanlar)
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        // HasQueryFilter sayesinde IsDeleted=true olanlar otomatik filtrelenir
        return await _dbSet.ToListAsync();
    }

    // FindAsync: Belirli bir koşula göre kayıtları bulur
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // Where ile koşulu uyguluyoruz
        return await _dbSet.Where(predicate).ToListAsync();
    }

    // GetByIdAsync: Guid ID'ye göre tek bir kayıt getirir
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        // FindAsync: Primary Key'e göre arama yapar (Guid ile)
        return await _dbSet.FindAsync(id);
    }

    // Yeni bir kayıt ekler
    // Id ve CreatedAt otomatik olarak set edilir
    public virtual async Task<T> AddAsync(T entity)
    {
        // Id boşsa yeni Guid oluştur (kullanıcı belirtmemişse)
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        
        // Oluşturulma tarihini otomatik set et
        entity.CreatedAt = DateTime.Now;
        
        await _dbSet.AddAsync(entity);
        
        return entity;
    }

    // Mevcut bir kaydı günceller
    // UpdatedDate otomatik olarak set edilir
    // NOT: Bu metod genellikle kullanılmaz, çünkü tracked entity üzerinde direkt değişiklik yapılır
    public virtual Task UpdateAsync(T entity)
    {
        entity.UpdatedDate = DateTime.Now;
        _dbSet.Update(entity);
        
        return Task.CompletedTask;
    }

    // Soft delete: Fiziksel olarak silmez, sadece IsDeleted=true yapar
    // Bu sayede veri geri kurtarılabilir ve referans bütünlüğü korunur
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.Now;
            
            // UpdateAsync kullanmak yerine direkt değişiklik yapıyoruz
            // Çünkü entity zaten tracked (GetByIdAsync ile alındığı için)
        }
    }

    // DeletePermanentlyAsync: Hard delete yapar (kaydı veritabanından gerçekten siler)
    public virtual async Task DeletePermanentlyAsync(Guid id)
    {
        // Kaydı buluyoruz
        var entity = await GetByIdAsync(id);
        
        if (entity != null)
        {
            // Remove: Entity'yi silinecek olarak işaretler
            _dbSet.Remove(entity);
        }
    }

    // AnyAsync: Belirli bir koşulu sağlayan kayıt var mı kontrol eder
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        // AnyAsync: En az bir kayıt varsa true, yoksa false döner
        return await _dbSet.AnyAsync(predicate);
    }
}
