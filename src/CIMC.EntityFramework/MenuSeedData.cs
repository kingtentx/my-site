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
    }
}
