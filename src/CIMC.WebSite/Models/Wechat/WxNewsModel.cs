using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载微信相关数据。</summary>
    public class WxNewsModel
    {
        /// <summary>主键。</summary>
        public long Id { get; set; }
        /// <summary>
        /// 封面
        /// </summary>

        public string CoverUrl { get; set; }
        /// <summary>
        /// 文章标题集合
        /// </summary>

        public string Introduction { get; set; }
        /// <summary>
        /// MediaId
        /// </summary>

        public string MediaId { get; set; }

        /// <summary>是否已软删除。</summary>
        public bool IsDelete { get; set; }


        /// <summary>文章集合。</summary>
        public virtual List<WxArticleModel> Articles { get; set; } = new List<WxArticleModel>();
    }
}
