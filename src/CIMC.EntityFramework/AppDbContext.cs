using CIMC.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CIMC.EntityFramework;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Menu> Menus => Set<Menu>();

    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<SitePage> SitePages => Set<SitePage>();

    public DbSet<SiteSection> SiteSections => Set<SiteSection>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AdminUser>().HasIndex(p => p.UserName).IsUnique();
        builder.Entity<Role>().HasIndex(p => p.Code).IsUnique();
        builder.Entity<Menu>().HasIndex(p => p.Code).IsUnique();
        builder.Entity<SitePage>().HasIndex(p => p.PageKey).IsUnique();

        builder.Entity<RoleMenu>()
            .HasIndex(p => new { p.RoleId, p.MenuId })
            .IsUnique();

        builder.Entity<SitePage>()
            .HasMany(p => p.Sections)
            .WithOne(p => p.SitePage)
            .HasForeignKey(p => p.SitePageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
