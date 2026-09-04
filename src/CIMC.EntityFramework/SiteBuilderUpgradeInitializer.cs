using CIMC.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CIMC.Data
{
    /// <summary>
    /// Site Builder 重写版启动清理/升级。
    ///
    /// 旧 DataInitializer 仍承担管理员、文章、产品等基础数据初始化，
    /// 本类在其执行完成后移除旧版页面构建器数据并修正新版后台菜单。
    /// 清理逻辑只删除明确的旧版数组页面和 system 默认导航，不会删除新版 Builder 页面或用户新建菜单。
    /// </summary>
    public class SiteBuilderUpgradeInitializer
    {
        private static readonly HashSet<string> LegacyNavigationKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "首页|/",
            "关于我们|/about",
            "产品中心|/products",
            "新闻中心|/news",
            "招聘中心|/jobs",
            "联系我们|/contact"
        };

        public void Apply(AppDbContext context)
        {
            CleanupLegacyBuilderPages(context);
            CleanupLegacyFooter(context);
            CleanupLegacyNavigationSeed(context);
            UpgradeAdminMenus(context);
            context.SaveChanges();
        }

        private static void CleanupLegacyBuilderPages(AppDbContext context)
        {
            // 新版 BuilderDocument 必须是 JSON object；旧版页面是 ComponentModel[] 数组。
            var legacyPages = context.WebsitePage
                .ToList()
                .Where(p => IsLegacyArrayJson(p.ComponentJson))
                .ToList();

            if (legacyPages.Count == 0)
            {
                return;
            }

            var pageIds = legacyPages.Select(p => p.Id).ToList();
            var versions = context.WebsitePageVersion
                .Where(p => pageIds.Contains(p.PageId))
                .ToList();

            if (versions.Count > 0)
            {
                context.WebsitePageVersion.RemoveRange(versions);
            }

            context.WebsitePage.RemoveRange(legacyPages);
            context.SaveChanges();
        }

        private static void CleanupLegacyFooter(AppDbContext context)
        {
            // WebsiteFooter 已被全局 Footer Builder 取代，不再保留旧单例配置。
            var footers = context.WebsiteFooter.ToList();
            if (footers.Count > 0)
            {
                context.WebsiteFooter.RemoveRange(footers);
                context.SaveChanges();
            }
        }

        private static void CleanupLegacyNavigationSeed(AppDbContext context)
        {
            // 只删除旧 DataInitializer 自动生成的六条 system 导航；用户在“菜单管理”新建的数据不受影响。
            var seeded = context.WebsiteNavigation
                .Where(p => p.CreationBy == "system")
                .ToList()
                .Where(p => LegacyNavigationKeys.Contains($"{p.Title}|{p.Path}"))
                .ToList();

            if (seeded.Count > 0)
            {
                context.WebsiteNavigation.RemoveRange(seeded);
                context.SaveChanges();
            }
        }

        private static void UpgradeAdminMenus(AppDbContext context)
        {
            var website = context.Menu
                .Where(p => p.Pid == 0 && p.PermissionKey == "Site")
                .OrderBy(p => p.Id)
                .FirstOrDefault();

            if (website == null)
            {
                website = new Menu
                {
                    Pid = 0,
                    Title = "网站管理",
                    Icon = "layui-icon-website",
                    PermissionKey = "Site",
                    MenuType = 1,
                    Spread = false,
                    IsShow = true,
                    IsDelete = false,
                    Sort = 10,
                    CreationBy = "system",
                    CreationTime = DateTime.Now
                };
                context.Menu.Add(website);
                context.SaveChanges();
            }
            else
            {
                website.Title = "网站管理";
                website.Icon = "layui-icon-website";
                website.MenuType = 1;
                website.Spread = false;
                website.IsShow = true;
                website.IsDelete = false;
                website.Sort = 10;
                website.UpdateBy = "system";
                website.UpdateTime = DateTime.Now;
            }

            EnsureAdminMenu(context, website.Id, "站点设置", "/siteconfig/index", "layui-icon-set", "Site_Info", "Edit", 11);
            EnsureAdminMenu(context, website.Id, "页面管理", "/page/index", "layui-icon-template", "Website_Page", "Add,Edit,Delete,Design,Publish", 12);
            EnsureGlobalRegionMenu(context, website.Id);
            EnsureAdminMenu(context, website.Id, "菜单管理", "/navigation/index", "layui-icon-nav", "Site_Navigation", "Add,Edit,Delete", 14);

            // 旧页脚设置已被全局 Footer Builder 取代。
            var footerMenus = context.Menu
                .Where(p => p.PermissionKey == "Site_Footer" || p.Path == "/footer/index" || (p.Pid == website.Id && p.Title == "页脚设置"))
                .ToList();
            if (footerMenus.Count > 0)
            {
                context.Menu.RemoveRange(footerMenus);
            }

            var footerPermissions = context.RoleMenu
                .Where(p => p.Permission == "Site_Footer" || p.Permission.StartsWith("Site_Footer_"))
                .ToList();
            if (footerPermissions.Count > 0)
            {
                context.RoleMenu.RemoveRange(footerPermissions);
            }
        }

        private static void EnsureGlobalRegionMenu(AppDbContext context, int websiteMenuId)
        {
            // GlobalRegionController 复用 Website_Page + Design 权限，因此该入口不新增权限代码。
            var menu = context.Menu
                .FirstOrDefault(p => p.Path == "/globalregion/index" || (p.Pid == websiteMenuId && p.Title == "全局区域设计"));

            if (menu == null)
            {
                menu = new Menu
                {
                    Pid = websiteMenuId,
                    Title = "全局区域设计",
                    Path = "/globalregion/index",
                    Icon = "layui-icon-component",
                    PermissionKey = "Website_Page",
                    Buttons = "Design,Publish",
                    MenuType = 2,
                    Spread = false,
                    IsShow = true,
                    IsDelete = false,
                    Sort = 13,
                    CreationBy = "system",
                    CreationTime = DateTime.Now
                };
                context.Menu.Add(menu);
                return;
            }

            menu.Pid = websiteMenuId;
            menu.Title = "全局区域设计";
            menu.Path = "/globalregion/index";
            menu.Icon = "layui-icon-component";
            menu.PermissionKey = "Website_Page";
            menu.Buttons = "Design,Publish";
            menu.MenuType = 2;
            menu.Spread = false;
            menu.IsShow = true;
            menu.IsDelete = false;
            menu.Sort = 13;
            menu.UpdateBy = "system";
            menu.UpdateTime = DateTime.Now;
        }

        private static void EnsureAdminMenu(
            AppDbContext context,
            int websiteMenuId,
            string title,
            string path,
            string icon,
            string permissionKey,
            string buttons,
            int sort)
        {
            var menu = context.Menu
                .Where(p => p.PermissionKey == permissionKey && p.Path != "/globalregion/index")
                .OrderBy(p => p.Id)
                .FirstOrDefault();

            if (menu == null)
            {
                menu = new Menu
                {
                    Pid = websiteMenuId,
                    Title = title,
                    Path = path,
                    Icon = icon,
                    PermissionKey = permissionKey,
                    Buttons = buttons,
                    MenuType = 2,
                    Spread = false,
                    IsShow = true,
                    IsDelete = false,
                    Sort = sort,
                    CreationBy = "system",
                    CreationTime = DateTime.Now
                };
                context.Menu.Add(menu);
                return;
            }

            menu.Pid = websiteMenuId;
            menu.Title = title;
            menu.Path = path;
            menu.Icon = icon;
            menu.Buttons = buttons;
            menu.MenuType = 2;
            menu.Spread = false;
            menu.IsShow = true;
            menu.IsDelete = false;
            menu.Sort = sort;
            menu.UpdateBy = "system";
            menu.UpdateTime = DateTime.Now;
        }

        private static bool IsLegacyArrayJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            return json.TrimStart().StartsWith("[", StringComparison.Ordinal);
        }
    }
}
