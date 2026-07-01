namespace CimcSite.Web.Models
{
    public class TenantModel
    {
        public int TenantId { get; set; }
        /// <summary>
        /// 商户名称
        /// </summary>    
        public string TenantName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>     
        public string Contacts { get; set; }
        /// <summary>
        /// 电话
        /// </summary>        
        public string Telephone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>       
        public string Email { get; set; }
        /// <summary>
        /// 备注
        /// </summary>      
        public string Remarks { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

    }
}
