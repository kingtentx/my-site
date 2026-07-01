using CIMC.Data;
using CIMC.Helper;
using CIMC.EntityFramework;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class MessageController : AdminBaseController
    {
        private IPermissionService _permission;
        private IRepository<MessageBoard> _msgService;

        public MessageController(IPermissionService permission, IRepository<MessageBoard> msgService)
        {
            _permission = permission;
            _msgService = msgService;
        }
        [PermissionFilter(MenuCode.Site_Message, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Site_Message, PermissionType.Delete);

            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Site_Message, PermissionType.View)]
        public async Task<JsonResult> GetList(int pageIndex = 1, int pageSize = 10)
        {
            var result = new ResultModel<List<MessageBoard>>() { Code = (int)ResultCode.ParmsError, Message = "失败" };

            var strRead = int.TryParse(HttpContext.Request.Query["IsRead"].ToString(), out int isRead);
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();

            var where = LambdaHelper.True<MessageBoard>();
            if (isRead == 1)//未读
            {
                where = where.And(p => p.IsRead == false);
            }
            if (isRead == 2)//已读
            {
                where = where.And(p => p.IsRead == true);
            }
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.UserName.Contains(keywords)).Or(p => p.Phone.Equals(keywords));
            }

            var query = await _msgService.GetListAsync(where, p => p.Id, pageIndex, pageSize);

            result.Code = (int)ResultCode.Success;
            result.Message = "成攻";
            result.Count = query.Count;
            result.Data = query.List;

            return Json(result);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Message, PermissionType.Delete)]
        public async Task<JsonResult> Delete(long id)
        {
            var result = new ResultModel() { Code = (int)ResultCode.ParmsError, Message = "失败" };

            if (await _msgService.DeleteAsync(id))
            {
                result.Code = (int)ResultCode.Success;
                result.Message = "删除成功";
            }

            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Site_Message, PermissionType.View)]
        public async Task<JsonResult> GetInfo(long id)
        {
            var result = new ResultModel() { Code = (int)ResultCode.ParmsError, Message = "失败" };

            var query = await _msgService.GetOneAsync(id);

            if (query != null)
            {
                query.IsRead = true;
                await _msgService.UpdateAsync(query);

                result.Code = (int)ResultCode.Success;
                result.Message = "成攻";
                result.Data = query;
            }

            return Json(result);
        }

    }
}
