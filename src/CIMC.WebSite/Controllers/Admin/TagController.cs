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
    public class TagController : AdminBaseController
    {
        private readonly IRepository<Tag> _repository; private readonly IPermissionService _permission;
        public TagController(IRepository<Tag> repository,IPermissionService permission){_repository=repository;_permission=permission;}

        [PermissionFilter(MenuCode.Content_Tags,PermissionType.View)]
        public IActionResult Index(){ViewData[PageCode.PAGE_Button_Add]=_permission.CheckPermission(LoginUser,MenuCode.Content_Tags,PermissionType.Add);ViewData[PageCode.PAGE_Button_Edit]=_permission.CheckPermission(LoginUser,MenuCode.Content_Tags,PermissionType.Edit);ViewData[PageCode.PAGE_Button_Delete]=_permission.CheckPermission(LoginUser,MenuCode.Content_Tags,PermissionType.Delete);return View();}

        [PermissionFilter(MenuCode.Content_Tags,PermissionType.Edit)]
        public IActionResult Edit(int id=0){var model=id>0?_repository.GetOne(id):new Tag{IsActive=true,Sort=10};return model==null?NotFound():View(model);}

        [HttpPost,PermissionFilter(MenuCode.Content_Tags,PermissionType.Edit)]
        public IActionResult Edit(int id,Tag input){if(input==null||string.IsNullOrWhiteSpace(input.TagName)||!Enum.IsDefined(typeof(TagType),input.TagType))return Json(Result("请填写分类名称并选择类型",ResultCode.ParmsError));var entity=id>0?_repository.GetOne(id):new Tag{CreationTime=DateTime.Now,CreationBy=LoginUser.UserName};if(entity==null)return Json(Result("记录不存在",ResultCode.NULL));entity.TagName=input.TagName.Trim();entity.TagName_EN=input.TagName_EN?.Trim();entity.TagType=input.TagType;entity.Sort=input.Sort;entity.IsActive=input.IsActive;entity.UpdateBy=LoginUser.UserName;entity.UpdateTime=DateTime.Now;if(id>0)_repository.Update(entity);else _repository.Add(entity);return Json(Result("保存成功",ResultCode.Success));}

        [HttpGet,PermissionFilter(MenuCode.Content_Tags,PermissionType.View)]
        public JsonResult GetList(int pageIndex=1,int pageSize=20){var keyword=Request.Query["keywords"].ToString().Trim();int.TryParse(Request.Query["tagType"],out var tagType);var where=LambdaHelper.True<Tag>();if(!string.IsNullOrWhiteSpace(keyword))where=where.And(p=>p.TagName.Contains(keyword));if(tagType>0)where=where.And(p=>p.TagType==tagType);var query=_repository.GetList(where,p=>p.Sort,Math.Max(1,pageIndex),pageSize<=0?20:pageSize,true);var data=query.List.Select(p=>new{p.Id,p.TagName,p.TagName_EN,p.TagType,TypeName=EnumHelper.GetDescription((TagType)p.TagType),p.Sort,p.IsActive,p.UpdateTime});return Json(new ResultModel<object>{Code=(int)ResultCode.Success,Message="成功",Count=query.Count,Data=data});}

        [HttpGet]
        public IActionResult GetOptions(string contentType){var type=contentType?.ToLowerInvariant() switch{"product"=>(int)TagType.Image,"job"=>(int)TagType.Job,_=>(int)TagType.Article};var rootName=type==(int)TagType.Image?"产品分类":type==(int)TagType.Job?"招聘分类":"新闻分类";var options=_repository.GetList(p=>p.IsActive&&p.TagType==type,p=>p.Sort,true).Select(p=>new{value=p.Id,text=p.TagName,parentId=0}).ToList();return Json(new{code=(int)ResultCode.Success,message="成功",data=new{rootId=0,rootName,allText="全部"+rootName,options}});}

        [HttpPost,PermissionFilter(MenuCode.Content_Tags,PermissionType.Delete)]
        public IActionResult Delete(int id){_repository.Delete(id);return Json(Result("删除成功",ResultCode.Success));}
        private static ResultModel Result(string message,ResultCode code)=>new ResultModel{Code=(int)code,Message=message};
    }
}
