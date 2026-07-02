using System.Diagnostics;
using CIMC.Data.Entities;
using CIMC.EntityFramework;

namespace CIMC.WebSite.Services;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public AuditLogMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
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

        if (!ShouldRecord(context, path))
        {
            return;
        }

        dbContext.AuditLogs.Add(new AuditLog
        {
            UserName = context.User.Identity?.Name,
            Action = ResolveOperation(context, path),
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

    private bool ShouldRecord(HttpContext context, string path)
    {
        if (!_configuration.GetValue<bool>("AuditLog:IsEnabled"))
        {
            return false;
        }

        if (!(path.StartsWith("/Admin") || path.StartsWith("/Account") || path.StartsWith("/Menu") || path.StartsWith("/Role") || path.StartsWith("/AuditLog") || path.StartsWith("/api")))
        {
            return false;
        }

        var operations = _configuration.GetSection("AuditLog:RecordOperations").Get<string[]>() ?? Array.Empty<string>();
        if (operations.Length == 0)
        {
            return true;
        }

        var operation = ResolveOperation(context, path);
        return operations.Contains(operation, StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolveOperation(HttpContext context, string path)
    {
        if (path.Contains("Login", StringComparison.OrdinalIgnoreCase)) return "Login";
        if (path.Contains("Upload", StringComparison.OrdinalIgnoreCase)) return "Upload";
        if (path.Contains("Role", StringComparison.OrdinalIgnoreCase) || path.Contains("Permission", StringComparison.OrdinalIgnoreCase)) return "Authorize";
        if (HttpMethods.IsPost(context.Request.Method)) return "Edit";
        if (HttpMethods.IsDelete(context.Request.Method)) return "Delete";
        return context.Request.Method;
    }
}
