using System.Linq.Expressions;

namespace CIMC.EntityFramework.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();

    Task<T?> GetAsync(int id);

    Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null);

    Task AddAsync(T entity);

    void Update(T entity);

    void Delete(T entity);

    Task<int> SaveChangesAsync();
}
