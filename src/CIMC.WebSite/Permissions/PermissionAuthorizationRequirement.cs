using Microsoft.AspNetCore.Authorization;

namespace MySite.Web
{
    public class PermissionAuthorizationRequirement : IAuthorizationRequirement
    {
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
