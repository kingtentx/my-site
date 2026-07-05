using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class NavigationController : AdminBaseController
    {
        private readonly IRepository<WebsiteNavigation> _repository;
        private readonly IPermissionService _permission;

        public NavigationController(IRepository<WebsiteNavigation> repository, IPermissionService permission)
        {
            _repository = repository;
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

        [HttpGet]
        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.View)]
        public JsonResult GetList()
        {
            var where = LambdaHelper.True<WebsiteNavigation>().And(p => !p.IsDelete);
            var list = _repository.GetList(where, p => p.Sort, true);

            var data = list.Select(p => new
            {
                p.Id,
                p.Pid,
                p.Title,
                p.Path,
                p.Icon,
                p.Target,
                p.Sort,
                p.IsShow,
                p.IsActive
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = data.Count, Data = data });
        }

        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.Edit)]
        public IActionResult Edit(int id = 0, int pid = 0)
        {
            var model = new NavigationModel { IsShow = true, IsActive = true, Sort = 0, Pid = pid };
            if (id > 0)
            {
                var entity = _repository.GetOne(id);
                if (entity == null || entity.IsDelete)
                {
                    return NotFound();
                }
                model = ToModel(entity);
            }
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.Edit)]
        public IActionResult Edit(int id, NavigationModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写标题" };
            if (input == null || string.IsNullOrWhiteSpace(input.Title))
            {
                return Json(result);
            }
            if (id > 0 && input.Pid == id)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "不能选择自身作为父级导航" });
            }
            if (input.Pid > 0)
            {
                var parent = _repository.GetOne(input.Pid);
                if (parent == null || parent.IsDelete)
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "父级导航不存在" });
                }
            }

            var title = input.Title.Trim();
            var exists = _repository.GetList(p => !p.IsDelete && p.Pid == input.Pid && p.Title == title && p.Id != id);
            if (exists.Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "同级导航标题已存在" });
            }

            var entity = id > 0 ? _repository.GetOne(id) : new WebsiteNavigation { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (entity == null || entity.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            entity.Pid = input.Pid;
            entity.Title = title;
            entity.Path = string.IsNullOrWhiteSpace(input.Path) ? "#" : input.Path.Trim();
            entity.Icon = input.Icon;
            entity.Target = input.Target;
            entity.Sort = input.Sort;
            entity.IsShow = input.IsShow;
            entity.IsActive = input.IsActive;
            entity.IsDelete = false;
            entity.UpdateBy = LoginUser.UserName;
            entity.UpdateTime = DateTime.Now;

            if (id > 0)
            {
                _repository.Update(entity);
            }
            else
            {
                _repository.Add(entity);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Navigation, PermissionType.Delete)]
        public IActionResult Delete(int id)
        {
            var entity = _repository.GetOne(id);
            if (entity == null || entity.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            var children = _repository.GetList(p => p.Pid == id && !p.IsDelete);
            if (children.Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "存在子菜单，请先删除子菜单" });
            }

            entity.IsDelete = true;
            entity.UpdateTime = DateTime.Now;
            entity.UpdateBy = LoginUser.UserName;
            _repository.Update(entity);

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private NavigationModel ToModel(WebsiteNavigation entity)
        {
            return new NavigationModel
            {
                Id = entity.Id,
                Pid = entity.Pid,
                Title = entity.Title,
                Path = entity.Path,
                Icon = entity.Icon,
                Target = entity.Target,
                Sort = entity.Sort,
                IsShow = entity.IsShow,
                IsActive = entity.IsActive
            };
        }
    }
}