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
    [ApiController]
    [Route("api/news-categories")]
    public class WebsiteNewsCategoryApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteNewsCategoryApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get() => Ok(_db.NewsCategories.AsNoTracking().OrderBy(x => x.Sort).ThenBy(x => x.Id).ToList());

        [HttpPost]
        public IActionResult Create([FromBody] ContentNewsCategory model)
        {
            if (string.IsNullOrWhiteSpace(model.CategoryName)) return BadRequest(new { message = "分类名称不能为空" });
            model.CreateTime = DateTime.Now;
            _db.NewsCategories.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ContentNewsCategory model)
        {
            var item = _db.NewsCategories.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound(new { message = "分类不存在" });
            item.CategoryName = model.CategoryName;
            item.CategoryCode = model.CategoryCode;
            item.Sort = model.Sort;
            item.IsEnabled = model.IsEnabled;
            _db.SaveChanges();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _db.NewsCategories.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound(new { message = "分类不存在" });
            if (_db.News.Any(x => x.CategoryId == id && !x.IsDeleted)) return BadRequest(new { message = "该分类下存在新闻，不能删除" });
            _db.NewsCategories.Remove(item);
            _db.SaveChanges();
            return Ok(new { message = "删除成功" });
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/product-categories")]
    public class WebsiteProductCategoryApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteProductCategoryApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get() => Ok(_db.ProductCategories.AsNoTracking().OrderBy(x => x.Sort).ThenBy(x => x.Id).ToList());

        [HttpPost]
        public IActionResult Create([FromBody] ContentProductCategory model)
        {
            if (string.IsNullOrWhiteSpace(model.CategoryName)) return BadRequest(new { message = "分类名称不能为空" });
            model.CreateTime = DateTime.Now;
            _db.ProductCategories.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ContentProductCategory model)
        {
            var item = _db.ProductCategories.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound(new { message = "分类不存在" });
            item.CategoryName = model.CategoryName;
            item.CategoryCode = model.CategoryCode;
            item.Sort = model.Sort;
            item.IsEnabled = model.IsEnabled;
            _db.SaveChanges();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _db.ProductCategories.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound(new { message = "分类不存在" });
            if (_db.Products.Any(x => x.CategoryId == id && !x.IsDeleted)) return BadRequest(new { message = "该分类下存在产品，不能删除" });
            _db.ProductCategories.Remove(item);
            _db.SaveChanges();
            return Ok(new { message = "删除成功" });
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/pages")]
    public class WebsitePageVersionsApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsitePageVersionsApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet("{id:int}/versions")]
        public IActionResult Versions(int id)
        {
            var page = _db.Pages.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (page == null) return NotFound(new { message = "页面不存在" });
            var versions = _db.PageVersions.AsNoTracking().Where(x => x.PageId == id).OrderByDescending(x => x.VersionNo).ToList();
            return Ok(versions);
        }

        [HttpPost("{id:int}/rollback")]
        public IActionResult Rollback(int id, [FromBody] WebsitePageVersionRollbackRequest request)
        {
            var page = _db.Pages.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (page == null) return NotFound(new { message = "页面不存在" });
            var version = _db.PageVersions.AsNoTracking().FirstOrDefault(x => x.Id == request.VersionId && x.PageId == id);
            if (version == null) return NotFound(new { message = "版本不存在" });
            page.DraftJson = version.PublishJson;
            page.PublishJson = version.PublishJson;
            page.Status = (int)WebsiteContentStatus.Published;
            page.PublishTime = DateTime.Now;
            page.UpdateTime = DateTime.Now;
            _db.SaveChanges();
            return Ok(new { message = "回滚成功", page.Id, request.VersionId });
        }
    }

    [AllowAnonymous]
    [ApiController]
    [Route("api/jobs/{jobId:int}/apply")]
    public class WebsiteJobApplyPublicApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteJobApplyPublicApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpPost]
        public IActionResult Apply(int jobId, [FromBody] ContentJobApply model)
        {
            var job = _db.Jobs.AsNoTracking().FirstOrDefault(x => x.Id == jobId && !x.IsDeleted && x.Status == (int)WebsiteContentStatus.Published);
            if (job == null) return NotFound(new { message = "岗位不存在或已关闭" });
            if (string.IsNullOrWhiteSpace(model.ApplicantName) || string.IsNullOrWhiteSpace(model.Phone)) return BadRequest(new { message = "姓名和联系电话不能为空" });
            model.JobId = jobId;
            model.Status = 0;
            model.CreateTime = DateTime.Now;
            _db.JobApplies.Add(model);
            _db.SaveChanges();
            return Ok(new { message = "投递成功", model.Id });
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/job-applies")]
    public class WebsiteJobApplyAdminApiController : ControllerBase
    {
        private readonly WebsiteBuilderDbContext _db;
        public WebsiteJobApplyAdminApiController(WebsiteBuilderDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get([FromQuery] int? jobId)
        {
            var query = _db.JobApplies.AsNoTracking().AsQueryable();
            if (jobId.HasValue) query = query.Where(x => x.JobId == jobId.Value);
            return Ok(query.OrderByDescending(x => x.Id).Take(200).ToList());
        }

        [HttpPut("{id:int}/status/{status:int}")]
        public IActionResult Status(int id, int status)
        {
            var item = _db.JobApplies.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound(new { message = "投递记录不存在" });
            item.Status = status;
            _db.SaveChanges();
            return Ok(item);
        }
    }
}