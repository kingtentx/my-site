using CIMC.Core.Enums;
using CIMC.WebSite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CIMC.WebSite.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionFilterAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _menuCode;
    private readonly PermissionType _permissionType;

    public PermissionFilterAttribute(string menuCode, PermissionType permissionType = PermissionType.View)
    {
        _menuCode = menuCode;
        _permissionType = permissionType;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var service = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        if (!await service.HasPermissionAsync(context.HttpContext.User, _menuCode, _permissionType))
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                context.Result = new JsonResult(new { message = "没有权限" }) { StatusCode = StatusCodes.Status403Forbidden };
            }
            else
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
