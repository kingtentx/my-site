using Microsoft.EntityFrameworkCore;
using MySite.Web.WebsiteBuilder.Models;

namespace MySite.Web.WebsiteBuilder.Data
{
    public class WebsiteBuilderDbContext : DbContext
    {
        public WebsiteBuilderDbContext(DbContextOptions<WebsiteBuilderDbContext> options) : base(options)
        {
        }

        public DbSet<WebsiteSiteConfig> SiteConfigs { get; set; }
        public DbSet<WebsitePage> Pages { get; set; }
        public DbSet<WebsitePageVersion> PageVersions { get; set; }
        public DbSet<WebsiteNavigation> Navigations { get; set; }
        public DbSet<WebsiteBanner> Banners { get; set; }
        public DbSet<WebsiteFooter> Footers { get; set; }
        public DbSet<ContentNewsCategory> NewsCategories { get; set; }
        public DbSet<ContentNews> News { get; set; }
        public DbSet<ContentProductCategory> ProductCategories { get; set; }
        public DbSet<ContentProduct> Products { get; set; }
        public DbSet<ContentJob> Jobs { get; set; }
        public DbSet<ContentJobApply> JobApplies { get; set; }
        public DbSet<MaterialFile> MaterialFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WebsitePage>().HasIndex(x => x.PagePath);
            modelBuilder.Entity<WebsitePage>().Property(x => x.LayoutJson).HasColumnType("longtext");
            modelBuilder.Entity<WebsitePage>().Property(x => x.ComponentJson).HasColumnType("longtext");
            modelBuilder.Entity<WebsitePage>().Property(x => x.DraftJson).HasColumnType("longtext");
            modelBuilder.Entity<WebsitePage>().Property(x => x.PublishJson).HasColumnType("longtext");
            modelBuilder.Entity<WebsitePageVersion>().Property(x => x.DraftJson).HasColumnType("longtext");
            modelBuilder.Entity<WebsitePageVersion>().Property(x => x.PublishJson).HasColumnType("longtext");
            modelBuilder.Entity<WebsiteFooter>().Property(x => x.FriendLinksJson).HasColumnType("longtext");
            modelBuilder.Entity<ContentNews>().Property(x => x.Content).HasColumnType("longtext");
            modelBuilder.Entity<ContentProduct>().Property(x => x.ImageList).HasColumnType("longtext");
            modelBuilder.Entity<ContentProduct>().Property(x => x.Description).HasColumnType("longtext");
            modelBuilder.Entity<ContentProduct>().Property(x => x.Specification).HasColumnType("longtext");
            modelBuilder.Entity<ContentProduct>().Property(x => x.Feature).HasColumnType("longtext");
            modelBuilder.Entity<ContentJob>().Property(x => x.Responsibilities).HasColumnType("longtext");
            modelBuilder.Entity<ContentJob>().Property(x => x.Requirements).HasColumnType("longtext");
        }
    }
}