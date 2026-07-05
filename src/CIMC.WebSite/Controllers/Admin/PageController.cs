using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class PageController : AdminBaseController
    {
        private readonly IRepository<WebsitePage> _pageRepository;
        private readonly IRepository<WebsitePageVersion> _versionRepository;
        private readonly IPermissionService _permission;

        public PageController(
            IRepository<WebsitePage> pageRepository,
            IRepository<WebsitePageVersion> versionRepository,
            IPermissionService permission)
        {
            _pageRepository = pageRepository;
            _versionRepository = versionRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Website_Page, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Website_Page, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Website_Page, PermissionType.Delete);
            ViewData[PageCode.PAGE_Button_Design] = _permission.CheckPermission(LoginUser, MenuCode.Website_Page, PermissionType.Design);
            ViewData[PageCode.PAGE_Button_Publish] = _permission.CheckPermission(LoginUser, MenuCode.Website_Page, PermissionType.Publish);
            ViewData[PageCode.PAGE_Button_Preview] = _permission.CheckPermission(LoginUser, MenuCode.Website_Page, PermissionType.View);
            return View();
        }

        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new PageModel { IsActive = true, PagePath = "/", Sort = 0 };
            if (id > 0)
            {
                var page = _pageRepository.GetOne(id);
                if (page == null)
                {
                    return NotFound();
                }
                model = ToModel(page);
            }
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult Edit(int id, PageModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写页面名称与路径" };
            if (input == null || string.IsNullOrWhiteSpace(input.PageName) || string.IsNullOrWhiteSpace(input.PagePath))
            {
                return Json(result);
            }

            var pathExists = _pageRepository.GetList(p => p.PagePath == input.PagePath && p.Id != id && !p.IsDelete);
            if (pathExists.Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "页面路径已存在" });
            }

            var page = id > 0 ? _pageRepository.GetOne(id) : new WebsitePage { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (page == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            page.SiteId = 1;
            page.PageName = input.PageName;
            page.PageCode = input.PageCode;
            page.PagePath = input.PagePath.StartsWith("/") ? input.PagePath : "/" + input.PagePath;
            page.PageTitle = input.PageTitle;
            page.SeoKeywords = input.SeoKeywords;
            page.SeoDescription = input.SeoDescription;
            page.IsActive = input.IsActive;
            page.Sort = input.Sort;
            page.IsDelete = false;
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;

            if (input.IsHome)
            {
                var oldHomes = _pageRepository.GetList(p => p.IsHome && p.Id != id && !p.IsDelete);
                foreach (var oh in oldHomes)
                {
                    oh.IsHome = false;
                    oh.UpdateTime = DateTime.Now;
                    oh.UpdateBy = LoginUser.UserName;
                    _pageRepository.Update(oh);
                }
                page.IsHome = true;
            }
            else
            {
                page.IsHome = false;
            }

            if (id > 0)
            {
                _pageRepository.Update(page);
            }
            else
            {
                _pageRepository.Add(page);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            var where = LambdaHelper.True<WebsitePage>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.PageName.Contains(keywords) || p.PagePath.Contains(keywords));
            }

            var query = _pageRepository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.PageName,
                p.PagePath,
                p.PageTitle,
                p.Status,
                p.IsHome,
                p.IsActive,
                p.Sort,
                p.CreationTime,
                p.PublishTime
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids, int isAll = 0)
        {
            var deleteIds = isAll == 1 ? ids : new[] { id };
            foreach (var deleteId in deleteIds.Where(p => p > 0))
            {
                var page = _pageRepository.GetOne(deleteId);
                if (page != null)
                {
                    page.IsDelete = true;
                    page.UpdateTime = DateTime.Now;
                    page.UpdateBy = LoginUser.UserName;
                    _pageRepository.Update(page);
                }
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult SetHome(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            var oldHomes = _pageRepository.GetList(p => p.IsHome && p.Id != id && !p.IsDelete);
            foreach (var oh in oldHomes)
            {
                oh.IsHome = false;
                oh.UpdateTime = DateTime.Now;
                oh.UpdateBy = LoginUser.UserName;
                _pageRepository.Update(oh);
            }

            page.IsHome = true;
            page.UpdateTime = DateTime.Now;
            page.UpdateBy = LoginUser.UserName;
            _pageRepository.Update(page);

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "设置成功" });
        }

        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult Design(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null)
            {
                return NotFound();
            }
            ViewBag.PageId = page.Id;
            ViewBag.PageName = page.PageName;
            ViewBag.PagePath = page.PagePath;
            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public JsonResult GetComponentData(int pageId)
        {
            var page = _pageRepository.GetOne(pageId);
            if (page == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }

            var componentJson = string.IsNullOrWhiteSpace(page.ComponentJson) ? "[]" : page.ComponentJson;
            return Json(new
            {
                code = (int)ResultCode.Success,
                message = "成功",
                pageId = page.Id,
                pageName = page.PageName,
                pagePath = page.PagePath,
                status = page.Status,
                components = Newtonsoft.Json.JsonConvert.DeserializeObject(componentJson)
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult SaveDraft(int id, string componentJson)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }

            page.ComponentJson = componentJson ?? "[]";
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);

            var version = _versionRepository.GetList(p => p.PageId == id).OrderByDescending(p => p.VersionNo).FirstOrDefault();
            var nextVersionNo = (version?.VersionNo ?? 0) + 1;

            var draftVersion = _versionRepository.GetList(p => p.PageId == id && p.Status == 0).OrderByDescending(p => p.VersionNo).FirstOrDefault();
            if (draftVersion == null)
            {
                _versionRepository.Add(new WebsitePageVersion
                {
                    PageId = id,
                    VersionNo = nextVersionNo,
                    DraftJson = componentJson,
                    PublishJson = version?.PublishJson,
                    Status = 0,
                    CreateUserId = LoginUser.UserId,
                    CreateUserName = LoginUser.UserName,
                    CreationTime = DateTime.Now
                });
            }
            else
            {
                draftVersion.DraftJson = componentJson;
                draftVersion.CreateUserName = LoginUser.UserName;
                _versionRepository.Update(draftVersion);
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "草稿已保存" });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Publish)]
        public IActionResult Publish(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }

            var componentJson = string.IsNullOrWhiteSpace(page.ComponentJson) ? "[]" : page.ComponentJson;

            var lastVersion = _versionRepository.GetList(p => p.PageId == id).OrderByDescending(p => p.VersionNo).FirstOrDefault();
            var nextVersionNo = (lastVersion?.VersionNo ?? 0) + 1;

            _versionRepository.Add(new WebsitePageVersion
            {
                PageId = id,
                VersionNo = nextVersionNo,
                DraftJson = componentJson,
                PublishJson = componentJson,
                Status = 1,
                PublishTime = DateTime.Now,
                CreateUserId = LoginUser.UserId,
                CreateUserName = LoginUser.UserName,
                CreationTime = DateTime.Now
            });

            page.Status = 1;
            page.PublishTime = DateTime.Now;
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "发布成功" });
        }

        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public IActionResult Preview(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null)
            {
                return NotFound();
            }
            ViewBag.PageId = page.Id;
            ViewBag.PageName = page.PageName;
            ViewBag.PagePath = page.PagePath;
            ViewBag.ComponentJson = string.IsNullOrWhiteSpace(page.ComponentJson) ? "[]" : page.ComponentJson;
            return View();
        }

        private PageModel ToModel(WebsitePage page)
        {
            return new PageModel
            {
                Id = page.Id,
                SiteId = page.SiteId,
                PageName = page.PageName,
                PageCode = page.PageCode,
                PagePath = page.PagePath,
                PageTitle = page.PageTitle,
                SeoKeywords = page.SeoKeywords,
                SeoDescription = page.SeoDescription,
                IsActive = page.IsActive,
                IsHome = page.IsHome,
                Sort = page.Sort,
                Status = page.Status,
                ComponentJson = page.ComponentJson,
                PublishTime = page.PublishTime,
                CreationTime = page.CreationTime,
                CreationBy = page.CreationBy,
                UpdateTime = page.UpdateTime,
                UpdateBy = page.UpdateBy
            };
        }
    }
}
