
using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载角色相关数据。</summary>
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

        /// <summary>角色相关项目集合。</summary>
        public List<ModuleModel> MenuList { get; set; }

        /// <summary>角色相关项目集合。</summary>
        public string[] PermissionList { get; set; } = new string[] { };
    }
}
