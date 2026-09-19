using System.Collections.Generic;

namespace MySite.Web
{
    /// <summary>
    /// 模块
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>标识键。</summary>
        public string Key { get; set; }

        /// <summary>名称。</summary>
        public string Name { get; set; }

        /// <summary>模块的类型。</summary>
        public MenuType MenuType => MenuType.Module;

        /// <summary>子菜单集合。</summary>
        public List<MenuInfo> Menus { get; set; }
    }

    /// <summary>
    /// 菜单
    /// </summary>
    public class MenuInfo
    {
        /// <summary>标识键。</summary>
        public string Key { get; set; }

        /// <summary>名称。</summary>
        public string Name { get; set; }

        /// <summary>菜单的类型。</summary>
        public MenuType MenuType => MenuType.Menu;

        /// <summary>访问地址。</summary>
        public string Url { get; set; }

        /// <summary>菜单按钮权限配置。</summary>
        public List<ButtonInfo> Buttons { get; set; }
    }

    /// <summary>
    /// 按钮
    /// </summary>
    public class ButtonInfo
    {
        /// <summary>标识键。</summary>
        public string Key { get; set; }

        /// <summary>名称。</summary>
        public string Name { get; set; }

        /// <summary>ButtonInfo的类型。</summary>
        public MenuType MenuType => MenuType.Button;
    }
}
