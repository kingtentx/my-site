
using System.Collections.Generic;

namespace MySite.Web.Models
{
    public class RoleModel
    {
        /// <summary>
        /// 角色ID
        /// </summary>     
        public int Id { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>       
        public string RoleName { get; set; }
        /// <summary>
        /// 角色类型 0-管理员 1-商家
        /// </summary>
        public int RoleType { get; set; }
        /// <summary>
        /// 描述
        /// </summary>       
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }

        public List<ModuleModel> MenuList { get; set; }

        public string[] PermissionList { get; set; } = new string[] { };
    }
}
