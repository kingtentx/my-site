using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Models;

namespace MySite.Web.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginInput());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginInput input, string? returnUrl = null)
    {
        var userName = _configuration["Admin:UserName"] ?? "admin";
        var password = _configuration["Admin:Password"] ?? "admin123";

        if (!string.Equals(input.UserName, userName, StringComparison.Ordinal) || input.Password != password)
        {
            ModelState.AddModelError(string.Empty, "账号或密码错误");
            ViewBag.ReturnUrl = returnUrl;
            return View(input);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, input.UserName),
            new(ClaimTypes.Role, "Administrator")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Admin");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
