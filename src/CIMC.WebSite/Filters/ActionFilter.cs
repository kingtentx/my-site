using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Diagnostics;
using System.Security.Claims;

namespace MySite.Web.Filters
{
    public class ActionFilter : IActionFilter
    {

        /// <summary>
        /// 请求参数
        /// </summary>
        private string ActionArguments { get; set; }
        /// <summary>
        /// 请求体中的所有值
        /// </summary>
        private string RequestBody { get; set; }

        private Stopwatch Stopwatch { get; set; }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //// 后续添加了获取请求的请求体，如果在实际项目中不需要删除即可
            //long contentLen = context.HttpContext.Request.ContentLength == null ? 0 : context.HttpContext.Request.ContentLength.Value;
            //if (contentLen > 0)
            //{
            //    // 读取请求体中所有内容
            //    if (context.HttpContext.Request.Method == "POST")
            //    {
            //        using (System.IO.Stream stream = context.HttpContext.Request.Body)
            //        {
            //            byte[] buffer = new byte[context.HttpContext.Request.ContentLength.Value];
            //            stream.Read(buffer, 0, buffer.Length);
            //            RequestBody = System.Text.Encoding.UTF8.GetString(buffer).Trim();
            //        }
            //    }
            //}

            ActionArguments = Newtonsoft.Json.JsonConvert.SerializeObject(context.ActionArguments);
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

            Stopwatch.Stop();
            string url = context.HttpContext.Request.Host + context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            string method = context.HttpContext.Request.Method;
            string pamras = method == "POST" ? $"Arguments：{ActionArguments}" : "";

            //dynamic result = context.Result.GetType().Name == "EmptyResult" ? new { Value = "无返回结果" } : context.Result as dynamic;
            //string res = "";
            //if (result != null)
            //{
            //    res = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value);
            //}

            //登录用户
            var identity = (ClaimsIdentity)context.HttpContext.User.Identity;
            var user = identity.FindFirst(ClaimTypes.Name) != null ? identity.FindFirst(ClaimTypes.Name).Value : "";

            Log.Information($"times：{Stopwatch.Elapsed.TotalMilliseconds} ms,{url} -- {method} -- user: {user} {pamras}");

        }
    }
}
