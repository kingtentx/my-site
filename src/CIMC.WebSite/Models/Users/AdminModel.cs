using System;
using System.Collections.Generic;

namespace CimcSite.Web.Models
{
    public class AdminModel
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string RealName { get; set; }

        public string Remark { get; set; }

        public string[] Roles { get; set; } = new string[] { };

        public bool IsAdmin { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>     
        public string CreationBy { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>       
        public string UpdateBy { get; set; }

        public DateTime? LastLoginDate { get; set; }

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
