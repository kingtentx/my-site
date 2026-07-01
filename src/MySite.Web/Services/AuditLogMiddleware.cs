using System.Diagnostics;
using CIMC.Data.Entities;
using CIMC.EntityFramework;

namespace CIMC.WebSite.Services;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/img") || path.StartsWith("/favicon"))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        if (path.StartsWith("/Admin") || path.StartsWith("/Account") || path.StartsWith("/Menu") || path.StartsWith("/Role") || path.StartsWith("/AuditLog") || path.StartsWith("/api"))
        {
            dbContext.AuditLogs.Add(new AuditLog
            {
                UserName = context.User.Identity?.Name,
                Action = $"{context.Request.Method} {path}",
                Method = context.Request.Method,
                Path = path,
                Ip = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                StatusCode = context.Response.StatusCode,
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                CreationTime = DateTime.Now
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
