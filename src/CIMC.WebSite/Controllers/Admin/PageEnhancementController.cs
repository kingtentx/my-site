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
    /// <summary>
    /// 页面管理增强接口。保留旧 PageController 的发布和版本能力，
    /// 在这里集中处理页面树、导航同步和单组件临时保存。
    /// </summary>
    [Authorize]
    public class PageEnhancementController : AdminBaseController
    {
        private readonly IRepository<WebsitePage> _pageRepository;
        private readonly IRepository<WebsiteNavigation> _navigationRepository;

        public PageEnhancementController(
            IRepository<WebsitePage> pageRepository,
            IRepository<WebsiteNavigation> navigationRepository)
        {
            _pageRepository = pageRepository;
            _navigationRepository = navigationRepository;
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public IActionResult GetTree(string keywords = null)
        {
            var pages = _pageRepository.GetList(p => !p.IsDelete, p => p.Sort, true)
                .OrderBy(p => p.ParentId)
                .ThenBy(p => p.Sort)
                .ThenBy(p => p.Id)
                .ToList();

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                var value = keywords.Trim();
                var matchedIds = pages
                    .Where(p => (p.PageName ?? string.Empty).Contains(value, StringComparison.OrdinalIgnoreCase)
                             || (p.PagePath ?? string.Empty).Contains(value, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id)
                    .ToHashSet();

                foreach (var parentId in pages.Where(p => matchedIds.Contains(p.Id)).Select(p => p.ParentId).Where(p => p > 0))
                {
                    matchedIds.Add(parentId);
                }
                pages = pages.Where(p => matchedIds.Contains(p.Id)).ToList();
            }

            var data = pages.Select(p => new
            {
                p.Id,
                Pid = p.ParentId,
                p.ParentId,
                p.PageName,
                p.PagePath,
                NavigationTitle = string.IsNullOrWhiteSpace(p.NavigationTitle) ? p.PageName : p.NavigationTitle,
                p.ShowInNavigation,
                p.NavigationTarget,
                p.Status,
                p.IsHome,
                p.IsActive,
                p.Sort,
                p.CreationTime,
                p.PublishTime
            }).ToList();

            return Json(new ResultModel<object>
            {
                Code = (int)ResultCode.Success,
                Message = "成功",
                Count = data.Count,
                Data = data
            });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.View)]
        public IActionResult GetNavigationSettings(int id = 0, int parentId = 0)
        {
            var page = id > 0 ? _pageRepository.GetOne(id) : null;
            var parents = _pageRepository.GetList(p => !p.IsDelete && p.ParentId == 0 && p.Id != id, p => p.Sort, true)
                .Select(p => new { p.Id, p.PageName, p.PagePath })
                .ToList();

            return Json(new
            {
                code = (int)ResultCode.Success,
                message = "成功",
                data = new
                {
                    parentId = page?.ParentId ?? parentId,
                    showInNavigation = page?.ShowInNavigation ?? true,
                    navigationTitle = page?.NavigationTitle ?? page?.PageName ?? string.Empty,
                    navigationIcon = page?.NavigationIcon ?? string.Empty,
                    navigationTarget = page?.NavigationTarget ?? 0,
                    parents
                }
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Edit)]
        public IActionResult SaveNavigationSettings(
            int id,
            string pagePath,
            int parentId = 0,
            bool showInNavigation = true,
            string navigationTitle = null,
            string navigationIcon = null,
            int navigationTarget = 0)
        {
            var page = id > 0 ? _pageRepository.GetOne(id) : null;
            if (page == null && !string.IsNullOrWhiteSpace(pagePath))
            {
                var normalizedPath = NormalizePath(pagePath);
                page = _pageRepository.GetOne(p => !p.IsDelete && p.PagePath == normalizedPath);
            }
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }

            if (parentId == page.Id)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "页面不能作为自己的父级" });
            }

            if (parentId > 0)
            {
                var parent = _pageRepository.GetOne(parentId);
                if (parent == null || parent.IsDelete)
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "父级页面不存在" });
                }
                if (parent.ParentId > 0)
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "前台导航最多支持两级，请选择一级页面" });
                }
            }

            if (page.IsHome && parentId > 0)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "首页不能设置为二级导航" });
            }

            page.ParentId = parentId;
            page.ShowInNavigation = showInNavigation;
            page.NavigationTitle = string.IsNullOrWhiteSpace(navigationTitle) ? page.PageName : navigationTitle.Trim();
            page.NavigationIcon = navigationIcon?.Trim();
            page.NavigationTarget = navigationTarget == 1 ? 1 : 0;
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);

            SyncNavigation(page);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "页面及导航设置已保存" });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Design)]
        public IActionResult SaveComponentDraft(int pageId, string componentId, string componentJson)
        {
            var page = _pageRepository.GetOne(pageId);
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }
            if (string.IsNullOrWhiteSpace(componentId) || string.IsNullOrWhiteSpace(componentJson))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "组件数据不能为空" });
            }

            JObject component;
            try
            {
                component = JObject.Parse(componentJson);
            }
            catch
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "组件配置不是合法 JSON" });
            }

            var type = component["type"]?.ToString();
            if (string.Equals(type, "navigation", StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, "footer", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "公共导航和页脚不能在页面中保存" });
            }

            component["id"] = componentId;
            JArray components;
            try
            {
                components = string.IsNullOrWhiteSpace(page.ComponentJson)
                    ? new JArray()
                    : JArray.Parse(page.ComponentJson);
            }
            catch
            {
                components = new JArray();
            }

            var old = components.OfType<JObject>()
                .FirstOrDefault(p => string.Equals(p["id"]?.ToString(), componentId, StringComparison.Ordinal));
            if (old == null) components.Add(component);
            else old.Replace(component);

            var clean = new JArray(components.Where(p =>
            {
                var itemType = p?["type"]?.ToString();
                return !string.Equals(itemType, "navigation", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(itemType, "footer", StringComparison.OrdinalIgnoreCase);
            }));
            for (var index = 0; index < clean.Count; index++)
            {
                if (clean[index] is JObject item) item["sort"] = index + 1;
            }

            page.ComponentJson = clean.ToString(Formatting.None);
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);

            return Json(new
            {
                code = (int)ResultCode.Success,
                message = "当前组件已临时保存",
                savedAt = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Website_Page, PermissionType.Delete)]
        public IActionResult Delete(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "页面不存在" });
            }
            if (page.IsHome)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "首页不能直接删除" });
            }
            if (_pageRepository.GetList(p => !p.IsDelete && p.ParentId == id).Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "该页面下存在二级导航，请先删除子页面" });
            }

            page.IsDelete = true;
            page.UpdateBy = LoginUser.UserName;
            page.UpdateTime = DateTime.Now;
            _pageRepository.Update(page);

            var nav = _navigationRepository.GetOne(p => !p.IsDelete && p.Path == page.PagePath);
            if (nav != null)
            {
                nav.IsDelete = true;
                nav.UpdateBy = LoginUser.UserName;
                nav.UpdateTime = DateTime.Now;
                _navigationRepository.Update(nav);
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private WebsiteNavigation SyncNavigation(WebsitePage page)
        {
            var parentNavigationId = 0;
            if (page.ParentId > 0)
            {
                var parentPage = _pageRepository.GetOne(page.ParentId);
                if (parentPage != null && !parentPage.IsDelete)
                {
                    parentNavigationId = SyncNavigation(parentPage).Id;
                }
            }

            var navigation = _navigationRepository.GetOne(p => !p.IsDelete && p.Path == page.PagePath)
                ?? new WebsiteNavigation
                {
                    CreationBy = LoginUser.UserName,
                    CreationTime = DateTime.Now
                };

            navigation.Pid = parentNavigationId;
            navigation.Title = string.IsNullOrWhiteSpace(page.NavigationTitle) ? page.PageName : page.NavigationTitle;
            navigation.Path = page.PagePath;
            navigation.Icon = page.NavigationIcon;
            navigation.Target = page.NavigationTarget == 1 ? 1 : 0;
            navigation.Sort = page.Sort;
            navigation.IsShow = page.ShowInNavigation;
            navigation.IsActive = page.IsActive;
            navigation.IsDelete = false;
            navigation.UpdateBy = LoginUser.UserName;
            navigation.UpdateTime = DateTime.Now;

            if (navigation.Id > 0) _navigationRepository.Update(navigation);
            else _navigationRepository.Add(navigation);
            return navigation;
        }

        private static string NormalizePath(string path)
        {
            path = (path ?? string.Empty).Trim().Replace("\\", "/");
            if (string.IsNullOrEmpty(path)) return "/";
            if (!path.StartsWith('/')) path = "/" + path;
            while (path.Contains("//")) path = path.Replace("//", "/");
            return path.Length > 1 ? path.TrimEnd('/') : path;
        }
    }
}
