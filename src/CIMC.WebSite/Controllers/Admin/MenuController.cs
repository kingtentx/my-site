using AutoMapper;
using CIMC.Data;
using CIMC.Helper;
using CIMC.EntityFramework;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class MenuController : AdminBaseController
    {
        private IPermissionService _permission;
        private IMapper _mapper;
        private IRepository<Menu> _menuRepository;

        public MenuController(
            IPermissionService permission,
            IMapper mapper,
            IRepository<Menu> menuRepository
            )
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

        /// <summary>
        /// 列表
        /// </summary>      
        /// <returns></returns>
        [HttpGet]
        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public async Task<JsonResult> GetList()
        {

            var result = new ResultModel<List<MenuModel>>();
            var keywords = HttpContext.Request.Query["keywords"];
            var where = LambdaHelper.True<Menu>();

            if (!string.IsNullOrWhiteSpace(keywords))
                where = where.And(p => p.Title.Contains(keywords));

            var query = await _menuRepository.GetListAsync(where, p => p.Sort, true);
            var data = _mapper.Map<List<MenuModel>>(query);

            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Count = query.Count;
            result.Data = data.ToList();

            return Json(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public async Task<JsonResult> GetMenuData()
        {
            var menulist = await _menuRepository.GetListAsync(p => p.IsShow);

            var treeList = new List<TreeSelectModel>();
            foreach (var parentNode in menulist.Where(t => t.Pid == 0))
            {
                var model = new TreeSelectModel();
                model.Id = parentNode.Id;
                model.Name = parentNode.Title;
                model.Sort = parentNode.Sort;
                model.Children = PageUtils.TreeSelect(menulist, model);
                treeList.Add(model);
            }
            return Json(treeList.OrderBy(p => p.Sort));
        }

        private Dictionary<string, string> GetPermissionType()
        {
            var result = new ResultModel();
            var buttons = new Dictionary<string, string>();
            foreach (var button in Enum.GetValues(typeof(PermissionType)))
            {
                buttons.Add(button.ToString(), EnumHelper.GetDescription((PermissionType)button));
            }
            buttons.Remove(PermissionType.View.ToString());//菜单默认有view权限，不用重复添加
            return buttons;
        }

        [PermissionFilter(MenuCode.System_Menu, PermissionType.View)]
        public ActionResult Edit(int id, int pid)
        {
            var model = new MenuModel() { Pid = pid };
            if (id > 0)
            {
                var query = _menuRepository.GetOne(id);
                if (query != null)
                {
                    model = _mapper.Map<MenuModel>(query);
                }
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

            //var query = await _menuRepository.GetQueryable(p => p.Id == id).FirstOrDefaultAsync();
            //if (query != null)
            //{
            //    query.IsDelete = true;
            //    if (await _menuRepository.UpdateAsync(query))
            //    {
            //        result.Code = (int)ResultCode.Success;
            //        result.Message = "删除成功！";
            //    }
            //}

            return Json(result);
        }
    }
}
