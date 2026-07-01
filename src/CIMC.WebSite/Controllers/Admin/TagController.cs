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
    public class TagController : AdminBaseController
    {
        private readonly IRepository<Tag> _tagRepository;
        private readonly IPermissionService _permission;

        public TagController(IRepository<Tag> tagRepository, IPermissionService permission)
        {
            _tagRepository = tagRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Tags, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Tags, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Tags, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Tags, PermissionType.Delete);
            return View();
        }

        [PermissionFilter(MenuCode.Content_Tags, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new Tag { IsActive = true, Sort = 10 };
            if (id > 0)
            {
                model = _tagRepository.GetOne(id);
                if (model == null)
                {
                    return NotFound();
                }
            }

            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Tags, PermissionType.Edit)]
        public IActionResult Edit(int id, Tag input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写标签名称" };
            if (input == null || string.IsNullOrWhiteSpace(input.TagName))
            {
                return Json(result);
            }

            var tag = id > 0 ? _tagRepository.GetOne(id) : new Tag { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (tag == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            tag.TagName = input.TagName;
            tag.TagName_EN = input.TagName_EN;
            tag.TagType = input.TagType;
            tag.Sort = input.Sort;
            tag.IsActive = input.IsActive;
            tag.UpdateBy = LoginUser.UserName;
            tag.UpdateTime = DateTime.Now;

            if (id > 0)
            {
                _tagRepository.Update(tag);
            }
            else
            {
                _tagRepository.Add(tag);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Tags, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 20)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            _ = int.TryParse(HttpContext.Request.Query["tagType"].ToString(), out var tagType);
            var where = LambdaHelper.True<Tag>();
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.TagName.Contains(keywords));
            }
            if (tagType > 0)
            {
                where = where.And(p => p.TagType == tagType);
            }

            var query = _tagRepository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = query.List });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Tags, PermissionType.Delete)]
        public IActionResult Delete(int id)
        {
            _tagRepository.Delete(id);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }
    }
}
