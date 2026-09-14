using CIMC.Data;
using CIMC.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class GlobalRegionController : AdminBaseController
    {
        private readonly IRepository<WebsitePage> _pageRepository;
        private readonly IRepository<WebsitePageVersion> _versionRepository;
        private readonly IPermissionService _permission;

        public GlobalRegionController(IRepository<WebsitePage> pageRepository, IRepository<WebsitePageVersion> versionRepository, IPermissionService permission)
        {
            _pageRepository = pageRepository;
            _versionRepository = versionRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult Index()
        {
            var header = EnsureRegion(
                BuilderDocumentFactory.GlobalHeaderPageCode,
                BuilderDocumentFactory.GlobalHeaderPath,
                "全局 Header",
                BuilderDocumentFactory.CreateDefaultHeader());
            var footer = EnsureRegion(
                BuilderDocumentFactory.GlobalFooterPageCode,
                BuilderDocumentFactory.GlobalFooterPath,
                "全局 Footer",
                BuilderDocumentFactory.CreateDefaultFooter());

            ViewBag.HeaderId = header.Id;
            ViewBag.HeaderStatus = header.Status;
            ViewBag.FooterId = footer.Id;
            ViewBag.FooterStatus = footer.Status;
            ViewBag.HeaderState = PublicationState(header);
            ViewBag.FooterState = PublicationState(footer);
            ViewBag.HeaderPublishTime = header.PublishTime?.ToString("yyyy-MM-dd HH:mm") ?? "尚未发布";
            ViewBag.FooterPublishTime = footer.PublishTime?.ToString("yyyy-MM-dd HH:mm") ?? "尚未发布";
            ViewBag.CanPublish = _permission.CheckPermission(LoginUser, MenuCode.Website_Page, PermissionType.Publish);
            return View();
        }

        private string PublicationState(WebsitePage page)
        {
            if (page.Status != 1) return "尚未发布";
            var version = _versionRepository.GetList(v => v.PageId == page.Id && v.Status == 1).OrderByDescending(v => v.VersionNo).FirstOrDefault();
            if (version == null) return "已发布";
            try { return JToken.DeepEquals(JToken.Parse(page.ComponentJson ?? "{}"), JToken.Parse(version.PublishJson ?? "{}")) ? "已发布 · 与草稿一致" : "已发布 · 有待发布修改"; }
            catch (JsonException) { return "已发布 · 请检查草稿"; }
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult Reset(string region)
        {
            if (!string.Equals(region, "header", StringComparison.OrdinalIgnoreCase) && !string.Equals(region, "footer", StringComparison.OrdinalIgnoreCase))
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择页头或页脚" });
            var isHeader = string.Equals(region, "header", StringComparison.OrdinalIgnoreCase);
            var code = isHeader ? BuilderDocumentFactory.GlobalHeaderPageCode : BuilderDocumentFactory.GlobalFooterPageCode;
            var page = _pageRepository.GetOne(p => p.PageCode == code && !p.IsDelete);
            if (page == null) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "全局区域不存在" });

            // A legacy published page without a snapshot reads ComponentJson on the public site.
            if (page.Status == 1 && !_versionRepository.GetList(v => v.PageId == page.Id && v.Status == 1).Any())
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "当前页面缺少发布快照，请先重新发布后再恢复默认" });
            var document = isHeader ? BuilderDocumentFactory.CreateDefaultHeader() : BuilderDocumentFactory.CreateDefaultFooter();
            page.ComponentJson = JsonConvert.SerializeObject(document);
            page.ParentId = 0;
            page.ShowInNavigation = false;
            // Preserve the live snapshot; resetting is a draft-only operation.
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "已恢复新版默认结构，请进入装修器调整并重新发布" });
        }

        private WebsitePage EnsureRegion(string code, string path, string name, BuilderDocumentModel defaultDocument)
        {
            var page = _pageRepository.GetOne(p => p.PageCode == code && !p.IsDelete);
            if (page != null)
            {
                if (page.ParentId != 0 || page.ShowInNavigation)
                {
                    page.ParentId = 0;
                    page.ShowInNavigation = false;
                    page.UpdateTime = DateTime.Now;
                    page.UpdateBy = LoginUser.UserName;
                    _pageRepository.Update(page);
                }
                return page;
            }

            page = new WebsitePage
            {
                SiteId = 1,
                ParentId = 0,
                PageName = name,
                PageCode = code,
                PagePath = path,
                PageTitle = name,
                ShowInNavigation = false,
                IsActive = true,
                IsHome = false,
                Sort = -100,
                Status = 0,
                ComponentJson = JsonConvert.SerializeObject(defaultDocument),
                IsDelete = false,
                CreationBy = LoginUser.UserName,
                CreationTime = DateTime.Now,
                UpdateBy = LoginUser.UserName,
                UpdateTime = DateTime.Now
            };
            _pageRepository.Add(page);
            return _pageRepository.GetOne(p => p.PageCode == code && !p.IsDelete) ?? page;
        }
    }
}
