namespace MySite.Web.Models
{
    public class ArticleOutput
    {
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


        public string CreationTime { get; set; }


    }
}
