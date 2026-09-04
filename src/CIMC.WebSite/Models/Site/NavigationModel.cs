using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>
    /// WebsitePage 导航投影。导航数据不再使用独立 WebsiteNavigation 表。
    /// </summary>
    public class NavigationModel
    {
        public int Id { get; set; }
        public int Pid { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Target { get; set; }
        public int Sort { get; set; }
        public bool IsShow { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public bool IsCurrent { get; set; }
        public List<NavigationModel> Children { get; set; } = new List<NavigationModel>();
    }
}
