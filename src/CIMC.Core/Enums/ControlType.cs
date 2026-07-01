using System.ComponentModel;

namespace CIMC.Core.Enums
{
    /// <summary>
    /// 控件类型
    /// </summary>
    public enum ControlType
    {
        /// <summary>
        /// 导航跟踪
        /// </summary>
        [Description("导航跟踪")]
        Track = 0,
        /// <summary>
        /// 标题
        /// </summary>
        [Description("标题")]
        Title = 1,
        /// <summary>
        /// 内容
        /// </summary>
        [Description("内容")]
        Content = 2,
        /// <summary>
        /// Banner
        /// </summary>     
        [Description("Banner")]
        Banner = 3,
        /// <summary>
        /// 图片
        /// </summary>
        [Description("图片")]
        Image = 4,
        /// <summary>
        /// 图片列表
        /// </summary>
        [Description("图片列表")]
        Album = 5,
        /// <summary>
        /// 文章列表
        /// </summary>
        [Description("文章列表")]
        Article = 6,
        /// <summary>
        /// 文字列表
        /// </summary>
        [Description("文字列表")]
        Job = 7,
        /// <summary>
        ///单图文
        /// </summary>
        [Description("单图文")]
        SimpleImgtxt = 8,
        /// <summary>
        /// 多图文
        /// </summary>
        [Description("多图文")]
        Imgtxt = 9,
        /// <summary>
        /// 图片墙
        /// </summary>
        [Description("图片墙")]
        Imgwall = 10,
        /// <summary>
        /// 联系我们
        /// </summary>
        [Description("联系我们")]
        Company = 11,
    }
}
