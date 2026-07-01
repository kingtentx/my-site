using CIMC.Core.Enums;
using CIMC.WebSite.Filters;
using CIMC.WebSite.Models;
using CIMC.WebSite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MySite.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/site-builder")]
[PermissionFilter("SiteBuilder", PermissionType.View)]
public class SiteBuilderController : ControllerBase
{
    private readonly ISiteBuilderService _siteBuilderService;

    public SiteBuilderController(ISiteBuilderService siteBuilderService)
    {
        _siteBuilderService = siteBuilderService;
    }

    [HttpGet("templates")]
    public IActionResult Templates()
    {
        return Ok(_siteBuilderService.GetTemplates());
    }

    [HttpGet("page/{pageKey}")]
    public async Task<IActionResult> Page(string pageKey)
    {
        return Ok(await _siteBuilderService.GetPageAsync(pageKey));
    }

    [HttpPost("page")]
    [PermissionFilter("SiteBuilder", PermissionType.Edit)]
    public async Task<IActionResult> SavePage([FromBody] SitePageDto page)
    {
        if (string.IsNullOrWhiteSpace(page.PageKey))
        {
            return BadRequest(new { message = "页面标识不能为空" });
        }

        await _siteBuilderService.SavePageAsync(page, User.Identity?.Name);
        return Ok(new { message = "页面配置保存成功" });
    }
}
