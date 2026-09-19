namespace MySite.Web.Models
{
    /// <summary>承载物流相关数据。</summary>
    public class LogisticsCompanyModel
    {
        /// <summary>
        /// ID
        /// </summary>      
        public int LogisticsId { get; set; }
        /// <summary>
        /// 物流公司
        /// </summary>    
        public string LogisticsCompanyName { get; set; }
        /// <summary>
        /// 物流公司代码
        /// </summary>       
        public string LogisticsCompanyCode { get; set; }

        /// <summary>
        /// 别名
        /// </summary>   

        public string AliasName { get; set; }

        /// <summary>排序值，数值越小越靠前。</summary>
        public int Sort { get; set; }
    }
}
