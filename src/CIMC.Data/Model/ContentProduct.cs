using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 产品
    /// </summary>
    public class ContentProduct : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_250)]
        public string ProductName { get; set; }

        /// <summary>
        /// 分类 ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string CoverImage { get; set; }

        /// <summary>
        /// 多图列表 JSON（["url1","url2",...]）
        /// </summary>
        public string ImageList { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        [StringLength(ModelUnits.Len_1000)]
        public string Summary { get; set; }

        /// <summary>
        /// 详情（富文本）
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 产品参数（富文本/JSON）
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 产品特点
        /// </summary>
        public string Feature { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否推荐
        /// </summary>
        public bool IsRecommend { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 浏览量
        /// </summary>
        public int ViewCount { get; set; } = 0;

        public bool IsDelete { get; set; } = false;
    }
}
