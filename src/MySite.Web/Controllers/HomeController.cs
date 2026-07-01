using CIMC.WebSite.Services;
using Microsoft.AspNetCore.Mvc;

namespace MySite.Web.Controllers;

public class HomeController : Controller
{
    private readonly ISiteBuilderService _siteBuilderService;

    public HomeController(ISiteBuilderService siteBuilderService)
    {
        _siteBuilderService = siteBuilderService;
    }

    public async Task<IActionResult> Index(string? slug = null)
    {
        var pageKey = string.IsNullOrWhiteSpace(slug) ? "home" : slug.Trim().Trim('/');
        var page = await _siteBuilderService.GetPageAsync(pageKey);
        return View(page);
    }

    public IActionResult Error()
    {
        return View();
    }
}
