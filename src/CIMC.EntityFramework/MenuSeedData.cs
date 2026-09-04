using System.Collections.Generic;

namespace CIMC.Data
{
    public static class MenuSeedData
    {
        public static Menu MainMenu => new Menu
        {
            Pid = 0,
            Title = "首页",
            Icon = "layui-icon-home",
            Path = "/admin/main",
            MenuType = 2,
            Spread = true,
            IsShow = true,
            Sort = 0
        };

        public static Menu SystemMenu => new Menu
        {
            Pid = 0,
            Title = "系统管理",
            Icon = "layui-icon-set",
            PermissionKey = "System",
            MenuType = 1,
            Spread = false,
            IsShow = true,
            Sort = 90
        };

        public static List<Menu> GetSubMenus(int systemMenuId)
        {
            return new List<Menu>
            {
                new Menu { Pid = systemMenuId, Title = "角色管理", Icon = "layui-icon-user", Path = "/role/index", PermissionKey = "System_Role", Buttons = "Add,Edit,Delete,Authorize", MenuType = 2, Spread = false, IsShow = true, Sort = 91 },
                new Menu { Pid = systemMenuId, Title = "管理员", Icon = "layui-icon-username", Path = "/manager/index", PermissionKey = "System_Admin", Buttons = "Add,Edit,Delete", MenuType = 2, Spread = false, IsShow = true, Sort = 92 },
                new Menu { Pid = systemMenuId, Title = "菜单管理", Icon = "layui-icon-align-left", Path = "/menu/index", PermissionKey = "System_Menu", Buttons = "Add,Edit,Delete", MenuType = 2, Spread = false, IsShow = true, Sort = 93 },
                new Menu { Pid = systemMenuId, Title = "审计日志", Icon = "layui-icon-survey", Path = "/auditlog/index", PermissionKey = "System_AuditLog", Buttons = "View,Delete", MenuType = 2, Spread = false, IsShow = true, Sort = 95 }
            };
        }

        public static Menu WebsiteMenu => new Menu
        {
            Pid = 0,
            Title = "网站管理",
            Icon = "layui-icon-website",
            PermissionKey = "Site",
            MenuType = 1,
            Spread = false,
            IsShow = true,
            Sort = 10
        };

        public static List<Menu> GetWebsiteMenus(int websiteMenuId)
        {
            return new List<Menu>
            {
                new Menu { Pid = websiteMenuId, Title = "站点设置", Icon = "layui-icon-set", Path = "/siteconfig/index", PermissionKey = "Site_Info", Buttons = "Edit", MenuType = 2, Spread = false, IsShow = true, Sort = 11 },
                new Menu { Pid = websiteMenuId, Title = "页面管理", Icon = "layui-icon-template", Path = "/page/index", PermissionKey = "Website_Page", Buttons = "Add,Edit,Delete,Design,Publish", MenuType = 2, Spread = false, IsShow = true, Sort = 12 },
                // GlobalRegionController 复用 Website_Page + Design 权限。
                new Menu { Pid = websiteMenuId, Title = "全局区域设计", Icon = "layui-icon-component", Path = "/globalregion/index", PermissionKey = "Website_Page", Buttons = "Design,Publish", MenuType = 2, Spread = false, IsShow = true, Sort = 13 },
                new Menu { Pid = websiteMenuId, Title = "菜单管理", Icon = "layui-icon-nav", Path = "/navigation/index", PermissionKey = "Site_Navigation", Buttons = "Add,Edit,Delete", MenuType = 2, Spread = false, IsShow = true, Sort = 14 }
            };
        }
    }
}
