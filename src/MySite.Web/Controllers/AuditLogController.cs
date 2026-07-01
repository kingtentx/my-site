using CIMC.Core.Enums;
using CIMC.EntityFramework;
using CIMC.WebSite.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MySite.Web.Controllers;

[Authorize]
[PermissionFilter("AuditLog", PermissionType.View)]
public class AuditLogController : Controller
{
    private readonly AppDbContext _dbContext;

    public AuditLogController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(page, 1);
        const int pageSize = 50;
        var query = _dbContext.AuditLogs.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreationTime);
        ViewBag.Total = await query.CountAsync();
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return View(logs);
    }
}
