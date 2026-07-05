using CIMC.Data;
using CIMC.EntityFramework;
using MySite.Web.Config;
using MySite.Web.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MySite.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuditLogFilter : Attribute, IAsyncActionFilter
    {
        private static readonly string[] IgnoreActions = new[]
        {
            "GetMenuData", "GetImg", "ReLogin", "Main", "Index",
            "ImageSelector", "Error"
        };

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var controllerName = context.ActionDescriptor.RouteValues["controller"];

            if (IgnoreActions.Contains(actionName))
            {
                await next();
                return;
            }

            var isLoginAction = actionName.IndexOf("Login", StringComparison.OrdinalIgnoreCase) >= 0;
            var isReadAction = IsReadAction(actionName, context.HttpContext.Request.Method);

            var stopwatch = Stopwatch.StartNew();

            string oldData = null;
            var httpMethod = context.HttpContext.Request.Method;
            var operationType = GetOperationType(httpMethod, actionName);
            var requestUrl = $"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";

            var auditConfig = context.HttpContext.RequestServices.GetService<IOptions<AuditLogConfig>>()?.Value;
            if (auditConfig != null && !auditConfig.ShouldRecord(operationType))
            {
                await next();
                return;
            }

            string requestData = null;
            try
            {
                if (context.ActionArguments != null && context.ActionArguments.Count > 0)
                {
                    requestData = Newtonsoft.Json.JsonConvert.SerializeObject(context.ActionArguments);
                }
            }
            catch { }

            if (!isReadAction && !isLoginAction && (operationType == "Edit" || operationType == "Delete"))
            {
                oldData = await CaptureOldDataAsync(context, controllerName, actionName);
            }

            var executedContext = await next();

            stopwatch.Stop();

            string newData = null;
            string resultStatus = "Success";
            string resultMessage = null;

            if (executedContext.Exception != null && !executedContext.ExceptionHandled)
            {
                resultStatus = "Fail";
                resultMessage = executedContext.Exception.Message?.Substring(0, Math.Min(executedContext.Exception.Message.Length, 500));
            }
            else if (executedContext.Result is Microsoft.AspNetCore.Mvc.JsonResult jsonResult && jsonResult.Value != null)
            {
                try
                {
                    var resultObj = jsonResult.Value;
                    var codeProp = resultObj.GetType().GetProperty("Code");
                    if (codeProp != null)
                    {
                        var codeVal = (int)codeProp.GetValue(resultObj);
                        if (codeVal != 200)
                        {
                            resultStatus = "Fail";
                            var msgProp = resultObj.GetType().GetProperty("Message");
                            if (msgProp != null)
                                resultMessage = msgProp.GetValue(resultObj)?.ToString();
                        }
                    }
                    newData = Newtonsoft.Json.JsonConvert.SerializeObject(resultObj);
                    if (newData?.Length > 2000) newData = newData.Substring(0, 2000);
                }
                catch { }
            }

            var identity = context.HttpContext.User.Identity as ClaimsIdentity;
            var userId = identity?.FindFirst(ClaimTypes.Sid)?.Value ?? "";
            var userName = identity?.FindFirst(ClaimTypes.Name)?.Value ?? "";

            if (isLoginAction && resultStatus == "Success")
            {
                try
                {
                    if (requestData != null)
                    {
                        var loginObj = Newtonsoft.Json.Linq.JObject.Parse(requestData);
                        var inputVal = loginObj?.First?.First;
                        if (inputVal != null)
                        {
                            var unProp = inputVal.SelectToken("UserName");
                            if (unProp != null) userName = unProp.ToString();
                        }
                    }
                }
                catch { }

                identity = context.HttpContext.User.Identity as ClaimsIdentity;
                userId = identity?.FindFirst(ClaimTypes.Sid)?.Value ?? userId;
                userName = identity?.FindFirst(ClaimTypes.Name)?.Value ?? userName;
            }

            var ip = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
                ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            var userAgent = context.HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "";

            var auditLogService = context.HttpContext.RequestServices.GetRequiredService<IAuditLogService>();

            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                IpAddress = ip,
                OperationType = operationType,
                OperationModule = controllerName,
                OperationDesc = BuildOperationDesc(operationType, controllerName, actionName),
                RequestUrl = requestUrl,
                HttpMethod = httpMethod,
                RequestData = Truncate(requestData, 8000),
                OldData = Truncate(oldData, 8000),
                NewData = Truncate(newData, 8000),
                ResultStatus = resultStatus,
                ResultMessage = resultMessage,
                OperationTime = DateTime.Now,
                Duration = stopwatch.ElapsedMilliseconds,
                UserAgent = Truncate(userAgent, 500),
                OperationTable = GetOperationTable(controllerName),
                RecordId = GetRecordId(context, actionName)
            };

            _ = auditLogService.LogAsync(log);
        }

        private static bool IsReadAction(string actionName, string httpMethod)
        {
            if (httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                return actionName.IndexOf("Get", StringComparison.OrdinalIgnoreCase) >= 0
                    || actionName.IndexOf("List", StringComparison.OrdinalIgnoreCase) >= 0
                    || actionName.IndexOf("Query", StringComparison.OrdinalIgnoreCase) >= 0
                    || actionName.IndexOf("Detail", StringComparison.OrdinalIgnoreCase) >= 0
                    || actionName.IndexOf("Info", StringComparison.OrdinalIgnoreCase) >= 0
                    || actionName.IndexOf("Export", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }

        private static async Task<string> CaptureOldDataAsync(ActionExecutingContext context, string controllerName, string actionName)
        {
            try
            {
                var id = ExtractIdFromArgs(context, actionName);
                if (id == null || Convert.ToInt32(id) == 0) return null;

                var entityType = GetEntityType(controllerName);
                if (entityType == null) return null;

                var repositoryType = typeof(IRepository<>).MakeGenericType(entityType);
                var serviceProvider = context.HttpContext.RequestServices;
                var repository = serviceProvider.GetService(repositoryType);
                if (repository == null) return null;

                var getOneMethod = repositoryType.GetMethod("GetOne", new[] { typeof(int) });
                if (getOneMethod == null) return null;

                var entity = getOneMethod.Invoke(repository, new object[] { Convert.ToInt32(id) });
                if (entity == null) return null;

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(entity);
                if (json.Length > 4000) json = json.Substring(0, 4000);
                return json;
            }
            catch
            {
                return null;
            }
        }

        private static object ExtractIdFromArgs(ActionExecutingContext context, string actionName)
        {
            if (context.ActionArguments.TryGetValue("id", out var idObj))
                return idObj;

            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null) continue;
                var idProp = arg.GetType().GetProperty("Id");
                if (idProp != null)
                    return idProp.GetValue(arg);
            }

            return null;
        }

        private static Type GetEntityType(string controllerName)
        {
            return controllerName switch
            {
                "Manager" => typeof(Admin),
                "Role" => typeof(Role),
                "Menu" => typeof(Menu),
                "Article" => typeof(Article),
                "Images" => typeof(Images),
                "Page" => typeof(WebsitePage),
                "SiteConfig" => typeof(WebsiteSiteConfig),
                "Navigation" => typeof(WebsiteNavigation),
                "Footer" => typeof(WebsiteFooter),
                "Product" => typeof(ContentProduct),
                "ProductCategory" => typeof(ContentProductCategory),
                "Job" => typeof(ContentJob),
                _ => null
            };
        }

        private static string GetOperationTable(string controllerName)
        {
            return controllerName switch
            {
                "Manager" => "Admin",
                "Role" => "Role",
                "Menu" => "Menu",
                "Article" => "Article",
                "Images" => "Images",
                "Page" => "WebsitePage",
                "SiteConfig" => "WebsiteSiteConfig",
                "Navigation" => "WebsiteNavigation",
                "Footer" => "WebsiteFooter",
                "Product" => "ContentProduct",
                "ProductCategory" => "ContentProductCategory",
                "Job" => "ContentJob",
                _ => controllerName
            };
        }

        private static string GetRecordId(ActionExecutingContext context, string actionName)
        {
            var id = ExtractIdFromArgs(context, actionName);
            return id?.ToString();
        }

        private static string BuildOperationDesc(string operationType, string controllerName, string actionName)
        {
            var moduleName = controllerName switch
            {
                "Manager" => "管理员",
                "Role" => "角色",
                "Menu" => "菜单",
                "Article" => "文章",
                "Images" => "素材",
                "Page" => "页面",
                "SiteConfig" => "站点配置",
                "Navigation" => "导航",
                "Footer" => "页脚",
                "Product" => "产品",
                "ProductCategory" => "产品分类",
                "Job" => "招聘",
                "Admin" => "系统设置",
                "Authorize" => "认证",
                "Upload" => "文件上传",
                _ => controllerName
            };

            var actionDesc = operationType switch
            {
                "Login" => "登录系统",
                "Logout" => "退出系统",
                "View" => $"查看{moduleName}",
                "Add" => $"新增{moduleName}",
                "Edit" => $"编辑{moduleName}",
                "Delete" => $"删除{moduleName}",
                "Authorize" => $"变更{moduleName}权限",
                "Upload" => $"上传文件",
                "Publish" => $"发布{moduleName}",
                "Design" => $"装修{moduleName}",
                _ => $"{operationType}{moduleName}"
            };

            return actionDesc;
        }

        private static string GetOperationType(string httpMethod, string actionName)
        {
            if (actionName.IndexOf("Login", StringComparison.OrdinalIgnoreCase) >= 0) return "Login";
            if (actionName.IndexOf("Logout", StringComparison.OrdinalIgnoreCase) >= 0) return "Logout";
            if (actionName.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0) return "Delete";
            if (actionName.IndexOf("Authorize", StringComparison.OrdinalIgnoreCase) >= 0
                || actionName.IndexOf("SaveRoleMenu", StringComparison.OrdinalIgnoreCase) >= 0) return "Authorize";
            if (actionName.IndexOf("Upload", StringComparison.OrdinalIgnoreCase) >= 0) return "Upload";
            if (actionName.IndexOf("Export", StringComparison.OrdinalIgnoreCase) >= 0) return "Export";
            if (actionName.IndexOf("Publish", StringComparison.OrdinalIgnoreCase) >= 0) return "Publish";

            if (httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                if (actionName.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0) return "Edit";
                if (actionName.IndexOf("Add", StringComparison.OrdinalIgnoreCase) >= 0) return "Add";
                if (actionName.IndexOf("SetHot", StringComparison.OrdinalIgnoreCase) >= 0) return "Edit";
                if (actionName.IndexOf("SetRecommend", StringComparison.OrdinalIgnoreCase) >= 0) return "Edit";
                if (actionName.IndexOf("SetHome", StringComparison.OrdinalIgnoreCase) >= 0) return "Edit";
                if (actionName.IndexOf("UpdatePassword", StringComparison.OrdinalIgnoreCase) >= 0) return "Edit";
                if (actionName.IndexOf("SaveDraft", StringComparison.OrdinalIgnoreCase) >= 0
                    || actionName.IndexOf("Save", StringComparison.OrdinalIgnoreCase) >= 0) return "Edit";
                return "Edit";
            }

            if (actionName.IndexOf("Get", StringComparison.OrdinalIgnoreCase) >= 0
                || actionName.IndexOf("List", StringComparison.OrdinalIgnoreCase) >= 0
                || actionName.IndexOf("Detail", StringComparison.OrdinalIgnoreCase) >= 0
                || actionName.IndexOf("Info", StringComparison.OrdinalIgnoreCase) >= 0
                || actionName.IndexOf("Design", StringComparison.OrdinalIgnoreCase) >= 0
                || actionName.IndexOf("Preview", StringComparison.OrdinalIgnoreCase) >= 0)
                return "View";

            return "View";
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
