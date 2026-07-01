namespace CimcSite.Web.Models
{
    /// <summary>
    /// 站点信息
    /// </summary>
    public class FooterModel
    {
        /// <summary>
        /// 公司名称
        /// </summary>     
        public string CompanyInfo { get; set; }
        /// <summary>
        /// QR code
        /// </summary>
        public string ImageA { get; set; }
        /// <summary>
        /// QR code
        /// </summary>
        public string ImageB { get; set; }
        /// <summary>
        /// 
        /// </summary>      
        public string Phone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>      
        public string Email { get; set; }
        /// <summary>
        /// 地址
        /// </summary>      
        public string Address { get; set; }
        /// <summary>
        /// 备案号
        /// </summary>       
        public string RecordNo { get; set; }
        /// <summary>
        /// 版权
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// 公司简介-EN
        /// </summary>
        public string CompanyInfo_EN { get; set; }
        /// <summary>
        /// 地址-EN
        /// </summary>
        public string Address_EN { get; set; }
        /// <summary>
        /// 版权-EN
        /// </summary>
        public string Copyright_EN { get; set; }
    }
}
