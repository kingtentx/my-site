# 可视化建站应用实施计划

## Context

基于 `doc/drag-drop-enterprise-website-skill.md` 的 Phase 1（最小闭环）实现一个企业官网可视化建站系统。当前项目是 ASP.NET Core MVC 8.0 + Layui/layuiAdmin + EF Core/MySQL，已有：`Article`（新闻）实体与 CRUD、`AdminBaseController` 基类、`[PermissionFilter]` 权限体系、`AuditLogFilter` 全局审计、`UploadController` 图片上传、`Admin/ImageSelector` 选图弹窗、`Sortable.js`/`moduleSet.js`/`pickr.min.js` 已落盘但未使用、`ControlType` 枚举、`Models/Pages/PageConfigModel.cs`（JSON 配置模式雏形）、空的 `Views/Home/` 与公共站点路由。

需要补齐：页面/版本/站点配置/导航/页脚/产品/招聘实体；可视化设计器；草稿/发布机制；公共站点 `HomeController` 与动态渲染；菜单与权限种子；审计过滤器的实体映射。

Phase 1 验收：能装修、能保存草稿、能预览、能发布、前台能渲染、新闻/产品/招聘能展示、PC+移动端基础适配。

## 架构总览

```
WebSite
├── Admin (后台)         复用现有 layuiAdmin iframe 壳
│   ├── 网站管理          站点设置/页面管理/页面装修/导航/页脚
│   └── 内容管理          新闻(已有)/产品/招聘
├── WebFront (前台)       新增 HomeController + 动态组件渲染
└── Api (接口)            复用 MVC Controller/Action，不另起 REST API
```

数据流：管理员在设计器中拖拉拽组件 → 序列化为 JSON 存入 `WebsitePageVersion.DraftJson` → 发布时拷贝到 `PublishJson` → 前台 `HomeController` 按 `PagePath` 取发布版本 → 反序列化 `PublishJson` → 按组件 `type` 渲染对应 partial。

## 关键复用模式（含文件路径）

- **Controller 骨架**：`src/CIMC.WebSite/Controllers/Admin/ArticleController.cs`（列表/编辑/GetList 分页/软删/按钮权限）+ `AdminBaseController.cs`（`LoginUser`、`GetIPAddress`）
- **权限**：`Permissions/Core/MenuCode.cs`（const string 常量）+ `Permissions/Core/PermissionType.cs`（enum）+ `[PermissionFilter(MenuCode.X, PermissionType.Y)]`
- **仓储**：`IRepository<T>`（已注册 `AddScoped(typeof(IRepository<>), typeof(AppRepository<>))`），分页 `GetList(where, orderBy, pageIndex, pageSize, isAsc)` 返回 `(List, Count)` 元组
- **实体基类**：`src/CIMC.Data/ExtModel/ExtModel.cs` 的 `ExtFullModifyModel` + `IModifyModel`/`IActiveModel`/`ISortModel`（软删自动过滤）
- **审计**：`Filters/AuditLogFilter.cs` 第 224/237/256 行的 `GetEntityType`/`GetOperationTable`/`BuildOperationDesc` 三处 switch 需补实体映射
- **种子**：`src/CIMC.EntityFramework/DataInitializer.cs` 的 `Create()` + `MenuSeedData.cs`
- **返回结构**：`Models/ResultModel.cs`（`ResultModel` / `ResultModel<T>` + `ResultCode` enum，200=成功）
- **图片上传**：`UploadController.UploadImage`（已校验类型/大小）+ `/Admin/ImageSelector` 选图弹窗 + `window.postMessage({type:'imageSelected', url})`
- **列表页视图**：`Views/Article/Index.cshtml`（layui `table.render` + `request:{pageName:'pageIndex',limitName:'pageSize'}` + 响应 `{code,message,count,data}`）
- **编辑页视图**：`Views/Article/Edit.cshtml`（`form.on('submit(form-page)')` + `$.ajax` + `return false` + wangEditor + 图片选择器）
- **拖拽库**：`wwwroot/resource/js/Sortable.js`（垂直排序）+ `wwwroot/resource/js/moduleSet.js`（块级 hover 工具条）+ `wwwroot/resource/js/pickr.min.js`（颜色选择）
- **DB 迁移**：`AppDbContextFactory` 已配置设计时工厂，可用 `dotnet ef migrations add ... -p src/CIMC.EntityFramework -s src/CIMC.WebSite`

## 实施步骤

### Part A：数据层（CIMC.Data + CIMC.EntityFramework）

**A1. 新增实体** `src/CIMC.Data/Model/`（参照 `Article.cs` 风格，`[Key] int Id`、`[StringLength(ModelUnits.Len_XXX)]`、enum 存 `int`、`ExtFullModifyModel, IModifyModel, IActiveModel, ISortModel`）：

- `WebsitePage.cs` — `SiteId, PageName, PageCode, PagePath(unique), PageTitle, SeoKeywords, SeoDescription, LayoutJson, ComponentJson(冗余草稿), Status(int: 0草稿/1已发布), IsHome, Sort`
- `WebsitePageVersion.cs` — `PageId, VersionNo, DraftJson(longtext), PublishJson(longtext), Status(int), PublishTime, CreateUserId`
- `WebsiteSiteConfig.cs` — 单例（Id=1）：`SiteName, Logo, BrowserTitle, Keywords, Description, IcpNo, PoliceNo, Phone, Email, Address, Copyright, Theme, Language, Status`
- `WebsiteNavigation.cs` — `Pid, Title, Path, Icon, Target(int), Sort, IsShow`
- `WebsiteFooter.cs` — 单例：`Logo, CompanyName, Intro, Phone, Email, Address, Qrcode, IcpNo, PoliceNo, Copyright, FriendLinks(JSON string), BgColor, TextColor`
- `ContentProduct.cs` — `ProductName, CategoryId, CoverImage, ImageList(JSON), Summary, Description(longtext), Specification(longtext), Feature(longtext), Sort, IsRecommend`
- `ContentProductCategory.cs` — `Name, Pid, Sort`
- `ContentJob.cs` — `JobTitle, Department, WorkLocation, SalaryRange, RecruitCount, JobType, Responsibilities(longtext), Requirements(longtext), ContactName, ContactPhone, ContactEmail, PublishTime`

**A2.** `AppDbContext.cs` 在 `#region 数据区域` 注册上述 DbSet；`OnModelCreating` 给 `WebsitePage.PagePath` 加唯一索引、`WebsitePageVersion.PageId` 加索引。

**A3.** `Startup.cs` 第 382 行 `dbContext.Database.EnsureCreated();` → `dbContext.Database.Migrate();`，确保迁移可应用。

**A4.** 新增迁移：`dotnet ef migrations add AddPageBuilder -p src/CIMC.EntityFramework -s src/CIMC.WebSite`（生成迁移文件，无需手写）。

### Part B：后台权限与菜单种子

**B1.** `Permissions/Core/MenuCode.cs` 增加 `Website`、`Website_Site`、`Website_Page`、`Website_Navigation`、`Website_Footer`、`Content_Product`、`Content_ProductCategory`（`Content_Job`、`Content_Article` 已存在）。

**B2.** `Permissions/Core/PermissionType.cs` 增加 `Design = 8`、`Publish = 9`（页面装修/发布专用）。

**B3.** `Common/PageCode.cs` 增加 `PAGE_Button_Design`、`PAGE_Button_Publish`、`PAGE_Button_Preview` 常量。

**B4.** `MenuSeedData.cs` 增加 `WebsiteMenu`（顶级"网站管理"，PermissionKey=`Website`）与 `GetWebsiteMenus(pid)` 返回 5 个子菜单（站点设置/页面管理/导航管理/页脚设置/页面装修占位）；在 `DataInitializer.InitMenu` 中调用 `EnsureMenu(...)` 注入，参照现有 System 分支写法。同时给"内容管理"补"产品管理"和"产品分类"两项。

**B5.** `DataInitializer.cs` 增加 4 个种子方法并在 `Create()` 中调用：
- `InitSiteConfig(context)` — 插入 Id=1 默认站点配置
- `InitFooter(context)` — 插入 Id=1 默认页脚
- `InitNavigation(context)` — 插入 6 条默认导航（首页/关于我们/产品中心/新闻中心/招聘中心/联系我们，按 skill doc 第 23 节）
- `InitSitePages(context)` — 插入默认页面（首页/关于我们/产品中心/新闻中心/招聘中心/联系我们），首页 `IsHome=true`，每页带一份示例 `DraftJson`/`PublishJson`（含 nav/banner/footer 组件）

### Part C：后台控制器与审计

**C1.** `Controllers/Admin/` 新增控制器，均 `[Authorize]` 继承 `AdminBaseController`，构造注入 `IRepository<T>` + `IPermissionService`：

- `SiteConfigController.cs` — `Index`(编辑视图)/`Edit`(POST 保存单例)
- `PageController.cs` — `Index`/`Edit`(GET+POST 基本信息与SEO)/`GetList`(分页)/`Delete`(软删,支持批量)/`SetHome`(设首页)/`Design(int id)`(返回设计器视图)/`SaveDraft(int id, string componentJson)`/`Publish(int id)`(拷贝 DraftJson→PublishJson,写 PageVersion)/`Preview(int id)`(返回预览视图)/`GetComponentData(int pageId)`(供设计器加载已存配置)
- `NavigationController.cs` — `Index`/`Edit`(GET+POST)/`GetList`(树形)/`Delete`
- `FooterController.cs` — `Index`(编辑视图)/`Edit`(POST)
- `ProductController.cs` — 镜像 `ArticleController` 全套（`Index`/`Edit`/`GetList`/`Delete`/`SetRecommend`）
- `ProductCategoryController.cs` — 树形 CRUD
- `JobController.cs` — 镜像 `ArticleController` 全套

**C2.** `Filters/AuditLogFilter.cs` 第 224/237/256 行三处 switch 补：`Page`/`Site`/`Navigation`/`Footer`/`Product`/`ProductCategory`/`Job` 的实体类型与中文名映射（参照现有 `Article` 项）。`Page.Publish`/`Page.Design` 等非标准动作名在 `GetOperationType`（约 287-314 行）补分支。

### Part D：后台视图

**D1.** 在 `Views/Site/` 下建（沿用 `Views/Article/` 风格，`Layout = null`，layui CSS/JS）：

- `SiteConfig/Edit.cshtml` — 站点设置表单（Logo 走 ImageSelector，文本字段走 layui input，备案/联系方式/状态）
- `Page/Index.cshtml` — 页面列表（`table.render`，列：ID/页面名/路径/状态/是否首页/排序/创建时间/操作[设计/预览/发布/编辑/删除]）
- `Page/Edit.cshtml` — 页面基本信息与 SEO 配置表单
- `Page/Designer.cshtml` — **核心可视化设计器**（详见 D2）
- `Page/Preview.cshtml` — 预览（iframe 渲染 `/home/render?pageId=X` 或直接渲染 PublishJson）
- `Navigation/Index.cshtml` — 树形表格（用 `layui.treetable` 模块，参照 `Views/Menu/Index.cshtml`）
- `Navigation/Edit.cshtml` — 导航编辑（`treeSelect` 选父级 + 图标选择器 + 路径 + 是否新窗口）
- `Footer/Edit.cshtml` — 页脚配置表单

**D2. 设计器视图** `Views/Site/Page/Designer.cshtml`（按 skill doc 第 5.3/15 节，纵向块拖拽，不做自由画布）：

布局：顶部工具栏（保存草稿/预览/发布/撤销/重做）+ 左侧组件库（卡片列表，可拖）+ 中间画布（纵向块列表）+ 右侧属性面板。

```html
顶栏: [保存草稿] [预览] [发布] [撤销] [重做]
左栏: 布局组件/内容组件 tab → 卡片列表 (data-type="banner"/"news"/"product"/"job"/"nav"/"footer"/"richText"/"image"/"title")
中栏: <div id="canvas"> 块列表，每块 = 标题条 + 内容预览，hover 显示 moduleSet 工具条(上移/下移/复制/删除/隐藏)
右栏: <div id="propsPanel"> 选中块的属性表单（动态切换）
```

JS 实现：
- `Sortable.create(canvasEl, { group: { name:'page', pull:false, put:true }, animation:150, handle:'.drag-handle', onEnd:updateOrder })` 处理画布内排序
- 左侧组件库 `Sortable.create(libEl, { group:{ name:'page', pull:'clone', put:false }, sort:false })`，`onEnd` 在画布上克隆时插入块模板
- 块模板：每个组件类型对应一个返回 HTML 字符串的函数（如 `renderBlock({type:'banner', props:{...}})`）
- 选中块 → 调用 `renderPropPanel(type, props)` 渲染右侧表单 → 表单 change 实时更新画布块数据（用一个全局 `pageState.components` 数组作为单一数据源）
- 图片字段走 `layer.open({type:2, content:'/Admin/ImageSelector'})` + `window.message` 监听
- 颜色字段走 `pickr.min.js`
- 富文本组件走 `wangEditor`（与 `Article/Edit.cshtml` 一致）
- 保存草稿：`JSON.stringify(pageState)` POST `/Page/SaveDraft?id=X`
- 发布：POST `/Page/Publish?id=X`，成功后 `layer.alert` 回列表
- 撤销/重做：维护 `history` 栈与指针，`JSON.parse(JSON.stringify(state))` 快照

**D3.** `Views/Product/Index.cshtml` + `Edit.cshtml`（镜像 `Views/Article/`，多图列表用 ImageSelector 多选 + ImageList JSON 字段；分类下拉取 `/ProductCategory/GetList`）。

**D4.** `Views/Job/Index.cshtml` + `Edit.cshtml`（镜像 `Views/Article/`，字段按 `ContentJob` 实体）。

### Part E：前台渲染（HomeController + Views）

**E1.** 新增 `Controllers/HomeController.cs : Controller`（非 `AdminBaseController`，无需登录）：
- `Index()` — `RouteData.Values[""]` 空路径 → 取 `IsHome=true` 的页面
- `About()` / `News(category?)` / `Products(category?)` / `Jobs()` / `Contact()` — 按 `PagePath` 取页面
- `Article(id)` / `ProductDetail(id)` — 详情页（从 `Article`/`ContentProduct` 取记录 + ViewCount 自增）
- 私有 `RenderPage(WebsitePage page)` — 反序列化 `page.Version.PublishJson` 为 `List<ComponentModel>`，构造 `PageRenderModel { Page, Components, SiteConfig, Navigation, Footer }` 返回 `View("~/Views/Home/Index.cshtml", model)`
- 注意：先取已发布 `WebsitePageVersion`（`Status=1`，按 `VersionNo` desc 取最新）

**E2.** `Views/Home/Index.cshtml` — 公共布局 + 组件遍历渲染：
```cshtml
@model PageRenderModel
@{ Layout = "~/Views/Shared/_PublicLayout.cshtml"; }
@foreach (var comp in Model.Components) {
    @switch (comp.Type) {
        case "navigation": @await Html.PartialAsync("~/Views/Home/Components/_Navigation.cshtml", comp); break;
        case "banner":     @await Html.PartialAsync("~/Views/Home/Components/_Banner.cshtml", comp); break;
        case "news":       @await Html.PartialAsync("~/Views/Home/Components/_NewsList.cshtml", comp); break;
        case "product":    @await Html.PartialAsync("~/Views/Home/Components/_ProductList.cshtml", comp); break;
        case "job":        @await Html.PartialAsync("~/Views/Home/Components/_JobList.cshtml", comp); break;
        case "footer":     @await Html.PartialAsync("~/Views/Home/Components/_Footer.cshtml", comp); break;
        case "richText":   @await Html.PartialAsync("~/Views/Home/Components/_RichText.cshtml", comp); break;
        case "image":      @await Html.PartialAsync("~/Views/Home/Components/_Image.cshtml", comp); break;
        case "title":      @await Html.PartialAsync("~/Views/Home/Components/_Title.cshtml", comp); break;
    }
}
```

**E3.** 新增 `Views/Shared/_PublicLayout.cshtml` — 公共布局（`<head>` 注入站点 SEO、`<body>` 渲染 `@RenderBody()`、引入 `wwwroot/site/css/site.css`），不复用现有 `_Layout.cshtml`（它指向不存在的 `~/syle/site.css`，是遗留物）。

**E4.** 新增 `Views/Home/Components/` 下 9 个 partial（每个接收一个 `ComponentModel { Type, Props, Style }`）：
- `_Navigation.cshtml` — 读 `WebsiteNavigation` 表渲染顶部导航（一级+二级，`IsShow` 过滤，当前页高亮）
- `_Banner.cshtml` — 根据 `Props.Items[]` 渲染轮播（多图用简单 JS 轮播或 CSS 动画；图片走 `<img src="@Url.Content("~")@item.Image">`）
- `_NewsList.cshtml` — 注入 `IRepository<Article>`，按 `Props.CategoryId`/`Props.PageSize` 取数据，渲染列表（封面/标题/摘要/时间/详情链接 `/news/info-{id}.html`）
- `_ProductList.cshtml` — 注入 `IRepository<ContentProduct>`，按 `Props` 取数据，栅格布局（PC 4 列/平板 2 列/手机 1 列，CSS Grid 媒体查询）
- `_JobList.cshtml` — 注入 `IRepository<ContentJob>`，列表渲染（岗位/地点/薪资/招聘人数/详情链接）
- `_Footer.cshtml` — 读 `WebsiteFooter` 单例渲染
- `_RichText.cshtml` / `_Image.cshtml` / `_Title.cshtml` — 直接渲染 `Props.Html`/`Props.Src`/`Props.Text`

**注**：partial 中需要注入仓储时，用 `@inject IRepository<Article>` 等（Razor 支持 `@inject`）。

**E5.** 静态资源 `wwwroot/site/css/site.css` + `wwwroot/site/js/site.js` — 前台样式（响应式栅格、导航移动端折叠、Banner 轮播 JS、页面通用样式）。修复 `Views/Shared/_Layout.cshtml` 中的 `~/syle/` → `~/site/` 路径错字（或弃用，用新的 `_PublicLayout.cshtml`）。

**E6.** `Startup.cs` 路由保持现状（已有 `/`、`/about`、`/news/...`、`/products/...`、`/jobs`、`/contact`），无需改动。

## 关键决策与依据

1. **enum 存 int**：遵循 `Article.TagType`/`Menu.MenuType` 的现有约定，新实体的 `Status`/`JobType`/`Target` 等都存 `int`。
2. **草稿与发布分离**：`WebsitePageVersion` 表保存 `DraftJson`/`PublishJson` 双版本，前台只读 `PublishJson`，符合 skill doc 第 13 节。
3. **JSON 用 longtext**：组件配置、`LayoutJson`、`ImageList` 等 JSON 字段不加 `[StringLength]`，MySQL 自动映射 longtext（参照 `Article.Detail`）。
4. **partial 注入仓储**：前台组件 partial 通过 `@inject IRepository<T>` 取数据，避免 controller 把所有数据预取（组件类型可扩展，符合 skill doc 第 21 节"组件类型要可扩展"）。
5. **纵向拖拽**：Phase 1 严格按 skill doc 第 15 节，组件按块上下排序，不做绝对定位。
6. **路由不变**：复用现有公共站点路由，不引入 `/api/...` REST 端点（与现有 `Article/GetList` 风格一致）。
7. **菜单与权限走现有体系**：新菜单项通过 `MenuSeedData` 注入，权限用 `[PermissionFilter(MenuCode.X, PermissionType.Y)]`，按钮可见性走 `ViewData[PageCode.PAGE_Button_*]`。

## 验证

1. **构建**：`dotnet build d:\MyProject\my-site\MySite.sln`（无错误）
2. **数据库**：`dotnet ef database update -p src/CIMC.EntityFramework -s src/CIMC.WebSite`（应用迁移）
3. **运行**：`dotnet run --project src/CIMC.WebSite`，启动后 `DataInitializer` 自动注入菜单/站点配置/默认页面
4. **后台登录**：`/admin/login`（admin/123qwe），左侧应出现"网站管理"菜单组
5. **页面装修**：网站管理 → 页面管理 → 设计 → 拖入 Banner/新闻/产品/招聘组件 → 配置属性 → 保存草稿 → 预览 → 发布
6. **内容管理**：内容管理 → 产品管理 → 新增几条；招聘管理 → 新增几条
7. **前台访问**：访问 `/`，应渲染首页发布后的组件；新闻/产品/招聘组件应展示真实内容；导航点击跳转正确；Banner 轮播正常；页脚显示站点配置
8. **响应式**：浏览器 DevTools 切换 PC/iPad/iPhone 视口，验证产品栅格 4→2→1、导航折叠、Banner 高度自适应
9. **权限**：用非超管角色登录，验证按钮级控制生效（无权限的按钮不显示）
10. **审计**：系统管理 → 审计日志，确认页面发布/编辑/删除操作有记录

## 文件清单（预估 ~45 个文件）

**新增**（~35）：
- 8 实体：`src/CIMC.Data/Model/{WebsitePage,WebsitePageVersion,WebsiteSiteConfig,WebsiteNavigation,WebsiteFooter,ContentProduct,ContentProductCategory,ContentJob}.cs`
- 1 迁移：`src/CIMC.EntityFramework/Migrations/*_AddPageBuilder.cs` + `.Designer.cs` + snapshot 更新
- 7 控制器：`src/CIMC.WebSite/Controllers/Admin/{SiteConfig,Page,Navigation,Footer,Product,ProductCategory,Job}Controller.cs` + `Controllers/HomeController.cs`
- 4 模型：`src/CIMC.WebSite/Models/Site/{PageModel,ComponentModel,SiteConfigModel,FooterModel}.cs`（视图模型 + 组件 JSON DTO）
- ~15 视图：`Views/Site/{SiteConfig/Edit,Page/Index,Page/Edit,Page/Designer,Page/Preview,Navigation/Index,Navigation/Edit,Footer/Edit}.cshtml` + `Views/Product/{Index,Edit}.cshtml` + `Views/Job/{Index,Edit}.cshtml` + `Views/Home/{Index,_PublicLayout}.cshtml` + `Views/Home/Components/{_Navigation,_Banner,_NewsList,_ProductList,_JobList,_Footer,_RichText,_Image,_Title}.cshtml`
- 2 静态资源：`wwwroot/site/css/site.css`、`wwwroot/site/js/site.js`

**修改**（~10）：
- `AppDbContext.cs`（注册 DbSet + 索引）
- `Startup.cs`（`EnsureCreated` → `Migrate`）
- `Permissions/Core/MenuCode.cs`（新常量）
- `Permissions/Core/PermissionType.cs`（`Design`/`Publish`）
- `Common/PageCode.cs`（新按钮常量）
- `Filters/AuditLogFilter.cs`（三处 switch 补实体）
- `EntityFramework/MenuSeedData.cs`（WebsiteMenu + 子菜单）
- `EntityFramework/DataInitializer.cs`（4 个 Init 方法 + 调用）
- `Views/Shared/_Layout.cshtml`（修复 `~/syle/` 路径或弃用）

## 不在 Phase 1 范围（按 skill doc 第 20 节延后）

- 页面模板/组件模板复用、多主题切换、页面版本回滚 UI（数据结构支持但 UI 暂不做）
- 多语言、表单组件、在线留言、招聘投递、访问统计、SEO 自动生成、静态化发布、CDN 适配
- 任意像素级自由拖动、复杂动画编排、多人协同、在线代码编辑、工作流审批
