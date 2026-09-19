using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Threading.Tasks;

namespace MySite.Web.Filters
{
    /// <summary>处理异常相关的请求筛选逻辑。</summary>
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        /// <summary>异步处理请求中的异常。</summary>
        public Task OnExceptionAsync(ExceptionContext context)
        {
            Log.Error(context.Exception.Message);

            return Task.CompletedTask;
        }
    }
}
