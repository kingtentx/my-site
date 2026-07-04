using CIMC.Data;
using Microsoft.EntityFrameworkCore;

namespace CIMC.EntityFrameworkCore
{
    public class AppDbContext : DbContext
    {
        //构造方法    
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => e.OperationTime);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.OperationType);
                entity.HasIndex(e => e.OperationModule);
                entity.HasIndex(e => new { e.OperationTime, e.OperationType });
            });
        }


        #region 数据区域

        public DbSet<Admin> Admin { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<RoleMenu> RoleMenu { get; set; }
        public DbSet<Images> Images { get; set; }
        public DbSet<Article> Article { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }


        #endregion



    }
}
