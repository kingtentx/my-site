using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载模块相关数据。</summary>
    public class ModuleModel
    {
        /// <summary>
        /// 名称
        /// </summary>       
        public string Name { get; set; }
        /// <summary>
        /// 权限代码
        /// </summary>   
        public string PermissionKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsChecked { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public List<MenuDto> Menus { get; set; }
    }

    /// <summary>承载菜单相关数据。</summary>
    public class MenuDto
    {
        /// <summary>
        /// 名称
        /// </summary>       
        public string Name { get; set; }
        /// <summary>
        /// Path
        /// </summary>       
        public string Path { get; set; }
        /// <summary>
        /// 权限代码
        /// </summary>   
        public string PermissionKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsChecked { get; set; } = false;

        /// <summary>菜单按钮权限配置。</summary>
        public List<ButtonDto> Buttons { get; set; }
    }

    /// <summary>承载Button相关数据。</summary>
    public class ButtonDto
    {
        /// <summary>
        /// 名称
        /// </summary>       
        public string Name { get; set; }
        /// <summary>
        /// 权限代码
        /// </summary>   
        public string PermissionKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsChecked { get; set; } = false;

    }
}
