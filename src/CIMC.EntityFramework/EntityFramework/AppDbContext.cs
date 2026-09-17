using CIMC.Data;
using Microsoft.EntityFrameworkCore;

namespace CIMC.EntityFrameworkCore
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

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
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => new { e.ParentId, e.Sort });
            });

            modelBuilder.Entity<WebsitePageVersion>(entity =>
            {
                entity.HasIndex(e => e.PageId);
                entity.HasIndex(e => new { e.PageId, e.VersionNo });
            });

            modelBuilder.Entity<Album>().HasIndex(e => new { e.TagType, e.TagId, e.Sort });
            modelBuilder.Entity<Job>().HasIndex(e => new { e.TagType, e.TagId, e.IsActive });
            modelBuilder.Entity<Tag>().HasIndex(e => new { e.TagType, e.Sort });

            modelBuilder.Entity<MessageBoard>(entity =>
            {
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreationTime);
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
        public DbSet<Album> Album { get; set; }
        public DbSet<Job> Job { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<MessageBoard> MessageBoard { get; set; }

        #endregion
    }
}
