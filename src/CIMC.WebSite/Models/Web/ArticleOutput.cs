namespace MySite.Web.Models
{
    /// <summary>承载面向网站输出的文章数据。</summary>
    public class ArticleOutput
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>      
        public string Title { get; set; }
        /// <summary>
        /// 描述
        /// </summary>       
        public string Description { get; set; }
        /// <summary>
        /// 作者
        /// </summary>       
        public string Author { get; set; }

        /// <summary>
        /// 图片
        /// </summary>

        public string ImageUrl { get; set; }


        /// <summary>创建时间。</summary>
        public string CreationTime { get; set; }


    }
}
