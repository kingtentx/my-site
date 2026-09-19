namespace MySite.Web.Models
{
    /// <summary>承载登录相关数据。</summary>
    public class LoginAdminModel
    {
        /// <summary>用户主键。</summary>
        public int UserId { get; set; } = 0;
        /// <summary>
        /// 管理员名称
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Roles { get; set; } = string.Empty;

    }
}
