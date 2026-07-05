using CIMC.Data;
using CIMC.EntityFramework;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class FooterController : AdminBaseController
    {
        private readonly IRepository<WebsiteFooter> _repository;
        private readonly IPermissionService _permission;

        public FooterController(IRepository<WebsiteFooter> repository, IPermissionService permission)
        {
            _repository = repository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Site_Footer, PermissionType.View)]
        public IActionResult Index()
        {
            var entity = _repository.GetOne(1) ?? new WebsiteFooter { Id = 1, CompanyName = "企业官网", BgColor = "#2c3e50", TextColor = "#ffffff", IsActive = true };
            var model = ToModel(entity);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Site_Footer, PermissionType.Edit);
            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Footer, PermissionType.Edit)]
        public IActionResult Edit(FooterModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写公司名称" };
            if (input == null || string.IsNullOrWhiteSpace(input.CompanyName))
            {
                return Json(result);
            }

            var entity = _repository.GetOne(1);
            if (entity == null)
            {
                entity = new WebsiteFooter { Id = 1, CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            }

            entity.Logo = input.Logo;
            entity.CompanyName = input.CompanyName;
            entity.Intro = input.Intro;
            entity.Phone = input.Phone;
            entity.Email = input.Email;
            entity.Address = input.Address;
            entity.Qrcode = input.Qrcode;
            entity.IcpNo = input.IcpNo;
            entity.PoliceNo = input.PoliceNo;
            entity.Copyright = input.Copyright;
            entity.FriendLinks = string.IsNullOrWhiteSpace(input.FriendLinks) ? "[]" : input.FriendLinks;
            entity.BgColor = input.BgColor;
            entity.TextColor = input.TextColor;
            entity.IsActive = input.IsActive;
            entity.IsDelete = false;
            entity.UpdateBy = LoginUser.UserName;
            entity.UpdateTime = DateTime.Now;

            if (entity.Id > 0 && _repository.GetOne(1) != null)
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

        private FooterModel ToModel(WebsiteFooter entity)
        {
            return new FooterModel
            {
                Id = entity.Id,
                Logo = entity.Logo,
                CompanyName = entity.CompanyName,
                Intro = entity.Intro,
                Phone = entity.Phone,
                Email = entity.Email,
                Address = entity.Address,
                Qrcode = entity.Qrcode,
                IcpNo = entity.IcpNo,
                PoliceNo = entity.PoliceNo,
                Copyright = entity.Copyright,
                FriendLinks = entity.FriendLinks,
                BgColor = entity.BgColor,
                TextColor = entity.TextColor,
                IsActive = entity.IsActive
            };
        }
    }
}
