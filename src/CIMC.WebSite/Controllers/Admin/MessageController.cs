using System;
using System.Linq;
using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Models;

namespace MySite.Web.Controllers
{
    /// <summary>处理留言相关的网页请求。</summary>
    [Authorize]
    public class MessageController : AdminBaseController
    {
        private readonly IRepository<MessageBoard> _repository;
        private readonly IPermissionService _permission;

        /// <summary>初始化留言。</summary>
        public MessageController(IRepository<MessageBoard> repository, IPermissionService permission)
        {
            _repository = repository;
            _permission = permission;
        }

        /// <summary>显示留言管理页面。</summary>
        [PermissionFilter(MenuCode.Site_Message, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Site_Message, PermissionType.Delete);
            return View();
        }

        /// <summary>查询留言列表。</summary>
        [HttpGet]
        [PermissionFilter(MenuCode.Site_Message, PermissionType.View)]
        public IActionResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = Request.Query["keywords"].ToString().Trim();
            int.TryParse(Request.Query["isRead"].ToString(), out var isRead);
            var where = LambdaHelper.True<MessageBoard>();
            if (isRead == 1) where = where.And(p => !p.IsRead);
            if (isRead == 2) where = where.And(p => p.IsRead);
            if (!string.IsNullOrWhiteSpace(keywords))
                where = where.And(p => p.UserName.Contains(keywords) || p.Phone.Contains(keywords) || p.Email.Contains(keywords));

            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
            var query = _repository.GetList(where, p => p.CreationTime, pageIndex, pageSize, false);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.UserName,
                p.Phone,
                p.Email,
                p.Message,
                p.IsRead,
                p.CreationTime
            }).ToList();
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        /// <summary>获取指定记录的信息。</summary>
        [HttpGet]
        [PermissionFilter(MenuCode.Site_Message, PermissionType.View)]
        public IActionResult GetInfo(long id)
        {
            var entity = _repository.GetOne(id);
            if (entity == null)
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "留言不存在" });
            if (!entity.IsRead)
            {
                entity.IsRead = true;
                _repository.Update(entity);
            }
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "成功", Data = entity });
        }

        /// <summary>删除指定留言记录。</summary>
        [HttpPost]
        [PermissionFilter(MenuCode.Site_Message, PermissionType.Delete)]
        public IActionResult Delete(long id)
        {
            if (id <= 0 || _repository.GetOne(id) == null)
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "留言不存在" });
            _repository.Delete(id);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }
    }
}
