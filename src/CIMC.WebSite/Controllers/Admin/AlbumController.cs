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
    [Authorize]
    public class AlbumController : ContentControllerBase
    {
        private readonly IRepository<Album> _repository; private readonly IPermissionService _permission;
        public AlbumController(IRepository<Album> repository,IRepository<Tag> tags,IPermissionService permission):base(tags){_repository=repository;_permission=permission;}

        [PermissionFilter(MenuCode.Content_Album,PermissionType.View)]
        public IActionResult Index(){ViewData[PageCode.PAGE_Button_Add]=_permission.CheckPermission(LoginUser,MenuCode.Content_Album,PermissionType.Add);ViewData[PageCode.PAGE_Button_Edit]=_permission.CheckPermission(LoginUser,MenuCode.Content_Album,PermissionType.Edit);ViewData[PageCode.PAGE_Button_Delete]=_permission.CheckPermission(LoginUser,MenuCode.Content_Album,PermissionType.Delete);return View(GetTags((int)TagType.Image));}

        [PermissionFilter(MenuCode.Content_Album,PermissionType.Edit)]
        public IActionResult Edit(int id=0){var model=new AlbumModel{IsActive=true,Author="中集洋山",TagsList=GetTags((int)TagType.Image)};if(id<=0)return View(model);var entity=_repository.GetOne(id);if(entity==null)return NotFound();model=ToModel(entity);model.TagsList=GetTags((int)TagType.Image);return View(model);}

        [HttpPost,PermissionFilter(MenuCode.Content_Album,PermissionType.Edit)]
        public IActionResult Edit(int id,AlbumModel input){if(input==null||string.IsNullOrWhiteSpace(input.Title)||string.IsNullOrWhiteSpace(input.ImageUrl))return Json(Result("请填写标题并上传图片",ResultCode.ParmsError));var entity=id>0?_repository.GetOne(id):new Album{CreationTime=DateTime.Now,CreationBy=LoginUser.UserName};if(entity==null)return Json(Result("记录不存在",ResultCode.NULL));Apply(entity,input);if(id>0)_repository.Update(entity);else _repository.Add(entity);return Json(Result("保存成功",ResultCode.Success));}

        [PermissionFilter(MenuCode.Content_Album,PermissionType.Add)]
        public IActionResult BatchUpload()=>View(new AlbumModel{IsActive=true,Author="中集洋山",TagsList=GetTags((int)TagType.Image)});

        [HttpPost,PermissionFilter(MenuCode.Content_Album,PermissionType.Add)]
        public IActionResult BatchUpload(AlbumModel input){var images=input?.ImageList??new();foreach(var image in images.Where(p=>!string.IsNullOrWhiteSpace(p))){var entity=new Album{CreationTime=DateTime.Now,CreationBy=LoginUser.UserName,ImageUrl=image};Apply(entity,input);_repository.Add(entity);}return Json(Result(images.Count>0?"保存成功":"请至少上传一张图片",images.Count>0?ResultCode.Success:ResultCode.ParmsError));}

        [HttpGet,PermissionFilter(MenuCode.Content_Album,PermissionType.View)]
        public JsonResult GetList(int pageIndex=1,int pageSize=10){int.TryParse(Request.Query["tagsId"],out var tagId);var where=LambdaHelper.True<Album>().And(p=>!p.IsDelete);if(tagId>0)where=where.And(p=>p.TagId==tagId);var query=_repository.GetList(where,p=>p.Sort,Math.Max(1,pageIndex),pageSize<=0?10:pageSize,true);var data=query.List.Select(p=>new{p.Id,p.ImageUrl,p.LinkUrl,p.Title,p.Description,p.Sort,TagName=GetTagName(p.TagId),p.CreationTime,p.IsActive});return Json(new ResultModel<object>{Code=(int)ResultCode.Success,Message="成功",Count=query.Count,Data=data});}

        [HttpPost,PermissionFilter(MenuCode.Content_Album,PermissionType.Delete)]
        public IActionResult Delete(int id,int[] ids,int isAll=0){foreach(var value in(isAll==1?ids??Array.Empty<int>():new[]{id}).Where(p=>p>0)){var entity=_repository.GetOne(value);if(entity==null)continue;entity.IsDelete=true;entity.UpdateTime=DateTime.Now;entity.UpdateBy=LoginUser.UserName;_repository.Update(entity);}return Json(Result("删除成功",ResultCode.Success));}

        private void Apply(Album entity,AlbumModel input){entity.Title=input.Title;entity.Title_EN=input.Title_EN;entity.Description=input.Description;entity.Description_EN=input.Description_EN;entity.Detail=input.Detail;entity.Detail_EN=input.Detail_EN;entity.ImageUrl=input.ImageUrl;entity.LinkUrl=input.LinkUrl;entity.Author=string.IsNullOrWhiteSpace(input.Author)?"中集洋山":input.Author;entity.TagType=(int)TagType.Image;entity.TagId=input.TagId;entity.Sort=input.Sort;entity.IsActive=input.IsActive;entity.IsDelete=false;entity.UpdateBy=LoginUser.UserName;entity.UpdateTime=DateTime.Now;}
        private static ResultModel Result(string message,ResultCode code)=>new ResultModel{Code=(int)code,Message=message};
        private static AlbumModel ToModel(Album p)=>new AlbumModel{Id=p.Id,ImageUrl=p.ImageUrl,LinkUrl=p.LinkUrl,Title=p.Title,Title_EN=p.Title_EN,Description=p.Description,Description_EN=p.Description_EN,Detail=p.Detail,Detail_EN=p.Detail_EN,Author=p.Author,TagType=p.TagType,TagId=p.TagId,Sort=p.Sort,IsActive=p.IsActive,IsDelete=p.IsDelete,CreationTime=p.CreationTime,CreationBy=p.CreationBy,UpdateTime=p.UpdateTime,UpdateBy=p.UpdateBy};
    }
}
