using System.Collections.Generic;

namespace CimcSite.Web.Models
{
    public class MenuTreeModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>
        public bool Spread { get; set; } = false;
        /// <summary>
        /// 权限代码
        /// </summary>
        public string PermissionKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string[] Buttons { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<MenuTreeModel> Children { get; set; }
    }

    /// <summary>
    /// layui tree控制
    /// </summary>
    public class TreeSelectModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<TreeSelectModel> Children { get; set; }
    }

}
