using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CIMC.EntityFramework.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<T> Query()
    {
        return _dbContext.Set<T>().AsQueryable();
    }

    public Task<T?> GetAsync(int id)
    {
        return _dbContext.Set<T>().FindAsync(id).AsTask();
    }

    public Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _dbContext.Set<T>().AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return query.ToListAsync();
    }

    public Task AddAsync(T entity)
    {
        return _dbContext.Set<T>().AddAsync(entity).AsTask();
    }

    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}
