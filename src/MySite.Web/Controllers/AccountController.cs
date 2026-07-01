using System.Security.Claims;
using CIMC.EntityFramework;
using CIMC.Helper;
using CIMC.WebSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MySite.Web.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _dbContext;

    public AccountController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
        var user = await _dbContext.AdminUsers.Include(p => p.Role)
            .FirstOrDefaultAsync(p => p.UserName == input.UserName && p.IsActive && !p.IsDeleted);

        if (user == null || !PasswordHelper.Verify(input.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "账号或密码错误");
            ViewBag.ReturnUrl = returnUrl;
            return View(input);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role?.Code ?? string.Empty),
            new("DisplayName", user.DisplayName)
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
