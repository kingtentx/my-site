using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MySite.Web
{
    /// <summary>处理权限相关的请求筛选逻辑。</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PermissionFilter : Attribute, IAsyncAuthorizationFilter
    {
        /// <summary>初始化权限。</summary>
        public PermissionFilter(string name, PermissionType type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>名称。</summary>
        public string Name { get; set; }
        /// <summary>业务类型。</summary>
        public PermissionType Type { get; set; }

        /// <summary>异步检查请求的访问权限。</summary>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var authorizationResult = await authorizationService.AuthorizeAsync(context.HttpContext.User, null, new PermissionAuthorizationRequirement(Name, Type));
            if (!authorizationResult.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
