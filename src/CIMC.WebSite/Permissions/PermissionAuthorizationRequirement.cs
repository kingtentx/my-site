using Microsoft.AspNetCore.Authorization;

namespace MySite.Web
{
    /// <summary>表示当前请求所需的菜单操作权限。</summary>
    public class PermissionAuthorizationRequirement : IAuthorizationRequirement
    {
        /// <summary>初始化权限。</summary>
        public PermissionAuthorizationRequirement(string name, PermissionType type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// 权限名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 按钮权限
        /// </summary>
        public PermissionType Type { get; set; }
    }
}
