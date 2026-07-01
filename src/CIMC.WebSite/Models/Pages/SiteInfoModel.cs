namespace CimcSite.Web.Models
{
    /// <summary>
    /// 站点信息
    /// </summary>
    public class SiteInfoModel
    {
        /// <summary>
        /// 公司名称
        /// </summary>     
        public string CompanyName { get; set; }
        /// <summary>
        /// SEO-Keywords
        /// </summary>      
        public string Keywords { get; set; }
        /// <summary>
        /// SEO-Description
        /// </summary>     
        public string Description { get; set; }
        /// <summary>
        /// Logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// Logo
        /// </summary>
        public string Logo_H5 { get; set; }

        /// <summary>
        /// 公司名称-EN
        /// </summary>
        public string CompanyName_EN { get; set; }
        /// <summary>
        /// SEO-Keywords-EN
        /// </summary>
        public string Keywords_EN { get; set; }
        /// <summary>
        /// SEO-Description-EN
        /// </summary>
        public string Description_EN { get; set; }
    }
}
