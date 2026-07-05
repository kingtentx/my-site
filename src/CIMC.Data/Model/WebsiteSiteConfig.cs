using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 站点配置（单例，Id=1）
    /// </summary>
    public class WebsiteSiteConfig : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        [Key]
        public int Id { get; set; } = 1;

        /// <summary>
        /// 站点名称
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string SiteName { get; set; }

        /// <summary>
        /// 站点 Logo
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Logo { get; set; }

        /// <summary>
        /// 浏览器标题
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string BrowserTitle { get; set; }

        /// <summary>
        /// 网站关键词
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Keywords { get; set; }

        /// <summary>
        /// 网站描述
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Description { get; set; }

        /// <summary>
        /// ICP 备案号
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string IcpNo { get; set; }

        /// <summary>
        /// 公安备案号
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string PoliceNo { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string Phone { get; set; }

        /// <summary>
        /// 联系邮箱
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string Email { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Address { get; set; }

        /// <summary>
        /// 版权信息
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Copyright { get; set; }

        /// <summary>
        /// 默认主题
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string Theme { get; set; }

        /// <summary>
        /// 默认语言
        /// </summary>
        [StringLength(ModelUnits.Len_20)]
        public string Language { get; set; }

        /// <summary>
        /// 顶部导航背景色
        /// </summary>
        [StringLength(ModelUnits.Len_20)]
        public string HeaderBgColor { get; set; } = "#ffffff";

        /// <summary>
        /// 顶部导航文字色
        /// </summary>
        [StringLength(ModelUnits.Len_20)]
        public string HeaderTextColor { get; set; } = "#333333";

        /// <summary>
        /// 顶部导航高亮色
        /// </summary>
        [StringLength(ModelUnits.Len_20)]
        public string HeaderActiveColor { get; set; } = "#1e9fff";

        /// <summary>
        /// 顶部导航是否固定在顶部
        /// </summary>
        public bool HeaderFixedTop { get; set; } = false;

        /// <summary>
        /// 网站状态：true=启用，false=停用
        /// </summary>
        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;
    }
}