using System.Collections.Generic;

namespace MySite.Web.Models
{
    public class WxNewsModel
    {
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

        public bool IsDelete { get; set; }


        public virtual List<WxArticleModel> Articles { get; set; } = new List<WxArticleModel>();
    }
}
