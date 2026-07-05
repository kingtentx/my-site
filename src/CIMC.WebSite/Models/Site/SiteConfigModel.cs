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

        [Display(Name = "ICP备案号")]
        public string IcpNo { get; set; }

        [Display(Name = "公安备案号")]
        public string PoliceNo { get; set; }

        [Display(Name = "联系电话")]
        public string Phone { get; set; }

        [Display(Name = "联系邮箱")]
        public string Email { get; set; }

        [Display(Name = "公司地址")]
        public string Address { get; set; }

        [Display(Name = "版权信息")]
        public string Copyright { get; set; }

        [Display(Name = "默认主题")]
        public string Theme { get; set; }

        [Display(Name = "默认语言")]
        public string Language { get; set; }

        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;
    }
}
