using CIMC.EntityFramework;

namespace CIMC.EntityFrameworkCore
{
    /// <summary>提供 EF Core 实体仓储实现。</summary>
    public class AppRepository<T> : Repository<T, AppDbContext> where T : class, new()
    {
        /// <summary>初始化App。</summary>
        public AppRepository(AppDbContext dbContext) : base(dbContext) { }
    }

}
