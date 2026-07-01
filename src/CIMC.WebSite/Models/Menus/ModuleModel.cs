using System.Collections.Generic;

namespace CimcSite.Web.Models
{
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

        public List<ButtonDto> Buttons { get; set; }
    }

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
