using Microsoft.AspNetCore.Mvc;
using MySite.Web.Services;

namespace MySite.Web.Controllers;

public class HomeController : Controller
{
    private readonly ISitePageStore _store;

    public HomeController(ISitePageStore store)
    {
        _store = store;
    }

    public async Task<IActionResult> Index(string? slug = null)
    {
        var pageKey = string.IsNullOrWhiteSpace(slug) ? "home" : slug.Trim().Trim('/');
        var page = await _store.GetPageAsync(pageKey);
        return View(page);
    }

    public IActionResult Error()
    {
        return View();
    }
}
