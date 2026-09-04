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

        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;
    }
}
