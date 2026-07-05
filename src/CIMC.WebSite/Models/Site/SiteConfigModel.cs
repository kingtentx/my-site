using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    public class SiteConfigModel
    {
        public int Id { get; set; } = 1;

        [Display(Name = "站点名称")]
        [Required(ErrorMessage = "请输入站点名称")]
        public string SiteName { get; set; }

        [Display(Name = "站点Logo")]
        public string Logo { get; set; }

        [Display(Name = "浏览器标题")]
        public string BrowserTitle { get; set; }

        [Display(Name = "关键词")]
        public string Keywords { get; set; }

        [Display(Name = "网站描述")]
        public string Description { get; set; }

        [Display(Name = "默认主题")]
        public string Theme { get; set; }

        [Display(Name = "默认语言")]
        public string Language { get; set; }

        [Display(Name = "顶部导航背景色")]
        public string HeaderBgColor { get; set; } = "#ffffff";

        [Display(Name = "顶部导航文字色")]
        public string HeaderTextColor { get; set; } = "#333333";

        [Display(Name = "顶部导航高亮色")]
        public string HeaderActiveColor { get; set; } = "#1e9fff";

        [Display(Name = "顶部导航固定")]
        public bool HeaderFixedTop { get; set; } = false;

        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;
    }
}