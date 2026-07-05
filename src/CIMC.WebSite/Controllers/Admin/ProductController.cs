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
    public class ProductController : AdminBaseController
    {
        private readonly IRepository<ContentProduct> _repository;
        private readonly IRepository<ContentProductCategory> _categoryRepository;
        private readonly IPermissionService _permission;

        public ProductController(
            IRepository<ContentProduct> repository,
            IRepository<ContentProductCategory> categoryRepository,
            IPermissionService permission)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Product, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Product, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Product, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Product, PermissionType.Delete);
            return View();
        }

        [PermissionFilter(MenuCode.Content_Product, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new ProductModel { IsActive = true, Sort = 0, CategoryId = 0 };
            if (id > 0)
            {
                var entity = _repository.GetOne(id);
                if (entity == null || entity.IsDelete)
                {
                    return NotFound();
                }
                model = ToModel(entity);
            }
            ViewBag.Categories = _categoryRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true);
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Product, PermissionType.Edit)]
        public IActionResult Edit(int id, ProductModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写产品名称" };
            if (input == null || string.IsNullOrWhiteSpace(input.ProductName))
            {
                return Json(result);
            }

            var entity = id > 0 ? _repository.GetOne(id) : new ContentProduct { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (entity == null || entity.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            entity.ProductName = input.ProductName.Trim();
            entity.CategoryId = input.CategoryId;
            entity.CoverImage = input.CoverImage;
            entity.ImageList = input.ImageList;
            entity.Summary = input.Summary;
            entity.Description = input.Description;
            entity.Specification = input.Specification;
            entity.Feature = input.Feature;
            entity.Sort = input.Sort;
            entity.IsRecommend = input.IsRecommend;
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

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Product, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            _ = int.TryParse(HttpContext.Request.Query["categoryId"].ToString(), out var categoryId);

            var where = LambdaHelper.True<ContentProduct>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.ProductName.Contains(keywords));
            }
            if (categoryId > 0)
            {
                where = where.And(p => p.CategoryId == categoryId);
            }

            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var query = _repository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.ProductName,
                p.CategoryId,
                p.CoverImage,
                p.Summary,
                p.Sort,
                p.IsRecommend,
                p.IsActive,
                p.ViewCount,
                p.CreationTime
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Product, PermissionType.Edit)]
        public IActionResult SetRecommend(int id, bool isRecommend)
        {
            var entity = _repository.GetOne(id);
            if (entity == null || entity.IsDelete)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            entity.IsRecommend = isRecommend;
            entity.UpdateTime = DateTime.Now;
            entity.UpdateBy = LoginUser.UserName;
            _repository.Update(entity);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "设置成功" });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Product, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids = null, int isAll = 0)
        {
            var deleteIds = (isAll == 1 ? (ids ?? Array.Empty<int>()) : new[] { id })
                .Where(p => p > 0)
                .Distinct()
                .ToList();
            if (!deleteIds.Any())
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择要删除的数据" });
            }

            foreach (var deleteId in deleteIds)
            {
                var entity = _repository.GetOne(deleteId);
                if (entity != null && !entity.IsDelete)
                {
                    entity.IsDelete = true;
                    entity.UpdateTime = DateTime.Now;
                    entity.UpdateBy = LoginUser.UserName;
                    _repository.Update(entity);
                }
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private ProductModel ToModel(ContentProduct entity)
        {
            return new ProductModel
            {
                Id = entity.Id,
                ProductName = entity.ProductName,
                CategoryId = entity.CategoryId,
                CoverImage = entity.CoverImage,
                ImageList = entity.ImageList,
                Summary = entity.Summary,
                Description = entity.Description,
                Specification = entity.Specification,
                Feature = entity.Feature,
                Sort = entity.Sort,
                IsRecommend = entity.IsRecommend,
                IsActive = entity.IsActive,
                ViewCount = entity.ViewCount,
                CreationTime = entity.CreationTime,
                CreationBy = entity.CreationBy,
                UpdateTime = entity.UpdateTime,
                UpdateBy = entity.UpdateBy
            };
        }
    }
}