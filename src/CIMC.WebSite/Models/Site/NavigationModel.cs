using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>
    /// WebsitePage 导航投影。导航数据不再使用独立 WebsiteNavigation 表。
    /// </summary>
    public class NavigationModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }
        /// <summary>父级记录的主键。</summary>
        public int Pid { get; set; }
        /// <summary>标题。</summary>
        public string Title { get; set; }
        /// <summary>访问路径。</summary>
        public string Path { get; set; }
        /// <summary>图标。</summary>
        public string Icon { get; set; }
        /// <summary>链接打开目标。</summary>
        public int Target { get; set; }
        /// <summary>排序值，数值越小越靠前。</summary>
        public int Sort { get; set; }
        /// <summary>是否满足 Show 条件。</summary>
        public bool IsShow { get; set; } = true;
        /// <summary>是否启用。</summary>
        public bool IsActive { get; set; } = true;
        /// <summary>是否满足 Current 条件。</summary>
        public bool IsCurrent { get; set; }
        /// <summary>子节点集合。</summary>
        public List<NavigationModel> Children { get; set; } = new List<NavigationModel>();
    }
}
