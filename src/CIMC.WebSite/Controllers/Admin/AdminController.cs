using AutoMapper;
using CIMC.Core.Enums;
using CIMC.Data;
using CIMC.Helper;
using CIMC.EntityFramework;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CimcSite.Web.Controllers
{

    [Authorize]
    public class AdminController : AdminBaseController
    {
        private IConfiguration _configuration;
        private ICacheService _cache;
        private IMapper _mapper;
        private IPermissionService _permission;
        private IRepository<Admin> _adminRepository;
        private IRepository<SiteInfo> _siteInfoRepository;
        private IRepository<FooterInfo> _footerInfoRepository;


        public AdminController(
            IConfiguration configuration,
            ICacheService cache,
             IMapper mapper,
             IPermissionService permission,
             IRepository<Admin> adminRepository,
             IRepository<SiteInfo> siteInfoRepository,
             IRepository<FooterInfo> footerInfoRepository

            )
        {
            _configuration = configuration;
            _cache = cache;
            _mapper = mapper;
            _adminRepository = adminRepository;
            _permission = permission;
            _siteInfoRepository = siteInfoRepository;
            _footerInfoRepository = footerInfoRepository;

        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ReLogin()
        {
            ViewBag.Route = _configuration["App:RouteName"];
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //清空前角色缓存
            _cache.Remove(CacheKey.PermissionMenu + LoginUser.Roles);
            return Json(new ResultModel() { Code = (int)ResultCode.Success, Message = "ok" });

        }

        // 主框架页面仅需要登录认证，左侧菜单已由 GetLeftMenus 按角色权限过滤
        public IActionResult Index()
        {
            var user = LoginUser;
            ViewBag.UserName = user.UserName;
            var model = _permission.GetLeftMenus(user);
            return View(model);
        }

        public IActionResult Main()
        {
            ViewBag.UserName = LoginUser.UserName;

            return View();
        }

        [PermissionFilter(MenuCode.Site_Info, PermissionType.View)]
        public IActionResult SiteInfo()
        {
            var entity = _siteInfoRepository.GetList().FirstOrDefault();
            if (entity != null)
            {
                return View(_mapper.Map<SiteInfoModel>(entity));
            }

            var legacy = ReadJson("site-info.json", new SiteInfoModel
            {
                CompanyName = "上海中集洋山物流装备有限公司",
                CompanyName_EN = "Shanghai CIMC Yangshan Logistics Equipment CO., LTD.",
                Keywords = "中集洋山,集装箱制造,物流装备",
                Description = "上海中集洋山物流装备有限公司，专业从事集装箱及物流装备制造。",
                Logo = "/syle/images/logo-h5.png",
                Logo_H5 = "/syle/images/logo-h5.png"
            });
            return View(legacy);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Info, PermissionType.Edit)]
        public IActionResult SiteInfo(SiteInfoModel input)
        {
            var entity = _siteInfoRepository.GetList().FirstOrDefault();
            if (entity != null)
            {
                _mapper.Map(input, entity);
                _siteInfoRepository.Update(entity);
            }
            else
            {
                entity = _mapper.Map<SiteInfo>(input);
                _siteInfoRepository.Add(entity);
            }
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "保存成功" });
        }

        [PermissionFilter(MenuCode.Site_Footer, PermissionType.View)]
        public IActionResult FooterInfo()
        {
            var entity = _footerInfoRepository.GetList().FirstOrDefault();
            if (entity != null)
            {
                return View(_mapper.Map<FooterModel>(entity));
            }

            var legacy = ReadJson("footer-info.json", new FooterModel
            {
                CompanyInfo = "秉承中集\"众星驱动\"发展战略，高品质、低成本、快交付，志在成为客户首选、员工依赖的物流装备制造商。",
                Address = "上海市浦东新区临港新片区层林路77号",
                Phone = "021-61186770",
                Email = "changhao.shen@cimc.com",
                Copyright = "Copyright © 上海中集洋山物流装备有限公司",
                RecordNo = "沪ICP备案号"
            });
            return View(legacy);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Site_Footer, PermissionType.Edit)]
        public IActionResult FooterInfo(FooterModel input)
        {
            var entity = _footerInfoRepository.GetList().FirstOrDefault();
            if (entity != null)
            {
                _mapper.Map(input, entity);
                _footerInfoRepository.Update(entity);
            }
            else
            {
                entity = _mapper.Map<FooterInfo>(input);
                _footerInfoRepository.Add(entity);
            }
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "保存成功" });
        }


        ///// <summary>
        ///// 获取内存容量
        ///// </summary>
        ///// <returns></returns>
        //private string GetMemoryCapacity()
        //{
        //    using (var ramCounter = new PerformanceCounter("Memory", "Available MBytes"))
        //    {
        //        float availableMemory = ramCounter.NextValue();
        //        float totalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (1024 * 1024); // 转换为MB  
        //        float usedMemory = totalMemory - availableMemory;

        //        return (usedMemory / totalMemory) * 100; // 返回内存使用率百分比  
        //    }
        //}

        /// <summary>
        /// 获取磁盘可用空间
        /// </summary>
        /// <returns></returns>
        private string GetAvailableDiskSpace()
        {
            string diskInfo = string.Empty;

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    diskInfo += $"{drive.Name}: Available Free Space: {drive.AvailableFreeSpace / (1024 * 1024)} MB\n";
                }
            }
            return diskInfo;
        }

        private T ReadJson<T>(string fileName, T defaultValue)
        {
            var path = GetSettingsPath(fileName);
            if (!System.IO.File.Exists(path))
            {
                return defaultValue;
            }

            var json = System.IO.File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json) ? defaultValue : JsonConvert.DeserializeObject<T>(json);
        }

        private void WriteJson<T>(string fileName, T value)
        {
            var path = GetSettingsPath(fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private string GetSettingsPath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "App_Data", fileName);
        }

        public IActionResult ImageSelector()
        {
            return View("~/Views/Shared/ImageSelector.cshtml");
        }

        public IActionResult Password()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UpdatePassword(string txtOld, string txtNew, string txtNew2)
        {
            var result = new ResultModel();

            if (string.IsNullOrEmpty(txtNew))
            {
                result.Message = "请输入新密码！";
                return Json(result);

            }
            if (string.Compare(txtNew, txtNew2) != 0)
            {
                result.Message = "两次密码输入不相同!";
                return Json(result);
            }
            if (string.Compare(txtOld, txtNew) == 0)
            {
                result.Message = "旧密码与新密码不能一样！";
                return Json(result);
            }

            var admin = _adminRepository.GetOne(LoginUser.UserId);

            if (admin != null && admin.Password == StringHelper.ToMD5(txtOld))
            {
                admin.Password = StringHelper.ToMD5(txtNew);
                if (_adminRepository.Update(admin))
                {
                    result.Code = (int)ResultCode.Success;
                    result.Message = "修改成功";
                }
            }
            else
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "原密码输入错误！";
            }

            return Json(result);
        }


    }
}
