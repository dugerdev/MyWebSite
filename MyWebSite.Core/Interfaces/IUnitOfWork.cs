using MyWebSite.Core.Entities;

namespace MyWebSite.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Project> Projects { get; }
    IGenericRepository<ContactMessage> ContactMessages { get; }
    IGenericRepository<ResumeItem> ResumeItems { get; }
    IGenericRepository<Skill> Skills { get; }
    IGenericRepository<AboutMe> AboutMe { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
