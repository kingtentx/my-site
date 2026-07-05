using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                if (page == null || page.IsDelete)
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

            var normalizedPath = NormalizePagePath(input.PagePath);
            var pathExists = _pageRepository.GetList(p => p.PagePath == normalizedPath && p.Id != id && !p.IsDelete);
            if (pathExists.Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "页面路径已存在" });
            }

            var page = id > 0 ? _pageRepository.GetOne(id) : new WebsitePage { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            page.SiteId = input.SiteId <= 0 ? 1 : input.SiteId;
            page.PageName = input.PageName.Trim();
            page.PageCode = input.PageCode?.Trim();
            page.PagePath = normalizedPath;
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

            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;
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
        public IActionResult Delete(int id, int[] ids = null, int isAll = 0)
        {
            var deleteIds = ResolveIds(id, ids, isAll);
            if (!deleteIds.Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择要删除的数据" });
            }

            foreach (var deleteId in deleteIds)
            {
                var page = _pageRepository.GetOne(deleteId);
                if (page == null || page.IsDelete)
                {
                    continue;
                }
                if (page.IsHome)
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "首页不能直接删除，请先设置其他页面为首页" });
                }

                page.IsDelete = true;
                page.UpdateTime = DateTime.Now;
                page.UpdateBy = LoginUser.UserName;
                _pageRepository.Update(page);
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult SetHome(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }
            if (!page.IsActive)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "禁用页面不能设为首页" });
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
            if (page == null || page.IsDelete)
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
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }

            var componentJson = NormalizeComponentJson(page.ComponentJson, out var error);
            if (!string.IsNullOrEmpty(error))
            {
                componentJson = "[]";
            }

            return Json(new
            {
                code = (int)ResultCode.Success,
                message = "成功",
                pageId = page.Id,
                pageName = page.PageName,
                pagePath = page.PagePath,
                status = page.Status,
                components = JsonConvert.DeserializeObject(componentJson)
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult SaveDraft(int id, string componentJson)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }

            var normalizedJson = NormalizeComponentJson(componentJson, out var error);
            if (!string.IsNullOrEmpty(error))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = error });
            }

            page.ComponentJson = normalizedJson;
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
                    DraftJson = normalizedJson,
                    PublishJson = version?.PublishJson,
                    Status = 0,
                    CreateUserId = LoginUser.UserId,
                    CreateUserName = LoginUser.UserName,
                    CreationTime = DateTime.Now
                });
            }
            else
            {
                draftVersion.DraftJson = normalizedJson;
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
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }
            if (!page.IsActive)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "禁用页面不能发布" });
            }

            var componentJson = NormalizeComponentJson(page.ComponentJson, out var error);
            if (!string.IsNullOrEmpty(error))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = error });
            }

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

            page.ComponentJson = componentJson;
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
            if (page == null || page.IsDelete)
            {
                return NotFound();
            }
            ViewBag.PageId = page.Id;
            ViewBag.PageName = page.PageName;
            ViewBag.PagePath = page.PagePath;
            ViewBag.ComponentJson = NormalizeComponentJson(page.ComponentJson, out _) ?? "[]";
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

        private static string NormalizePagePath(string path)
        {
            path = (path ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(path)) return "/";
            path = path.Replace("\\", "/");
            if (!path.StartsWith("/")) path = "/" + path;
            while (path.Contains("//")) path = path.Replace("//", "/");
            if (path.Length > 1) path = path.TrimEnd('/');
            return path;
        }

        private static string NormalizeComponentJson(string componentJson, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(componentJson)) return "[]";
            try
            {
                var token = JToken.Parse(componentJson);
                if (token.Type != JTokenType.Array)
                {
                    error = "组件配置格式错误，必须是数组 JSON";
                    return null;
                }

                var components = (JArray)token;
                var pageComponents = new JArray(components.Where(item =>
                {
                    var type = item?["type"]?.ToString();
                    return !string.Equals(type, "navigation", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(type, "footer", StringComparison.OrdinalIgnoreCase);
                }));
                for (var i = 0; i < pageComponents.Count; i++)
                {
                    if (pageComponents[i] is JObject obj)
                    {
                        obj["sort"] = i + 1;
                    }
                }
                return pageComponents.ToString(Formatting.None);
            }
            catch
            {
                error = "组件配置不是合法 JSON，请重新保存页面装修内容";
                return null;
            }
        }

        private static List<int> ResolveIds(int id, int[] ids, int isAll)
        {
            var source = isAll == 1 ? (ids ?? Array.Empty<int>()) : new[] { id };
            return source.Where(p => p > 0).Distinct().ToList();
        }
    }
}