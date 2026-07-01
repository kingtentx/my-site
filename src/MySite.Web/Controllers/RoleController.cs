using CIMC.Core.Enums;
using CIMC.Data.Entities;
using CIMC.EntityFramework;
using CIMC.WebSite.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MySite.Web.Controllers;

[Authorize]
[PermissionFilter("Role", PermissionType.View)]
public class RoleController : Controller
{
    private readonly AppDbContext _dbContext;

    public RoleController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(int roleId = 0)
    {
        ViewBag.Menus = await _dbContext.Menus.Where(p => p.IsEnabled && !p.IsDeleted).OrderBy(p => p.Sort).ToListAsync();
        ViewBag.RoleMenus = await _dbContext.RoleMenus.Where(p => roleId > 0 && p.RoleId == roleId).ToListAsync();
        ViewBag.CurrentRoleId = roleId;
        var roles = await _dbContext.Roles.Where(p => !p.IsDeleted).OrderBy(p => p.Id).ToListAsync();
        return View(roles);
    }

    [HttpPost]
    [PermissionFilter("Role", PermissionType.Edit)]
    public async Task<IActionResult> SaveRole(Role input)
    {
        if (string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.Name))
        {
            return RedirectToAction(nameof(Index));
        }

        var role = input.Id > 0 ? await _dbContext.Roles.FindAsync(input.Id) : null;
        if (role == null)
        {
            role = new Role { CreationTime = DateTime.Now, CreatedBy = User.Identity?.Name };
            _dbContext.Roles.Add(role);
        }

        role.Code = input.Code.Trim();
        role.Name = input.Name.Trim();
        role.Description = input.Description;
        role.UpdateTime = DateTime.Now;
        role.UpdatedBy = User.Identity?.Name;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { roleId = role.Id });
    }

    [HttpPost]
    [PermissionFilter("Role", PermissionType.Edit)]
    public async Task<IActionResult> SavePermissions(int roleId, int[] menuIds, int[] viewIds, int[] addIds, int[] editIds, int[] deleteIds)
    {
        var old = await _dbContext.RoleMenus.Where(p => p.RoleId == roleId).ToListAsync();
        _dbContext.RoleMenus.RemoveRange(old);

        foreach (var menuId in menuIds.Distinct())
        {
            _dbContext.RoleMenus.Add(new RoleMenu
            {
                RoleId = roleId,
                MenuId = menuId,
                CanView = viewIds.Contains(menuId),
                CanAdd = addIds.Contains(menuId),
                CanEdit = editIds.Contains(menuId),
                CanDelete = deleteIds.Contains(menuId),
                CreatedBy = User.Identity?.Name,
                CreationTime = DateTime.Now
            });
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { roleId });
    }
}
