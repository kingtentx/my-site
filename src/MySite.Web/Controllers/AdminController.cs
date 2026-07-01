using CIMC.Core.Enums;
using CIMC.WebSite.Filters;
using CIMC.WebSite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MySite.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ISiteBuilderService _siteBuilderService;
    private readonly IPermissionService _permissionService;

    public AdminController(ISiteBuilderService siteBuilderService, IPermissionService permissionService)
    {
        _siteBuilderService = siteBuilderService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Menus = await _permissionService.GetUserMenusAsync(User);
        var pages = await _siteBuilderService.GetPagesAsync();
        return View(pages);
    }

    [PermissionFilter("SiteBuilder", PermissionType.View)]
    public IActionResult Designer(string pageKey = "home")
    {
        ViewBag.PageKey = string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey.Trim().Trim('/').ToLowerInvariant();
        return View();
    }
}
