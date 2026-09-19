using CIMC.Data;
using CIMC.EntityFramework;
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
    /// <summary>处理页面相关的网页请求。</summary>
    [Authorize]
    public class PageController : AdminBaseController
    {
        private readonly IRepository<WebsitePage> _pageRepository;
        private readonly IRepository<WebsitePageVersion> _versionRepository;
        private readonly IPermissionService _permission;

        private static readonly HashSet<string> AllowedNodeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "section", "container", "grid", "column",
            "heading", "text", "richText", "image", "banner", "button", "icon", "video", "divider", "spacer",
            "articleList", "productList", "jobList", "contactForm",
            "logo", "navigation", "search", "language", "contact", "social", "copyright"
        };

        /// <summary>初始化页面。</summary>
        public PageController(
            IRepository<WebsitePage> pageRepository,
            IRepository<WebsitePageVersion> versionRepository,
            IPermissionService permission)
        {
            _pageRepository = pageRepository;
            _versionRepository = versionRepository;
            _permission = permission;
        }

        /// <summary>显示页面管理页面。</summary>
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

        /// <summary>显示页面的编辑内容。</summary>
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult Edit(int id = 0, int parentId = 0)
        {
            var model = new PageModel
            {
                IsActive = true,
                ShowInNavigation = true,
                PagePath = "/",
                ParentId = Math.Max(0, parentId),
                Sort = 0
            };

            if (id > 0)
            {
                var page = _pageRepository.GetOne(id);
                if (page == null || page.IsDelete || IsGlobalPage(page)) return NotFound();
                model = ToModel(page);
            }

            var allPages = GetManagedPages();
            ViewBag.HasChildren = id > 0 && allPages.Any(p => p.ParentId == id);
            var blockedIds = id > 0 ? GetDescendantIds(id, allPages) : new HashSet<int>();
            if (id > 0) blockedIds.Add(id);
            ViewBag.ParentPages = allPages
                .Where(p => !blockedIds.Contains(p.Id))
                .OrderBy(p => p.Sort)
                .ThenBy(p => p.Id)
                .Select(ToModel)
                .ToList();

            return View(model);
        }

        /// <summary>保存页面的编辑内容。</summary>
        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult Edit(int id, PageModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写页面名称与路径" };
            if (input == null || string.IsNullOrWhiteSpace(input.PageName) || string.IsNullOrWhiteSpace(input.PagePath)) return Json(result);

            var normalizedPath = NormalizePagePath(input.PagePath);
            var pathExists = _pageRepository.GetList(p => p.PagePath == normalizedPath && p.Id != id && !p.IsDelete);
            if (pathExists.Any()) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "页面路径已存在" });

            var allPages = GetManagedPages();
            if (input.ParentId > 0)
            {
                var parent = allPages.FirstOrDefault(p => p.Id == input.ParentId);
                if (parent == null)
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "父级页面不存在" });
                }

                if (id > 0 && (input.ParentId == id || GetDescendantIds(id, allPages).Contains(input.ParentId)))
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "不能将页面移动到自身或其子页面下面" });
                }
            }

            var page = id > 0
                ? _pageRepository.GetOne(id)
                : new WebsitePage { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (page == null || page.IsDelete || IsGlobalPage(page))
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            page.SiteId = input.SiteId <= 0 ? 1 : input.SiteId;
            page.ParentId = input.IsHome ? 0 : Math.Max(0, input.ParentId);
            page.PageName = input.PageName.Trim();
            page.PageCode = string.IsNullOrWhiteSpace(input.PageCode) ? null : input.PageCode.Trim();
            page.PagePath = normalizedPath;
            page.PageTitle = input.PageTitle;
            page.SeoKeywords = input.SeoKeywords;
            page.SeoDescription = input.SeoDescription;
            page.ShowInNavigation = input.ShowInNavigation;
            page.NavigationTitle = string.IsNullOrWhiteSpace(input.NavigationTitle) ? null : input.NavigationTitle.Trim();
            page.NavigationIcon = string.IsNullOrWhiteSpace(input.NavigationIcon) ? null : input.NavigationIcon.Trim();
            page.NavigationTarget = input.NavigationTarget == 1 ? 1 : 0;
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
            else
            {
                page.IsHome = false;
            }

            if (id > 0) _pageRepository.Update(page);
            else _pageRepository.Add(page);

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功，页面层级与导航配置已同步";
            return Json(result);
        }

        /// <summary>查询页面列表。</summary>
        [HttpGet]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public JsonResult GetList()
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            var pages = GetManagedPages();
            var directoryIds = pages.Where(p => p.ParentId > 0).Select(p => p.ParentId).ToHashSet();

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                var matchedIds = pages
                    .Where(p => (p.PageName ?? string.Empty).Contains(keywords, StringComparison.OrdinalIgnoreCase)
                                || (p.PagePath ?? string.Empty).Contains(keywords, StringComparison.OrdinalIgnoreCase)
                                || (p.NavigationTitle ?? string.Empty).Contains(keywords, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id)
                    .ToHashSet();

                var byId = pages.ToDictionary(p => p.Id);
                foreach (var id in matchedIds.ToList())
                {
                    var current = byId.TryGetValue(id, out var item) ? item : null;
                    var guard = 0;
                    while (current != null && current.ParentId > 0 && guard++ < 100)
                    {
                        if (!byId.TryGetValue(current.ParentId, out var parent)) break;
                        matchedIds.Add(parent.Id);
                        current = parent;
                    }
                }
                // 命中目录时保留其子树，搜索结果仍可展开查看完整层级。
                foreach (var id in matchedIds.ToList())
                {
                    matchedIds.UnionWith(GetDescendantIds(id, pages));
                }
                pages = pages.Where(p => matchedIds.Contains(p.Id)).ToList();
            }

            var data = FlattenPages(pages, directoryIds);
            return Json(new ResultModel<object>
            {
                Code = (int)ResultCode.Success,
                Message = "成功",
                Count = data.Count,
                Data = data
            });
        }

        /// <summary>删除指定页面记录。</summary>
        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids = null, int isAll = 0)
        {
            var deleteIds = ResolveIds(id, ids, isAll);
            if (!deleteIds.Any()) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择要删除的数据" });

            var allPages = GetManagedPages();
            foreach (var deleteId in deleteIds)
            {
                var page = allPages.FirstOrDefault(p => p.Id == deleteId);
                if (page == null) continue;
                if (page.IsHome) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "首页不能直接删除，请先设置其他页面为首页" });
                if (allPages.Any(p => p.ParentId == deleteId && !deleteIds.Contains(p.Id)))
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = $"页面“{page.PageName}”下面还有子页面，请先移动或删除子页面" });
                }
            }

            foreach (var deleteId in deleteIds)
            {
                var page = _pageRepository.GetOne(deleteId);
                if (page == null || page.IsDelete || IsGlobalPage(page)) continue;
                page.IsDelete = true;
                page.UpdateTime = DateTime.Now;
                page.UpdateBy = LoginUser.UserName;
                _pageRepository.Update(page);
            }
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        /// <summary>设置网站首页。</summary>
        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult SetHome(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete || IsGlobalPage(page)) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
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
            page.ParentId = 0;
            page.UpdateTime = DateTime.Now;
            page.UpdateBy = LoginUser.UserName;
            _pageRepository.Update(page);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "设置成功" });
        }

        /// <summary>打开页面装修界面。</summary>
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult Design(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete) return NotFound();
            if (HasChildren(page)) return BadRequest("目录页面已有子页面，不能装修页面内容；请装修子页面。");
            ViewBag.PageId = page.Id;
            ViewBag.PageName = page.PageName;
            ViewBag.PagePath = page.PagePath;
            return View();
        }

        /// <summary>获取可用的链接目标。</summary>
        [HttpGet]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public JsonResult LinkOptions()
        {
            var pages = _pageRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true)
                .Where(p => !IsGlobalPage(p))
                .Select(p => new { id = p.Id, name = p.PageName, path = p.PagePath, published = p.Status == 1 });
            return Json(new { code = (int)ResultCode.Success, data = pages });
        }

        /// <summary>获取页面组件数据。</summary>
        [HttpGet]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public JsonResult GetComponentData(int pageId)
        {
            var page = _pageRepository.GetOne(pageId);
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            if (HasChildren(page)) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "目录页面已有子页面，不能装修页面内容；请装修子页面。" });

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

        /// <summary>保存页面草稿。</summary>
        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult SaveDraft(int id, string documentJson)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            if (HasChildren(page)) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "目录页面已有子页面，不能保存页面内容；请装修子页面。" });

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

        /// <summary>发布页面内容。</summary>
        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Publish)]
        public IActionResult Publish(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete) return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            if (!page.IsActive) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "禁用页面不能发布" });

            var directory = HasChildren(page);
            // 目录页只发布导航状态；保留原有草稿，避免以后移除子页面时丢失内容。
            string error = null;
            var documentJson = directory ? CreateEmptyDocumentJson(page.PageName) : NormalizeBuilderDocument(page.ComponentJson, out error);
            if (!directory && !string.IsNullOrEmpty(error)) return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = error });

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

            if (!directory) page.ComponentJson = documentJson;
            page.Status = 1;
            page.PublishTime = DateTime.Now;
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "发布成功" });
        }

        /// <summary>预览指定页面。</summary>
        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public IActionResult Preview(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete) return NotFound();
            if (IsGlobalPage(page)) return RedirectToAction("BuilderPreview", "Home", new { id });
            if (HasChildren(page)) return BadRequest("目录页面没有独立内容，请预览子页面。");
            var json = NormalizeBuilderDocument(page.ComponentJson, out var error);
            if (!string.IsNullOrEmpty(error)) return BadRequest(error);
            ViewBag.PageId = page.Id;
            ViewBag.PageName = page.PageName;
            ViewBag.PagePath = page.PagePath;
            ViewBag.DocumentJson = json;
            return View();
        }

        /// <summary>获取后台管理的页面集合。</summary>
        private List<WebsitePage> GetManagedPages()
        {
            return _pageRepository
                .GetList(p => !p.IsDelete, p => p.Sort, true)
                .Where(p => !IsGlobalPage(p))
                .OrderBy(p => p.Sort)
                .ThenBy(p => p.Id)
                .ToList();
        }

        /// <summary>判断页面是否为全局区域。</summary>
        private static bool IsGlobalPage(WebsitePage page)
        {
            if (page == null) return false;
            return (!string.IsNullOrWhiteSpace(page.PageCode) && page.PageCode.StartsWith("__GLOBAL_", StringComparison.OrdinalIgnoreCase))
                   || (!string.IsNullOrWhiteSpace(page.PagePath) && page.PagePath.StartsWith("/__global/", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>判断页面是否包含子页面。</summary>
        private bool HasChildren(WebsitePage page)
        {
            return page != null && !IsGlobalPage(page) &&
                   _pageRepository.GetList(p => p.ParentId == page.Id && !p.IsDelete).Any();
        }

        /// <summary>获取页面的所有后代主键。</summary>
        private static HashSet<int> GetDescendantIds(int pageId, List<WebsitePage> pages)
        {
            var result = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(pageId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in pages.Where(p => p.ParentId == current))
                {
                    if (result.Add(child.Id)) queue.Enqueue(child.Id);
                }
            }
            return result;
        }

        /// <summary>将页面树展开为列表。</summary>
        private static List<object> FlattenPages(List<WebsitePage> pages, HashSet<int> directoryIds)
        {
            var result = new List<object>();
            var visited = new HashSet<int>();
            var ids = pages.Select(p => p.Id).ToHashSet();
            var groups = pages
                .GroupBy(p => ids.Contains(p.ParentId) ? p.ParentId : 0)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Sort).ThenBy(p => p.Id).ToList());

            void Walk(int parentId, int level)
            {
                if (!groups.TryGetValue(parentId, out var children)) return;
                foreach (var page in children)
                {
                    if (!visited.Add(page.Id)) continue;
                    result.Add(new
                    {
                        page.Id,
                        page.ParentId,
                        Level = level,
                        page.PageName,
                        page.PagePath,
                        page.PageTitle,
                        NavigationTitle = string.IsNullOrWhiteSpace(page.NavigationTitle) ? page.PageName : page.NavigationTitle,
                        page.NavigationIcon,
                        page.NavigationTarget,
                        page.ShowInNavigation,
                        page.Status,
                        page.IsHome,
                        page.IsActive,
                        page.Sort,
                        page.CreationTime,
                        page.PublishTime,
                        HasChildren = directoryIds.Contains(page.Id)
                    });
                    Walk(page.Id, level + 1);
                }
            }

            Walk(0, 0);
            foreach (var page in pages.Where(p => !visited.Contains(p.Id)).OrderBy(p => p.Sort).ThenBy(p => p.Id))
            {
                result.Add(new
                {
                    page.Id,
                    page.ParentId,
                    Level = 0,
                    page.PageName,
                    page.PagePath,
                    page.PageTitle,
                    NavigationTitle = string.IsNullOrWhiteSpace(page.NavigationTitle) ? page.PageName : page.NavigationTitle,
                    page.NavigationIcon,
                    page.NavigationTarget,
                    page.ShowInNavigation,
                    page.Status,
                    page.IsHome,
                    page.IsActive,
                    page.Sort,
                    page.CreationTime,
                    page.PublishTime,
                    HasChildren = directoryIds.Contains(page.Id)
                });
            }
            return result;
        }

        /// <summary>将实体转换为页面模型。</summary>
        private static PageModel ToModel(WebsitePage page)
        {
            return new PageModel
            {
                Id = page.Id,
                SiteId = page.SiteId,
                ParentId = page.ParentId,
                PageName = page.PageName,
                PageCode = page.PageCode,
                PagePath = page.PagePath,
                PageTitle = page.PageTitle,
                SeoKeywords = page.SeoKeywords,
                SeoDescription = page.SeoDescription,
                ShowInNavigation = page.ShowInNavigation,
                NavigationTitle = page.NavigationTitle,
                NavigationIcon = page.NavigationIcon,
                NavigationTarget = page.NavigationTarget,
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

        /// <summary>规范化页面访问路径。</summary>
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

        /// <summary>创建空白页面文档的 JSON 内容。</summary>
        private static string CreateEmptyDocumentJson(string name)
        {
            return JsonConvert.SerializeObject(new BuilderDocumentModel { SchemaVersion = 1, Name = name ?? string.Empty });
        }

        /// <summary>规范化页面装修文档。</summary>
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

        /// <summary>验证页面装修节点。</summary>
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
            if (node.Type == "richText") node.Style.Clear();
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

        /// <summary>解析页面节点及其关联主键。</summary>
        private static List<int> ResolveIds(int id, int[] ids, int isAll)
        {
            var source = isAll == 1 ? (ids ?? Array.Empty<int>()) : new[] { id };
            return source.Where(p => p > 0).Distinct().ToList();
        }
    }
}
