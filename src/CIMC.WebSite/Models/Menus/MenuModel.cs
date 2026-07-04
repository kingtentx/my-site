using System.Collections.Generic;

namespace MySite.Web.Models
{
    public class MenuModel
    {
        /// <summary>
        /// ID
        /// </summary>      
        public int Id { get; set; }
        /// <summary>
        /// 菜单标题
        /// </summary>     
        public string Title { get; set; }
        /// <summary>
        /// 路径
        /// </summary>     
        public string Path { get; set; }
        /// <summary>
        /// 图标
        /// </summary>      
        public string Icon { get; set; }
        /// <summary>
        /// 菜单类型 1-模块 2-菜单
        /// </summary>      
        public int MenuType { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public int Pid { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>      
        public bool Spread { get; set; } = false;
        /// <summary>
        /// 权限代码
        /// </summary>       
        public string PermissionKey { get; set; }
        /// <summary>
        /// 按钮集合
        /// </summary>     
        public string Buttons { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual List<string> PermissionKeys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual Dictionary<string, string> PermissionTypes { get; set; }
    }
}
