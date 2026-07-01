using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class SiteModuleController : AdminBaseController
    {
        private readonly IRepository<SiteModule> _moduleRepository;
        private readonly IRepository<Navigation> _navRepository;
        private readonly IPermissionService _permission;

        public SiteModuleController(IRepository<SiteModule> moduleRepository, IRepository<Navigation> navRepository, IPermissionService permission)
        {
            _moduleRepository = moduleRepository;
            _navRepository = navRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Module, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Module, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Module, PermissionType.Delete);
            return View();
        }

        [PermissionFilter(MenuCode.Content_Module, PermissionType.Edit)]
        public IActionResult Edit(int id = 0, int? navigationId = null)
        {
            ViewData["LinkOptions"] = GetLinkOptions();
            ViewData["ModuleOptions"] = GetModuleOptions();
            ViewData["NavigationOptions"] = GetNavigationOptions();
            var model = new SiteModule { PageKey = "home", ModuleKey = "hero", ModuleName = "首页Banner", ModuleType = "banner", IsActive = true, Sort = 10 };
            if (id > 0)
            {
                model = _moduleRepository.GetOne(id);
                if (model == null)
                {
                    return NotFound();
                }
            }
            else if (navigationId.HasValue && navigationId.Value > 0)
            {
                var nav = _navRepository.GetOne(navigationId.Value);
                if (nav != null)
                {
                    model.PageKey = nav.IsHomePage ? "home" : (nav.RewriteName ?? "home");
                    model.NavigationId = navigationId.Value;
                }
            }

            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.Edit)]
        public IActionResult Edit(int id, SiteModule input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择所属页面和模块位置" };
            if (input == null || string.IsNullOrWhiteSpace(input.PageKey) || string.IsNullOrWhiteSpace(input.ModuleKey))
            {
                return Json(result);
            }

            var enableLink = string.Equals(Request.Form["EnableLink"], "true", StringComparison.OrdinalIgnoreCase);
            var module = id > 0 ? _moduleRepository.GetOne(id) : new SiteModule { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (module == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            module.PageKey = input.PageKey;
            module.ModuleKey = input.ModuleKey;
            module.ModuleName = string.IsNullOrWhiteSpace(input.ModuleName) ? GetModuleDisplayName(input.ModuleKey) : input.ModuleName;
            module.ModuleName_EN = input.ModuleName_EN;
            module.ModuleType = ResolveModuleType(input.ModuleKey, input.ModuleType);
            module.Title = input.Title;
            module.Title_EN = input.Title_EN;
            module.SubTitle = input.SubTitle;
            module.SubTitle_EN = input.SubTitle_EN;
            module.LinkUrl = enableLink ? input.LinkUrl : string.Empty;
            module.ImageUrl = input.ImageUrl;
            module.Sort = input.Sort;
            module.IsActive = input.IsActive;
            module.NavigationId = input.NavigationId;
            module.SettingsJson = Request.Form["SettingsJson"].ToString();
            module.SettingsJson_EN = Request.Form["SettingsJson_EN"].ToString();
            module.IsDelete = false;
            module.UpdateBy = LoginUser.UserName;
            module.UpdateTime = DateTime.Now;

            if (id > 0)
            {
                _moduleRepository.Update(module);
            }
            else
            {
                _moduleRepository.Add(module);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            var pageKey = HttpContext.Request.Query["pageKey"].ToString().Trim();
            var navigationIdStr = HttpContext.Request.Query["navigationId"].ToString().Trim();
            var where = LambdaHelper.True<SiteModule>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.ModuleName.Contains(keywords) || p.Title.Contains(keywords));
            }

            if (!string.IsNullOrWhiteSpace(pageKey))
            {
                where = where.And(p => p.PageKey == pageKey);
            }

            if (int.TryParse(navigationIdStr, out int navigationId) && navigationId > 0)
            {
                var nav = _navRepository.GetOne(navigationId);
                if (nav != null)
                {
                    var navPageKey = nav.IsHomePage ? "home" : (nav.RewriteName ?? "");
                    if (!string.IsNullOrWhiteSpace(navPageKey))
                    {
                        where = where.And(p => p.PageKey == navPageKey);
                    }
                }
            }

            var query = _moduleRepository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = query.List });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.Delete)]
        public IActionResult Delete(int id)
        {
            var module = _moduleRepository.GetOne(id);
            if (module != null)
            {
                module.IsDelete = true;
                module.UpdateTime = DateTime.Now;
                module.UpdateBy = LoginUser.UserName;
                _moduleRepository.Update(module);
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult GetNavigationModules(int navigationId)
        {
            var result = new ResultModel<object> { Code = (int)ResultCode.ParmsError, Message = "请传入导航ID" };
            if (navigationId <= 0)
            {
                return Json(result);
            }

            var nav = _navRepository.GetOne(navigationId);
            if (nav == null)
            {
                result.Message = "导航不存在";
                return Json(result);
            }

            var pageKey = nav.IsHomePage ? "home" : (nav.RewriteName ?? "");
            var modules = _moduleRepository.GetList(p => !p.IsDelete && p.PageKey == pageKey, p => p.Sort, true);
            var data = modules.Select(m => new
            {
                m.Id,
                m.PageKey,
                m.ModuleKey,
                m.ModuleName,
                m.ModuleType,
                m.Title,
                m.SubTitle,
                m.LinkUrl,
                m.ImageUrl,
                m.Sort,
                m.IsActive,
                m.NavigationId,
                Settings = string.IsNullOrWhiteSpace(m.SettingsJson) ? null : JsonConvert.DeserializeObject<object>(m.SettingsJson)
            }).ToList();

            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Count = data.Count;
            result.Data = data;
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult Preview(int id)
        {
            var result = new ResultModel<object> { Code = (int)ResultCode.ParmsError, Message = "模块不存在" };
            var module = _moduleRepository.GetOne(id);
            if (module == null)
            {
                return Json(result);
            }

            var url = GetPagePreviewUrl(module.PageKey);
            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Data = new { Url = url, PageKey = module.PageKey };
            return Json(result);
        }

        private SelectListItem[] GetLinkOptions()
        {
            var items = new List<SelectListItem> { new SelectListItem("不跳转", "") };
            var navigations = _navRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.Sort, true);
            foreach (var nav in navigations)
            {
                var url = nav.IsHomePage ? "/" : (nav.IsEnableLink && !string.IsNullOrWhiteSpace(nav.LinkUrl) ? nav.LinkUrl : "/" + nav.RewriteName);
                items.Add(new SelectListItem(nav.NavigationName, url));
            }

            return items.ToArray();
        }

        private SelectListItem[] GetModuleOptions()
        {
            var existing = _moduleRepository.GetList(p => !p.IsDelete, p => p.Sort, true)
                .GroupBy(p => p.ModuleKey)
                .Select(g => g.FirstOrDefault())
                .Where(p => p != null)
                .Select(p => new SelectListItem(p.ModuleName ?? p.ModuleKey, p.ModuleKey))
                .ToList();

            var defaults = new List<SelectListItem>
            {
                new SelectListItem("Banner", "hero"),
                new SelectListItem("产品服务", "products"),
                new SelectListItem("企业介绍", "about"),
                new SelectListItem("合作客户", "partners"),
                new SelectListItem("新闻资讯", "news"),
                new SelectListItem("人才招聘", "careers"),
                new SelectListItem("内页Banner", "banner"),
                new SelectListItem("资质证书", "certificates"),
                new SelectListItem("在线留言", "message")
            };

            foreach (var d in defaults)
            {
                if (!existing.Any(e => e.Value == d.Value))
                {
                    existing.Add(d);
                }
            }

            return existing.ToArray();
        }

        private SelectListItem[] GetNavigationOptions()
        {
            var items = new List<SelectListItem> { new SelectListItem("请选择导航", "0") };
            var navigations = _navRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.Sort, true);
            foreach (var nav in navigations)
            {
                items.Add(new SelectListItem(nav.NavigationName, nav.Id.ToString()));
            }

            return items.ToArray();
        }

        private string ResolveModuleType(string moduleKey, string fallback)
        {
            if (string.Equals(moduleKey, "hero", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(moduleKey, "banner", StringComparison.OrdinalIgnoreCase))
            {
                return "banner";
            }

            if (string.Equals(moduleKey, "certificates", StringComparison.OrdinalIgnoreCase))
            {
                return "carousel";
            }

            if (string.Equals(moduleKey, "message", StringComparison.OrdinalIgnoreCase))
            {
                return "form";
            }

            return string.IsNullOrWhiteSpace(fallback) ? "section" : fallback;
        }

        private string GetModuleDisplayName(string moduleKey)
        {
            return GetModuleOptions().FirstOrDefault(p => p.Value == moduleKey)?.Text ?? moduleKey;
        }

        private string GetPagePreviewUrl(string pageKey)
        {
            return pageKey?.ToLower() switch
            {
                "home" => "/",
                "about" => "/about",
                "products" => "/products",
                "news" => "/news",
                "jobs" => "/jobs",
                "contact" => "/contact",
                _ => "/" + pageKey
            };
        }
    }
}
