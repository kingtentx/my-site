using System.ComponentModel.DataAnnotations;

namespace CimcSite.Web.Models
{
    public class AdminInputModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        public string Password { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        [Required]
        public string ValidateKey { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        [Required]
        public string ValidateCode { get; set; }


    }
}
