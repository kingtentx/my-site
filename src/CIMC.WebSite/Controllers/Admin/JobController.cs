using CIMC.Core.Enums;
using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class JobController : ContentControllerBase
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IPermissionService _permission;

        public JobController(IRepository<Job> jobRepository, IRepository<Tag> tagRepository, IPermissionService permission)
            : base(tagRepository)
        {
            _jobRepository = jobRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Job, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Job, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Job, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Job, PermissionType.Delete);
            return View(GetTags((int)TagType.Job));
        }

        [PermissionFilter(MenuCode.Content_Job, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new JobModel { IsActive = true, Author = "中集洋山", TagId = 0, UpdateTime = DateTime.Now, TagsList = GetTags((int)TagType.Job) };
            if (id > 0)
            {
                var job = _jobRepository.GetOne(id);
                if (job == null)
                {
                    return NotFound();
                }

                model = ToModel(job);
                model.TagsList = GetTags((int)TagType.Job);
            }

            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Job, PermissionType.Edit)]
        public IActionResult Edit(int id, JobModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写岗位名称" };
            if (input == null || string.IsNullOrWhiteSpace(input.JobName))
            {
                return Json(result);
            }

            var job = id > 0 ? _jobRepository.GetOne(id) : new Job { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (job == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            job.JobName = input.JobName;
            job.JobName_EN = input.JobName_EN;
            job.Author = string.IsNullOrWhiteSpace(input.Author) ? "中集洋山" : input.Author;
            job.Detail = input.Detail;
            job.Detail_EN = input.Detail_EN;
            job.TagType = (int)TagType.Job;
            job.TagId = input.TagId;
            job.IsActive = input.IsActive;
            job.IsDelete = false;
            job.UpdateBy = LoginUser.UserName;
            job.UpdateTime = input.UpdateTime ?? DateTime.Now;

            if (id > 0)
            {
                _jobRepository.Update(job);
            }
            else
            {
                _jobRepository.Add(job);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Job, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            _ = int.TryParse(HttpContext.Request.Query["tagId"].ToString(), out var tagId);
            if (tagId == 0)
            {
                _ = int.TryParse(HttpContext.Request.Query["tagsId"].ToString(), out tagId);
            }

            var where = LambdaHelper.True<Job>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.JobName.Contains(keywords));
            }

            if (tagId > 0)
            {
                where = where.And(p => p.TagId == tagId);
            }

            var query = _jobRepository.GetList(where, p => p.CreationTime, pageIndex, pageSize, false);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.JobName,
                TagName = GetTagName(p.TagId),
                p.UpdateTime,
                p.IsActive
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Job, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids, int isAll = 0)
        {
            var deleteIds = isAll == 1 ? ids : new[] { id };
            foreach (var deleteId in deleteIds.Where(p => p > 0))
            {
                var job = _jobRepository.GetOne(deleteId);
                if (job != null)
                {
                    job.IsDelete = true;
                    job.UpdateTime = DateTime.Now;
                    job.UpdateBy = LoginUser.UserName;
                    _jobRepository.Update(job);
                }
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private JobModel ToModel(Job job)
        {
            return new JobModel
            {
                Id = job.Id,
                JobName = job.JobName,
                JobName_EN = job.JobName_EN,
                Author = job.Author,
                Detail = job.Detail,
                Detail_EN = job.Detail_EN,
                TagType = job.TagType,
                TagId = job.TagId,
                IsActive = job.IsActive,
                IsDelete = job.IsDelete,
                CreationTime = job.CreationTime,
                UpdateTime = job.UpdateTime,
                CreateBy = job.CreationBy,
                UpdateBy = job.UpdateBy
            };
        }
    }
}
