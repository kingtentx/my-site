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
    public class ContentCategoryController : AdminBaseController
    {
        private readonly IRepository<ContentProductCategory> _repository;
        private readonly IPermissionService _permission;

        public ContentCategoryController(IRepository<ContentProductCategory> repository, IPermissionService permission)
        {
            _repository = repository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.View)]
        public IActionResult Index()
        {
            ContentCategoryHelper.EnsureRoots(_repository);
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_ProductCategory, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_ProductCategory, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_ProductCategory, PermissionType.Delete);
            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.View)]
        public JsonResult GetList()
        {
            ContentCategoryHelper.EnsureRoots(_repository);
            var list = _repository.GetList(LambdaHelper.True<ContentProductCategory>().And(p => !p.IsDelete), p => p.Sort, true);
            var data = list.Select(p => new
            {
                p.Id,
                p.Pid,
                p.Name,
                p.Sort,
                p.IsActive,
                IsRoot = ContentCategoryHelper.IsFixedRoot(p)
            }).ToList();
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = data.Count, Data = data });
        }

        [HttpGet]
        public IActionResult GetOptions(string contentType)
        {
            ContentCategoryHelper.EnsureRoots(_repository);
            var rootId = ContentCategoryHelper.GetRootId(_repository, contentType);
            var rootName = ContentCategoryHelper.ResolveRootName(contentType);
            var descendants = ContentCategoryHelper.GetDescendants(_repository, rootId, true);
            var all = _repository.GetList(p => !p.IsDelete, p => p.Sort, true);
            var options = descendants.Select(p => new
            {
                value = p.Id,
                text = ContentCategoryHelper.GetIndentedName(p, all, rootId),
                parentId = p.Pid
            }).ToList();
            return Json(new
            {
                code = (int)ResultCode.Success,
                message = "成功",
                data = new { rootId, rootName, allText = "全部" + rootName, options }
            });
        }

        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.Edit)]
        public IActionResult Edit(int id = 0, int pid = 0)
        {
            ContentCategoryHelper.EnsureRoots(_repository);
            var model = new ProductCategoryModel { IsActive = true, Pid = pid };
            if (id > 0)
            {
                var entity = _repository.GetOne(id);
                if (entity == null || entity.IsDelete) return NotFound();
                model = ToModel(entity);
                ViewBag.IsFixedRoot = ContentCategoryHelper.IsFixedRoot(entity);
            }
            else ViewBag.IsFixedRoot = false;
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.Edit)]
        public IActionResult Edit(int id, ProductCategoryModel input)
        {
            ContentCategoryHelper.EnsureRoots(_repository);
            if (input == null || string.IsNullOrWhiteSpace(input.Name))
                return Json(Error("请填写分类名称"));

            var entity = id > 0 ? _repository.GetOne(id) : null;
            if (id > 0 && (entity == null || entity.IsDelete))
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });

            var fixedRoot = ContentCategoryHelper.IsFixedRoot(entity);
            if (id == 0 && input.Pid <= 0)
                return Json(Error("请在文章、产品或招聘大类下添加分类"));
            if (id > 0 && input.Pid == id)
                return Json(Error("不能选择自身作为父级分类"));

            if (fixedRoot)
            {
                input.Pid = 0;
                input.Name = entity.Name;
            }
            else
            {
                var parent = _repository.GetOne(input.Pid);
                if (parent == null || parent.IsDelete) return Json(Error("父级分类不存在"));
                if (id > 0 && ContentCategoryHelper.GetDescendantIds(_repository, id).Contains(input.Pid))
                    return Json(Error("不能移动到自己的子分类下"));
            }

            var name = input.Name.Trim();
            if (_repository.GetList(p => !p.IsDelete && p.Pid == input.Pid && p.Name == name && p.Id != id).Any())
                return Json(Error("同级分类名称已存在"));

            entity ??= new ContentProductCategory { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            entity.Pid = input.Pid;
            entity.Name = name;
            entity.Sort = input.Sort;
            entity.IsActive = input.IsActive;
            entity.IsDelete = false;
            entity.UpdateBy = LoginUser.UserName;
            entity.UpdateTime = DateTime.Now;
            if (id > 0) _repository.Update(entity); else _repository.Add(entity);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "保存成功" });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_ProductCategory, PermissionType.Delete)]
        public IActionResult Delete(int id)
        {
            ContentCategoryHelper.EnsureRoots(_repository);
            var entity = _repository.GetOne(id);
            if (entity == null || entity.IsDelete)
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            if (ContentCategoryHelper.IsFixedRoot(entity)) return Json(Error("系统大类不能删除"));
            if (_repository.GetList(p => p.Pid == id && !p.IsDelete).Any()) return Json(Error("存在子分类，请先删除子分类"));

            entity.IsDelete = true;
            entity.UpdateTime = DateTime.Now;
            entity.UpdateBy = LoginUser.UserName;
            _repository.Update(entity);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private static ResultModel Error(string message)
        {
            return new ResultModel { Code = (int)ResultCode.ParmsError, Message = message };
        }

        private static ProductCategoryModel ToModel(ContentProductCategory entity)
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
