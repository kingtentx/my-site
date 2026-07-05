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
    public class ProductCategoryController : AdminBaseController
    {
        private readonly IRepository<ContentProductCategory> _repository;
        private readonly IPermissionService _permission;

        public ProductCategoryController(IRepository<ContentProductCategory> repository, IPermissionService permission)
        {
            _repository = repository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_ProductCategory, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_ProductCategory, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_ProductCategory, PermissionType.Delete);
            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.View)]
        public JsonResult GetList()
        {
            var where = LambdaHelper.True<ContentProductCategory>().And(p => !p.IsDelete);
            var list = _repository.GetList(where, p => p.Sort, true);
            var data = list.Select(p => new
            {
                p.Id,
                p.Pid,
                p.Name,
                p.Sort,
                p.IsActive
            }).ToList();
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = data.Count, Data = data });
        }

        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.Edit)]
        public IActionResult Edit(int id = 0, int pid = 0)
        {
            var model = new ProductCategoryModel { IsActive = true, Sort = 0, Pid = pid };
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
        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.Edit)]
        public IActionResult Edit(int id, ProductCategoryModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写分类名称" };
            if (input == null || string.IsNullOrWhiteSpace(input.Name))
            {
                return Json(result);
            }
            if (id > 0 && input.Pid == id)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "不能选择自身作为父级分类" });
            }
            if (input.Pid > 0)
            {
                var parent = _repository.GetOne(input.Pid);
                if (parent == null || parent.IsDelete)
                {
                    return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "父级分类不存在" });
                }
            }

            var name = input.Name.Trim();
            var exists = _repository.GetList(p => !p.IsDelete && p.Pid == input.Pid && p.Name == name && p.Id != id);
            if (exists.Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "同级分类名称已存在" });
            }

            var entity = id > 0 ? _repository.GetOne(id) : new ContentProductCategory { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (entity == null || entity.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            entity.Pid = input.Pid;
            entity.Name = name;
            entity.Sort = input.Sort;
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
        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.Delete)]
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
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "存在子分类，请先删除子分类" });
            }

            entity.IsDelete = true;
            entity.UpdateTime = DateTime.Now;
            entity.UpdateBy = LoginUser.UserName;
            _repository.Update(entity);

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private ProductCategoryModel ToModel(ContentProductCategory entity)
        {
            return new ProductCategoryModel
            {
                Id = entity.Id,
                Pid = entity.Pid,
                Name = entity.Name,
                Sort = entity.Sort,
                IsActive = entity.IsActive
            };
        }
    }
}