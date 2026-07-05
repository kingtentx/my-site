using System;
using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 网站页面
    /// </summary>
    public class WebsitePage : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 站点 ID（预留多站点）
        /// </summary>
        public int SiteId { get; set; } = 1;

        /// <summary>
        /// 页面名称
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string PageName { get; set; }

        /// <summary>
        /// 页面编码
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string PageCode { get; set; }

        /// <summary>
        /// 页面路径（唯一，如 / /about /news）
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_200)]
        public string PagePath { get; set; }

        /// <summary>
        /// 浏览器标题（SEO）
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string PageTitle { get; set; }

        /// <summary>
        /// SEO 关键词
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string SeoKeywords { get; set; }

        /// <summary>
        /// SEO 描述
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string SeoDescription { get; set; }

        /// <summary>
        /// 布局配置 JSON
        /// </summary>
        public string LayoutJson { get; set; }

        /// <summary>
        /// 组件配置 JSON（草稿，冗余便于设计器快速加载）
        /// </summary>
        public string ComponentJson { get; set; }

        /// <summary>
        /// 状态：0=草稿，1=已发布
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 是否首页
        /// </summary>
        public bool IsHome { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        public bool IsDelete { get; set; } = false;
    }
}
