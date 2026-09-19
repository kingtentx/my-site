using System;
using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 网站页面。页面层级、排序与导航属性共同构成网站导航树，
    /// 页面装修统一存储为新版 BuilderDocument JSON。
    /// </summary>
    public class WebsitePage : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        /// <summary>主键。</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 站点 ID（预留多站点）。
        /// </summary>
        public int SiteId { get; set; } = 1;

        /// <summary>
        /// 父页面 ID，0 表示一级页面。导航按该字段递归形成层级。
        /// </summary>
        public int ParentId { get; set; } = 0;

        /// <summary>页面名称。</summary>
        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string PageName { get; set; }

        /// <summary>页面编码。</summary>
        [StringLength(ModelUnits.Len_50)]
        public string PageCode { get; set; }

        /// <summary>
        /// 页面路径（唯一，如 /、/about、/about/company）。
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_200)]
        public string PagePath { get; set; }

        /// <summary>浏览器页面标题。</summary>
        [StringLength(ModelUnits.Len_250)]
        public string PageTitle { get; set; }

        /// <summary>页面 SEO 关键词。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string SeoKeywords { get; set; }

        /// <summary>页面 SEO 描述。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string SeoDescription { get; set; }

        /// <summary>
        /// 页面是否作为网站导航节点显示。隐藏后页面仍可通过 URL 访问。
        /// </summary>
        public bool ShowInNavigation { get; set; } = true;

        /// <summary>
        /// 导航标题；为空时使用页面名称。
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string NavigationTitle { get; set; }

        /// <summary>导航图标。</summary>
        [StringLength(ModelUnits.Len_100)]
        public string NavigationIcon { get; set; }

        /// <summary>
        /// 0=本窗口，1=新窗口。
        /// </summary>
        public int NavigationTarget { get; set; } = 0;

        /// <summary>
        /// 新版 Site Builder BuilderDocument JSON（草稿）。布局也包含在该树结构中。
        /// </summary>
        public string ComponentJson { get; set; }

        /// <summary>
        /// 0=草稿，1=已发布。
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>是否为首页。</summary>
        public bool IsHome { get; set; }
        /// <summary>排序值，数值越小越靠前。</summary>
        public int Sort { get; set; } = 0;
        /// <summary>是否启用。</summary>
        public bool IsActive { get; set; } = true;
        /// <summary>发布时间。</summary>
        public DateTime? PublishTime { get; set; }
        /// <summary>是否已软删除。</summary>
        public bool IsDelete { get; set; } = false;
    }
}
