using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MySite.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PermissionFilter : Attribute, IAsyncAuthorizationFilter
    {
        public PermissionFilter(string name, PermissionType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public PermissionType Type { get; set; }

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
