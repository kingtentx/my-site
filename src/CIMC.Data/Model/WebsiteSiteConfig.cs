using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 站点基础配置（单例，Id=1）。
    /// Header/Footer 的布局、颜色、定位与语言入口统一在全局区域设计中维护。
    /// </summary>
    public class WebsiteSiteConfig : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        /// <summary>主键。</summary>
        [Key]
        public int Id { get; set; } = 1;

        /// <summary>WebsiteSite的名称。</summary>
        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string SiteName { get; set; }

        /// <summary>站点 Logo 地址。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string Logo { get; set; }

        /// <summary>浏览器标题。</summary>
        [StringLength(ModelUnits.Len_250)]
        public string BrowserTitle { get; set; }

        /// <summary>站点关键词。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string Keywords { get; set; }

        /// <summary>描述。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string Description { get; set; }

        /// <summary>
        /// 整站是否启用。该状态属于站点级配置，不属于 Header。
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>是否已软删除。</summary>
        public bool IsDelete { get; set; } = false;
    }
}
