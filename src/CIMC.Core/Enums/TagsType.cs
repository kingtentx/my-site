using System.ComponentModel;

namespace CIMC.Core.Enums
{
    /// <summary>内容分类对应的业务类型。</summary>
    public enum TagType
    {
        /// <summary>文章分类。</summary>
        [Description("文章")]
        Article = 1,
        /// <summary>产品分类。</summary>
        [Description("产品")]
        Product = 2,
        /// <summary>招聘分类。</summary>
        [Description("招聘")]
        Job = 3,
    }
}
