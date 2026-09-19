using System;
using System.Linq;
using CIMC.Core.Enums;
using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Models;

namespace MySite.Web.Controllers
{
    /// <summary>管理产品内容，包括编辑、批量上传、查询和删除。</summary>
    [Authorize]
    public class ProductController : ContentControllerBase
    {
        private readonly IRepository<Product> _repository; private readonly IPermissionService _permission;

        /// <summary>初始化产品管理控制器。</summary>
        /// <param name="repository">产品数据仓储。</param>
        /// <param name="tags">分类数据仓储。</param>
        /// <param name="permission">菜单权限服务。</param>
        public ProductController(IRepository<Product> repository,IRepository<Tag> tags,IPermissionService permission):base(tags){_repository=repository;_permission=permission;}

        /// <summary>显示产品列表及可用分类。</summary>
        /// <returns>产品管理页面。</returns>
        [PermissionFilter(MenuCode.Content_Product,PermissionType.View)]
        public IActionResult Index(){ViewData[PageCode.PAGE_Button_Add]=_permission.CheckPermission(LoginUser,MenuCode.Content_Product,PermissionType.Add);ViewData[PageCode.PAGE_Button_Edit]=_permission.CheckPermission(LoginUser,MenuCode.Content_Product,PermissionType.Edit);ViewData[PageCode.PAGE_Button_Delete]=_permission.CheckPermission(LoginUser,MenuCode.Content_Product,PermissionType.Delete);return View(GetTags((int)TagType.Product));}

        /// <summary>显示新增或编辑产品的表单。</summary>
        /// <param name="id">产品主键；为 0 时创建新产品。</param>
        /// <returns>产品编辑页面，或未找到结果。</returns>
        [PermissionFilter(MenuCode.Content_Product,PermissionType.Edit)]
        public IActionResult Edit(int id=0){var model=new ProductModel{IsActive=true,Author="中集洋山",TagsList=GetTags((int)TagType.Product)};if(id<=0)return View(model);var entity=_repository.GetOne(id);if(entity==null)return NotFound();model=ToModel(entity);model.TagsList=GetTags((int)TagType.Product);return View(model);}

        /// <summary>保存新增或编辑的产品。</summary>
        /// <param name="id">产品主键；为 0 时创建新产品。</param>
        /// <param name="input">提交的产品字段。</param>
        /// <returns>包含保存结果的 JSON 响应。</returns>
        [HttpPost,PermissionFilter(MenuCode.Content_Product,PermissionType.Edit)]
        public IActionResult Edit(int id,ProductModel input){if(input==null||string.IsNullOrWhiteSpace(input.Title)||string.IsNullOrWhiteSpace(input.ImageUrl))return Json(Result("请填写标题并上传图片",ResultCode.ParmsError));var entity=id>0?_repository.GetOne(id):new Product{CreationTime=DateTime.Now,CreationBy=LoginUser.UserName};if(entity==null)return Json(Result("记录不存在",ResultCode.NULL));Apply(entity,input);if(id>0)_repository.Update(entity);else _repository.Add(entity);return Json(Result("保存成功",ResultCode.Success));}

        /// <summary>显示批量上传产品图片的表单。</summary>
        /// <returns>批量上传页面。</returns>
        [PermissionFilter(MenuCode.Content_Product,PermissionType.Add)]
        public IActionResult BatchUpload()=>View(new ProductModel{IsActive=true,Author="中集洋山",TagsList=GetTags((int)TagType.Product)});

        /// <summary>根据上传的图片批量创建产品。</summary>
        /// <param name="input">包含图片地址和共用字段的表单数据。</param>
        /// <returns>包含创建结果的 JSON 响应。</returns>
        [HttpPost,PermissionFilter(MenuCode.Content_Product,PermissionType.Add)]
        public IActionResult BatchUpload(ProductModel input){var images=input?.ImageList??new();foreach(var image in images.Where(p=>!string.IsNullOrWhiteSpace(p))){var entity=new Product{CreationTime=DateTime.Now,CreationBy=LoginUser.UserName,ImageUrl=image};Apply(entity,input);_repository.Add(entity);}return Json(Result(images.Count>0?"保存成功":"请至少上传一张图片",images.Count>0?ResultCode.Success:ResultCode.ParmsError));}

        /// <summary>按分类分页查询未删除的产品。</summary>
        /// <param name="pageIndex">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <returns>包含产品列表和总数的 JSON 响应。</returns>
        [HttpGet,PermissionFilter(MenuCode.Content_Product,PermissionType.View)]
        public JsonResult GetList(int pageIndex=1,int pageSize=10){int.TryParse(Request.Query["tagsId"],out var tagId);var where=LambdaHelper.True<Product>().And(p=>!p.IsDelete);if(tagId>0)where=where.And(p=>p.TagId==tagId);var query=_repository.GetList(where,p=>p.Sort,Math.Max(1,pageIndex),pageSize<=0?10:pageSize,true);var data=query.List.Select(p=>new{p.Id,p.ImageUrl,p.LinkUrl,p.Title,p.Description,p.Sort,TagName=GetTagName(p.TagId),p.CreationTime,p.IsActive});return Json(new ResultModel<object>{Code=(int)ResultCode.Success,Message="成功",Count=query.Count,Data=data});}

        /// <summary>软删除单个或多个产品。</summary>
        /// <param name="id">单条删除时的产品主键。</param>
        /// <param name="ids">批量删除时的产品主键集合。</param>
        /// <param name="isAll">为 1 时使用批量删除模式。</param>
        /// <returns>包含删除结果的 JSON 响应。</returns>
        [HttpPost,PermissionFilter(MenuCode.Content_Product,PermissionType.Delete)]
        public IActionResult Delete(int id,int[] ids,int isAll=0){foreach(var value in(isAll==1?ids??Array.Empty<int>():new[]{id}).Where(p=>p>0)){var entity=_repository.GetOne(value);if(entity==null)continue;entity.IsDelete=true;entity.UpdateTime=DateTime.Now;entity.UpdateBy=LoginUser.UserName;_repository.Update(entity);}return Json(Result("删除成功",ResultCode.Success));}

        /// <summary>将表单字段应用到产品实体并更新修改信息。</summary>
        private void Apply(Product entity,ProductModel input){entity.Title=input.Title;entity.Title_EN=input.Title_EN;entity.Description=input.Description;entity.Description_EN=input.Description_EN;entity.Detail=input.Detail;entity.Detail_EN=input.Detail_EN;entity.ImageUrl=input.ImageUrl;entity.LinkUrl=input.LinkUrl;entity.Author=string.IsNullOrWhiteSpace(input.Author)?"中集洋山":input.Author;entity.TagType=(int)TagType.Product;entity.TagId=input.TagId;entity.Sort=input.Sort;entity.IsActive=input.IsActive;entity.IsDelete=false;entity.UpdateBy=LoginUser.UserName;entity.UpdateTime=DateTime.Now;}

        /// <summary>构造统一的操作结果。</summary>
        private static ResultModel Result(string message,ResultCode code)=>new ResultModel{Code=(int)code,Message=message};

        /// <summary>将产品实体转换为编辑页面模型。</summary>
        private static ProductModel ToModel(Product p)=>new ProductModel{Id=p.Id,ImageUrl=p.ImageUrl,LinkUrl=p.LinkUrl,Title=p.Title,Title_EN=p.Title_EN,Description=p.Description,Description_EN=p.Description_EN,Detail=p.Detail,Detail_EN=p.Detail_EN,Author=p.Author,TagType=p.TagType,TagId=p.TagId,Sort=p.Sort,IsActive=p.IsActive,IsDelete=p.IsDelete,CreationTime=p.CreationTime,CreationBy=p.CreationBy,UpdateTime=p.UpdateTime,UpdateBy=p.UpdateBy};
    }
}
