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


        public AdminController(
            IConfiguration configuration,
            ICacheService cache,
             IMapper mapper,
             IPermissionService permission,
             IRepository<Admin> adminRepository

            )
        {
            _configuration = configuration;
            _cache = cache;
            _mapper = mapper;
            _adminRepository = adminRepository;
            _permission = permission;

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
