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
    public class JobController : ContentControllerBase
    {
        private readonly IRepository<Job> _repository; private readonly IPermissionService _permission;
        public JobController(IRepository<Job> repository, IRepository<Tag> tags, IPermissionService permission) : base(tags) { _repository=repository; _permission=permission; }

        [PermissionFilter(MenuCode.Content_Job, PermissionType.View)]
        public IActionResult Index() { ViewData[PageCode.PAGE_Button_Add]=_permission.CheckPermission(LoginUser,MenuCode.Content_Job,PermissionType.Add); ViewData[PageCode.PAGE_Button_Edit]=_permission.CheckPermission(LoginUser,MenuCode.Content_Job,PermissionType.Edit); ViewData[PageCode.PAGE_Button_Delete]=_permission.CheckPermission(LoginUser,MenuCode.Content_Job,PermissionType.Delete); return View(GetTags((int)TagType.Job)); }

        [PermissionFilter(MenuCode.Content_Job, PermissionType.Edit)]
        public IActionResult Edit(int id=0) { var model=new JobModel{IsActive=true,Author="中集洋山",UpdateTime=DateTime.Now,TagsList=GetTags((int)TagType.Job)}; if(id<=0)return View(model); var entity=_repository.GetOne(id); if(entity==null)return NotFound(); model=ToModel(entity); model.TagsList=GetTags((int)TagType.Job); return View(model); }

        [HttpPost, PermissionFilter(MenuCode.Content_Job, PermissionType.Edit)]
        public IActionResult Edit(int id, JobModel input) { if(input==null||string.IsNullOrWhiteSpace(input.JobName))return Json(Result("请填写岗位名称",ResultCode.ParmsError)); var entity=id>0?_repository.GetOne(id):new Job{CreationTime=DateTime.Now,CreationBy=LoginUser.UserName}; if(entity==null)return Json(Result("记录不存在",ResultCode.NULL)); entity.JobName=input.JobName;entity.JobName_EN=input.JobName_EN;entity.Author=string.IsNullOrWhiteSpace(input.Author)?"中集洋山":input.Author;entity.Detail=input.Detail;entity.Detail_EN=input.Detail_EN;entity.TagType=(int)TagType.Job;entity.TagId=input.TagId;entity.IsActive=input.IsActive;entity.IsDelete=false;entity.UpdateBy=LoginUser.UserName;entity.UpdateTime=input.UpdateTime??DateTime.Now;if(id>0)_repository.Update(entity);else _repository.Add(entity);return Json(Result("保存成功",ResultCode.Success)); }

        [HttpGet, PermissionFilter(MenuCode.Content_Job, PermissionType.View)]
        public JsonResult GetList(int pageIndex=1,int pageSize=10) { var keyword=Request.Query["keywords"].ToString().Trim();int.TryParse(Request.Query["tagId"],out var tagId);if(tagId==0)int.TryParse(Request.Query["tagsId"],out tagId);var where=LambdaHelper.True<Job>().And(p=>!p.IsDelete);if(!string.IsNullOrWhiteSpace(keyword))where=where.And(p=>p.JobName.Contains(keyword));if(tagId>0)where=where.And(p=>p.TagId==tagId);var query=_repository.GetList(where,p=>p.CreationTime,Math.Max(1,pageIndex),pageSize<=0?10:pageSize,false);var data=query.List.Select(p=>new{p.Id,p.JobName,TagName=GetTagName(p.TagId),p.UpdateTime,p.IsActive});return Json(new ResultModel<object>{Code=(int)ResultCode.Success,Message="成功",Count=query.Count,Data=data}); }

        [HttpPost, PermissionFilter(MenuCode.Content_Job, PermissionType.Delete)]
        public IActionResult Delete(int id,int[] ids,int isAll=0) { foreach(var value in(isAll==1?ids??Array.Empty<int>():new[]{id}).Where(p=>p>0)){var entity=_repository.GetOne(value);if(entity==null)continue;entity.IsDelete=true;entity.UpdateTime=DateTime.Now;entity.UpdateBy=LoginUser.UserName;_repository.Update(entity);}return Json(Result("删除成功",ResultCode.Success)); }
        private static ResultModel Result(string message,ResultCode code)=>new ResultModel{Code=(int)code,Message=message};
        private static JobModel ToModel(Job p)=>new JobModel{Id=p.Id,JobName=p.JobName,JobName_EN=p.JobName_EN,Author=p.Author,Detail=p.Detail,Detail_EN=p.Detail_EN,TagType=p.TagType,TagId=p.TagId,IsActive=p.IsActive,IsDelete=p.IsDelete,CreationTime=p.CreationTime,UpdateTime=p.UpdateTime,CreateBy=p.CreationBy,UpdateBy=p.UpdateBy};
    }
}
