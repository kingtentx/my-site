using System.Collections.Generic;

namespace CimcSite.Web.Models
{

    /// <summary>
    /// 文章详情
    /// </summary>
    public class ArticleInfoOutput
    {
        /// <summary>
        /// 文章详情
        /// </summary>
        public ArticleModel Article { get; set; }
        /// <summary>
        /// 最新文章
        /// </summary>
        public List<ArticleOutput> LatestArticle { get; set; }
        /// <summary>
        /// 热门文章
        /// </summary>
        public List<ArticleOutput> HotArticle { get; set; }
    }

}
