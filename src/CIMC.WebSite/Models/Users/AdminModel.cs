using System;
using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载管理员相关数据。</summary>
    public class AdminModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }

        /// <summary>用户名。</summary>
        public string UserName { get; set; }

        /// <summary>登录密码。</summary>
        public string Password { get; set; }

        /// <summary>真实姓名。</summary>
        public string RealName { get; set; }

        /// <summary>备注信息。</summary>
        public string Remark { get; set; }

        /// <summary>用户所属角色集合。</summary>
        public string[] Roles { get; set; } = new string[] { };

        /// <summary>是否为超级管理员。</summary>
        public bool IsAdmin { get; set; }

        /// <summary>是否启用。</summary>
        public bool IsActive { get; set; }

        /// <summary>创建时间。</summary>
        public DateTime? CreationTime { get; set; }

        /// <summary>最后更新时间。</summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>     
        public string CreationBy { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>       
        public string UpdateBy { get; set; }

        /// <summary>管理员相关时间。</summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>最后登录的 IP 地址。</summary>
        public string LastLoginIP { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 角色列表
        /// </summary>
        public List<RoleModel> RoleList { get; set; }
    }
}
