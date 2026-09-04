using CIMC.Data;
using CIMC.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Models;
using Newtonsoft.Json;
using System;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class GlobalRegionController : AdminBaseController
    {
        private readonly IRepository<WebsitePage> _pageRepository;

        public GlobalRegionController(IRepository<WebsitePage> pageRepository)
        {
            _pageRepository = pageRepository;
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
            return View();
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult Reset(string region)
        {
            var isHeader = string.Equals(region, "header", StringComparison.OrdinalIgnoreCase);
            var code = isHeader ? BuilderDocumentFactory.GlobalHeaderPageCode : BuilderDocumentFactory.GlobalFooterPageCode;
            var page = _pageRepository.GetOne(p => p.PageCode == code && !p.IsDelete);
            if (page == null) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "全局区域不存在" });

            var document = isHeader ? BuilderDocumentFactory.CreateDefaultHeader() : BuilderDocumentFactory.CreateDefaultFooter();
            page.ComponentJson = JsonConvert.SerializeObject(document);
            page.ParentId = 0;
            page.ShowInNavigation = false;
            page.Status = 0;
            page.PublishTime = null;
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
