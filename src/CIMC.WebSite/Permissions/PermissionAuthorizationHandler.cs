using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CimcSite.Web
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
    {
        private IPermissionService _permission;

        public PermissionAuthorizationHandler(IPermissionService permission)
        {
            _permission = permission;
        }

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
