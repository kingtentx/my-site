using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    public class NavigationModel
    {
        public int Id { get; set; }

        public int Pid { get; set; }

        [Display(Name = "标题")]
        [Required(ErrorMessage = "请输入标题")]
        public string Title { get; set; }

        [Display(Name = "路径")]
        public string Path { get; set; }

        [Display(Name = "图标")]
        public string Icon { get; set; }

        [Display(Name = "跳转方式")]
        public int Target { get; set; } = 0;

        [Display(Name = "排序")]
        public int Sort { get; set; } = 0;

        [Display(Name = "是否显示")]
        public bool IsShow { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}
