using CIMC.Data;
using CIMC.Helper;
using CIMC.EntityFramework;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CimcSite.Web
{
    public class PermissionService : IPermissionService
    {
        private IWebHostEnvironment _hostingEnv;
        private IRepository<RoleMenu> _roleMenuRepository;
        private IRepository<Menu> _menuRepository;
        private ICacheService _cache;

        public PermissionService(
            ICacheService cache,
            IWebHostEnvironment hostingEnv,
            IRepository<RoleMenu> rolemenuRepository,
            IRepository<Menu> menuRepository
            )
        {
            _roleMenuRepository = rolemenuRepository;
            _cache = cache;
            _hostingEnv = hostingEnv;
            _menuRepository = menuRepository;
        }

        #region 菜单

        /// <summary>
        /// 获取当前角色的菜单模块
        /// </summary>
        /// <param name="user">角色ID</param>     
        /// <returns></returns>
        public List<ModuleModel> GetRoleMenus(LoginAdminModel user)
        {
            List<ModuleModel> menuList = new List<ModuleModel>();

            List<string> list = new List<string>();
            if (!user.IsAdmin)
            {
                list = this.GetRolePermission(user.Roles);
            }

            foreach (var module in GetMenuTree())
            {
                if (module.Spread)
                {
                    continue;
                }

                var model = new ModuleModel();
                model.Name = module.Title;
                model.PermissionKey = module.PermissionKey;
                if (list.Contains(module.PermissionKey) || user.IsAdmin)
                {
                    model.IsChecked = true;
                }

                if (module.Children != null)
                {
                    List<MenuDto> menus = new List<MenuDto>();
                    foreach (var menu in module.Children)
                    {
                        var node = new MenuDto();
                        node.Name = menu.Title;
                        node.PermissionKey = menu.PermissionKey;
                        node.Path = menu.Href;
                        if (list.Contains(menu.PermissionKey) || user.IsAdmin)
                        {
                            node.IsChecked = true;
                        }

                        if (menu.Buttons != null)
                        {
                            List<ButtonDto> buttons = new List<ButtonDto>();
                            foreach (var button in menu.Buttons)
                            {
                                var btn = new ButtonDto();
                                btn.Name = EnumHelper.GetDescription<PermissionType>(button);
                                btn.PermissionKey = menu.PermissionKey + "_" + button;
                                if (list.Contains(btn.PermissionKey) || user.IsAdmin)
                                {
                                    btn.IsChecked = true;
                                }
                                buttons.Add(btn);

                            }
                            node.Buttons = buttons;
                        }
                        menus.Add(node);
                    }
                    model.Menus = menus;
                }
                menuList.Add(model);
            }
            return menuList;
        }

        /// <summary>
        /// 获取左侧菜单
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<MenuTreeModel> GetLeftMenus(LoginAdminModel user)
        {
            if (user.IsAdmin)
            {
                return GetMenuTree();
            }

            List<MenuTreeModel> menuList = new List<MenuTreeModel>();
            var list = this.GetRolePermission(user.Roles);

            foreach (var module in GetMenuTree())
            {
                var model = new MenuTreeModel();
                if (module.Spread)
                {
                    model.Title = module.Title;
                    //model.PermissionKey = module.PermissionKey;
                    model.Href = module.Href;
                    model.Icon = module.Icon;
                    model.Spread = module.Spread;

                    menuList.Add(model);
                }
                else
                {
                    if (list.Contains(module.PermissionKey))
                    {
                        model.Title = module.Title;
                        //model.PermissionKey = module.PermissionKey;
                        model.Href = module.Href;
                        model.Icon = module.Icon;
                        model.Spread = module.Spread;

                        if (module.Children != null)
                        {
                            List<MenuTreeModel> menus = new List<MenuTreeModel>();
                            foreach (var menu in module.Children)
                            {
                                var node = new MenuTreeModel();
                                if (list.Contains(menu.PermissionKey))
                                {
                                    node.Title = menu.Title;
                                    //node.PermissionKey = menu.PermissionKey;
                                    node.Href = menu.Href;
                                    node.Icon = menu.Icon;
                                    node.Spread = menu.Spread;
                                    menus.Add(node);
                                }
                            }
                            model.Children = menus;
                        }
                        menuList.Add(model);
                    }
                }
            }
            return menuList;
        }

        /// <summary>
        /// 树菜单
        /// </summary>
        /// <returns></returns>
        public List<MenuTreeModel> GetMenuTree()
        {
            var menuList = _menuRepository.GetList(p => p.IsShow == true);
            var list = CreateChildTree(menuList, menuList.Where(p => p.Pid == 0).ToList());
            return list.OrderBy(p => p.Sort).ToList();
        }

        /// <summary>
        /// 遍历菜单树
        /// </summary>
        /// <param name="root"></param>
        /// <param name="menus"></param>
        /// <returns></returns>
        private List<MenuTreeModel> CreateChildTree(List<Menu> root, List<Menu> menus)
        {
            var list = new List<MenuTreeModel>();
            foreach (var menu in menus.OrderBy(p => p.Sort))
            {
                var tree = new MenuTreeModel()
                {
                    Title = menu.Title,
                    Icon = menu.Icon,
                    Href = menu.Path,
                    PermissionKey = menu.PermissionKey,
                    Spread = menu.Spread,
                    Sort = menu.Sort,
                    Buttons = !string.IsNullOrWhiteSpace(menu.Buttons) ? menu.Buttons.Split(',') : null,
                    Children = CreateChildTree(root, root.Where(p => p.Pid == menu.Id).ToList()),
                };
                list.Add(tree);
            }
            return list;
        }

        /// <summary>
        /// 获取菜单的权限代码
        /// </summary>
        /// <returns></returns>
        public List<string> GetPermissionKeys()
        {
            var menuCodes = new List<string>();
            // 获取 MenuCode 类的所有字段  
            FieldInfo[] fields = typeof(MenuCode).GetFields(BindingFlags.Public | BindingFlags.Static);
            // 遍历字段并将常量值添加到列表中  
            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly) // 确保是常量
                {
                    menuCodes.Add((string)field.GetValue(null));
                }
            }
            return menuCodes;
        }

        #endregion

        #region 权限

        /// <summary>
        /// 检查菜单权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CheckPermission(LoginAdminModel user, string code, PermissionType type = PermissionType.View)
        {
            try
            {
                if (user.IsAdmin)
                {
                    return true;
                }
                else
                {
                    var list = this.GetRolePermission(user.Roles);
                    var key = type == PermissionType.View ? code : code + "_" + type;
                    return list.Contains(key) ? true : false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("检查权限异常CheckPermission" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查菜单权限（异步）
        /// </summary>
        public async Task<bool> CheckPermissionAsync(LoginAdminModel user, string code, PermissionType type = PermissionType.View)
        {
            try
            {
                if (user.IsAdmin)
                {
                    return true;
                }
                else
                {
                    var list = await this.GetRolePermissionAsync(user.Roles);
                    var key = type == PermissionType.View ? code : code + "_" + type;
                    return list.Contains(key);
                }
            }
            catch (Exception ex)
            {
                Log.Error("检查权限异常CheckPermissionAsync" + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 检查菜单权限
        /// </summary>
        /// <param name="role"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckPermission(string role, string code)
        {
            try
            {
                var list = this.GetRolePermission(role);
                return list.Contains(code);
            }
            catch (Exception ex)
            {
                Log.Error("检查权限异常CheckPermission" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查菜单权限（异步）
        /// </summary>
        public async Task<bool> CheckPermissionAsync(string role, string code)
        {
            try
            {
                var list = await this.GetRolePermissionAsync(role);
                return list.Contains(code);
            }
            catch (Exception ex)
            {
                Log.Error("检查权限异常CheckPermissionAsync" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取角色权限（同步）
        /// </summary>
        private List<string> GetRolePermission(string role, bool isAdmin = false)
        {
            var list = new List<string>();

            if (isAdmin)
                return list;

            if (_cache.Exists(CacheKey.PermissionMenu + role))
            {
                list = _cache.Get<List<string>>(CacheKey.PermissionMenu + role);
            }
            else
            {
                string[] arrRoles = role.Split(',');
                var roleArr = Array.ConvertAll(arrRoles, int.Parse);
                list = _roleMenuRepository.GetList(p => roleArr.Contains(p.RoleId)).Select(p => p.Permission).ToList();

                _cache.Add(CacheKey.PermissionMenu + role, list, TimeSpan.FromHours(CacheKey.ExpirationTimeLen_2), true);
            }

            return list;
        }

        /// <summary>
        /// 获取角色权限（异步）
        /// </summary>
        private async Task<List<string>> GetRolePermissionAsync(string role, bool isAdmin = false)
        {
            var list = new List<string>();

            if (isAdmin)
                return list;

            if (_cache.Exists(CacheKey.PermissionMenu + role))
            {
                list = _cache.Get<List<string>>(CacheKey.PermissionMenu + role);
            }
            else
            {
                string[] arrRoles = role.Split(',');
                var roleArr = Array.ConvertAll(arrRoles, int.Parse);
                list = (await _roleMenuRepository.GetListAsync(p => roleArr.Contains(p.RoleId))).Select(p => p.Permission).ToList();

                _cache.Add(CacheKey.PermissionMenu + role, list, TimeSpan.FromHours(CacheKey.ExpirationTimeLen_2), true);
            }

            return list;
        }

        #endregion

    }
}
