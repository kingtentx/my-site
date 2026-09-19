using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    /// <summary>承载站点配置相关数据。</summary>
    public class SiteConfigModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; } = 1;

        /// <summary>站点配置的名称。</summary>
        [Display(Name = "站点名称")]
        [Required(ErrorMessage = "请输入站点名称")]
        public string SiteName { get; set; }

        /// <summary>站点 Logo 地址。</summary>
        [Display(Name = "站点Logo")]
        public string Logo { get; set; }

        /// <summary>浏览器标题。</summary>
        [Display(Name = "浏览器标题")]
        public string BrowserTitle { get; set; }

        /// <summary>站点关键词。</summary>
        [Display(Name = "关键词")]
        public string Keywords { get; set; }

        /// <summary>描述。</summary>
        [Display(Name = "网站描述")]
        public string Description { get; set; }

        /// <summary>是否启用。</summary>
        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;
    }
}
