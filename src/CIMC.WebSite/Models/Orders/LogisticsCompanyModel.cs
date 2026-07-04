namespace MySite.Web.Models
{
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

        public int Sort { get; set; }
    }
}
