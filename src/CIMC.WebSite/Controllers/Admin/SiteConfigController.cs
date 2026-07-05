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
            var model = ToModel(entity);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Site_Info, PermissionType.Edit);
            return View(model);
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
            var entity = existed ?? new WebsiteSiteConfig { Id = 1, CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };

            entity.SiteName = input.SiteName.Trim();
            entity.Logo = input.Logo;
            entity.BrowserTitle = input.BrowserTitle;
            entity.Keywords = input.Keywords;
            entity.Description = input.Description;
            entity.IcpNo = input.IcpNo;
            entity.PoliceNo = input.PoliceNo;
            entity.Phone = input.Phone;
            entity.Email = input.Email;
            entity.Address = input.Address;
            entity.Copyright = input.Copyright;
            entity.Theme = string.IsNullOrWhiteSpace(input.Theme) ? "default" : input.Theme;
            entity.Language = string.IsNullOrWhiteSpace(input.Language) ? "zh-CN" : input.Language;
            entity.HeaderBgColor = string.IsNullOrWhiteSpace(input.HeaderBgColor) ? "#ffffff" : input.HeaderBgColor;
            entity.HeaderTextColor = string.IsNullOrWhiteSpace(input.HeaderTextColor) ? "#333333" : input.HeaderTextColor;
            entity.HeaderActiveColor = string.IsNullOrWhiteSpace(input.HeaderActiveColor) ? "#1e9fff" : input.HeaderActiveColor;
            entity.HeaderFixedTop = input.HeaderFixedTop;
            entity.IsActive = input.IsActive;
            entity.IsDelete = false;
            entity.UpdateBy = LoginUser.UserName;
            entity.UpdateTime = DateTime.Now;

            if (existed != null)
            {
                _repository.Update(entity);
            }
            else
            {
                _repository.Add(entity);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        private SiteConfigModel ToModel(WebsiteSiteConfig entity)
        {
            return new SiteConfigModel
            {
                Id = entity.Id,
                SiteName = entity.SiteName,
                Logo = entity.Logo,
                BrowserTitle = entity.BrowserTitle,
                Keywords = entity.Keywords,
                Description = entity.Description,
                IcpNo = entity.IcpNo,
                PoliceNo = entity.PoliceNo,
                Phone = entity.Phone,
                Email = entity.Email,
                Address = entity.Address,
                Copyright = entity.Copyright,
                Theme = entity.Theme,
                Language = entity.Language,
                HeaderBgColor = string.IsNullOrWhiteSpace(entity.HeaderBgColor) ? "#ffffff" : entity.HeaderBgColor,
                HeaderTextColor = string.IsNullOrWhiteSpace(entity.HeaderTextColor) ? "#333333" : entity.HeaderTextColor,
                HeaderActiveColor = string.IsNullOrWhiteSpace(entity.HeaderActiveColor) ? "#1e9fff" : entity.HeaderActiveColor,
                HeaderFixedTop = entity.HeaderFixedTop,
                IsActive = entity.IsActive
            };
        }
    }
}