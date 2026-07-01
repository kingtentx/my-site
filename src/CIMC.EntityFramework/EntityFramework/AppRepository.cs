using CIMC.EntityFramework;

namespace CIMC.EntityFrameworkCore
{
    public class AppRepository<T> : Repository<T, AppDbContext> where T : class, new()
    {
        public AppRepository(AppDbContext dbContext) : base(dbContext) { }
    }

}
