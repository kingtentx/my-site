using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 页脚配置（单例，Id=1）
    /// </summary>
    public class WebsiteFooter : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        [Key]
        public int Id { get; set; } = 1;

        /// <summary>
        /// Logo
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Logo { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string CompanyName { get; set; }

        /// <summary>
        /// 公司简介
        /// </summary>
        [StringLength(ModelUnits.Len_1000)]
        public string Intro { get; set; }

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
        /// 二维码图片
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Qrcode { get; set; }

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
        /// 版权信息
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Copyright { get; set; }

        /// <summary>
        /// 友情链接 JSON（[{"name":"x","url":"y"},...]）
        /// </summary>
        public string FriendLinks { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        [StringLength(ModelUnits.Len_20)]
        public string BgColor { get; set; } = "#2c3e50";

        /// <summary>
        /// 文字颜色
        /// </summary>
        [StringLength(ModelUnits.Len_20)]
        public string TextColor { get; set; } = "#ffffff";

        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;
    }
}
