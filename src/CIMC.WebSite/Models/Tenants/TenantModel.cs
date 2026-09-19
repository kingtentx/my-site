namespace MySite.Web.Models
{
    /// <summary>承载租户相关数据。</summary>
    public class TenantModel
    {
        /// <summary>租户关联记录的主键。</summary>
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

        /// <summary>用户名。</summary>
        public string UserName { get; set; }

        /// <summary>登录密码。</summary>
        public string Password { get; set; }

    }
}
