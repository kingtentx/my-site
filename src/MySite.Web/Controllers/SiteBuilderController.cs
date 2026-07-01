using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Models;
using MySite.Web.Services;

namespace MySite.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/site-builder")]
public class SiteBuilderController : ControllerBase
{
    private readonly ISitePageStore _store;

    public SiteBuilderController(ISitePageStore store)
    {
        _store = store;
    }

    [HttpGet("templates")]
    public IActionResult Templates()
    {
        return Ok(_store.GetTemplates());
    }

    [HttpGet("page/{pageKey}")]
    public async Task<IActionResult> Page(string pageKey)
    {
        return Ok(await _store.GetPageAsync(pageKey));
    }

    [HttpPost("page")]
    public async Task<IActionResult> SavePage([FromBody] SitePage page)
    {
        if (string.IsNullOrWhiteSpace(page.PageKey))
        {
            return BadRequest(new { message = "页面标识不能为空" });
        }

        await _store.SavePageAsync(page);
        return Ok(new { message = "页面配置保存成功" });
    }
}
