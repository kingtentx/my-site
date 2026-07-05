using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    public class FooterModel
    {
        public int Id { get; set; } = 1;

        [Display(Name = "Logo")]
        public string Logo { get; set; }

        [Display(Name = "公司名称")]
        public string CompanyName { get; set; }

        [Display(Name = "公司简介")]
        public string Intro { get; set; }

        [Display(Name = "联系电话")]
        public string Phone { get; set; }

        [Display(Name = "联系邮箱")]
        public string Email { get; set; }

        [Display(Name = "公司地址")]
        public string Address { get; set; }

        [Display(Name = "二维码")]
        public string Qrcode { get; set; }

        [Display(Name = "ICP备案号")]
        public string IcpNo { get; set; }

        [Display(Name = "公安备案号")]
        public string PoliceNo { get; set; }

        [Display(Name = "版权信息")]
        public string Copyright { get; set; }

        [Display(Name = "友情链接")]
        public string FriendLinks { get; set; } = "[]";

        [Display(Name = "背景颜色")]
        public string BgColor { get; set; } = "#2c3e50";

        [Display(Name = "文字颜色")]
        public string TextColor { get; set; } = "#ffffff";

        public bool IsActive { get; set; } = true;
    }
}
