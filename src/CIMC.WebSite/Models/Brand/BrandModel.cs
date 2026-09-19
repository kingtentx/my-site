namespace MySite.Web.Models
{
    /// <summary>承载品牌相关数据。</summary>
    public class BrandModel
    {
        /// <summary>
        /// 品牌ID
        /// </summary>     
        public long Id { get; set; }
        /// <summary>
        /// 品牌名称
        /// </summary>      
        public string BrandName { get; set; }
        /// <summary>
        /// 
        /// </summary>     
        public string BrandLogo { get; set; }
    }
}
