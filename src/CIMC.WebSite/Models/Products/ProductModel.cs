using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    public class ProductModel
    {
        public int Id { get; set; }

        [Display(Name = "产品名称")]
        [Required(ErrorMessage = "请输入产品名称")]
        public string ProductName { get; set; }

        [Display(Name = "分类")]
        public int CategoryId { get; set; }

        [Display(Name = "封面图")]
        public string CoverImage { get; set; }

        [Display(Name = "多图列表")]
        public string ImageList { get; set; }

        [Display(Name = "摘要")]
        public string Summary { get; set; }

        [Display(Name = "详情")]
        public string Description { get; set; }

        [Display(Name = "产品参数")]
        public string Specification { get; set; }

        [Display(Name = "产品特点")]
        public string Feature { get; set; }

        [Display(Name = "排序")]
        public int Sort { get; set; } = 0;

        [Display(Name = "是否推荐")]
        public bool IsRecommend { get; set; }

        [Display(Name = "是否上架")]
        public bool IsActive { get; set; } = true;

        public int ViewCount { get; set; }

        public System.DateTime? CreationTime { get; set; }

        public string CreationBy { get; set; }

        public System.DateTime? UpdateTime { get; set; }

        public string UpdateBy { get; set; }
    }

    public class ProductCategoryModel
    {
        public int Id { get; set; }

        public int Pid { get; set; }

        [Display(Name = "分类名称")]
        [Required(ErrorMessage = "请输入分类名称")]
        public string Name { get; set; }

        [Display(Name = "排序")]
        public int Sort { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
