using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySite.Web.WebsiteBuilder.Data;
using MySite.Web.WebsiteBuilder.Models;

namespace MySite.Web.WebsiteBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/navigation")]
    public class WebsiteNavigationApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteNavigationApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get() => Ok(_db.Navigations.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Sort).ThenBy(x => x.Id).ToList());

        [HttpPost]
        public IActionResult Create([FromBody] WebsiteNavigation model)
        {
            if (string.IsNullOrWhiteSpace(model.Title)) return BadRequest(new { message = "导航名称不能为空" });
            model.CreateTime = DateTime.Now;
            model.UpdateTime = DateTime.Now;
            _db.Navigations.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] WebsiteNavigation model)
        {
            var item = _db.Navigations.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "导航不存在" });
            item.ParentId = model.ParentId;
            item.Title = model.Title;
            item.LinkType = model.LinkType;
            item.PageId = model.PageId;
            item.LinkUrl = model.LinkUrl;
            item.Target = model.Target;
            item.Icon = model.Icon;
            item.Sort = model.Sort;
            item.IsEnabled = model.IsEnabled;
            item.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _db.Navigations.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "导航不存在" });
            item.IsDeleted = true;
            item.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(new { message = "删除成功" });
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/banners")]
    public class WebsiteBannerApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteBannerApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get() => Ok(_db.Banners.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Sort).ThenByDescending(x => x.Id).ToList());

        [HttpPost]
        public IActionResult Create([FromBody] WebsiteBanner model)
        {
            if (string.IsNullOrWhiteSpace(model.Title)) return BadRequest(new { message = "Banner标题不能为空" });
            model.CreateTime = DateTime.Now;
            model.UpdateTime = DateTime.Now;
            _db.Banners.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] WebsiteBanner model)
        {
            var item = _db.Banners.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "Banner不存在" });
            item.Title = model.Title;
            item.Subtitle = model.Subtitle;
            item.ImageUrl = model.ImageUrl;
            item.VideoUrl = model.VideoUrl;
            item.ButtonText = model.ButtonText;
            item.ButtonLink = model.ButtonLink;
            item.LinkUrl = model.LinkUrl;
            item.Height = model.Height;
            item.Sort = model.Sort;
            item.AutoPlay = model.AutoPlay;
            item.Interval = model.Interval;
            item.IsEnabled = model.IsEnabled;
            item.BeginTime = model.BeginTime;
            item.EndTime = model.EndTime;
            item.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _db.Banners.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "Banner不存在" });
            item.IsDeleted = true;
            item.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(new { message = "删除成功" });
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/footer")]
    public class WebsiteFooterApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteFooterApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get()
        {
            var footer = _db.Footers.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault();
            return Ok(footer ?? new WebsiteFooter { CompanyName = "企业官网", FriendLinksJson = "[]", BackgroundColor = "#111827", TextColor = "#ffffff" });
        }

        [HttpPut]
        public IActionResult Update([FromBody] WebsiteFooter model)
        {
            var footer = _db.Footers.OrderBy(x => x.Id).FirstOrDefault();
            if (footer == null)
            {
                model.CreateTime = DateTime.Now;
                model.UpdateTime = DateTime.Now;
                _db.Footers.Add(model);
            }
            else
            {
                footer.Logo = model.Logo;
                footer.CompanyName = model.CompanyName;
                footer.Description = model.Description;
                footer.Tel = model.Tel;
                footer.Email = model.Email;
                footer.Address = model.Address;
                footer.QrCode = model.QrCode;
                footer.IcpNo = model.IcpNo;
                footer.PoliceNo = model.PoliceNo;
                footer.Copyright = model.Copyright;
                footer.FriendLinksJson = model.FriendLinksJson;
                footer.BackgroundColor = model.BackgroundColor;
                footer.TextColor = model.TextColor;
                footer.UpdateTime = DateTime.Now;
            }
            _db.SaveChanges();
            return Ok(footer ?? model);
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/materials")]
    public class WebsiteMaterialsApiController : ControllerBase
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg", ".mp4", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        private readonly WebsiteBuilderDbContext _db;
        private readonly IWebHostEnvironment _env;
        public WebsiteMaterialsApiController(WebsiteBuilderDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

        [HttpGet]
        public IActionResult Get([FromQuery] string category)
        {
            var query = _db.MaterialFiles.AsNoTracking().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category);
            return Ok(query.OrderByDescending(x => x.Id).Take(200).ToList());
        }

        [HttpPost("upload")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string category)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "请选择文件" });
            if (file.Length > 30 * 1024 * 1024) return BadRequest(new { message = "文件不能超过30MB" });
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) return BadRequest(new { message = "文件类型不允许" });

            var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath) ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot") : _env.WebRootPath;
            var relativeDir = Path.Combine("uploads", "website", DateTime.Now.ToString("yyyyMM"), DateTime.Now.ToString("dd"));
            var saveDir = Path.Combine(webRoot, relativeDir);
            Directory.CreateDirectory(saveDir);
            var saveName = Guid.NewGuid().ToString("N") + ext;
            var savePath = Path.Combine(saveDir, saveName);
            await using (var stream = System.IO.File.Create(savePath))
            {
                await file.CopyToAsync(stream);
            }
            var url = "/" + relativeDir.Replace("\\", "/") + "/" + saveName;
            var item = new MaterialFile { FileName = Path.GetFileName(file.FileName), FileUrl = url, FileType = ext.TrimStart('.'), FileSize = file.Length, Category = category, CreateTime = DateTime.Now };
            _db.MaterialFiles.Add(item);
            _db.SaveChanges();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _db.MaterialFiles.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (item == null) return NotFound(new { message = "素材不存在" });
            item.IsDeleted = true;
            _db.SaveChanges();
            return Ok(new { message = "删除成功" });
        }
    }
}