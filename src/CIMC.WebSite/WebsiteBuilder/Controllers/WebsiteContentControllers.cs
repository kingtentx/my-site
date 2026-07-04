using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySite.Web.WebsiteBuilder.Data;
using MySite.Web.WebsiteBuilder.Models;

namespace MySite.Web.WebsiteBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/news")]
    public class WebsiteNewsApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteNewsApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var q = _db.News.AsNoTracking().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(x => x.Title.Contains(keyword) || x.Summary.Contains(keyword));
            var total = q.Count();
            var items = q.OrderByDescending(x => x.IsTop).ThenByDescending(x => x.PublishTime ?? x.CreateTime).Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1)).Take(Math.Max(pageSize, 1)).ToList();
            return Ok(new { total, items });
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id) => OkOr404(_db.News.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted), "新闻不存在");

        [HttpPost]
        public IActionResult Create([FromBody] ContentNews model)
        {
            model.Content = SafeHtml(model.Content);
            model.CreateTime = DateTime.Now;
            model.UpdateTime = DateTime.Now;
            _db.News.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ContentNews model)
        {
            var x = _db.News.FirstOrDefault(n => n.Id == id && !n.IsDeleted);
            if (x == null) return NotFound(new { message = "新闻不存在" });
            x.Title = model.Title; x.CategoryId = model.CategoryId; x.CoverImage = model.CoverImage; x.Summary = model.Summary; x.Content = SafeHtml(model.Content); x.Author = model.Author; x.Source = model.Source; x.Tags = model.Tags; x.IsTop = model.IsTop; x.IsRecommend = model.IsRecommend; x.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(x);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id) { var x = _db.News.FirstOrDefault(n => n.Id == id && !n.IsDeleted); if (x == null) return NotFound(new { message = "新闻不存在" }); x.IsDeleted = true; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(new { message = "删除成功" }); }
        [HttpPost("{id:int}/publish")]
        public IActionResult Publish(int id) { var x = _db.News.FirstOrDefault(n => n.Id == id && !n.IsDeleted); if (x == null) return NotFound(new { message = "新闻不存在" }); x.Status = (int)WebsiteContentStatus.Published; x.PublishTime = DateTime.Now; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
        [HttpPost("{id:int}/offline")]
        public IActionResult Offline(int id) { var x = _db.News.FirstOrDefault(n => n.Id == id && !n.IsDeleted); if (x == null) return NotFound(new { message = "新闻不存在" }); x.Status = (int)WebsiteContentStatus.Offline; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
        private IActionResult OkOr404(object item, string message) => item == null ? NotFound(new { message }) : Ok(item);
        private static string SafeHtml(string html) => string.IsNullOrWhiteSpace(html) ? html : Regex.Replace(html, "<script[\\s\\S]*?>[\\s\\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);
    }

    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class WebsiteProductsApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteProductsApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var q = _db.Products.AsNoTracking().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(x => x.ProductName.Contains(keyword) || x.Summary.Contains(keyword));
            var total = q.Count();
            var items = q.OrderBy(x => x.Sort).ThenByDescending(x => x.Id).Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1)).Take(Math.Max(pageSize, 1)).ToList();
            return Ok(new { total, items });
        }
        [HttpGet("{id:int}")]
        public IActionResult Get(int id) { var x = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id && !p.IsDeleted); return x == null ? NotFound(new { message = "产品不存在" }) : Ok(x); }
        [HttpPost]
        public IActionResult Create([FromBody] ContentProduct model) { model.CreateTime = DateTime.Now; model.UpdateTime = DateTime.Now; _db.Products.Add(model); _db.SaveChanges(); return Ok(model); }
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ContentProduct model) { var x = _db.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted); if (x == null) return NotFound(new { message = "产品不存在" }); x.ProductName = model.ProductName; x.CategoryId = model.CategoryId; x.CoverImage = model.CoverImage; x.ImageList = model.ImageList; x.Summary = model.Summary; x.Description = model.Description; x.Specification = model.Specification; x.Feature = model.Feature; x.Sort = model.Sort; x.IsRecommend = model.IsRecommend; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id) { var x = _db.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted); if (x == null) return NotFound(new { message = "产品不存在" }); x.IsDeleted = true; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(new { message = "删除成功" }); }
        [HttpPost("{id:int}/online")]
        public IActionResult Online(int id) { var x = _db.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted); if (x == null) return NotFound(new { message = "产品不存在" }); x.Status = (int)WebsiteContentStatus.Published; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
        [HttpPost("{id:int}/offline")]
        public IActionResult Offline(int id) { var x = _db.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted); if (x == null) return NotFound(new { message = "产品不存在" }); x.Status = (int)WebsiteContentStatus.Offline; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
    }

    [Authorize]
    [ApiController]
    [Route("api/jobs")]
    public class WebsiteJobsApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteJobsApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var q = _db.Jobs.AsNoTracking().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(x => x.JobTitle.Contains(keyword) || x.Department.Contains(keyword) || x.WorkLocation.Contains(keyword));
            var total = q.Count();
            var items = q.OrderByDescending(x => x.PublishTime ?? x.CreateTime).Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1)).Take(Math.Max(pageSize, 1)).ToList();
            return Ok(new { total, items });
        }
        [HttpGet("{id:int}")]
        public IActionResult Get(int id) { var x = _db.Jobs.AsNoTracking().FirstOrDefault(j => j.Id == id && !j.IsDeleted); return x == null ? NotFound(new { message = "岗位不存在" }) : Ok(x); }
        [HttpPost]
        public IActionResult Create([FromBody] ContentJob model) { model.CreateTime = DateTime.Now; model.UpdateTime = DateTime.Now; _db.Jobs.Add(model); _db.SaveChanges(); return Ok(model); }
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ContentJob model) { var x = _db.Jobs.FirstOrDefault(j => j.Id == id && !j.IsDeleted); if (x == null) return NotFound(new { message = "岗位不存在" }); x.JobTitle = model.JobTitle; x.Department = model.Department; x.WorkLocation = model.WorkLocation; x.SalaryRange = model.SalaryRange; x.RecruitCount = model.RecruitCount; x.JobType = model.JobType; x.Responsibilities = model.Responsibilities; x.Requirements = model.Requirements; x.ContactName = model.ContactName; x.ContactPhone = model.ContactPhone; x.ContactEmail = model.ContactEmail; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id) { var x = _db.Jobs.FirstOrDefault(j => j.Id == id && !j.IsDeleted); if (x == null) return NotFound(new { message = "岗位不存在" }); x.IsDeleted = true; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(new { message = "删除成功" }); }
        [HttpPost("{id:int}/publish")]
        public IActionResult Publish(int id) { var x = _db.Jobs.FirstOrDefault(j => j.Id == id && !j.IsDeleted); if (x == null) return NotFound(new { message = "岗位不存在" }); x.Status = (int)WebsiteContentStatus.Published; x.PublishTime = DateTime.Now; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
        [HttpPost("{id:int}/close")]
        public IActionResult Close(int id) { var x = _db.Jobs.FirstOrDefault(j => j.Id == id && !j.IsDeleted); if (x == null) return NotFound(new { message = "岗位不存在" }); x.Status = (int)WebsiteContentStatus.Closed; x.UpdateTime = DateTime.Now; _db.SaveChanges(); return Ok(x); }
    }
}