using CIMC.Data;
using CIMC.Helper;
using CIMC.EntityFramework;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class RoleController : AdminBaseController
    {

        private IRepository<Role> _roleRepository;
        private IRepository<RoleMenu> _roleMenuRepository;
        private IPermissionService _permission;
        private ICacheService _cache;

        public RoleController(ICacheService cache, IRepository<Role> roleService, IRepository<RoleMenu> rolemenuRepository, IPermissionService permission)
        {
            _roleRepository = roleService;
            _roleMenuRepository = rolemenuRepository;
            _cache = cache;
            _permission = permission;

        }

        [PermissionFilter(MenuCode.System_Role, PermissionType.View)]
        public IActionResult Index()
        {

            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.System_Role, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.System_Role, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.System_Role, PermissionType.Delete);
            ViewData[PageCode.PAGE_Button_Authorize] = _permission.CheckPermission(LoginUser, MenuCode.System_Role, PermissionType.Authorize);

            return View();
        }

        [PermissionFilter(MenuCode.System_Role, PermissionType.View)]
        public ActionResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var result = new ResultModel<List<RoleModel>>();

            var rolename = HttpContext.Request.Query["rolename"].ToString() ?? "";

            var where = LambdaHelper.True<Role>();

            if (!string.IsNullOrWhiteSpace(rolename))
                where = where.And(p => p.RoleName == rolename);

            var query = _roleRepository.GetList(where, p => p.Id, pageIndex, pageSize);

            var data = from q in query.List
                       select new RoleModel()
                       {
                           Id = q.Id,
                           RoleName = q.RoleName,
                           Description = q.Description,
                           IsActive = q.IsActive
                       };

            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Count = query.Count;
            result.Data = data.ToList();

            return Json(result);
        }

        [PermissionFilter(MenuCode.System_Role, PermissionType.View)]
        public ActionResult Edit(int? id)
        {
            if (id.HasValue == false) id = 0;

            RoleModel model = new RoleModel();
            if (id > 0)
            {
                var role = _roleRepository.GetOne(id.Value);
                if (role == null)
                    return View();

                model.Id = role.Id;
                model.RoleName = role.RoleName;
                model.IsActive = role.IsActive;
                model.RoleType = role.RoleType;
                model.Description = role.Description;
            }

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int? id, RoleModel input)
        {
            var result = new ResultModel();

            if (id.HasValue == false) id = 0;

            if (id > 0)
            {
                if (!_permission.CheckPermission(LoginUser, MenuCode.System_Role, PermissionType.Edit))
                {
                    result.Code = (int)ResultCode.Nopermit;
                    result.Message = "无操作权限";
                    return Json(result);
                }

                var editmodel = _roleRepository.GetOne(id.Value);
                editmodel.Id = id.Value;
                editmodel.RoleName = input.RoleName;
                editmodel.RoleType = input.RoleType;
                editmodel.IsActive = input.IsActive;
                editmodel.Description = input.Description;

                if (_roleRepository.Update(editmodel))
                {
                    result.Code = (int)ResultCode.Success;
                    result.Message = "修改成功";
                }

            }
            else
            {
                if (!_permission.CheckPermission(LoginUser, MenuCode.System_Role, PermissionType.Add))
                {
                    result.Code = (int)ResultCode.Nopermit;
                    result.Message = "无操作权限";
                    return Json(result);
                }

                Role model = new Role();
                model.RoleName = input.RoleName;
                model.IsActive = input.IsActive;
                model.RoleType = input.RoleType;
                model.Description = input.Description;

                if (_roleRepository.Add(model).Id > 0)
                {
                    result.Code = (int)ResultCode.Success;
                    result.Message = "添加成功";
                }
            }

            return Json(result);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.System_Role, PermissionType.Delete)]
        public ActionResult Delete(int? id)
        {
            var result = new ResultModel();

            var i = _roleRepository.Delete(id.Value);
            if (i)
            {
                result.Code = (int)ResultCode.Success;
                result.Message = "删除成功！";
            }

            return Json(result);
        }


        [PermissionFilter(MenuCode.System_Role, PermissionType.Authorize)]
        public ActionResult Authorize(int? id)
        {
            if (id.HasValue == false) id = 0;

            var model = new RoleModel();
            model.MenuList = _permission.GetRoleMenus(LoginUser);

            if (id > 0)
            {
                var role = _roleRepository.GetOne(id.Value);
                if (role == null)
                    return View();

                model.Id = role.Id;
                model.RoleName = role.RoleName;
                model.IsActive = role.IsActive;
                model.Description = role.Description;

                var rolemenu = _roleMenuRepository.GetList(p => p.RoleId == id);
                if (rolemenu.Count > 0)
                {
                    model.PermissionList = rolemenu.Select(p => p.Permission).ToArray();
                }
            }

            return View(model);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="Menus"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(MenuCode.System_Role, PermissionType.Authorize)]
        public JsonResult SaveRoleMenu(string[] Menus, int Id = 0)
        {
            var result = new ResultModel();

            if (Menus.Length == 0)
            {
                result.Code = (int)ResultCode.ParmsError;
                result.Message = "请选择菜单";
            }
            else
            {
                var oldArray = _roleMenuRepository.GetList(p => p.RoleId == Id).Select(p => p.Permission).ToArray();
                var newArray = Menus;

                var delArray = oldArray.Except(newArray).ToArray();//新菜单中没有的删除
                var addArray = newArray.Except(oldArray).ToArray();//旧菜单中没有的新增

                //删除
                if (delArray.Length > 0)
                {
                    if (!string.IsNullOrEmpty(delArray[0]))
                    {
                        _roleMenuRepository.DeleteMany(p => p.RoleId == Id && delArray.Contains(p.Permission));
                    }
                }
                //新增
                if (addArray.Length > 0)
                {
                    if (!string.IsNullOrEmpty(addArray[0]))
                    {
                        List<RoleMenu> list = new List<RoleMenu>();
                        foreach (var item in addArray)
                        {
                            RoleMenu model = new RoleMenu();
                            model.Id = Id;
                            model.Permission = item;
                            list.Add(model);
                        }
                        _roleMenuRepository.AddMany(list);
                    }
                }

                //修改角色菜单后清空所有角色缓存
                _cache.RemoveByPrefix(CacheKey.PermissionMenu);
                result.Code = (int)ResultCode.Success;
                result.Message = "保存成功";

            }
            return Json(result);
        }

    }
}