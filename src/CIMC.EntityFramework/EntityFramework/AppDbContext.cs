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

            modelBuilder.Entity<WebsitePage>(entity =>
            {
                entity.HasIndex(e => e.PagePath).IsUnique();
                entity.HasIndex(e => e.IsHome);
            });

            modelBuilder.Entity<WebsitePageVersion>(entity =>
            {
                entity.HasIndex(e => e.PageId);
                entity.HasIndex(e => new { e.PageId, e.VersionNo });
            });

            modelBuilder.Entity<WebsiteNavigation>(entity =>
            {
                entity.HasIndex(e => e.Pid);
            });

            modelBuilder.Entity<ContentProduct>(entity =>
            {
                entity.HasIndex(e => e.CategoryId);
            });

            modelBuilder.Entity<ContentProductCategory>(entity =>
            {
                entity.HasIndex(e => e.Pid);
            });

            modelBuilder.Entity<ContentJob>(entity =>
            {
                entity.HasIndex(e => e.IsActive);
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

        public DbSet<WebsitePage> WebsitePage { get; set; }
        public DbSet<WebsitePageVersion> WebsitePageVersion { get; set; }
        public DbSet<WebsiteSiteConfig> WebsiteSiteConfig { get; set; }
        public DbSet<WebsiteNavigation> WebsiteNavigation { get; set; }
        public DbSet<WebsiteFooter> WebsiteFooter { get; set; }
        public DbSet<ContentProduct> ContentProduct { get; set; }
        public DbSet<ContentProductCategory> ContentProductCategory { get; set; }
        public DbSet<ContentJob> ContentJob { get; set; }


        #endregion



    }
}
