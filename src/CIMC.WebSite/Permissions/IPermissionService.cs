using CimcSite.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CimcSite.Web
{
    public interface IPermissionService
    {

        /// <summary>
        /// 检查菜单权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool CheckPermission(LoginAdminModel user, string code, PermissionType type = PermissionType.View);

        /// <summary>
        /// 检查菜单权限
        /// </summary>
        /// <param name="role"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        bool CheckPermission(string role, string code);

        /// <summary>
        /// 检查菜单权限（异步）
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<bool> CheckPermissionAsync(LoginAdminModel user, string code, PermissionType type = PermissionType.View);

        /// <summary>
        /// 检查菜单权限（异步）
        /// </summary>
        /// <param name="role"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<bool> CheckPermissionAsync(string role, string code);

        /// <summary>
        /// 获取角色菜单
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        List<ModuleModel> GetRoleMenus(LoginAdminModel user);

        /// <summary>
        /// 获取左侧菜单
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        List<MenuTreeModel> GetLeftMenus(LoginAdminModel user);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<MenuTreeModel> GetMenuTree();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> GetPermissionKeys();
    }
}
