using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MySite.Web
{
    /// <summary>使用权限服务处理菜单操作授权要求。</summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
    {
        private IPermissionService _permission;

        /// <summary>初始化权限。</summary>
        public PermissionAuthorizationHandler(IPermissionService permission)
        {
            _permission = permission;
        }

        /// <summary>处理授权要求。</summary>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
        {
            if (context.User?.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {
                var systemClaim = identity.FindFirst(ClaimTypes.System)?.Value;
                if (!string.IsNullOrEmpty(systemClaim) && Convert.ToBoolean(systemClaim))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    var role = identity.FindFirst(ClaimTypes.Role)?.Value;
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        var code = requirement.Type == PermissionType.View ? requirement.Name : requirement.Name + "_" + requirement.Type;
                        if (await _permission.CheckPermissionAsync(role, code))
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
            }
        }
    }
}
