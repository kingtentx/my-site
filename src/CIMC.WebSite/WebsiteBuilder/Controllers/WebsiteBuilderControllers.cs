using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySite.Web.WebsiteBuilder.Data;
using MySite.Web.WebsiteBuilder.Models;

namespace MySite.Web.WebsiteBuilder.Controllers
{
    [Authorize]
    public class WebsiteBuilderController : Controller
    {
        public IActionResult Index() => RedirectToAction(nameof(Pages));
        public IActionResult Pages() => View();
        public IActionResult Designer(int id) { ViewData["PageId"] = id; return View(); }
        public IActionResult Site() => View();
        public IActionResult Contents() => View();
    }

    [Authorize]
    [ApiController]
    [Route("api/site/config")]
    public class WebsiteSiteApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteSiteApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get()
        {
            var config = _db.SiteConfigs.OrderBy(x => x.Id).FirstOrDefault();
            return Ok(config ?? new WebsiteSiteConfig { SiteName = "企业官网", BrowserTitle = "企业官网", Theme = "default", Language = "zh-CN", IsEnabled = true });
        }

        [HttpPut]
        public IActionResult Update([FromBody] WebsiteSiteConfig model)
        {
            var config = _db.SiteConfigs.OrderBy(x => x.Id).FirstOrDefault();
            if (config == null)
            {
                model.CreateTime = DateTime.Now;
                model.UpdateTime = DateTime.Now;
                _db.SiteConfigs.Add(model);
            }
            else
            {
                config.SiteName = model.SiteName;
                config.Logo = model.Logo;
                config.BrowserTitle = model.BrowserTitle;
                config.Keywords = model.Keywords;
                config.Description = model.Description;
                config.IcpNo = model.IcpNo;
                config.PoliceNo = model.PoliceNo;
                config.Tel = model.Tel;
                config.Email = model.Email;
                config.Address = model.Address;
                config.Copyright = model.Copyright;
                config.Theme = model.Theme;
                config.Language = model.Language;
                config.IsEnabled = model.IsEnabled;
                config.UpdateTime = DateTime.Now;
            }
            _db.SaveChanges();
            return Ok(config ?? model);
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/pages")]
    public class WebsitePagesApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsitePagesApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _db.Pages.AsNoTracking().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.PageName.Contains(keyword) || x.PagePath.Contains(keyword) || x.PageCode.Contains(keyword));
            var total = query.Count();
            var items = query.OrderBy(x => x.Sort).ThenByDescending(x => x.Id).Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1)).Take(Math.Max(pageSize, 1)).ToList();
            return Ok(new { total, items });
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            var item = _db.Pages.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            return item == null ? NotFound(new { message = "页面不存在" }) : Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] WebsitePage model)
        {
            if (string.IsNullOrWhiteSpace(model.PageName) || string.IsNullOrWhiteSpace(model.PagePath)) return BadRequest(new { message = "页面名称和页面路径不能为空" });
            model.PagePath = NormalizePagePath(model.PagePath);
            if (_db.Pages.Any(x => !x.IsDeleted && x.PagePath == model.PagePath)) return BadRequest(new { message = "页面路径已存在" });
            model.PageCode = string.IsNullOrWhiteSpace(model.PageCode) ? (model.PagePath == "/" ? "home" : model.PagePath.Trim('/').Replace('/', '-')) : model.PageCode;
            model.PageTitle = string.IsNullOrWhiteSpace(model.PageTitle) ? model.PageName : model.PageTitle;
            model.LayoutJson = string.IsNullOrWhiteSpace(model.LayoutJson) ? "{\"width\":\"full\",\"theme\":\"default\"}" : model.LayoutJson;
            model.ComponentJson = string.IsNullOrWhiteSpace(model.ComponentJson) ? "[]" : model.ComponentJson;
            model.DraftJson = BuildPageJson(model);
            model.Status = (int)WebsiteContentStatus.Draft;
            model.CreateTime = DateTime.Now;
            model.UpdateTime = DateTime.Now;
            if (model.IsHome) foreach (var p in _db.Pages.Where(x => !x.IsDeleted && x.IsHome)) p.IsHome = false;
            _db.Pages.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] WebsitePage model)
        {
            var item = _db.Pages.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "页面不存在" });
            var path = NormalizePagePath(model.PagePath);
            if (_db.Pages.Any(x => !x.IsDeleted && x.Id != id && x.PagePath == path)) return BadRequest(new { message = "页面路径已存在" });
            item.PageName = model.PageName;
            item.PageCode = string.IsNullOrWhiteSpace(model.PageCode) ? item.PageCode : model.PageCode;
            item.PagePath = path;
            item.PageTitle = model.PageTitle;
            item.SeoKeywords = model.SeoKeywords;
            item.SeoDescription = model.SeoDescription;
            item.CanonicalUrl = model.CanonicalUrl;
            item.Sort = model.Sort;
            item.IsHome = model.IsHome;
            item.UpdateTime = DateTime.Now;
            if (item.IsHome) foreach (var p in _db.Pages.Where(x => !x.IsDeleted && x.Id != item.Id && x.IsHome)) p.IsHome = false;
            _db.SaveChanges();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _db.Pages.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "页面不存在" });
            if (item.IsHome) return BadRequest(new { message = "首页不能删除" });
            item.IsDeleted = true;
            item.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(new { message = "删除成功" });
        }

        [HttpPost("{id:int}/copy")]
        public IActionResult Copy(int id)
        {
            var item = _db.Pages.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "页面不存在" });
            var copy = new WebsitePage { SiteId = item.SiteId, PageName = item.PageName + " - 副本", PageCode = item.PageCode + "_copy_" + DateTime.Now.ToString("yyyyMMddHHmmss"), PagePath = NormalizePagePath(item.PagePath.TrimEnd('/') + "-copy-" + DateTime.Now.ToString("HHmmss")), PageTitle = item.PageTitle, SeoKeywords = item.SeoKeywords, SeoDescription = item.SeoDescription, LayoutJson = item.LayoutJson, ComponentJson = item.ComponentJson, DraftJson = item.DraftJson, Status = (int)WebsiteContentStatus.Draft, Sort = item.Sort + 1, CreateTime = DateTime.Now, UpdateTime = DateTime.Now };
            _db.Pages.Add(copy);
            _db.SaveChanges();
            return Ok(copy);
        }

        [HttpPut("{id:int}/design")]
        public IActionResult SaveDesign(int id, [FromBody] WebsitePageDesignRequest request)
        {
            var item = _db.Pages.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "页面不存在" });
            item.LayoutJson = string.IsNullOrWhiteSpace(request.LayoutJson) ? "{\"width\":\"full\",\"theme\":\"default\"}" : request.LayoutJson;
            item.ComponentJson = string.IsNullOrWhiteSpace(request.ComponentJson) ? "[]" : request.ComponentJson;
            item.DraftJson = string.IsNullOrWhiteSpace(request.DraftJson) ? BuildPageJson(item) : request.DraftJson;
            item.Status = (int)WebsiteContentStatus.Draft;
            item.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(new { message = "草稿保存成功", item.Id, item.UpdateTime });
        }

        [HttpPost("{id:int}/publish")]
        public IActionResult Publish(int id)
        {
            var item = _db.Pages.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "页面不存在" });
            item.DraftJson = string.IsNullOrWhiteSpace(item.DraftJson) ? BuildPageJson(item) : item.DraftJson;
            item.PublishJson = item.DraftJson;
            item.Status = (int)WebsiteContentStatus.Published;
            item.PublishTime = DateTime.Now;
            item.UpdateTime = DateTime.Now;
            var versionNo = _db.PageVersions.Count(x => x.PageId == item.Id) + 1;
            _db.PageVersions.Add(new WebsitePageVersion { PageId = item.Id, VersionNo = versionNo, DraftJson = item.DraftJson, PublishJson = item.PublishJson, Status = (int)WebsiteContentStatus.Published, CreateTime = DateTime.Now, PublishTime = DateTime.Now });
            _db.SaveChanges();
            return Ok(new { message = "发布成功", item.Id, versionNo, item.PublishTime });
        }

        private static string NormalizePagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/") return "/";
            path = path.Trim();
            if (!path.StartsWith("/")) path = "/" + path;
            return path.TrimEnd('/');
        }
        private static string BuildPageJson(WebsitePage page)
        {
            var layout = string.IsNullOrWhiteSpace(page.LayoutJson) ? "{\"width\":\"full\",\"theme\":\"default\"}" : page.LayoutJson;
            var components = string.IsNullOrWhiteSpace(page.ComponentJson) ? "[]" : page.ComponentJson;
            return $$"""{ "pageId": {{page.Id}}, "pageName": "{{EscapeJson(page.PageName)}}", "pagePath": "{{EscapeJson(page.PagePath)}}", "layout": {{layout}}, "components": {{components}} }""";
        }
        private static string EscapeJson(string value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    [AllowAnonymous]
    public class WebsiteFrontController : Controller
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteFrontController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet("website")]
        [HttpGet("website/{*path}")]
        public IActionResult Page(string path)
        {
            var pagePath = NormalizePath(path);
            var page = _db.Pages.AsNoTracking().FirstOrDefault(x => !x.IsDeleted && x.Status == (int)WebsiteContentStatus.Published && x.PagePath == pagePath);
            if (page == null) return NotFound("页面未发布或不存在");
            return RenderPage(page, false);
        }

        [HttpGet("website/preview/{id:int}")]
        public IActionResult Preview(int id)
        {
            var page = _db.Pages.AsNoTracking().FirstOrDefault(x => !x.IsDeleted && x.Id == id);
            if (page == null) return NotFound("页面不存在");
            return RenderPage(page, true);
        }

        private IActionResult RenderPage(WebsitePage page, bool isPreview)
        {
            var model = new WebsitePageRenderModel
            {
                SiteConfig = _db.SiteConfigs.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault(),
                Page = page,
                IsPreview = isPreview,
                News = _db.News.AsNoTracking().Where(x => !x.IsDeleted && x.Status == (int)WebsiteContentStatus.Published).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.PublishTime ?? x.CreateTime).Take(20).ToList(),
                Products = _db.Products.AsNoTracking().Where(x => !x.IsDeleted && x.Status == (int)WebsiteContentStatus.Published).OrderByDescending(x => x.IsRecommend).ThenBy(x => x.Sort).Take(20).ToList(),
                Jobs = _db.Jobs.AsNoTracking().Where(x => !x.IsDeleted && x.Status == (int)WebsiteContentStatus.Published).OrderByDescending(x => x.PublishTime ?? x.CreateTime).Take(20).ToList()
            };
            ViewData["Title"] = string.IsNullOrWhiteSpace(page.PageTitle) ? model.SiteConfig?.BrowserTitle ?? page.PageName : page.PageTitle;
            ViewData["Keywords"] = page.SeoKeywords ?? model.SiteConfig?.Keywords;
            ViewData["Description"] = page.SeoDescription ?? model.SiteConfig?.Description;
            return View("DynamicPage", model);
        }
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "/";
            path = path.Trim();
            if (!path.StartsWith("/")) path = "/" + path;
            return path.TrimEnd('/');
        }
    }
}