using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Services;

namespace MySite.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ISitePageStore _store;

    public AdminController(ISitePageStore store)
    {
        _store = store;
    }

    public async Task<IActionResult> Index()
    {
        var pages = await _store.GetAllPagesAsync();
        return View(pages);
    }

    public IActionResult Designer(string pageKey = "home")
    {
        ViewBag.PageKey = string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey.Trim().Trim('/').ToLowerInvariant();
        return View();
    }
}
