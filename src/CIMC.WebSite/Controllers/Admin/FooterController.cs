using CIMC.Data;
using CIMC.EntityFramework;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class FooterController : AdminBaseController
    {
        private readonly IRepository<WebsiteFooter> _repository;
        private readonly IRepository<Menu> _menuRepository;
        private readonly IPermissionService _permission;

        public FooterController(
            IRepository<WebsiteFooter> repository,
            IRepository<Menu> menuRepository,
            IPermissionService permission)
        {
            _repository = repository;
            _menuRepository = menuRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Site_Footer, PermissionType.View)]
        public IActionResult Index()
        {
            EnsureFooterMenuIcon();

            var entity = _repository
                .GetList(p => !p.IsDelete, p => p.Id, true)
                .FirstOrDefault();

            if (entity == null)
            {
                entity = new WebsiteFooter
                {
                    Id = 0,
                    CompanyName = "企业官网",
                    BgColor = "#2c3e50",
                    TextColor = "#ffffff",
                    FriendLinks = "[]",
                    IsActive = true
                };
            }

            var model = ToModel(entity);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(
                LoginUser,
                MenuCode.Site_Footer,
                PermissionType.Edit);

            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Footer, PermissionType.Edit)]
        public IActionResult Edit(int id, FooterModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.CompanyName))
            {
                return Json(new ResultModel
                {
                    Code = (int)ResultCode.ParmsError,
                    Message = "请填写公司名称"
                });
            }

            try
            {
                EnsureFooterMenuIcon();

                var friendLinks = string.IsNullOrWhiteSpace(input.FriendLinks)
                    ? "[]"
                    : input.FriendLinks.Trim();

                try
                {
                    JArray.Parse(friendLinks);
                }
                catch
                {
                    return Json(new ResultModel
                    {
                        Code = (int)ResultCode.ParmsError,
                        Message = "友情链接数据格式不正确"
                    });
                }

                WebsiteFooter entity = null;
                if (id > 0)
                {
                    entity = _repository.GetOne(p => p.Id == id && !p.IsDelete);
                }

                entity ??= _repository
                    .GetList(p => !p.IsDelete, p => p.Id, true)
                    .FirstOrDefault();

                var isNew = entity == null;
                if (isNew)
                {
                    entity = new WebsiteFooter
                    {
                        Id = 0,
                        CreationTime = DateTime.Now,
                        CreationBy = LoginUser.UserName
                    };
                }

                entity.Logo = input.Logo?.Trim();
                entity.CompanyName = input.CompanyName.Trim();
                entity.Intro = input.Intro?.Trim();
                entity.Phone = input.Phone?.Trim();
                entity.Email = input.Email?.Trim();
                entity.Address = input.Address?.Trim();
                entity.Qrcode = input.Qrcode?.Trim();
                entity.IcpNo = input.IcpNo?.Trim();
                entity.PoliceNo = input.PoliceNo?.Trim();
                entity.Copyright = input.Copyright?.Trim();
                entity.FriendLinks = friendLinks;
                entity.BgColor = string.IsNullOrWhiteSpace(input.BgColor) ? "#2c3e50" : input.BgColor.Trim();
                entity.TextColor = string.IsNullOrWhiteSpace(input.TextColor) ? "#ffffff" : input.TextColor.Trim();
                entity.IsActive = input.IsActive;
                entity.IsDelete = false;
                entity.UpdateBy = LoginUser.UserName;
                entity.UpdateTime = DateTime.Now;

                var saved = isNew
                    ? _repository.Add(entity)?.Id > 0
                    : _repository.Update(entity);

                if (!saved)
                {
                    return Json(new ResultModel
                    {
                        Code = (int)ResultCode.Error,
                        Message = "页脚设置保存失败，请稍后重试"
                    });
                }

                return Json(new
                {
                    code = (int)ResultCode.Success,
                    message = "保存成功",
                    data = new { id = entity.Id }
                });
            }
            catch (Exception ex)
            {
                return Json(new ResultModel
                {
                    Code = (int)ResultCode.Error,
                    Message = "保存失败：" + ex.GetBaseException().Message
                });
            }
        }

        private void EnsureFooterMenuIcon()
        {
            var menu = _menuRepository.GetOne(p =>
                !p.IsDelete && p.PermissionKey == MenuCode.Site_Footer);

            if (menu == null || !string.IsNullOrWhiteSpace(menu.Icon))
            {
                return;
            }

            menu.Icon = "layui-icon-layouts";
            menu.UpdateBy = LoginUser.UserName;
            menu.UpdateTime = DateTime.Now;
            _menuRepository.Update(menu);
        }

        private static FooterModel ToModel(WebsiteFooter entity)
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
                FriendLinks = string.IsNullOrWhiteSpace(entity.FriendLinks) ? "[]" : entity.FriendLinks,
                BgColor = string.IsNullOrWhiteSpace(entity.BgColor) ? "#2c3e50" : entity.BgColor,
                TextColor = string.IsNullOrWhiteSpace(entity.TextColor) ? "#ffffff" : entity.TextColor,
                IsActive = entity.IsActive
            };
        }
    }
}
