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

        private static readonly HashSet<string> AllowedNodeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "section", "container", "grid", "column",
            "heading", "text", "image", "button", "icon", "video", "divider", "spacer",
            "articleList", "productList", "jobList",
            "logo", "navigation", "search", "language", "contact", "social", "copyright"
        };

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
                if (page == null || page.IsDelete) return NotFound();
                model = ToModel(page);
            }
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult Edit(int id, PageModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写页面名称与路径" };
            if (input == null || string.IsNullOrWhiteSpace(input.PageName) || string.IsNullOrWhiteSpace(input.PagePath)) return Json(result);

            var normalizedPath = NormalizePagePath(input.PagePath);
            var pathExists = _pageRepository.GetList(p => p.PagePath == normalizedPath && p.Id != id && !p.IsDelete);
            if (pathExists.Any()) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "页面路径已存在" });

            var page = id > 0 ? _pageRepository.GetOne(id) : new WebsitePage { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });

            page.SiteId = input.SiteId <= 0 ? 1 : input.SiteId;
            page.PageName = input.PageName.Trim();
            page.PageCode = input.PageCode == null ? null : input.PageCode.Trim();
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
                foreach (var oldHome in oldHomes)
                {
                    oldHome.IsHome = false;
                    oldHome.UpdateTime = DateTime.Now;
                    oldHome.UpdateBy = LoginUser.UserName;
                    _pageRepository.Update(oldHome);
                }
                page.IsHome = true;
            }
            else page.IsHome = false;

            if (id > 0) _pageRepository.Update(page);
            else _pageRepository.Add(page);

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
            if (!string.IsNullOrWhiteSpace(keywords)) where = where.And(p => p.PageName.Contains(keywords) || p.PagePath.Contains(keywords));

            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var query = _pageRepository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            var data = query.List.Select(p => new
            {
                p.Id, p.PageName, p.PagePath, p.PageTitle, p.Status, p.IsHome, p.IsActive, p.Sort, p.CreationTime, p.PublishTime
            }).ToList();
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids = null, int isAll = 0)
        {
            var deleteIds = ResolveIds(id, ids, isAll);
            if (!deleteIds.Any()) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择要删除的数据" });

            foreach (var deleteId in deleteIds)
            {
                var page = _pageRepository.GetOne(deleteId);
                if (page == null || page.IsDelete) continue;
                if (page.IsHome) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "首页不能直接删除，请先设置其他页面为首页" });
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
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            if (!page.IsActive) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "禁用页面不能设为首页" });

            var oldHomes = _pageRepository.GetList(p => p.IsHome && p.Id != id && !p.IsDelete);
            foreach (var oldHome in oldHomes)
            {
                oldHome.IsHome = false;
                oldHome.UpdateTime = DateTime.Now;
                oldHome.UpdateBy = LoginUser.UserName;
                _pageRepository.Update(oldHome);
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
            if (page == null || page.IsDelete) return NotFound();
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
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });

            if (string.IsNullOrWhiteSpace(page.ComponentJson))
            {
                return Json(new
                {
                    code = (int)ResultCode.Success,
                    message = "成功",
                    pageId = page.Id,
                    pageName = page.PageName,
                    pagePath = page.PagePath,
                    status = page.Status,
                    document = JsonConvert.DeserializeObject(CreateEmptyDocumentJson(page.PageName))
                });
            }

            var documentJson = NormalizeBuilderDocument(page.ComponentJson, out var error);
            if (!string.IsNullOrEmpty(error))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = error });
            }

            return Json(new
            {
                code = (int)ResultCode.Success,
                message = "成功",
                pageId = page.Id,
                pageName = page.PageName,
                pagePath = page.PagePath,
                status = page.Status,
                document = JsonConvert.DeserializeObject(documentJson)
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult SaveDraft(int id, string documentJson)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });

            var normalizedJson = NormalizeBuilderDocument(documentJson, out var error);
            if (!string.IsNullOrEmpty(error)) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = error });

            page.ComponentJson = normalizedJson;
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);

            var version = _versionRepository.GetList(p => p.PageId == id).OrderByDescending(p => p.VersionNo).FirstOrDefault();
            var nextVersionNo = (version == null ? 0 : version.VersionNo) + 1;
            var draftVersion = _versionRepository.GetList(p => p.PageId == id && p.Status == 0).OrderByDescending(p => p.VersionNo).FirstOrDefault();
            if (draftVersion == null)
            {
                _versionRepository.Add(new WebsitePageVersion
                {
                    PageId = id,
                    VersionNo = nextVersionNo,
                    DraftJson = normalizedJson,
                    PublishJson = version == null ? null : version.PublishJson,
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
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            if (!page.IsActive) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "禁用页面不能发布" });

            var documentJson = NormalizeBuilderDocument(page.ComponentJson, out var error);
            if (!string.IsNullOrEmpty(error)) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = error });

            var lastVersion = _versionRepository.GetList(p => p.PageId == id).OrderByDescending(p => p.VersionNo).FirstOrDefault();
            var nextVersionNo = (lastVersion == null ? 0 : lastVersion.VersionNo) + 1;
            _versionRepository.Add(new WebsitePageVersion
            {
                PageId = id,
                VersionNo = nextVersionNo,
                DraftJson = documentJson,
                PublishJson = documentJson,
                Status = 1,
                PublishTime = DateTime.Now,
                CreateUserId = LoginUser.UserId,
                CreateUserName = LoginUser.UserName,
                CreationTime = DateTime.Now
            });

            page.ComponentJson = documentJson;
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
            if (page == null || page.IsDelete) return NotFound();
            var json = NormalizeBuilderDocument(page.ComponentJson, out var error);
            if (!string.IsNullOrEmpty(error)) return BadRequest(error);
            ViewBag.PageId = page.Id;
            ViewBag.PageName = page.PageName;
            ViewBag.PagePath = page.PagePath;
            ViewBag.DocumentJson = json;
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

        private static string CreateEmptyDocumentJson(string name)
        {
            return JsonConvert.SerializeObject(new BuilderDocumentModel { SchemaVersion = 1, Name = name ?? string.Empty });
        }

        private static string NormalizeBuilderDocument(string documentJson, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(documentJson))
            {
                error = "页面结构不能为空";
                return null;
            }

            try
            {
                var token = JToken.Parse(documentJson);
                if (token.Type != JTokenType.Object)
                {
                    error = "旧版数组页面结构已不再支持，请使用新版 Site Builder 重新设计页面";
                    return null;
                }

                var document = token.ToObject<BuilderDocumentModel>();
                if (document == null || document.SchemaVersion != 1)
                {
                    error = "不支持的页面结构版本";
                    return null;
                }
                if (document.Nodes == null) document.Nodes = new List<BuilderNodeModel>();
                if (document.Settings == null) document.Settings = new Dictionary<string, object>();

                var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var node in document.Nodes)
                {
                    if (!ValidateNode(node, ids, out error)) return null;
                }
                return JsonConvert.SerializeObject(document, Formatting.None);
            }
            catch (JsonException)
            {
                error = "页面结构不是合法 JSON";
                return null;
            }
        }

        private static bool ValidateNode(BuilderNodeModel node, HashSet<string> ids, out string error)
        {
            error = null;
            if (node == null || string.IsNullOrWhiteSpace(node.Id) || string.IsNullOrWhiteSpace(node.Type))
            {
                error = "页面中存在缺少 Id 或 Type 的组件";
                return false;
            }
            if (!AllowedNodeTypes.Contains(node.Type))
            {
                error = "页面中存在未注册组件：" + node.Type;
                return false;
            }
            if (!ids.Add(node.Id))
            {
                error = "页面中存在重复组件 Id：" + node.Id;
                return false;
            }

            node.Props = node.Props ?? new Dictionary<string, object>();
            node.Style = node.Style ?? new Dictionary<string, object>();
            node.Bindings = node.Bindings ?? new Dictionary<string, object>();
            node.Actions = node.Actions ?? new Dictionary<string, object>();
            node.Children = node.Children ?? new List<BuilderNodeModel>();
            node.Slots = node.Slots ?? new Dictionary<string, List<BuilderNodeModel>>();

            foreach (var child in node.Children)
            {
                if (!ValidateNode(child, ids, out error)) return false;
            }
            foreach (var slot in node.Slots)
            {
                if (slot.Value == null) continue;
                foreach (var child in slot.Value)
                {
                    if (!ValidateNode(child, ids, out error)) return false;
                }
            }
            return true;
        }

        private static List<int> ResolveIds(int id, int[] ids, int isAll)
        {
            var source = isAll == 1 ? (ids ?? Array.Empty<int>()) : new[] { id };
            return source.Where(p => p > 0).Distinct().ToList();
        }
    }
}