using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class NavigationController : AdminBaseController
    {
        private readonly IRepository<Navigation> _navRepository;
        private readonly IRepository<SiteModule> _moduleRepository;
        private readonly IPermissionService _permission;

        public NavigationController(IRepository<Navigation> navRepository, IRepository<SiteModule> moduleRepository, IPermissionService permission)
        {
            _navRepository = navRepository;
            _moduleRepository = moduleRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Site_Navigation, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Site_Navigation, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Site_Navigation, PermissionType.Delete);
            return View();
        }

        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new Navigation { IsActive = true, IsShow = true, Sort = 10 };
            if (id > 0)
            {
                model = _navRepository.GetOne(id);
                if (model == null)
                {
                    return NotFound();
                }
            }

            ViewData["ParentList"] = _navRepository.GetList(p => p.Pid == 0 && !p.IsDelete, p => p.Sort, true);
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.Edit)]
        public IActionResult Edit(int id, Navigation input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写导航名称" };
            if (input == null || string.IsNullOrWhiteSpace(input.NavigationName))
            {
                return Json(result);
            }

            var nav = id > 0 ? _navRepository.GetOne(id) : new Navigation { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (nav == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            nav.Pid = input.Pid;
            nav.NavigationName = input.NavigationName;
            nav.NavigationName_EN = input.NavigationName_EN;
            nav.RewriteName = input.RewriteName;
            nav.Description = input.Description;
            nav.IsEnableLink = input.IsEnableLink;
            nav.LinkUrl = input.LinkUrl;
            nav.Sort = input.Sort;
            nav.IsHomePage = input.IsHomePage;
            nav.IsShow = input.IsShow;
            nav.IsActive = input.IsActive;
            nav.IsDelete = false;
            nav.UpdateBy = LoginUser.UserName;
            nav.UpdateTime = DateTime.Now;

            if (id > 0)
            {
                _navRepository.Update(nav);
            }
            else
            {
                _navRepository.Add(nav);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 50)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            var where = LambdaHelper.True<Navigation>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.NavigationName.Contains(keywords) || p.RewriteName.Contains(keywords));
            }

            var all = _navRepository.GetList(where, p => p.Sort, true).ToList();
            var pageKeys = all.Select(p => p.IsHomePage ? "home" : p.RewriteName).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToList();
            var moduleCounts = _moduleRepository.GetList(m => m.IsActive && !m.IsDelete && pageKeys.Contains(m.PageKey))
                .GroupBy(m => m.PageKey)
                .ToDictionary(g => g.Key, g => g.Count());

            var data = all.Select(p => new
            {
                p.Id,
                p.Pid,
                p.NavigationName,
                p.RewriteName,
                p.Sort,
                p.IsHomePage,
                p.IsShow,
                p.IsActive,
                p.UpdateTime,
                ModuleCount = p.IsHomePage
                    ? (moduleCounts.ContainsKey("home") ? moduleCounts["home"] : 0)
                    : (!string.IsNullOrWhiteSpace(p.RewriteName) && moduleCounts.ContainsKey(p.RewriteName) ? moduleCounts[p.RewriteName] : 0)
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = all.Count, Data = data });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.View)]
        public JsonResult GetModules(int navigationId)
        {
            var nav = _navRepository.GetOne(navigationId);
            if (nav == null || nav.IsDelete)
            {
                return Json(new ResultModel<object> { Code = (int)ResultCode.NULL, Message = "导航不存在", Data = new object[0] });
            }

            var pageKey = nav.IsHomePage ? "home" : nav.RewriteName;
            var modules = _moduleRepository.GetList(m => !m.IsDelete && m.PageKey == pageKey, m => m.Sort, true).ToList();
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
                m.IsActive
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = modules.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.Delete)]
        public IActionResult Delete(int id)
        {
            var nav = _navRepository.GetOne(id);
            if (nav != null)
            {
                nav.IsDelete = true;
                nav.UpdateTime = DateTime.Now;
                nav.UpdateBy = LoginUser.UserName;
                _navRepository.Update(nav);
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }
    }
}
