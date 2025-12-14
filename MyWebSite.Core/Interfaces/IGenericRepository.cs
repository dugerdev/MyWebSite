using MyWebSite.Core.Entities;
using System.Linq.Expressions;

namespace MyWebSite.Core.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeletePermanenlyAsync(int id);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
