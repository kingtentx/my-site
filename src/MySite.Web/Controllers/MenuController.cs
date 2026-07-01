using CIMC.Core.Enums;
using CIMC.Data.Entities;
using CIMC.EntityFramework;
using CIMC.WebSite.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MySite.Web.Controllers;

[Authorize]
[PermissionFilter("Menu", PermissionType.View)]
public class MenuController : Controller
{
    private readonly AppDbContext _dbContext;

    public MenuController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var menus = await _dbContext.Menus.Where(p => !p.IsDeleted).OrderBy(p => p.Sort).ToListAsync();
        return View(menus);
    }

    [HttpPost]
    [PermissionFilter("Menu", PermissionType.Edit)]
    public async Task<IActionResult> Save(Menu input)
    {
        if (string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.Name))
        {
            return RedirectToAction(nameof(Index));
        }

        var menu = input.Id > 0 ? await _dbContext.Menus.FindAsync(input.Id) : null;
        if (menu == null)
        {
            menu = new Menu { CreationTime = DateTime.Now, CreatedBy = User.Identity?.Name };
            _dbContext.Menus.Add(menu);
        }

        menu.Code = input.Code.Trim();
        menu.Name = input.Name.Trim();
        menu.Path = input.Path ?? string.Empty;
        menu.Icon = input.Icon;
        menu.Sort = input.Sort;
        menu.IsEnabled = input.IsEnabled;
        menu.UpdateTime = DateTime.Now;
        menu.UpdatedBy = User.Identity?.Name;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
