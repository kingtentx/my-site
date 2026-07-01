using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class ImagesController : AdminBaseController
    {
        private readonly IRepository<Images> _repository;
        private readonly IPermissionService _permission;
        private readonly IWebHostEnvironment _environment;

        public ImagesController(IRepository<Images> repository, IPermissionService permission, IWebHostEnvironment environment)
        {
            _repository = repository;
            _permission = permission;
            _environment = environment;
        }

        [PermissionFilter(MenuCode.Content_Images, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Images, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Images, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Images, PermissionType.Delete);
            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Images, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 15)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            var extName = HttpContext.Request.Query["extName"].ToString().Trim();
            var where = LambdaHelper.True<Images>().And(p => !string.IsNullOrWhiteSpace(p.Url));
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.FileName.Contains(keywords) || p.Url.Contains(keywords));
            }
            if (!string.IsNullOrWhiteSpace(extName))
            {
                where = where.And(p => p.ExtensionName == extName);
            }
            var query = _repository.GetList(where, p => p.Id, pageIndex, pageSize, false);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.FileName,
                p.Url,
                p.ExtensionName,
                SizeDisplay = p.Size > 1024 * 1024 ? (p.Size / 1024m / 1024m).ToString("F2") + " MB" : (p.Size / 1024m).ToString("F0") + " KB",
                p.CreationBy,
                CreationTime = p.CreationTime?.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Images, PermissionType.Delete)]
        public IActionResult Delete(long id)
        {
            var image = _repository.GetOne(id);
            if (image == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            if (!string.IsNullOrWhiteSpace(image.Url))
            {
                var filePath = Path.Combine(_environment.WebRootPath, image.Url.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch { }
                }
            }

            _repository.Delete(id);

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }
    }
}
