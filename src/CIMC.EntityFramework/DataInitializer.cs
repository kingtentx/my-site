using CIMC.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CIMC.Data
{
    /// <summary>
    /// 应用基础数据初始化。
    ///
    /// 只负责系统运行必须的数据：
    /// 1. 超级管理员；
    /// 2. 后台系统/网站/内容管理菜单；
    /// 3. 站点基础配置。
    ///
    /// 不再初始化文章、产品、招聘、素材、旧 WebsiteFooter、旧前台导航、
    /// 旧数组格式页面、演示角色和演示权限。业务数据由后台人工维护。
    /// </summary>
    public class DataInitializer
    {
        public void Create(AppDbContext context)
        {
            InitUser(context);
            InitMenus(context);
            InitSiteConfig(context);
        }

        private static void InitUser(AppDbContext context)
        {
            const string superAdmin = "admin";
            var admin = context.Admin.FirstOrDefault(p => p.UserName == superAdmin);
            if (admin != null)
            {
                return;
            }

            context.Admin.Add(new Admin
            {
                UserName = superAdmin,
                RealName = "超级管理员",
                Password = ToMd5("123qwe"),
                IsAdmin = true,
                IsActive = true
            });
            context.SaveChanges();
        }

        private static void InitMenus(AppDbContext context)
        {
            var home = EnsureMenu(
                context, "首页", "/admin/main", "layui-icon-home",
                2, 0, true, null, null, 0);

            var system = EnsureMenu(
                context, "系统管理", null, "layui-icon-set",
                1, 0, false, "System", null, 90);

            EnsureMenu(context, "角色管理", "/role/index", "layui-icon-user", 2, system.Id, false,
                "System_Role", "Add,Edit,Delete,Authorize", 91);
            EnsureMenu(context, "管理员", "/manager/index", "layui-icon-username", 2, system.Id, false,
                "System_Admin", "Add,Edit,Delete", 92);
            EnsureMenu(context, "菜单管理", "/menu/index", "layui-icon-align-left", 2, system.Id, false,
                "System_Menu", "Add,Edit,Delete", 93);
            EnsureMenu(context, "审计日志", "/auditlog/index", "layui-icon-survey", 2, system.Id, false,
                "System_AuditLog", "View,Delete", 95);

            var website = EnsureMenu(
                context, "网站管理", null, "layui-icon-website",
                1, 0, false, "Site", null, 10);

            EnsureMenu(context, "站点设置", "/siteconfig/index", "layui-icon-set", 2, website.Id, false,
                "Site_Info", "Edit", 11);
            EnsureMenu(context, "页面管理", "/page/index", "layui-icon-template", 2, website.Id, false,
                "Website_Page", "Add,Edit,Delete,Design,Publish", 12);
            EnsureMenu(context, "全局区域设计", "/globalregion/index", "layui-icon-component", 2, website.Id, false,
                "Website_Page", "Design,Publish", 13);
            EnsureMenu(context, "菜单管理", "/navigation/index", "layui-icon-nav", 2, website.Id, false,
                "Site_Navigation", "Add,Edit,Delete", 14);

            var content = EnsureMenu(
                context, "内容管理", null, "layui-icon-read",
                1, 0, false, "Content", null, 30);

            EnsureMenu(context, "新闻管理", "/article/index", "layui-icon-list", 2, content.Id, false,
                "Content_Article", "Add,Edit,Delete", 31);
            EnsureMenu(context, "产品分类", "/productcategory/index", "layui-icon-cols", 2, content.Id, false,
                "Content_ProductCategory", "Add,Edit,Delete", 32);
            EnsureMenu(context, "产品管理", "/product/index", "layui-icon-component", 2, content.Id, false,
                "Content_Product", "Add,Edit,Delete", 33);
            EnsureMenu(context, "招聘管理", "/job/index", "layui-icon-friends", 2, content.Id, false,
                "Content_Job", "Add,Edit,Delete", 34);
            EnsureMenu(context, "素材管理", "/images/index", "layui-icon-picture", 2, content.Id, false,
                "Content_Images", "Add,Edit,Delete", 35);

            // 明确清除 Site Builder 重写前已经废弃的后台菜单。
            var obsoleteMenus = context.Menu
                .Where(p => p.PermissionKey == "Site_Footer"
                            || p.Path == "/footer/index"
                            || (p.Pid == website.Id && (p.Title == "页脚设置" || p.Title == "导航管理")))
                .ToList();
            if (obsoleteMenus.Count > 0)
            {
                context.Menu.RemoveRange(obsoleteMenus);
            }

            var obsoletePermissions = context.RoleMenu
                .Where(p => p.Permission == "Site_Footer"
                            || (p.Permission != null && p.Permission.StartsWith("Site_Footer_")))
                .ToList();
            if (obsoletePermissions.Count > 0)
            {
                context.RoleMenu.RemoveRange(obsoletePermissions);
            }

            context.SaveChanges();
        }

        private static Menu EnsureMenu(
            AppDbContext context,
            string title,
            string path,
            string icon,
            int menuType,
            int pid,
            bool spread,
            string permissionKey,
            string buttons,
            int sort)
        {
            Menu menu = null;

            // 页面菜单以 Path 作为最稳定的唯一标识；模块菜单以 PermissionKey 标识。
            if (!string.IsNullOrWhiteSpace(path))
            {
                menu = context.Menu.FirstOrDefault(p => p.Path == path);
            }
            else if (!string.IsNullOrWhiteSpace(permissionKey))
            {
                menu = context.Menu.FirstOrDefault(p => p.Pid == 0 && p.PermissionKey == permissionKey);
            }

            if (menu == null)
            {
                menu = new Menu
                {
                    CreationBy = "system",
                    CreationTime = DateTime.Now
                };
                context.Menu.Add(menu);
            }

            menu.Title = title;
            menu.Path = path;
            menu.Icon = icon;
            menu.MenuType = menuType;
            menu.Pid = pid;
            menu.Spread = spread;
            menu.PermissionKey = permissionKey;
            menu.Buttons = buttons;
            menu.Sort = sort;
            menu.IsShow = true;
            menu.IsDelete = false;
            menu.UpdateBy = "system";
            menu.UpdateTime = DateTime.Now;

            // 父菜单 ID 需要立即生成，后续子菜单才能使用。
            context.SaveChanges();
            return menu;
        }

        private static void InitSiteConfig(AppDbContext context)
        {
            if (context.WebsiteSiteConfig.Any(p => p.Id == 1))
            {
                return;
            }

            context.WebsiteSiteConfig.Add(new WebsiteSiteConfig
            {
                Id = 1,
                SiteName = "企业官网",
                BrowserTitle = "企业官网",
                Keywords = string.Empty,
                Description = string.Empty,
                Theme = "default",
                Language = "zh-CN",
                IsActive = true,
                IsDelete = false,
                CreationBy = "system",
                CreationTime = DateTime.Now
            });
            context.SaveChanges();
        }

        private static string ToMd5(string value)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
