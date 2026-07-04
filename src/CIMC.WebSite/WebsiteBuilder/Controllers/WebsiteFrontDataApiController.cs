using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySite.Web.WebsiteBuilder.Data;
using MySite.Web.WebsiteBuilder.Models;

namespace MySite.Web.WebsiteBuilder.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/front")]
    public class WebsiteFrontDataApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteFrontDataApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet("navigation")]
        public IActionResult Navigation()
        {
            var items = _db.Navigations.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsEnabled)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Id)
                .Select(x => new { x.Id, x.ParentId, x.Title, x.LinkType, x.LinkUrl, x.Target, x.Sort })
                .ToList();
            return Ok(items);
        }

        [HttpGet("banners")]
        public IActionResult Banners()
        {
            var now = DateTime.Now;
            var items = _db.Banners.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsEnabled)
                .Where(x => x.BeginTime == null || x.BeginTime <= now)
                .Where(x => x.EndTime == null || x.EndTime >= now)
                .OrderBy(x => x.Sort)
                .ThenByDescending(x => x.Id)
                .Take(10)
                .ToList();
            return Ok(items);
        }

        [HttpGet("footer")]
        public IActionResult Footer()
        {
            var item = _db.Footers.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault();
            return Ok(item ?? new WebsiteFooter { CompanyName = "企业官网", FriendLinksJson = "[]", BackgroundColor = "#111827", TextColor = "#ffffff" });
        }
    }
}