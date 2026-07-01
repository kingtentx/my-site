using CIMC.Data;
using CIMC.Helper;
using CIMC.EntityFramework;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class ManagerController : AdminBaseController
    {
        private IPermissionService _permission;
        private IRepository<Admin> _adminRepository;
        private IRepository<Role> _roleRepository;


        public ManagerController(IPermissionService permission, IRepository<Admin> adminRepository, IRepository<Role> roleService)
        {
            _adminRepository = adminRepository;
            _roleRepository = roleService;
            _permission = permission;

        }

        [PermissionFilter(MenuCode.System_Admin, PermissionType.View)]
        public IActionResult Index()
        {

            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.System_Admin, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.System_Admin, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.System_Admin, PermissionType.Delete);

            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_Admin, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var result = new ResultModel<List<AdminModel>>();

            var username = HttpContext.Request.Query["username"];

            var where = LambdaHelper.True<Admin>();

            if (!string.IsNullOrWhiteSpace(username))
                where = where.And(p => p.UserName.Equals(username));

            var query = _adminRepository.GetList(where, p => p.Id, pageIndex, pageSize);

            var data = from q in query.List
                       select new AdminModel()
                       {
                           Id = q.Id,
                           UserName = q.UserName,
                           RealName = q.RealName,
                           CreationTime = q.CreationTime,
                           IsAdmin = q.IsAdmin,
                           IsActive = q.IsActive,
                           Roles = !string.IsNullOrEmpty(q.Roles) ? q.Roles.Split(',') : new string[] { }
                       };

            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Count = query.Count;
            result.Data = data.ToList();

            return Json(result);
        }

        [PermissionFilter(MenuCode.System_Admin, PermissionType.View)]
        public ActionResult Edit(int? id)
        {
            if (id.HasValue == false) id = 0;

            AdminModel model = new AdminModel();
            if (id > 0)
            {
                var admin = _adminRepository.GetOne(id.Value);
                if (admin == null)
                    return View();

                model.Id = admin.Id;
                model.UserName = admin.UserName;
                model.RealName = admin.RealName;
                model.IsAdmin = admin.IsAdmin;
                model.IsActive = admin.IsActive;
                model.Remark = admin.Remark;
                model.Roles = !string.IsNullOrWhiteSpace(admin.Roles) ? admin.Roles.Split(',') : new string[] { };

            }
            var roledata = _roleRepository.GetList(p => p.IsActive == true);
            var rolellist = from r in roledata
                            select new RoleModel()
                            {
                                Id = r.Id,
                                RoleName = r.RoleName
                            };
            model.RoleList = rolellist.ToList();

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int? id, AdminModel input)
        {

            var result = new ResultModel();

            if (id.HasValue == false) id = 0;

            if (id > 0)
            {
                if (!_permission.CheckPermission(LoginUser, MenuCode.System_Admin, PermissionType.Edit))
                {
                    result.Code = (int)ResultCode.Nopermit;
                    result.Message = "无操作权限";

                    return Json(result);
                }

                var editmodel = _adminRepository.GetOne(id.Value);

                //是否修改系统超级管理员信息
                var user = LoginUser;
                if (!user.IsAdmin)
                {
                    if (editmodel.IsAdmin)
                    {
                        result.Code = (int)ResultCode.Nopermit;
                        result.Message = "无操作权限";
                        return Json(result);
                    }
                }
                editmodel.Id = id.Value;
                editmodel.RealName = input.RealName;
                editmodel.IsAdmin = false;
                editmodel.IsActive = input.IsActive;
                editmodel.Remark = input.Remark;
                editmodel.Roles = string.Join(",", input.Roles);
                editmodel.UpdateTime = DateTime.Now;

                if (!string.IsNullOrEmpty(input.Password))
                    editmodel.Password = StringHelper.ToMD5(input.Password);

                if (_adminRepository.Update(editmodel))
                {
                    result.Code = (int)ResultCode.Success;
                    result.Message = "修改成功";
                }

            }
            else
            {
                if (!_permission.CheckPermission(LoginUser, MenuCode.System_Admin, PermissionType.Add))
                {
                    result.Code = (int)ResultCode.Nopermit;
                    result.Message = "无操作权限";

                    return Json(result);
                }

                var admin = _adminRepository.GetOne(p => p.UserName.ToLower() == input.UserName.ToLower());
                if (admin != null)
                {
                    result.Code = (int)ResultCode.Fail;
                    result.Message = "用户名已存在";
                }
                else
                {
                    Admin model = new Admin();
                    model.UserName = input.UserName;
                    model.RealName = input.RealName;
                    model.IsAdmin = false;
                    model.IsActive = input.IsActive;
                    model.Remark = input.Remark;
                    model.Roles = string.Join(",", input.Roles);
                    model.Password = StringHelper.ToMD5(input.Password);

                    if (_adminRepository.Add(model).Id > 0)
                    {
                        result.Code = (int)ResultCode.Success;
                        result.Message = "添加成功";
                    }
                }
            }

            return Json(result);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.System_Admin, PermissionType.Delete)]
        public async Task<JsonResult> Delete(int id)
        {
            var result = new ResultModel();

            try
            {
                //是否删除超级管理员
                if (_adminRepository.GetOne(id).IsAdmin)
                {
                    result.Code = (int)ResultCode.Nopermit;
                    result.Message = "无操作权限";
                    return Json(result);
                }

                //int[] intArray = Array.ConvertAll<string, int>(ids, s => int.Parse(s));
                var i = await _adminRepository.DeleteAsync(id);
                if (i)
                {
                    result.Code = (int)ResultCode.Success;
                    result.Message = "删除成功！";
                    Log.Information($"用户：{LoginUser.UserName}操作-[删除管理员]：{id}");
                }
            }
            catch (Exception ex)
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "删除失败";
                Log.Error($"用户：{LoginUser.UserName}操作-[删除管理员异常]：" + ex.Message);
            }
            return Json(result);
        }


    }
}