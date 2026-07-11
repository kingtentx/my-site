using AutoMapper;
using CIMC.Data;
using CIMC.Helper;
using CIMC.EntityFramework;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class MenuController : AdminBaseController
    {
        private readonly IPermissionService _permission;
        private readonly IMapper _mapper;
        private readonly IRepository<Menu> _menuRepository;

        public MenuController(IPermissionService permission, IMapper mapper, IRepository<Menu> menuRepository)
        {
            _permission = permission;
            _mapper = mapper;
            _menuRepository = menuRepository;
        }

        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.System_Menu, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.System_Menu, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.System_Menu, PermissionType.Delete);
            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public async Task<JsonResult> GetList()
        {
            var result = new ResultModel<List<MenuModel>>();
            var keywords = HttpContext.Request.Query["keywords"];
            var where = LambdaHelper.True<Menu>();
            if (!string.IsNullOrWhiteSpace(keywords)) where = where.And(p => p.Title.Contains(keywords));

            var query = await _menuRepository.GetListAsync(where, p => p.Sort, true);
            var data = _mapper.Map<List<MenuModel>>(query);
            foreach (var item in data.Where(p => p.PermissionKey == "Content_ProductCategory")) item.Title = "内容分类";

            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Count = query.Count;
            result.Data = data;
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public async Task<JsonResult> GetMenuData()
        {
            // 页面管理已包含导航树配置，后台菜单不再展示重复的导航管理入口。
            var menulist = await _menuRepository.GetListAsync(p => p.IsShow && p.PermissionKey != "Site_Navigation");
            var categoryMenu = menulist.FirstOrDefault(p => p.PermissionKey == "Content_ProductCategory");
            if (categoryMenu != null) categoryMenu.Title = "内容分类";

            var treeList = new List<TreeSelectModel>();
            foreach (var parentNode in menulist.Where(t => t.Pid == 0))
            {
                var model = new TreeSelectModel
                {
                    Id = parentNode.Id,
                    Name = parentNode.Title,
                    Sort = parentNode.Sort
                };
                model.Children = PageUtils.TreeSelect(menulist, model);
                treeList.Add(model);
            }
            return Json(treeList.OrderBy(p => p.Sort));
        }

        private Dictionary<string, string> GetPermissionType()
        {
            var buttons = new Dictionary<string, string>();
            foreach (var button in Enum.GetValues(typeof(PermissionType)))
            {
                buttons.Add(button.ToString(), EnumHelper.GetDescription((PermissionType)button));
            }
            buttons.Remove(PermissionType.View.ToString());
            return buttons;
        }

        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public ActionResult Edit(int id, int pid)
        {
            var model = new MenuModel { Pid = pid };
            if (id > 0)
            {
                var query = _menuRepository.GetOne(id);
                if (query != null) model = _mapper.Map<MenuModel>(query);
            }
            model.PermissionKeys = _permission.GetPermissionKeys();
            model.PermissionTypes = GetPermissionType();
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public async Task<ActionResult> EditPost(int id, MenuModel input)
        {
            var result = new ResultModel();
            if (id > 0)
            {
                if (!_permission.CheckPermission(LoginUser, MenuCode.System_Menu, PermissionType.Edit))
                {
                    result.Code = (int)ResultCode.Nopermit;
                    result.Message = "无操作权限";
                    return Json(result);
                }

                var editmodel = _menuRepository.GetOne(id);
                if (editmodel == null)
                {
                    result.Code = (int)ResultCode.NULL;
                    result.Message = "菜单不存在";
                    return Json(result);
                }
                editmodel.Pid = input.Pid;
                editmodel.MenuType = input.MenuType;
                editmodel.Title = input.Title;
                editmodel.Path = input.Path;
                editmodel.Icon = input.Icon;
                editmodel.Sort = input.Sort;
                editmodel.PermissionKey = input.PermissionKey;
                editmodel.Buttons = input.Buttons;
                editmodel.IsShow = input.IsShow;
                editmodel.UpdateBy = LoginUser.UserName;
                editmodel.UpdateTime = DateTime.Now;
                if (await _menuRepository.UpdateAsync(editmodel))
                {
                    result.Code = (int)ResultCode.Success;
                    result.Message = "修改成功";
                }
            }
            else
            {
                if (!_permission.CheckPermission(LoginUser, MenuCode.System_Menu, PermissionType.Add))
                {
                    result.Code = (int)ResultCode.Nopermit;
                    result.Message = "无操作权限";
                    return Json(result);
                }
                var model = _mapper.Map<Menu>(input);
                model.CreationBy = LoginUser.UserName;
                if ((await _menuRepository.AddAsync(model)).Id > 0)
                {
                    result.Code = (int)ResultCode.Success;
                    result.Message = "添加成功";
                }
            }
            return Json(result);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.System_Menu, PermissionType.Delete)]
        public async Task<ActionResult> Delete(int id)
        {
            var result = new ResultModel();
            var sub = await _menuRepository.GetListAsync(p => p.Pid == id);
            if (sub.Any())
            {
                result.Message = "该菜单有子菜单，请先删除子菜单！";
                return Json(result);
            }
            if (await _menuRepository.DeleteAsync(id))
            {
                result.Code = (int)ResultCode.Success;
                result.Message = "删除成功！";
            }
            return Json(result);
        }
    }
}
