using System.Collections.Generic;

namespace MySite.Web
{
    /// <summary>
    /// 模块
    /// </summary>
    public class ModuleInfo
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public MenuType MenuType => MenuType.Module;

        public List<MenuInfo> Menus { get; set; }
    }

    /// <summary>
    /// 菜单
    /// </summary>
    public class MenuInfo
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public MenuType MenuType => MenuType.Menu;

        public string Url { get; set; }

        public List<ButtonInfo> Buttons { get; set; }
    }

    /// <summary>
    /// 按钮
    /// </summary>
    public class ButtonInfo
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public MenuType MenuType => MenuType.Button;
    }
}
