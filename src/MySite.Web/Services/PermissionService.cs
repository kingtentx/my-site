using System.Security.Claims;
using CIMC.Core;
using CIMC.Core.Enums;
using CIMC.Data.Entities;
using CIMC.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace CIMC.WebSite.Services;

public interface IPermissionService
{
    Task<List<Menu>> GetUserMenusAsync(ClaimsPrincipal user);
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string menuCode, PermissionType permissionType);
}

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _dbContext;

    public PermissionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Menu>> GetUserMenusAsync(ClaimsPrincipal user)
    {
        var userName = user.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return new List<Menu>();
        }

        var admin = await _dbContext.AdminUsers.Include(p => p.Role).FirstOrDefaultAsync(p => p.UserName == userName && p.IsActive && !p.IsDeleted);
        if (admin == null)
        {
            return new List<Menu>();
        }

        if (admin.Role?.Code == Consts.SuperAdminRoleCode)
        {
            return await _dbContext.Menus.Where(p => p.IsEnabled && !p.IsDeleted).OrderBy(p => p.Sort).ToListAsync();
        }

        return await _dbContext.RoleMenus
            .Include(p => p.Menu)
            .Where(p => p.RoleId == admin.RoleId && p.CanView && p.Menu != null && p.Menu.IsEnabled && !p.Menu.IsDeleted)
            .Select(p => p.Menu!)
            .OrderBy(p => p.Sort)
            .ToListAsync();
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string menuCode, PermissionType permissionType)
    {
        var userName = user.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        var admin = await _dbContext.AdminUsers.Include(p => p.Role).FirstOrDefaultAsync(p => p.UserName == userName && p.IsActive && !p.IsDeleted);
        if (admin?.Role == null)
        {
            return false;
        }

        if (admin.Role.Code == Consts.SuperAdminRoleCode)
        {
            return true;
        }

        var roleMenu = await _dbContext.RoleMenus.Include(p => p.Menu)
            .FirstOrDefaultAsync(p => p.RoleId == admin.RoleId && p.Menu != null && p.Menu.Code == menuCode);
        if (roleMenu == null)
        {
            return false;
        }

        return permissionType switch
        {
            PermissionType.View => roleMenu.CanView,
            PermissionType.Add => roleMenu.CanAdd,
            PermissionType.Edit => roleMenu.CanEdit,
            PermissionType.Delete => roleMenu.CanDelete,
            _ => false
        };
    }
}
