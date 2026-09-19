using CIMC.Data;
using Microsoft.EntityFrameworkCore;

namespace CIMC.EntityFrameworkCore
{
    /// <summary>网站业务数据的 EF Core 数据库上下文。</summary>
    public class AppDbContext : DbContext
    {
        /// <summary>使用给定的数据库选项初始化上下文。</summary>
        /// <param name="options">数据库连接及提供程序配置。</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>配置实体索引和数据库映射。</summary>
        /// <param name="modelBuilder">EF Core 模型构建器。</param>
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

            modelBuilder.Entity<Product>().HasIndex(e => new { e.TagType, e.TagId, e.Sort });
            modelBuilder.Entity<Job>().HasIndex(e => new { e.TagType, e.TagId, e.IsActive });
            modelBuilder.Entity<Tag>().HasIndex(e => new { e.TagType, e.Sort });

            modelBuilder.Entity<MessageBoard>(entity =>
            {
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreationTime);
            });
        }

        #region 数据区域

        /// <summary>管理员数据集。</summary>
        public DbSet<Admin> Admin { get; set; }
        /// <summary>角色数据集。</summary>
        public DbSet<Role> Role { get; set; }
        /// <summary>菜单数据集。</summary>
        public DbSet<Menu> Menu { get; set; }
        /// <summary>角色数据集。</summary>
        public DbSet<RoleMenu> RoleMenu { get; set; }
        /// <summary>素材图片数据集。</summary>
        public DbSet<Images> Images { get; set; }
        /// <summary>文章数据集。</summary>
        public DbSet<Article> Article { get; set; }
        /// <summary>审计日志数据集。</summary>
        public DbSet<AuditLog> AuditLog { get; set; }
        /// <summary>网站页面数据集。</summary>
        public DbSet<WebsitePage> WebsitePage { get; set; }
        /// <summary>网站页面数据集。</summary>
        public DbSet<WebsitePageVersion> WebsitePageVersion { get; set; }
        /// <summary>WebsiteSite数据集。</summary>
        public DbSet<WebsiteSiteConfig> WebsiteSiteConfig { get; set; }
        /// <summary>产品数据集。</summary>
        public DbSet<Product> Product { get; set; }
        /// <summary>招聘岗位数据集。</summary>
        public DbSet<Job> Job { get; set; }
        /// <summary>分类标签数据集。</summary>
        public DbSet<Tag> Tag { get; set; }
        /// <summary>留言数据集。</summary>
        public DbSet<MessageBoard> MessageBoard { get; set; }

        #endregion
    }
}
