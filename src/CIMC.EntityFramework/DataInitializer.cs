using CIMC.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace CIMC.Data
{
    /// <summary>
    /// 初始化数据
    /// </summary>
    public class DataInitializer
    {
        public void Create(AppDbContext context)
        {
            InitUser(context);
            InitMenu(context);
        }

        private void InitUser(AppDbContext context)
        {
            const string SuperAdmin = "admin";
            var system_user = context.Admin.FirstOrDefault(p => p.UserName.Equals(SuperAdmin));
            if (system_user == null)
            {
                var admin = new Admin
                {
                    UserName = SuperAdmin,
                    RealName = "超级管理员",
                    Password = ToMD5("123qwe"),
                    IsAdmin = true,
                    IsActive = true
                };
                context.Admin.Add(admin);
                context.SaveChanges();
            }
        }

        private void InitMenu(AppDbContext context)
        {
            var exists = context.Menu.FirstOrDefault(p => p.Title.Equals(MenuSeedData.MainMenu.Title));
            if (exists == null)
            {
                var main = MenuSeedData.MainMenu;
                context.Menu.Add(main);
                context.SaveChanges();

                var system = MenuSeedData.SystemMenu;
                context.Menu.Add(system);
                context.SaveChanges();

                var subMenus = MenuSeedData.GetSubMenus(system.Id);
                context.Menu.AddRange(subMenus);
                context.SaveChanges();
            }

            InitSiteMenus(context);
            InitWebsiteBuilderMenus(context);
        }

        private void InitSiteMenus(AppDbContext context)
        {
            var content = EnsureMenu(context, "内容管理", "Content", 0, "", "layui-icon-read", 1, 30);
            EnsureMenu(context, "新闻管理", "Content_Article", content.Id, "/article/index", "layui-icon-list", 2, 31, "Add,Edit,Delete");
            EnsureMenu(context, "素材管理", "Content_Images", content.Id, "/images/index", "layui-icon-picture", 2, 35, "Add,Edit,Delete");
        }

        private void InitWebsiteBuilderMenus(AppDbContext context)
        {
            var website = EnsureMenu(context, "企业建站", "WebsiteBuilder", 0, "", "layui-icon-website", 1, 40);

            EnsureMenu(context, "页面管理", "WebsiteBuilder_Pages", website.Id, "/WebsiteBuilder/Pages", "layui-icon-template-1", 2, 41, "Add,Edit,Delete,Copy,Design,Publish,Preview");
            EnsureMenu(context, "页面装修", "WebsiteBuilder_Designer", website.Id, "/WebsiteBuilder/Pages", "layui-icon-layouts", 2, 42, "Design,Save,Publish,Preview");
            EnsureMenu(context, "站点设置", "WebsiteBuilder_Site", website.Id, "/WebsiteBuilder/Site", "layui-icon-set", 2, 43, "Edit,Save");
            EnsureMenu(context, "内容管理", "WebsiteBuilder_Contents", website.Id, "/WebsiteBuilder/Contents", "layui-icon-read", 2, 44, "Add,Edit,Delete,Publish,Offline");
            EnsureMenu(context, "分类管理", "WebsiteBuilder_Categories", website.Id, "/WebsiteBuilder/Categories", "layui-icon-tabs", 2, 45, "Add,Edit,Delete");
            EnsureMenu(context, "导航管理", "WebsiteBuilder_Navigation", website.Id, "/WebsiteBuilder/Navigation", "layui-icon-link", 2, 46, "Add,Edit,Delete,Sort");
            EnsureMenu(context, "Banner管理", "WebsiteBuilder_Banners", website.Id, "/WebsiteBuilder/Banners", "layui-icon-carousel", 2, 47, "Add,Edit,Delete,Enable,Disable");
            EnsureMenu(context, "页脚设置", "WebsiteBuilder_Footer", website.Id, "/WebsiteBuilder/Footer", "layui-icon-about", 2, 48, "Edit,Save");
            EnsureMenu(context, "素材管理", "WebsiteBuilder_Materials", website.Id, "/WebsiteBuilder/Materials", "layui-icon-picture", 2, 49, "Upload,Delete");
            EnsureMenu(context, "简历投递", "WebsiteBuilder_Applications", website.Id, "/WebsiteBuilder/Applications", "layui-icon-list", 2, 50, "View,Handle");
        }

        private Menu EnsureMenu(AppDbContext context, string title, string permissionKey, int pid, string path, string icon, int menuType, int sort, string buttons = null)
        {
            var menu = context.Menu.FirstOrDefault(p => p.PermissionKey == permissionKey || (p.Title == title && p.Pid == pid));
            if (menu == null)
            {
                menu = new Menu
                {
                    Title = title,
                    PermissionKey = permissionKey,
                    Pid = pid,
                    Path = path,
                    Icon = icon,
                    Buttons = buttons,
                    MenuType = menuType,
                    IsShow = true,
                    Spread = false,
                    Sort = sort,
                    CreationTime = DateTime.Now,
                    CreationBy = "system"
                };
                context.Menu.Add(menu);
                context.SaveChanges();
            }
            else
            {
                menu.Path = path;
                menu.Icon = icon;
                menu.Buttons = buttons;
                menu.MenuType = menuType;
                menu.IsShow = true;
                menu.Sort = sort;
                context.SaveChanges();
            }

            return menu;
        }

        private string ToMD5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes_out = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            string result = BitConverter.ToString(bytes_out).Replace("-", "");
            return result;
        }
    }
}