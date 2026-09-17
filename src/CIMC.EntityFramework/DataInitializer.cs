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
    /// 页面树同时承担前台网站导航，不再初始化独立 WebsiteNavigation、
    /// 旧 WebsiteFooter、旧数组格式页面、文章/产品/招聘/素材演示数据和演示角色权限。
    ///
    /// 写入规则（2026-09-14 明确）：<b>种子数据只在对应数据不存在时才写入</b>。
    /// 每一步都先查存在性再决定是否插入，已存在的记录一律不改写，
    /// 因此重复启动、并发启动都不会产生重复行，也不会覆盖后台的修改。
    /// </summary>
    public class DataInitializer
    {
        public void Create(AppDbContext context)
        {
            InitUser(context);        // Admin 表无同名管理员时才创建
            InitMenus(context);       // 仅补建缺失的菜单，已存在的菜单不改写
            InitSiteConfig(context);  // 仅在站点配置不存在时创建
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
            EnsureMenu(
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
            EnsureMenu(context, "在线留言", "/message/index", "layui-icon-dialogue", 2, website.Id, false,
                "Site_Message", "Delete", 14);

            var content = EnsureMenu(
                context, "内容管理", null, "layui-icon-read",
                1, 0, false, "Content", null, 30);

            EnsureMenu(context, "新闻管理", "/article/index", "layui-icon-list", 2, content.Id, false,
                "Content_Article", "Add,Edit,Delete", 31);
            EnsureMenu(context, "产品管理", "/album/index", "layui-icon-picture", 2, content.Id, false,
                "Content_Album", "Add,Edit,Delete", 32);
            EnsureMenu(context, "招聘管理", "/job/index", "layui-icon-friends", 2, content.Id, false,
                "Content_Job", "Add,Edit,Delete", 33);
            EnsureMenu(context, "分类管理", "/tag/index", "layui-icon-tabs", 2, content.Id, false,
                "Content_Tags", "Add,Edit,Delete", 34);
            EnsureMenu(context, "素材管理", "/images/index", "layui-icon-picture", 2, content.Id, false,
                "Content_Images", "Add,Edit,Delete", 35);

            var obsoleteContentMenus = context.Menu.Where(p => p.Path == "/product/index"
                || p.Path == "/productcategory/index" || p.Path == "/contentcategory/index"
                || p.PermissionKey == "Content_Product" || p.PermissionKey == "Content_ProductCategory").ToList();
            if (obsoleteContentMenus.Count > 0) context.Menu.RemoveRange(obsoleteContentMenus);
            var obsoleteContentPermissions = context.RoleMenu.Where(p => p.Permission == "Content_Product"
                || p.Permission == "Content_ProductCategory"
                || (p.Permission != null && (p.Permission.StartsWith("Content_Product_") || p.Permission.StartsWith("Content_ProductCategory_")))).ToList();
            if (obsoleteContentPermissions.Count > 0) context.RoleMenu.RemoveRange(obsoleteContentPermissions);

            // 页面树已承担网站导航；旧导航管理和旧 Footer 设置都不再存在。
            var obsoleteMenus = context.Menu
                .Where(p => p.PermissionKey == "Site_Footer"
                            || p.PermissionKey == "Site_Navigation"
                            || p.Path == "/footer/index"
                            || p.Path == "/navigation/index"
                            || (p.Pid == website.Id && (p.Title == "页脚设置" || p.Title == "导航管理" || p.Title == "菜单管理")))
                .ToList();
            if (obsoleteMenus.Count > 0)
            {
                context.Menu.RemoveRange(obsoleteMenus);
            }

            var obsoletePermissions = context.RoleMenu
                .Where(p => p.Permission == "Site_Footer"
                            || p.Permission == "Site_Navigation"
                            || (p.Permission != null && (p.Permission.StartsWith("Site_Footer_") || p.Permission.StartsWith("Site_Navigation_"))))
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

            if (!string.IsNullOrWhiteSpace(path))
            {
                menu = context.Menu.FirstOrDefault(p => p.Path == path);
            }
            else if (!string.IsNullOrWhiteSpace(permissionKey))
            {
                menu = context.Menu.FirstOrDefault(p => p.Pid == 0 && p.PermissionKey == permissionKey);
            }
            else
            {
                // 既没有 Path 也没有 PermissionKey 就无法判重，直接跳过，避免每次启动插一条新记录。
                return null;
            }

            // 已存在则直接返回：种子数据只在「缺失」时补建。
            // 旧实现每次启动都会把 Title/Icon/Pid/Sort/IsShow 等字段重新写一遍，
            // 后台手工调整过的菜单会被静默改回去，也不符合「有数据就不写」的约定。
            if (menu != null)
            {
                return menu;
            }

            menu = new Menu
            {
                Title = title,
                Path = path,
                Icon = icon,
                MenuType = menuType,
                Pid = pid,
                Spread = spread,
                PermissionKey = permissionKey,
                Buttons = buttons,
                Sort = sort,
                IsShow = true,
                IsDelete = false,
                CreationBy = "system",
                CreationTime = DateTime.Now,
                UpdateBy = "system",
                UpdateTime = DateTime.Now
            };
            context.Menu.Add(menu);

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
