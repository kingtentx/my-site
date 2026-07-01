using CIMC.Core;
using CIMC.Data.Entities;
using CIMC.Helper;
using Microsoft.EntityFrameworkCore;

namespace CIMC.EntityFramework;

public class DataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IConfigurationLike _configuration;

    public DataSeeder(AppDbContext dbContext, IConfigurationLike configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
        await SeedRolesAndMenusAsync();
        await SeedAdminAsync();
        await SeedDefaultPagesAsync();
    }

    private async Task SeedRolesAndMenusAsync()
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(p => p.Code == Consts.SuperAdminRoleCode);
        if (role == null)
        {
            role = new Role { Code = Consts.SuperAdminRoleCode, Name = "超级管理员", IsSystem = true };
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();
        }

        var menus = new[]
        {
            new Menu { Code = "Dashboard", Name = "控制台", Path = "/Admin", Sort = 10, IsSystem = true },
            new Menu { Code = "SiteBuilder", Name = "页面设计", Path = "/Admin/Designer?pageKey=home", Sort = 20, IsSystem = true },
            new Menu { Code = "Menu", Name = "菜单管理", Path = "/Menu", Sort = 30, IsSystem = true },
            new Menu { Code = "Role", Name = "角色权限", Path = "/Role", Sort = 40, IsSystem = true },
            new Menu { Code = "AuditLog", Name = "审计日志", Path = "/AuditLog", Sort = 50, IsSystem = true }
        };

        foreach (var seed in menus)
        {
            var menu = await _dbContext.Menus.FirstOrDefaultAsync(p => p.Code == seed.Code);
            if (menu == null)
            {
                _dbContext.Menus.Add(seed);
            }
            else
            {
                menu.Name = seed.Name;
                menu.Path = seed.Path;
                menu.Sort = seed.Sort;
                menu.IsSystem = true;
                menu.IsEnabled = true;
            }
        }
        await _dbContext.SaveChangesAsync();

        var dbMenus = await _dbContext.Menus.Where(p => p.IsEnabled && !p.IsDeleted).ToListAsync();
        foreach (var menu in dbMenus)
        {
            if (!await _dbContext.RoleMenus.AnyAsync(p => p.RoleId == role.Id && p.MenuId == menu.Id))
            {
                _dbContext.RoleMenus.Add(new RoleMenu
                {
                    RoleId = role.Id,
                    MenuId = menu.Id,
                    CanView = true,
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = true
                });
            }
        }
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedAdminAsync()
    {
        if (await _dbContext.AdminUsers.AnyAsync(p => p.UserName == Consts.AdminUserName))
        {
            return;
        }

        var role = await _dbContext.Roles.FirstAsync(p => p.Code == Consts.SuperAdminRoleCode);
        var password = _configuration.Get("Admin:Password") ?? "1q2w3E*";
        _dbContext.AdminUsers.Add(new AdminUser
        {
            UserName = Consts.AdminUserName,
            DisplayName = "超级管理员",
            PasswordHash = PasswordHelper.HashPassword(password),
            RoleId = role.Id,
            IsActive = true
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedDefaultPagesAsync()
    {
        if (await _dbContext.SitePages.AnyAsync(p => p.PageKey == "home"))
        {
            return;
        }

        var home = new SitePage
        {
            PageKey = "home",
            Title = "通用门户网站",
            Description = "后台可视化拖拽配置的通用 PC 门户网站",
            Keywords = "门户网站,可视化建站,拖拽配置",
            Sections = new List<SiteSection>
            {
                new SiteSection { Component = "hero", Name = "首页 Banner", Title = "通用型企业门户网站", SubTitle = "后台拖拽配置页面模块，快速生成 PC 官网。", LinkText = "进入后台", LinkUrl = "/Admin", ImagesJson = "[\"/img/default-hero.svg\"]", SettingsJson = "{\"height\":\"640px\"}", Sort = 10 },
                new SiteSection { Component = "feature-grid", Name = "核心能力", Title = "核心能力", SubTitle = "从固定门户升级为通用可配置门户", SettingsJson = "{\"columns\":3,\"items\":[{\"title\":\"分层架构\",\"description\":\"保留 Core/Data/EF/Helper/Web 分层。\",\"icon\":\"01\"},{\"title\":\"权限体系\",\"description\":\"内置菜单、角色、权限控制。\",\"icon\":\"02\"},{\"title\":\"审计日志\",\"description\":\"记录后台操作与访问轨迹。\",\"icon\":\"03\"}]}", Sort = 20 }
            }
        };
        _dbContext.SitePages.Add(home);
        await _dbContext.SaveChangesAsync();
    }
}

public interface IConfigurationLike
{
    string? Get(string key);
}
