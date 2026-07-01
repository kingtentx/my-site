using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class SiteModuleController : AdminBaseController
    {
        private readonly IRepository<SiteModule> _moduleRepository;
        private readonly IRepository<Navigation> _navRepository;
        private readonly IPermissionService _permission;

        public SiteModuleController(IRepository<SiteModule> moduleRepository, IRepository<Navigation> navRepository, IPermissionService permission)
        {
            _moduleRepository = moduleRepository;
            _navRepository = navRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Module, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Module, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Module, PermissionType.Delete);
            return View();
        }

        [PermissionFilter(MenuCode.Content_Module, PermissionType.Edit)]
        public IActionResult Designer(string pageKey = "home", int? navigationId = null)
        {
            if (navigationId.HasValue && navigationId.Value > 0)
            {
                var nav = _navRepository.GetOne(navigationId.Value);
                if (nav != null)
                {
                    pageKey = nav.IsHomePage ? "home" : (nav.RewriteName ?? pageKey);
                    ViewData["NavigationName"] = nav.NavigationName;
                }
            }

            ViewData["PageKey"] = string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey;
            ViewData["NavigationId"] = navigationId ?? 0;
            ViewData["NavigationOptions"] = GetNavigationOptions();
            return View();
        }

        [PermissionFilter(MenuCode.Content_Module, PermissionType.Edit)]
        public IActionResult Edit(int id = 0, int? navigationId = null)
        {
            ViewData["LinkOptions"] = GetLinkOptions();
            ViewData["ModuleOptions"] = GetModuleOptions();
            ViewData["NavigationOptions"] = GetNavigationOptions();
            var model = new SiteModule { PageKey = "home", ModuleKey = "hero", ModuleName = "首页Banner", ModuleType = "banner", IsActive = true, Sort = 10 };
            if (id > 0)
            {
                model = _moduleRepository.GetOne(id);
                if (model == null)
                {
                    return NotFound();
                }
            }
            else if (navigationId.HasValue && navigationId.Value > 0)
            {
                var nav = _navRepository.GetOne(navigationId.Value);
                if (nav != null)
                {
                    model.PageKey = nav.IsHomePage ? "home" : (nav.RewriteName ?? "home");
                    model.NavigationId = navigationId.Value;
                }
            }

            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.Edit)]
        public IActionResult Edit(int id, SiteModule input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择所属页面和模块位置" };
            if (input == null || string.IsNullOrWhiteSpace(input.PageKey) || string.IsNullOrWhiteSpace(input.ModuleKey))
            {
                return Json(result);
            }

            var enableLink = string.Equals(Request.Form["EnableLink"], "true", StringComparison.OrdinalIgnoreCase);
            var module = id > 0 ? _moduleRepository.GetOne(id) : new SiteModule { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (module == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            module.PageKey = input.PageKey;
            module.ModuleKey = input.ModuleKey;
            module.ModuleName = string.IsNullOrWhiteSpace(input.ModuleName) ? GetModuleDisplayName(input.ModuleKey) : input.ModuleName;
            module.ModuleName_EN = input.ModuleName_EN;
            module.ModuleType = ResolveModuleType(input.ModuleKey, input.ModuleType);
            module.Title = input.Title;
            module.Title_EN = input.Title_EN;
            module.SubTitle = input.SubTitle;
            module.SubTitle_EN = input.SubTitle_EN;
            module.LinkUrl = enableLink ? input.LinkUrl : string.Empty;
            module.ImageUrl = input.ImageUrl;
            module.Sort = input.Sort;
            module.IsActive = input.IsActive;
            module.NavigationId = input.NavigationId;
            module.SettingsJson = Request.Form["SettingsJson"].ToString();
            module.SettingsJson_EN = Request.Form["SettingsJson_EN"].ToString();
            module.IsDelete = false;
            module.UpdateBy = LoginUser.UserName;
            module.UpdateTime = DateTime.Now;

            if (id > 0)
            {
                _moduleRepository.Update(module);
            }
            else
            {
                _moduleRepository.Add(module);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            var pageKey = HttpContext.Request.Query["pageKey"].ToString().Trim();
            var navigationIdStr = HttpContext.Request.Query["navigationId"].ToString().Trim();
            var where = LambdaHelper.True<SiteModule>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.ModuleName.Contains(keywords) || p.Title.Contains(keywords));
            }

            if (!string.IsNullOrWhiteSpace(pageKey))
            {
                where = where.And(p => p.PageKey == pageKey);
            }

            if (int.TryParse(navigationIdStr, out int navigationId) && navigationId > 0)
            {
                var nav = _navRepository.GetOne(navigationId);
                if (nav != null)
                {
                    var navPageKey = nav.IsHomePage ? "home" : (nav.RewriteName ?? "");
                    if (!string.IsNullOrWhiteSpace(navPageKey))
                    {
                        where = where.And(p => p.PageKey == navPageKey);
                    }
                }
            }

            var query = _moduleRepository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = query.List });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult GetDesignerData(string pageKey = "home", int navigationId = 0)
        {
            pageKey = ResolvePageKey(pageKey, navigationId);
            var modules = _moduleRepository.GetList(p => !p.IsDelete && p.PageKey == pageKey, p => p.Sort, true)
                .Select(m => new
                {
                    m.Id,
                    m.PageKey,
                    m.ModuleKey,
                    m.ModuleName,
                    m.ModuleName_EN,
                    m.ModuleType,
                    m.Title,
                    m.Title_EN,
                    m.SubTitle,
                    m.SubTitle_EN,
                    m.LinkUrl,
                    m.ImageUrl,
                    Images = SplitImages(m.ImageUrl),
                    m.Sort,
                    m.IsActive,
                    m.NavigationId,
                    Settings = ParseSettingsObject(m.SettingsJson),
                    Settings_EN = ParseSettingsObject(m.SettingsJson_EN)
                })
                .ToList();

            return Json(new ResultModel<object>
            {
                Code = (int)ResultCode.Success,
                Message = "成功",
                Count = modules.Count,
                Data = new
                {
                    PageKey = pageKey,
                    NavigationId = navigationId,
                    Modules = modules,
                    Templates = GetDesignerComponentTemplates()
                }
            });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult GetDesignerTemplates()
        {
            return Json(new ResultModel<object>
            {
                Code = (int)ResultCode.Success,
                Message = "成功",
                Data = GetDesignerComponentTemplates()
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.Edit)]
        public IActionResult SaveDesigner([FromBody] SitePageDesignerSaveModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.PageKey))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "页面标识不能为空" });
            }

            input.PageKey = ResolvePageKey(input.PageKey, input.NavigationId ?? 0);
            input.Modules ??= new List<SitePageDesignerModuleModel>();

            var now = DateTime.Now;
            var keepIds = input.Modules.Where(p => p.Id > 0).Select(p => p.Id).ToHashSet();
            var currentModules = _moduleRepository.GetList(p => !p.IsDelete && p.PageKey == input.PageKey, p => p.Sort, true);

            foreach (var module in currentModules.Where(p => p.Id > 0 && !keepIds.Contains(p.Id)))
            {
                module.IsDelete = true;
                module.UpdateTime = now;
                module.UpdateBy = LoginUser.UserName;
                _moduleRepository.Update(module);
            }

            for (var index = 0; index < input.Modules.Count; index++)
            {
                var item = input.Modules[index];
                if (item == null || string.IsNullOrWhiteSpace(item.ModuleKey))
                {
                    continue;
                }

                var module = item.Id > 0 ? _moduleRepository.GetOne(item.Id) : null;
                if (module == null)
                {
                    module = new SiteModule
                    {
                        CreationTime = now,
                        CreationBy = LoginUser.UserName
                    };
                }

                module.PageKey = input.PageKey;
                module.ModuleKey = item.ModuleKey.Trim();
                module.ModuleName = string.IsNullOrWhiteSpace(item.ModuleName) ? GetModuleDisplayName(item.ModuleKey) : item.ModuleName.Trim();
                module.ModuleName_EN = item.ModuleName_EN;
                module.ModuleType = ResolveModuleType(item.ModuleKey, item.ModuleType);
                module.Title = item.Title;
                module.Title_EN = item.Title_EN;
                module.SubTitle = item.SubTitle;
                module.SubTitle_EN = item.SubTitle_EN;
                module.LinkUrl = item.LinkUrl;
                module.ImageUrl = NormalizeImageUrl(item.ImageUrl, item.Images);
                module.NavigationId = input.NavigationId > 0 ? input.NavigationId : item.NavigationId;
                module.Sort = (index + 1) * 10;
                module.IsActive = item.IsActive;
                module.SettingsJson = NormalizeJson(item.Settings);
                module.SettingsJson_EN = NormalizeJson(item.Settings_EN);
                module.IsDelete = false;
                module.UpdateBy = LoginUser.UserName;
                module.UpdateTime = now;

                if (module.Id > 0)
                {
                    _moduleRepository.Update(module);
                }
                else
                {
                    _moduleRepository.Add(module);
                }
            }

            return Json(new ResultModel
            {
                Code = (int)ResultCode.Success,
                Message = "可视化页面配置保存成功"
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.Delete)]
        public IActionResult Delete(int id)
        {
            var module = _moduleRepository.GetOne(id);
            if (module != null)
            {
                module.IsDelete = true;
                module.UpdateTime = DateTime.Now;
                module.UpdateBy = LoginUser.UserName;
                _moduleRepository.Update(module);
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult GetNavigationModules(int navigationId)
        {
            var result = new ResultModel<object> { Code = (int)ResultCode.ParmsError, Message = "请传入导航ID" };
            if (navigationId <= 0)
            {
                return Json(result);
            }

            var nav = _navRepository.GetOne(navigationId);
            if (nav == null)
            {
                result.Message = "导航不存在";
                return Json(result);
            }

            var pageKey = nav.IsHomePage ? "home" : (nav.RewriteName ?? "");
            var modules = _moduleRepository.GetList(p => !p.IsDelete && p.PageKey == pageKey, p => p.Sort, true);
            var data = modules.Select(m => new
            {
                m.Id,
                m.PageKey,
                m.ModuleKey,
                m.ModuleName,
                m.ModuleType,
                m.Title,
                m.SubTitle,
                m.LinkUrl,
                m.ImageUrl,
                m.Sort,
                m.IsActive,
                m.NavigationId,
                Settings = string.IsNullOrWhiteSpace(m.SettingsJson) ? null : JsonConvert.DeserializeObject<object>(m.SettingsJson)
            }).ToList();

            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Count = data.Count;
            result.Data = data;
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Module, PermissionType.View)]
        public JsonResult Preview(int id)
        {
            var result = new ResultModel<object> { Code = (int)ResultCode.ParmsError, Message = "模块不存在" };
            var module = _moduleRepository.GetOne(id);
            if (module == null)
            {
                return Json(result);
            }

            var url = GetPagePreviewUrl(module.PageKey);
            result.Code = (int)ResultCode.Success;
            result.Message = "成功";
            result.Data = new { Url = url, PageKey = module.PageKey };
            return Json(result);
        }

        private SelectListItem[] GetLinkOptions()
        {
            var items = new List<SelectListItem> { new SelectListItem("不跳转", "") };
            var navigations = _navRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.Sort, true);
            foreach (var nav in navigations)
            {
                var url = nav.IsHomePage ? "/" : (nav.IsEnableLink && !string.IsNullOrWhiteSpace(nav.LinkUrl) ? nav.LinkUrl : "/" + nav.RewriteName);
                items.Add(new SelectListItem(nav.NavigationName, url));
            }

            return items.ToArray();
        }

        private SelectListItem[] GetModuleOptions()
        {
            var existing = _moduleRepository.GetList(p => !p.IsDelete, p => p.Sort, true)
                .GroupBy(p => p.ModuleKey)
                .Select(g => g.FirstOrDefault())
                .Where(p => p != null)
                .Select(p => new SelectListItem(p.ModuleName ?? p.ModuleKey, p.ModuleKey))
                .ToList();

            var defaults = GetDesignerComponentTemplates()
                .Select(p => new SelectListItem(p.Name, p.Key))
                .ToList();

            foreach (var d in defaults)
            {
                if (!existing.Any(e => e.Value == d.Value))
                {
                    existing.Add(d);
                }
            }

            return existing.ToArray();
        }

        private SelectListItem[] GetNavigationOptions()
        {
            var items = new List<SelectListItem> { new SelectListItem("请选择导航", "0") };
            var navigations = _navRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.Sort, true);
            foreach (var nav in navigations)
            {
                items.Add(new SelectListItem(nav.NavigationName, nav.Id.ToString()));
            }

            return items.ToArray();
        }

        private string ResolveModuleType(string moduleKey, string fallback)
        {
            if (string.Equals(moduleKey, "hero", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(moduleKey, "banner", StringComparison.OrdinalIgnoreCase))
            {
                return "banner";
            }

            if (string.Equals(moduleKey, "certificates", StringComparison.OrdinalIgnoreCase))
            {
                return "carousel";
            }

            if (string.Equals(moduleKey, "message", StringComparison.OrdinalIgnoreCase))
            {
                return "form";
            }

            if (string.Equals(moduleKey, "stats", StringComparison.OrdinalIgnoreCase))
            {
                return "stats";
            }

            if (string.Equals(moduleKey, "feature-grid", StringComparison.OrdinalIgnoreCase))
            {
                return "grid";
            }

            if (string.Equals(moduleKey, "image-text", StringComparison.OrdinalIgnoreCase))
            {
                return "image-text";
            }

            if (string.Equals(moduleKey, "rich-text", StringComparison.OrdinalIgnoreCase))
            {
                return "rich-text";
            }

            if (string.Equals(moduleKey, "cta", StringComparison.OrdinalIgnoreCase))
            {
                return "cta";
            }

            return string.IsNullOrWhiteSpace(fallback) ? "section" : fallback;
        }

        private string GetModuleDisplayName(string moduleKey)
        {
            return GetModuleOptions().FirstOrDefault(p => p.Value == moduleKey)?.Text ?? moduleKey;
        }

        private string GetPagePreviewUrl(string pageKey)
        {
            return pageKey?.ToLower() switch
            {
                "home" => "/",
                "about" => "/about",
                "products" => "/products",
                "news" => "/news",
                "jobs" => "/jobs",
                "contact" => "/contact",
                _ => "/" + pageKey
            };
        }

        private string ResolvePageKey(string pageKey, int navigationId)
        {
            if (navigationId > 0)
            {
                var nav = _navRepository.GetOne(navigationId);
                if (nav != null)
                {
                    return nav.IsHomePage ? "home" : (nav.RewriteName ?? pageKey);
                }
            }

            return string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey.Trim();
        }

        private static List<string> SplitImages(string imageUrl)
        {
            return (imageUrl ?? string.Empty)
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }

        private static string NormalizeImageUrl(string imageUrl, List<string> images)
        {
            if (images != null && images.Any())
            {
                return string.Join(",", images.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));
            }

            return imageUrl ?? string.Empty;
        }

        private static object ParseSettingsObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new JObject();
            }

            try
            {
                return JsonConvert.DeserializeObject<object>(json);
            }
            catch
            {
                return new JObject();
            }
        }

        private static string NormalizeJson(object settings)
        {
            if (settings == null)
            {
                return "{}";
            }

            if (settings is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return "{}";
                }

                try
                {
                    return JToken.Parse(text).ToString(Formatting.None);
                }
                catch
                {
                    return "{}";
                }
            }

            return JsonConvert.SerializeObject(settings);
        }

        private static List<DesignerComponentTemplate> GetDesignerComponentTemplates()
        {
            return new List<DesignerComponentTemplate>
            {
                new DesignerComponentTemplate
                {
                    Key = "hero",
                    Name = "首页/页面 Banner",
                    Type = "banner",
                    Icon = "layui-icon-carousel",
                    Description = "适合网站首页首屏、内页横幅，支持多图轮播、标题、副标题、跳转链接。",
                    DefaultSettings = JObject.FromObject(new { buttonText = "了解更多", height = "640px", align = "left" })
                },
                new DesignerComponentTemplate
                {
                    Key = "products",
                    Name = "产品服务",
                    Type = "section",
                    Icon = "layui-icon-template-1",
                    Description = "用于产品业务线展示，可在 JSON 中配置 items 数组。",
                    DefaultSettings = JObject.Parse(@"{""items"":[{""title"":""业务板块"",""description"":""填写业务介绍"",""image"":""/syle/images/43974676.png"",""link"":""/products""}]}")
                },
                new DesignerComponentTemplate
                {
                    Key = "about",
                    Name = "企业介绍",
                    Type = "section",
                    Icon = "layui-icon-about",
                    Description = "展示公司介绍与核心指标，可配置 description 和 metrics。",
                    DefaultSettings = JObject.Parse(@"{""description"":""填写企业介绍"",""metrics"":[{""value"":""25万"",""label"":""年产能""}]}")
                },
                new DesignerComponentTemplate
                {
                    Key = "partners",
                    Name = "合作客户",
                    Type = "section",
                    Icon = "layui-icon-group",
                    Description = "展示客户或合作伙伴 Logo，数据来源于素材/相册标签。",
                    DefaultSettings = new JObject()
                },
                new DesignerComponentTemplate
                {
                    Key = "news",
                    Name = "新闻资讯",
                    Type = "section",
                    Icon = "layui-icon-read",
                    Description = "展示新闻列表，数据来源于内容管理。",
                    DefaultSettings = JObject.FromObject(new { count = 6 })
                },
                new DesignerComponentTemplate
                {
                    Key = "careers",
                    Name = "人才招聘",
                    Type = "section",
                    Icon = "layui-icon-user",
                    Description = "展示招聘入口与最新岗位。",
                    DefaultSettings = JObject.FromObject(new { description = "欢迎加入我们，共同创造价值。" })
                },
                new DesignerComponentTemplate
                {
                    Key = "rich-text",
                    Name = "富文本内容",
                    Type = "rich-text",
                    Icon = "layui-icon-fonts-html",
                    Description = "通用内容区，可配置 HTML、背景、内边距。",
                    DefaultSettings = JObject.FromObject(new { html = "<p>请填写内容</p>", padding = "80px 0" })
                },
                new DesignerComponentTemplate
                {
                    Key = "feature-grid",
                    Name = "能力宫格",
                    Type = "grid",
                    Icon = "layui-icon-component",
                    Description = "用于核心能力、解决方案、优势卖点展示。",
                    DefaultSettings = JObject.Parse(@"{""columns"":3,""items"":[{""title"":""核心能力"",""description"":""填写能力说明"",""icon"":""01""}]}")
                },
                new DesignerComponentTemplate
                {
                    Key = "image-text",
                    Name = "图文介绍",
                    Type = "image-text",
                    Icon = "layui-icon-picture",
                    Description = "左图右文或左文右图的通用介绍模块。",
                    DefaultSettings = JObject.FromObject(new { image = "/syle/images/44456253.jpeg", reverse = false, buttonText = "查看详情" })
                },
                new DesignerComponentTemplate
                {
                    Key = "stats",
                    Name = "数据指标",
                    Type = "stats",
                    Icon = "layui-icon-chart",
                    Description = "适合展示产能、项目数量、客户数量等数字指标。",
                    DefaultSettings = JObject.Parse(@"{""items"":[{""value"":""100+"",""label"":""服务客户""},{""value"":""20+"",""label"":""行业经验""}]}")
                },
                new DesignerComponentTemplate
                {
                    Key = "cta",
                    Name = "行动号召",
                    Type = "cta",
                    Icon = "layui-icon-release",
                    Description = "用于联系我们、了解方案、提交需求等转化入口。",
                    DefaultSettings = JObject.FromObject(new { buttonText = "联系我们", background = "#0b2b5c" })
                }
            };
        }
    }

    public class SitePageDesignerSaveModel
    {
        public string PageKey { get; set; }

        public int? NavigationId { get; set; }

        public List<SitePageDesignerModuleModel> Modules { get; set; }
    }

    public class SitePageDesignerModuleModel
    {
        public int Id { get; set; }

        public string PageKey { get; set; }

        public string ModuleKey { get; set; }

        public string ModuleName { get; set; }

        public string ModuleName_EN { get; set; }

        public string ModuleType { get; set; }

        public string Title { get; set; }

        public string Title_EN { get; set; }

        public string SubTitle { get; set; }

        public string SubTitle_EN { get; set; }

        public string LinkUrl { get; set; }

        public string ImageUrl { get; set; }

        public List<string> Images { get; set; }

        public int? NavigationId { get; set; }

        public bool IsActive { get; set; } = true;

        public object Settings { get; set; }

        public object Settings_EN { get; set; }
    }

    public class DesignerComponentTemplate
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Icon { get; set; }

        public string Description { get; set; }

        public JObject DefaultSettings { get; set; }
    }
}