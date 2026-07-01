using CimcSite.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace CimcSite.Web.Controllers
{

    public class AdminBaseController : Controller
    {
        /// <summary>
        /// 后台登录用户
        /// </summary>   
        /// <returns></returns>      
        public LoginAdminModel LoginUser
        {
            get
            {
                var identity = (ClaimsIdentity)HttpContext.User.Identity;

                LoginAdminModel user = new LoginAdminModel()
                {
                    UserId = Convert.ToInt32(identity.FindFirst(ClaimTypes.Sid)?.Value),
                    UserName = identity.FindFirst(ClaimTypes.Name)?.Value,
                    Roles = identity.FindFirst(ClaimTypes.Role)?.Value,
                    IsAdmin = Convert.ToBoolean(identity.FindFirst(ClaimTypes.System)?.Value)
                };

                return user;
            }
        }

        /// <summary>
        /// 获取IP
        /// </summary>
        public static string GetIPAddress()
        {
            var httpContextAccessor = new HttpContextAccessor();
            var ip = httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();//X-Forwarded-For可能会包含多个IP
            if (string.IsNullOrEmpty(ip))
            {
                return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else
            {
                return ip.IndexOf(',') > 0 ? ip.Split(',')[0] : ip;
            }
        }
    }
}
