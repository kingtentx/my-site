using CIMC.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        }

        private void InitSiteMenus(AppDbContext context)
        {
            var content = EnsureMenu(context, "内容管理", "Content", 0, "", "layui-icon-read", 1, 30);
            EnsureMenu(context, "新闻管理", "Content_Article", content.Id, "/article/index", "layui-icon-list", 2, 31, "Add,Edit,Delete");
            EnsureMenu(context, "素材管理", "Content_Images", content.Id, "/images/index", "layui-icon-picture", 2, 35, "Add,Edit,Delete");
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
