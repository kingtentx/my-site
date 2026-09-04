using CIMC.Data;
using CIMC.EntityFramework;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class SiteConfigController : AdminBaseController
    {
        private readonly IRepository<WebsiteSiteConfig> _repository;
        private readonly IPermissionService _permission;

        public SiteConfigController(IRepository<WebsiteSiteConfig> repository, IPermissionService permission)
        {
            _repository = repository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Site_Info, PermissionType.View)]
        public IActionResult Index()
        {
            var entity = _repository.GetOne(1) ?? new WebsiteSiteConfig { Id = 1, SiteName = "企业官网", IsActive = true };
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Site_Info, PermissionType.Edit);
            return View(ToModel(entity));
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Info, PermissionType.Edit)]
        public IActionResult Edit(SiteConfigModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写站点名称" };
            if (input == null || string.IsNullOrWhiteSpace(input.SiteName))
            {
                return Json(result);
            }

            var existed = _repository.GetOne(1);
            var entity = existed ?? new WebsiteSiteConfig
            {
                Id = 1,
                CreationTime = DateTime.Now,
                CreationBy = LoginUser.UserName
            };

            entity.SiteName = input.SiteName.Trim();
            entity.Logo = input.Logo;
            entity.BrowserTitle = input.BrowserTitle;
            entity.Keywords = input.Keywords;
            entity.Description = input.Description;
            entity.IsActive = input.IsActive;
            entity.IsDelete = false;
            entity.UpdateBy = LoginUser.UserName;
            entity.UpdateTime = DateTime.Now;

            if (existed != null) _repository.Update(entity);
            else _repository.Add(entity);

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        private static SiteConfigModel ToModel(WebsiteSiteConfig entity)
        {
            return new SiteConfigModel
            {
                Id = entity.Id,
                SiteName = entity.SiteName,
                Logo = entity.Logo,
                BrowserTitle = entity.BrowserTitle,
                Keywords = entity.Keywords,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
    }
}
