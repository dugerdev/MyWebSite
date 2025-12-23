using Microsoft.EntityFrameworkCore.Storage;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;
using MyWebSite.Data.Context;
using MyWebSite.Data.Repositories;

namespace MyWebSite.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;  

    public IGenericRepository<Project>? _projects;

    public IGenericRepository<ContactMessage>? _contactMessages;

    public IGenericRepository<ResumeItem>? _resumeItems;

    public IGenericRepository<Skill>? _skills;

    public IGenericRepository<AboutMe>? _aboutMe;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }


    public IGenericRepository<Project> Projects
    {
        get
        {
            _projects ??= new GenericRepository<Project>(_context);
            return _projects;
        }
    }

    public IGenericRepository<ContactMessage> ContactMessages
    {
        get
        {
            // Eğer _contactMessages null ise, yeni bir GenericRepository oluştur
            _contactMessages ??= new GenericRepository<ContactMessage>(_context);
            return _contactMessages;
        }
    }

    public IGenericRepository<ResumeItem> ResumeItems
    {
        get
        {
            _resumeItems ??= new GenericRepository<ResumeItem>(_context);
            return _resumeItems;
        }
    }

    public IGenericRepository<Skill> Skills 
    {
        get
        {
            _skills ??= new GenericRepository<Skill>(_context);
            return _skills;
        }
    }

    public IGenericRepository<AboutMe> AboutMe
    {
        get
        {
            _aboutMe ??= new GenericRepository<AboutMe>(_context);
            return _aboutMe;
        }
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if(_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    public async Task RollbackTransactionAsync()
    {
       if( _transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
