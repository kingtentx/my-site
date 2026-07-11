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
    public class JobController : AdminBaseController
    {
        private readonly IRepository<ContentJob> _repository;
        private readonly IRepository<ContentProductCategory> _categoryRepository;
        private readonly IPermissionService _permission;

        public JobController(
            IRepository<ContentJob> repository,
            IRepository<ContentProductCategory> categoryRepository,
            IPermissionService permission)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Job, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Job, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Job, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Job, PermissionType.Delete);
            return View();
        }

        [PermissionFilter(MenuCode.Content_Job, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new JobModel { IsActive = true, RecruitCount = 1, JobType = "全职" };
            if (id > 0)
            {
                var entity = _repository.GetOne(id);
                if (entity == null || entity.IsDelete) return NotFound();
                model = ToModel(entity);
            }
            LoadCategories();
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Job, PermissionType.Edit)]
        public IActionResult Edit(int id, JobModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.JobTitle))
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写岗位名称" });

            var rootId = ContentCategoryHelper.GetRootId(_categoryRepository, "job");
            var allowedIds = ContentCategoryHelper.GetDescendantIds(_categoryRepository, rootId);
            if (input.CategoryId > 0 && !allowedIds.Contains(input.CategoryId))
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择有效的招聘分类" });

            var entity = id > 0 ? _repository.GetOne(id) : new ContentJob { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (entity == null || entity.IsDelete)
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });

            entity.CategoryId = input.CategoryId;
            entity.JobTitle = input.JobTitle.Trim();
            entity.Department = input.Department;
            entity.WorkLocation = input.WorkLocation;
            entity.SalaryRange = input.SalaryRange;
            entity.RecruitCount = input.RecruitCount <= 0 ? 1 : input.RecruitCount;
            entity.JobType = string.IsNullOrWhiteSpace(input.JobType) ? "全职" : input.JobType;
            entity.Responsibilities = input.Responsibilities;
            entity.Requirements = input.Requirements;
            entity.ContactName = input.ContactName;
            entity.ContactPhone = input.ContactPhone;
            entity.ContactEmail = input.ContactEmail;
            entity.Sort = input.Sort;
            entity.IsActive = input.IsActive;
            entity.IsDelete = false;
            entity.PublishTime = entity.IsActive ? entity.PublishTime ?? DateTime.Now : null;
            entity.UpdateBy = LoginUser.UserName;
            entity.UpdateTime = DateTime.Now;
            if (id > 0) _repository.Update(entity); else _repository.Add(entity);

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "保存成功" });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Job, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            int.TryParse(HttpContext.Request.Query["categoryId"].ToString(), out var categoryId);
            var where = LambdaHelper.True<ContentJob>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
                where = where.And(p => p.JobTitle.Contains(keywords) || p.Department.Contains(keywords));
            if (categoryId > 0) where = where.And(p => p.CategoryId == categoryId);

            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var query = _repository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            var categories = _categoryRepository.GetList(p => !p.IsDelete).ToDictionary(p => p.Id, p => p.Name);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.CategoryId,
                CategoryName = categories.TryGetValue(p.CategoryId, out var name) ? name : "未分类",
                p.JobTitle,
                p.Department,
                p.WorkLocation,
                p.SalaryRange,
                p.RecruitCount,
                p.JobType,
                p.Sort,
                p.IsActive,
                p.PublishTime,
                p.CreationTime
            }).ToList();
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Job, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids = null, int isAll = 0)
        {
            var deleteIds = (isAll == 1 ? (ids ?? Array.Empty<int>()) : new[] { id }).Where(p => p > 0).Distinct().ToList();
            if (!deleteIds.Any())
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择要删除的数据" });

            foreach (var deleteId in deleteIds)
            {
                var entity = _repository.GetOne(deleteId);
                if (entity == null || entity.IsDelete) continue;
                entity.IsDelete = true;
                entity.UpdateTime = DateTime.Now;
                entity.UpdateBy = LoginUser.UserName;
                _repository.Update(entity);
            }
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private void LoadCategories()
        {
            var rootId = ContentCategoryHelper.GetRootId(_categoryRepository, "job");
            ViewBag.Categories = ContentCategoryHelper.GetDescendants(_categoryRepository, rootId, true);
        }

        private static JobModel ToModel(ContentJob entity)
        {
            return new JobModel
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                JobTitle = entity.JobTitle,
                Department = entity.Department,
                WorkLocation = entity.WorkLocation,
                SalaryRange = entity.SalaryRange,
                RecruitCount = entity.RecruitCount,
                JobType = entity.JobType,
                Responsibilities = entity.Responsibilities,
                Requirements = entity.Requirements,
                ContactName = entity.ContactName,
                ContactPhone = entity.ContactPhone,
                ContactEmail = entity.ContactEmail,
                Sort = entity.Sort,
                IsActive = entity.IsActive,
                PublishTime = entity.PublishTime,
                CreationTime = entity.CreationTime,
                CreationBy = entity.CreationBy,
                UpdateTime = entity.UpdateTime,
                UpdateBy = entity.UpdateBy
            };
        }
    }
}
