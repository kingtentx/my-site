# 拖拽建站应用 — Part D/E/F 实施计划

## 摘要

基于 `doc/drag-drop-enterprise-website-skill.md` 的 Phase 1（最小闭环）要求，本计划完成已批准方案的后三部分：

- **Part D（后台视图）**：15 个 `.cshtml` 视图文件，覆盖站点配置 / 页面管理 / 设计器 / 预览 / 导航 / 页脚 / 产品 / 招聘 的所有后台 CRUD 页面。其中 `Page/Design.cshtml` 是核心可视化设计器，使用 `Sortable.js` 实现左侧组件库 → 中间画布拖入+纵向排序+右侧属性面板。
- **Part E（前台渲染）**：`HomeController`（9 个 action 对应已有路由）+ `Views/Home/Index.cshtml` 主视图 + 9 个组件 partial + `Views/Shared/_PublicLayout.cshtml` 公共布局 + `wwwroot/site/css/site.css` + `wwwroot/site/js/site.js`。
- **Part F（验证）**：`dotnet build` → `dotnet ef migrations add AddPageBuilder` → `dotnet ef database update` → 启动验证。

Parts A（数据层 8 个实体）、B（权限/菜单/种子）、C（7 个 admin 控制器 + 6 个 view model + AuditLogFilter 扩展）已在前序对话完成并经核对存在。本计划仅执行剩余工作。

---

## 当前状态分析（Phase 1 探索结果）

### 已完成（核对存在）

| 部分 | 文件 | 状态 |
|---|---|---|
| A 数据层 | `src/CIMC.Data/Model/Website{Page,PageVersion,SiteConfig,Navigation,Footer}.cs`、`Content{Product,ProductCategory,Job}.cs` | 8 实体均已创建 |
| A 数据层 | `src/CIMC.EntityFramework/EntityFramework/AppDbContext.cs` | 8 个 DbSet 已注册；唯一索引 PagePath、PageId、Pid、CategoryId 等已配置 |
| B 权限 | `Permissions/Core/MenuCode.cs` | `Website_Page`、`Content_Product`、`Content_ProductCategory` 常量存在 |
| B 权限 | `Permissions/Core/PermissionType.cs` | `Design = 8`、`Publish = 9` 存在 |
| B 权限 | `Common/PageCode.cs` | `PAGE_Button_Design/Publish/Preview` 存在 |
| B 种子 | `EntityFramework/MenuSeedData.cs` | `WebsiteMenu` + `GetWebsiteMenus` 已注入 |
| B 种子 | `EntityFramework/DataInitializer.cs` | `InitSiteConfig`、`InitFooter`、`InitNavigation`、`InitSitePages` 已在 `Create()` 中调用；`InitSiteMenus` 注入"网站管理"组 |
| C 审计 | `Filters/AuditLogFilter.cs` | `GetEntityType`、`GetOperationTable`、`BuildOperationDesc` 三个 switch 已扩展；`GetOperationType` 已加 `Publish`/`Design` 分支 |
| C 视图模型 | `Models/Site/{SiteConfigModel,PageModel,NavigationModel,FooterModel}.cs`、`Models/Products/ProductModel.cs`、`Models/Jobs/JobModel.cs` | 全部存在 |
| C 控制器 | `Controllers/Admin/{SiteConfig,Page,Navigation,Footer,Product,ProductCategory,Job}Controller.cs` | 7 个控制器全部创建；PageController 关键 action `Design/GetComponentData/SaveDraft/Publish/Preview` 已具备 |

### 关键调用约定（来自已存在代码）

**`PageController.Design(int id)` 行为（已核实）**：
- ViewBag 仅传 `PageId`、`PageName`、`PagePath`
- **不**通过 ViewBag 传 ComponentJson；设计器视图必须 AJAX 调用 `/Page/GetComponentData?pageId=X` 加载组件 JSON
- 默认 `return View()` → 视图文件名按 action 名为 `Views/Page/Design.cshtml`

**`PageController.Preview(int id)` 行为（已核实）**：
- ViewBag 传 `PageId`、`PageName`、`PagePath`、`ComponentJson`（直接内联 JSON 字符串）
- 默认 `return View()` → 视图文件 `Views/Page/Preview.cshtml`

**`PageController.GetComponentData(int pageId)` 返回结构（已核实）**：
```json
{
  "code": 200, "message": "成功",
  "pageId": 1, "pageName": "首页", "pagePath": "/",
  "status": 1,
  "components": [<反序列化自 page.ComponentJson，默认 []>]
}
```

**`PageController.SaveDraft(int id, string componentJson)`**：
- HTTP POST，**form/query 参数** `componentJson` 为字符串（非 JSON body）
- 前端需 `JSON.stringify(pageState.components)` 后作为 form 字段提交

**`PageController.Publish(int id)`**：
- HTTP POST，无 body；服务端读取已持久化的 `page.ComponentJson` 作为 PublishJson
- 每次发布创建新版本行（VersionNo 递增）

### 待解决缺口

| 缺口 | 影响 | 解决方案 |
|---|---|---|
| `wwwroot/syle/` 目录不存在 | `_Layout.cshtml` 引用 `~/syle/site.css` 和 `~/syle/site.js` 都 404 | 新建 `_PublicLayout.cshtml` 引用 `~/site/...`，不动 `_Layout.cshtml` |
| `wwwroot/site/` 不存在 | 前台样式无落点 | 创建该目录及 `css/site.css`、`js/site.js` |
| `Views/Home/` 不存在 | 路由已配置 9 个 Home action 但无视图 | 创建 `Views/Home/Index.cshtml` + `Views/Home/Components/_*.cshtml` |
| `HomeController.cs` 不存在 | 9 个公共路由全部 404 | 创建 `Controllers/HomeController.cs`（不继承 `AdminBaseController`、不加 `[Authorize]`） |
| `_Layout.cshtml` 的 `~/syle/` 引用失效 | 影响所有默认 Layout 视图 | 新建 `_PublicLayout.cshtml` 替代；不改 `_Layout.cshtml`（保留兼容性，因 admin 页面 `Layout=null` 不走它） |
| `DataInitializer` 引用 `/syle/images/...` 路径 | Article 种子数据图片 404（既有问题，与本任务无关） | 不处理 |

### 路由现状（已存在于 `Startup.cs`）

```csharp
endpoints.MapControllerRoute("home", "", new { controller = "Home", action = "Index" });
endpoints.MapControllerRoute("About", "about", new { controller = "Home", action = "About" });
endpoints.MapControllerRoute("ProductDetail", "products/detail-{id}.html", new { controller = "Home", action = "ProductDetail" });
endpoints.MapControllerRoute("Products", "products/{category?}", new { controller = "Home", action = "Products" });
endpoints.MapControllerRoute("ArticlePreview", "news/preview-{id}.html", new { controller = "Home", action = "ArticlePreview" });
endpoints.MapControllerRoute("Article", "news/info-{id}.html", new { controller = "Home", action = "Article" });
endpoints.MapControllerRoute("News", "news/{category?}", new { controller = "Home", action = "News" });
endpoints.MapControllerRoute("Jobs", "jobs", new { controller = "Home", action = "Jobs" });
endpoints.MapControllerRoute("Contact", "contact", new { controller = "Home", action = "Contact" });
endpoints.MapControllerRoute("default", "{controller=Admin}/{action=Index}/{id?}");
```

`HomeController` 必须 1:1 实现这 9 个 action 名。

### 关键约定（来自 `Views/Article/`、`Views/Menu/` 探索）

- 视图放在 `Views/<ControllerName>/<ActionName>.cshtml`（**不是** `Views/Admin/...`）
- 所有 admin 页 `@{ Layout = null; }` + 完整独立 HTML 文档
- layui 头部：`<link rel="stylesheet" href="~/resource/layuiadmin/layui/css/layui.css">` + `~/resource/layuiadmin/style/admin.css`
- layui 入口：`layui.config({ base: '@Url.Content("~")/resource/layuiadmin/' }).extend({ index: 'lib/index' }).use([...], function(){ var $ = layui.$, form = layui.form, table = layui.table; ... })`
- `table.render` 必须设 `request:{ pageName:'pageIndex', limitName:'pageSize' }`，`response:{ statusName:'code', statusCode:'200', msgName:'message', countName:'count', dataName:'data' }`
- 编辑表单 `<form class="layui-form" lay-filter="form-group">`，提交按钮 `lay-filter="form-page"`，handler 末尾 `return false`
- 按钮权限：`@if ((bool)ViewData[PageCode.PAGE_Button_Add]) { ... }`（**强转 bool**，controller 必须赋值，否则抛 InvalidCastException）
- 图片选择器：`layer.open({ type:2, content:'/Admin/ImageSelector' })` + `window.addEventListener('message', e => e.data.type==='imageSelected' && (imgUrl=e.data.url))`
- 富文本：`~/resource/wangeditor-4.7.9/wangEditor.min.js`，`new E('#container')`，`uploadImgServer='/Upload/UploadImage'`，`uploadFileName='file'`
- 树表：`use(['index','table','treetable'])`，`treetable.render({ treeColIndex, treeSpid:0, treeIdName:'id', treePidName:'pid', data:[...] })`

### 拖拽库已就位

- `wwwroot/resource/js/Sortable.js`（1.14.0，127KB）和 `Sortable.min.js`（44KB）
- `wwwroot/resource/js/moduleSet.js`（2.3KB）—— jQuery 插件 `$.fn.moduleSet`，给 hover 块覆盖编辑/美化/删除按钮条
- `wwwroot/resource/js/pickr.min.js`（23KB）—— Pickr 颜色选择器，输出 HEX/RGBA

---

## 拟定变更

### Part D — 后台视图（15 个 .cshtml）

> **执行前必读**：先用 Read 工具读取每个对应控制器的 action 源码，确认 `return View()` / `return View("Edit")` / `return View(model)` 的精确形式，再决定视图文件名。下面按 action 默认名给出。

#### D1. `Views/SiteConfig/Edit.cshtml`（站点设置）

- 路径：`src/CIMC.WebSite/Views/SiteConfig/Edit.cshtml`
- 控制器：`SiteConfigController.Index` 返回 Edit 视图（请核实 `return View("Edit", model)` 或 `return View(model)`，两者决定文件名 `Edit.cshtml` vs `Index.cshtml`）
- 形态：单例编辑表单，`@model SiteConfigModel`
- 字段：SiteName、Logo（图片选择器）、BrowserTitle、Keywords、Description、IcpNo、PoliceNo、Phone、Email、Address、Copyright、Theme、Language、IsActive
- Logo 字段：复用 Article/Edit.cshtml 的 `openImageSelector()` 模式
- 表单提交：`form.on('submit(form-page)')` → `$.ajax POST /siteconfig/edit?id=1`
- 验证：SiteName 必填

#### D2. `Views/Page/Index.cshtml`（页面列表）

- 路径：`src/CIMC.WebSite/Views/Page/Index.cshtml`
- 形态：layui `table.render` 列表页
- 列：Id、PageName、PagePath、Status（Status=1 已发布 / 0 草稿）、IsHome（templet）、Sort、CreateTime、操作
- 操作列 templet（按按钮权限）：
  - `@if PAGE_Button_Design` → `<a lay-event="design">装修</a>` 跳 `/page/design/{id}`
  - `@if PAGE_Button_Preview` → `<a lay-event="preview">预览</a>` 跳 `/page/preview/{id}`
  - `@if PAGE_Button_Publish` → `<a lay-event="publish">发布</a>` AJAX POST `/page/publish?id={id}`
  - `@if PAGE_Button_Edit` → `<a lay-event="edit">编辑</a>` 跳 `/page/edit/{id}`
  - `@if PAGE_Button_Delete` → `<a lay-event="del">删除</a>`
- 顶部工具条：`@if PAGE_Button_Add` → `<a href="/page/edit">新增页面</a>`
- URL：`@Url.Content("~")/Page/GetList`，`request:{pageName:'pageIndex',limitName:'pageSize'}`

#### D3. `Views/Page/Edit.cshtml`（页面基础信息编辑）

- 路径：`src/CIMC.WebSite/Views/Page/Edit.cshtml`
- `@model PageModel`
- 字段：PageName、PageCode、PagePath、PageTitle、SeoKeywords、SeoDescription、IsHome(radio)、IsActive(radio)、Sort
- 表单提交 POST `/page/edit?id=@Model.Id`

#### D4. `Views/Page/Design.cshtml`（核心可视化设计器）

- 路径：`src/CIMC.WebSite/Views/Page/Design.cshtml`
- `@{ Layout = null; }`，**不**带 `@model`（数据走 AJAX）
- 头部：layui.css + admin.css + `Sortable.min.js` + `moduleSet.js` + `pickr.min.js` + `wangEditor.min.js`
- 布局：
  ```
  ┌────────────────────────────────────────────────┐
  │ 顶栏：[←返回] [页面名] [保存草稿][预览][发布]  │
  ├──────────┬───────────────────┬─────────────────┤
  │ 左：组件库│ 中：画布 #canvas   │ 右：属性 #props │
  │ 布局组件  │  块列表（块=标题条+ │  选中块的属性表 │
  │ 内容组件  │  内容预览+hover工具）│  （动态切换）  │
  └──────────┴───────────────────┴─────────────────┘
  ```
- 左侧组件库：HTML 卡片列表，每个 `<div class="lib-item" data-type="banner">Banner</div>` 等
  - 类型清单：navigation、banner、news、product、job、footer、richText、image、title
- 中间画布：`<div id="canvas"></div>`，每块 = `<div class="block" data-id="...">` 标题条 + `<div class="block-body">`预览 + `data-type` 控制渲染
- 右侧属性面板：`<div id="propsPanel">`，根据选中 `data-type` 渲染不同表单字段
- **JS 全局状态**：
  ```js
  var pageState = {
    pageId: @ViewBag.PageId,
    pageName: '@ViewBag.PageName',
    pagePath: '@ViewBag.PagePath',
    components: []  // List<{id,type,name,sort,visible,locked,props,style}>
  };
  var history = []; var historyIndex = -1;  // 撤销/重做
  ```
- **Sortable 初始化**：
  ```js
  // 左侧组件库（克隆拖入）
  Sortable.create(document.getElementById('libList'), {
    group: { name: 'page', pull: 'clone', put: false },
    sort: false,
    animation: 150,
    onEnd: function(evt) {
      // 仅当 put 到 canvas 时插入；删除 sortable 自带的 clone DOM
      var type = $(evt.item).attr('data-type');
      if (evt.to.id === 'canvas') {
        $(evt.item).remove();  // 移除 sortable 自动 clone 的 DOM
        addBlock(type);  // 用我们自己的模板插入真实 block
      }
    }
  });
  // 中间画布（纵向排序）
  Sortable.create(document.getElementById('canvas'), {
    group: { name: 'page', pull: false, put: true },
    animation: 150,
    handle: '.drag-handle',
    onEnd: updateOrder
  });
  ```
- **block 模板函数**：每个 type 一个 `renderBlock(comp)` 函数返回 HTML 字符串，包含 `.drag-handle`、标题、内容预览、hover 工具条（上移/下移/复制/删除/隐藏/编辑属性）
- **属性面板渲染**：`renderPropPanel(comp)` 根据 `comp.type` 渲染对应字段；表单 `change` 事件回写到 `pageState.components[idx].props[k]=v` 并刷新画布对应 block 预览
- **关键 prop 字段示例**（按 type）：
  - `banner`: `height`(number)、`autoplay`(checkbox)、`interval`(number)、`items`[](array of `{title, subtitle, image, buttonText, buttonLink}`)
  - `news`: `categoryId`(number)、`pageSize`(number)、`showStyle`(select: list/card)、`showCover`(checkbox)、`showSummary`(checkbox)、`showDate`(checkbox)、`moreLink`(text)
  - `product`: `categoryId`(number)、`pageSize`(number)、`colsPerRow`(select 1-4)、`showImage`、`showSummary`、`showSpec`、`moreLink`
  - `job`: `pageSize`、`showLocation`、`showSalary`、`showCount`、`showPublishTime`
  - `navigation`: `navStyle`(select)、`isFixedTop`、`bgColor`、`textColor`、`activeColor`
  - `footer`: `bgColor`、`textColor`
  - `richText`: `html`(wangEditor)、`paddingTop`、`paddingBottom`
  - `image`: `src`(图片选择器)、`alt`、`width`(number|%)、`align`(select)
  - `title`: `text`、`level`(select 1-6)、`align`、`color`
- **图片字段**：`openImageSelector` 弹窗选完写回当前选中 block 的对应 prop
- **颜色字段**：`Pickr.create({ el, default, components: true })`，`onChange` 写回 prop 并刷新预览
- **富文本字段**：`new E('#richTextContainer')` + `uploadImgServer='/Upload/UploadImage'`
- **顶部工具条按钮**：
  - 保存草稿：`$.ajax POST /Page/SaveDraft?id={pageId}`，data `{ componentJson: JSON.stringify(pageState.components) }`
  - 预览：`window.open('/page/preview/{pageId}')`
  - 发布：`layer.confirm` → `$.ajax POST /Page/Publish?id={pageId}` → `layer.alert('发布成功')` → `location.href='/page/index'`
  - 撤销/重做：`undo()`/`redo()`，操作前 `pushHistory(JSON.parse(JSON.stringify(pageState)))`
- **启动流程**：DOM ready → `$.ajax GET /Page/GetComponentData?pageId={pageId}` → `pageState.components = res.components` → `renderCanvas()` → `bindEvents()`

#### D5. `Views/Page/Preview.cshtml`（页面预览）

- 路径：`src/CIMC.WebSite/Views/Page/Preview.cshtml`
- `@{ Layout = null; }`（独立全屏预览）
- 使用 `ViewBag.ComponentJson`（已核实 Preview action 内联传入）
- 渲染：内嵌一个简化版前台布局，遍历组件 JSON，根据 type 渲染简化预览（**不查数据库**，只渲染配置内容，如 banner 显示图片、news 显示"将展示 N 条新闻"占位）
- 提供"返回装修"按钮 `history.back()`

#### D6. `Views/Navigation/Index.cshtml`（导航树表）

- 路径：`src/CIMC.WebSite/Views/Navigation/Index.cshtml`
- 形态：treetable（参考 `Views/Menu/Index.cshtml`）
- 列：Title、Path、Icon、Sort、IsShow(templet)、操作
- 数据加载：`$.ajax GET /Navigation/GetList` → `treetable.render({ treeIdName:'id', treePidName:'pid', treeSpid:0, data:res.data })`
- 操作列：编辑（`/navigation/edit/{id}`）、删除、新增子导航（仅 pid=0 时显示）
- 顶部：`@if PAGE_Button_Add` → `<a href="/navigation/edit">新增导航</a>`

#### D7. `Views/Navigation/Edit.cshtml`（导航编辑）

- 路径：`src/CIMC.WebSite/Views/Navigation/Edit.cshtml`
- `@model NavigationModel`
- 字段：Pid（treeSelect 异步加载 `/Navigation/GetList`）、Title、Path、Icon（iconPicker 或文本）、Target(radio: 0=本窗口/1=新窗口)、Sort、IsShow(radio)
- 提交 POST `/navigation/edit?id=@Model.Id`

#### D8. `Views/Footer/Edit.cshtml`（页脚配置）

- 路径：`src/CIMC.WebSite/Views/Footer/Edit.cshtml`
- 控制器：`FooterController.Index` 返回 Edit 视图（请核实 `return View("Edit")`）
- `@model FooterModel`
- 字段：Logo、CompanyName、Intro、Phone、Email、Address、Qrcode、IcpNo、PoliceNo、Copyright、FriendLinks（JSON 数组，UI 用动态行表格编辑：title+url）、BgColor（pickr）、TextColor（pickr）
- 提交 POST `/footer/edit?id=1`

#### D9. `Views/Product/Index.cshtml`（产品列表）

- 路径：`src/CIMC.WebSite/Views/Product/Index.cshtml`
- 形态：layui table.render，镜像 `Views/Article/Index.cshtml`
- 列：Id、ProductName、CategoryName、CoverImage(templet img)、Sort、IsRecommend(templet switch)、IsActive(templet)、操作
- 操作：编辑、删除、推荐切换（`SetRecommend` AJAX）
- 顶部：新增、批量删除
- 搜索栏：关键字、分类下拉（异步 `/ProductCategory/GetList`）

#### D10. `Views/Product/Edit.cshtml`（产品编辑）

- 路径：`src/CIMC.WebSite/Views/Product/Edit.cshtml`
- `@model ProductModel`
- 字段：ProductName、CategoryId（select 异步加载）、CoverImage（图片选择器）、ImageList（多图，重复使用图片选择器+缩略图列表+删除）、Summary、Description（wangEditor）、Specification（wangEditor 或 textarea）、Feature、Sort、IsRecommend(radio)、IsActive(radio)
- 提交 POST `/product/edit?id=@Model.Id`

#### D11. `Views/ProductCategory/Index.cshtml`（产品分类树表）

- 路径：`src/CIMC.WebSite/Views/ProductCategory/Index.cshtml`
- 形态：treetable
- 列：Name、Sort、IsActive(templet)、操作（编辑/删除/新增子分类）

#### D12. `Views/ProductCategory/Edit.cshtml`（分类编辑）

- 路径：`src/CIMC.WebSite/Views/ProductCategory/Edit.cshtml`
- `@model ProductCategoryModel`（核实 ProductModel.cs 中是否含 ProductCategoryModel 类，否则用 dynamic 或在 ProductCategoryController ViewBag 传）
- 字段：Pid（treeSelect）、Name、Sort、IsActive(radio)
- 提交 POST `/productcategory/edit?id=@Model.Id`

#### D13. `Views/Job/Index.cshtml`（招聘列表）

- 路径：`src/CIMC.WebSite/Views/Job/Index.cshtml`
- 形态：layui table.render
- 列：Id、JobTitle、Department、WorkLocation、SalaryRange、RecruitCount、Sort、IsActive(templet)、操作（编辑/删除）
- 搜索：关键字、IsActive 下拉

#### D14. `Views/Job/Edit.cshtml`（招聘编辑）

- 路径：`src/CIMC.WebSite/Views/Job/Edit.cshtml`
- `@model JobModel`
- 字段：JobTitle、Department、WorkLocation、SalaryRange、RecruitCount、JobType(select)、Responsibilities(wangEditor)、Requirements(wangEditor)、ContactName、ContactPhone、ContactEmail、Sort、IsActive(radio)
- 提交 POST `/job/edit?id=@Model.Id`

---

### Part E — 前台渲染

#### E1. `Controllers/HomeController.cs`

- 路径：`src/CIMC.WebSite/Controllers/HomeController.cs`
- 命名空间：`MySite.Web.Controllers`
- 继承 `Controller`（**不**继承 `AdminBaseController`，**不**加 `[Authorize]`）
- 构造注入：`IRepository<WebsitePage>`、`IRepository<WebsitePageVersion>`、`IRepository<WebsiteSiteConfig>`、`IRepository<WebsiteNavigation>`、`IRepository<WebsiteFooter>`、`IRepository<Article>`、`IRepository<ContentProduct>`、`IRepository<ContentProductCategory>`、`IRepository<ContentJob>`
- 9 个 action（**严格匹配 Startup.cs 路由名**）：

```csharp
public IActionResult Index() => RenderPage(p => p.IsHome && !p.IsDelete);
public IActionResult About() => RenderPage(p => p.PagePath == "/about" && !p.IsDelete);
public IActionResult Products(string category) => RenderPage(p => p.PagePath == "/products" && !p.IsDelete);
public IActionResult News(string category) => RenderPage(p => p.PagePath == "/news" && !p.IsDelete);
public IActionResult Jobs() => RenderPage(p => p.PagePath == "/jobs" && !p.IsDelete);
public IActionResult Contact() => RenderPage(p => p.PagePath == "/contact" && !p.IsDelete);

public IActionResult Article(int id) { /* 查 Article 表 + ViewCount++ + 返回 detail view */ }
public IActionResult ArticlePreview(int id) { /* 同 Article 但不+ViewCount */ }
public IActionResult ProductDetail(int id) { /* 查 ContentProduct + ViewCount++ + 返回 detail view */ }
```

- 私有 `RenderPage(Expression<Func<WebsitePage, bool>> predicate)`：
  ```csharp
  var page = _pageRepo.GetList(predicate, p => p.Id, 1, 1, false).List.FirstOrDefault();
  if (page == null) return NotFound();
  var version = _versionRepo.GetList(v => v.PageId == page.Id && v.Status == 1, v => v.VersionNo, 1, 1, false).List.FirstOrDefault();
  var json = version?.PublishJson ?? page.ComponentJson ?? "[]";
  var components = JsonConvert.DeserializeObject<List<ComponentModel>>(json) ?? new List<ComponentModel>();
  var siteConfig = _siteConfigRepo.GetOne(1) ?? new WebsiteSiteConfig();
  var navs = _navRepo.GetList(n => !n.IsDelete && n.IsActive, n => n.Sort, 1, 100, true).List.ToList();
  var footer = _footerRepo.GetOne(1) ?? new WebsiteFooter();
  var model = new PageRenderModel {
      PageId = page.Id, PageName = page.PageName, PagePath = page.PagePath,
      PageTitle = page.PageTitle, SeoKeywords = page.SeoKeywords, SeoDescription = page.SeoDescription,
      Components = components,
      SiteConfig = ToConfigModel(siteConfig),
      Navigation = navs.Select(ToNavModel).ToList(),
      Footer = ToFooterModel(footer)
  };
  return View("~/Views/Home/Index.cshtml", model);
  ```
- `Article(id)`：
  ```csharp
  var article = _articleRepo.GetOne(id);
  if (article == null || article.IsDelete) return NotFound();
  article.ViewCount++;
  _articleRepo.Update(article);
  return View("~/Views/Home/Article.cshtml", article);
  ```
- `ProductDetail(id)`：类似，查 ContentProduct + ViewCount++

#### E2. `Views/Shared/_PublicLayout.cshtml`

- 路径：`src/CIMC.WebSite/Views/Shared/_PublicLayout.cshtml`
- 形态：完整 HTML 框架（不是 admin 的 `Layout = null` 独立页）
- 内容：
  ```cshtml
  @model PageRenderModel
  @{
      var title = string.IsNullOrWhiteSpace(Model.PageTitle) ? Model.SiteConfig?.BrowserTitle : Model.PageTitle;
      var keywords = string.IsNullOrWhiteSpace(Model.SeoKeywords) ? Model.SiteConfig?.Keywords : Model.SeoKeywords;
      var description = string.IsNullOrWhiteSpace(Model.SeoDescription) ? Model.SiteConfig?.Description : Model.SeoDescription;
  }
  <!DOCTYPE html>
  <html lang="zh-CN">
  <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1.0">
      <title>@(title) - @(Model.SiteConfig?.SiteName)</title>
      <meta name="keywords" content="@keywords">
      <meta name="description" content="@description">
      <link rel="stylesheet" href="~/resource/layuiadmin/layui/css/layui.css">
      <link rel="stylesheet" href="~/site/css/site.css">
  </head>
  <body>
      @RenderBody()
      <script src="~/resource/layuiadmin/layui/layui.js"></script>
      <script src="~/site/js/site.js"></script>
  </body>
  </html>
  ```

#### E3. `Views/Home/Index.cshtml`（页面主视图）

- 路径：`src/CIMC.WebSite/Views/Home/Index.cshtml`
- 内容：
  ```cshtml
  @model PageRenderModel
  @{ Layout = "~/Views/Shared/_PublicLayout.cshtml"; }
  <div class="site-page" data-page-id="@Model.PageId">
      @foreach (var comp in Model.Components)
      {
          if (!comp.Visible) continue;
          <div class="component-block" data-type="@comp.Type" data-id="@comp.Id">
              @switch (comp.Type)
              {
                  case "navigation": <partial name="~/Views/Home/Components/_Navigation.cshtml" model="comp" /> break;
                  case "banner":     <partial name="~/Views/Home/Components/_Banner.cshtml" model="comp" /> break;
                  case "news":       <partial name="~/Views/Home/Components/_NewsList.cshtml" model="comp" /> break;
                  case "product":    <partial name="~/Views/Home/Components/_ProductList.cshtml" model="comp" /> break;
                  case "job":        <partial name="~/Views/Home/Components/_JobList.cshtml" model="comp" /> break;
                  case "footer":     <partial name="~/Views/Home/Components/_Footer.cshtml" model="comp" /> break;
                  case "richText":   <partial name="~/Views/Home/Components/_RichText.cshtml" model="comp" /> break;
                  case "image":      <partial name="~/Views/Home/Components/_Image.cshtml" model="comp" /> break;
                  case "title":      <partial name="~/Views/Home/Components/_Title.cshtml" model="comp" /> break;
              }
          </div>
      }
  </div>
  ```
- 注意：partial model 类型为 `ComponentModel`，partial 内通过 `Model.Props["key"]` 取值；需要数据库的 partial 用 `@inject IRepository<T>` 注入仓储

#### E4. `Views/Home/Article.cshtml`（新闻详情）

- 路径：`src/CIMC.WebSite/Views/Home/Article.cshtml`
- `@model Article`
- 形态：标准新闻详情页，显示 Title、Author、Source、CreationTime、ImageUrl、Detail（Html.Raw）
- 顶部导航栏与页脚复用 Index 渲染逻辑（简化：手动注入 SiteConfig/Footer/Navigation 到 ViewBag 或 layout）
- **简化方案**：Article 详情页可不走组件渲染，直接用静态头部+底部布局；只渲染文章内容主体。后续可改为动态。

#### E5. `Views/Home/ProductDetail.cshtml`（产品详情）

- 路径：`src/CIMC.WebSite/Views/Home/ProductDetail.cshtml`
- `@model ContentProduct`
- 显示 ProductName、CoverImage、ImageList、Specification、Description、Feature

#### E6-E14. 9 个组件 partial（`Views/Home/Components/`）

每个 partial 接收 `@model ComponentModel`，通过 `Model.Props` 取配置项。需要查数据库的 partial 用 `@inject IRepository<T>` 注入仓储。

##### E6. `_Navigation.cshtml`
- `@model ComponentModel`
- `@inject IRepository<WebsiteNavigation>`
- 读取 props：`navStyle`、`isFixedTop`、`bgColor`、`textColor`、`activeColor`（覆盖默认）
- 实际导航数据：通过 `ViewContext.HttpContext.RequestServices` 获取已查询好的导航（或在 partial 内重新查询）
- **简化方案**：直接在 partial 内重新查询 `WebsiteNavigation` 表（`IsActive && !IsDelete`，按 Sort 排序），渲染一级+二级菜单；高亮当前路径（通过 `ViewContext.HttpContext.Request.Path`）

##### E7. `_Banner.cshtml`
- `@model ComponentModel`
- props：`height`、`autoplay`、`interval`、`items[]`（每个 `{title, subtitle, image, buttonText, buttonLink}`）
- 渲染：layui carousel `layui.carousel.render({ elem:'#banner-{id}', autoplay, interval })`
- 每个 item：背景图 + 文字层 + 按钮

##### E8. `_NewsList.cshtml`
- `@model ComponentModel`
- `@inject IRepository<Article>`
- props：`categoryId`、`pageSize`、`showStyle`、`showCover`、`showSummary`、`showDate`、`moreLink`
- 查询：`GetList(a => !a.IsDelete && a.IsActive, a => a.CreationTime, 1, pageSize, false)`
- 渲染：列表/卡片样式；详情链接 `/news/info-{id}.html`

##### E9. `_ProductList.cshtml`
- `@model ComponentModel`
- `@inject IRepository<ContentProduct>`
- props：`categoryId`、`pageSize`、`colsPerRow`、`showImage`、`showSummary`、`showSpec`、`moreLink`
- 查询：按 `categoryId` 过滤（0=全部）、`IsActive && !IsDelete`、Sort
- 渲染：CSS Grid `grid-template-columns: repeat(var(--cols), 1fr)`，PC 4 / 平板 2 / 手机 1（媒体查询在 site.css）
- 详情链接 `/products/detail-{id}.html`

##### E10. `_JobList.cshtml`
- `@model ComponentModel`
- `@inject IRepository<ContentJob>`
- props：`pageSize`、`showLocation`、`showSalary`、`showCount`、`showPublishTime`
- 查询：`GetList(j => !j.IsDelete && j.IsActive, j => j.Sort, 1, pageSize, true)`
- 渲染：表格样式（职位/部门/地点/薪资/人数/发布时间）

##### E11. `_Footer.cshtml`
- `@model ComponentModel`
- `@inject IRepository<WebsiteFooter>`
- 查询：`GetOne(1)`
- props：`bgColor`、`textColor`（覆盖实体默认值）
- 渲染：四列（公司简介+Logo / 联系方式 / 友情链接 JSON / 二维码）+ ICP/Police 备案

##### E12. `_RichText.cshtml`
- `@model ComponentModel`
- props：`html`、`paddingTop`、`paddingBottom`
- 渲染：`<div style="padding:@(paddingTop)px 0 @(paddingBottom)px">@Html.Raw(html)</div>`

##### E13. `_Image.cshtml`
- `@model ComponentModel`
- props：`src`、`alt`、`width`、`align`
- 渲染：`<img src="@Url.Content("~")@src" alt="@alt" style="width:@width; text-align:@align">`

##### E14. `_Title.cshtml`
- `@model ComponentModel`
- props：`text`、`level`(1-6)、`align`、`color`
- 渲染：`<h{level} style="text-align:@align; color:@color">@text</h{level}>`

#### E15. `wwwroot/site/css/site.css`

- 路径：`src/CIMC.WebSite/wwwroot/site/css/site.css`
- 内容：
  - Reset / box-sizing
  - `.site-page` 容器
  - `.component-block` 间距
  - `.site-nav`（固定顶栏、Logo+菜单横排、移动端折叠按钮）
  - `.site-banner` carousel 样式
  - `.site-news-list`（卡片/列表两种）
  - `.site-product-grid` CSS Grid + 媒体查询：`@media (max-width:768px){ grid-template-columns: repeat(1,1fr) }`、`@media (max-width:992px){ repeat(2,1fr) }`、默认 `repeat(var(--cols,4),1fr)`
  - `.site-job-table`
  - `.site-footer`（多列网格、移动端堆叠）
  - 公共工具：`.container`（max-width:1200px 居中）、`.section-title`

#### E16. `wwwroot/site/js/site.js`

- 路径：`src/CIMC.WebSite/wwwroot/site/js/site.js`
- 内容：
  - 移动端导航折叠按钮点击事件
  - layui carousel 初始化（遍历 `[data-banner]` 元素）
  - 产品/新闻列表图片懒加载（可选）
  - 当前页导航高亮（根据 `window.location.pathname` 匹配菜单 `data-path`）

---

### Part F — 验证

#### F1. 编译

```powershell
dotnet build d:\MyProject\my-site\MySite.sln
```
- 期望：0 错误、0 警告（或仅无关警告）
- 修复：根据编译报错调整 using、命名空间、类型不匹配等

#### F2. 添加 EF Core 迁移

```powershell
# 必须在 WebSite 目录运行（appsettings.json 在此处）
cd d:\MyProject\my-site\src\CIMC.WebSite
dotnet ef migrations add AddPageBuilder --project src\CIMC.EntityFramework --startup-project .
```
- **注意**：路径相对当前目录。如果在 `d:\MyProject\my-site\` 则用 `--project src\CIMC.EntityFramework --startup-project src\CIMC.WebSite`
- 期望：在 `src/CIMC.EntityFramework/Migrations/` 下生成 `YYYYMMDDHHMMSS_AddPageBuilder.cs` + `.Designer.cs` + `.model.json` 快照
- 8 个新实体应生成对应 `CreateTable` 操作

#### F3. 应用迁移

```powershell
cd d:\MyProject\my-site\src\CIMC.WebSite
dotnet ef database update --project src\CIMC.EntityFramework --startup-project .
```
- 期望：MySQL 数据库 `my_site` 出现新表：`WebsitePage`、`WebsitePageVersion`、`WebsiteSiteConfig`、`WebsiteNavigation`、`WebsiteFooter`、`ContentProduct`、`ContentProductCategory`、`ContentJob`

> **替代方案**：因 `Startup.cs` 中 `Configure()` 已调用 `dbContext.Database.Migrate()`，直接 `dotnet run` 也会自动应用迁移。但显式 `database update` 可在运行前确认 SQL 正确性。

#### F4. 运行验证

```powershell
cd d:\MyProject\my-site\src\CIMC.WebSite
dotnet run
```
- 访问 `http://localhost:5076/admin/login`，用 `admin / 123qwe` 登录
- 左侧菜单应出现"网站管理"组（站点设置/页面管理/导航管理/页脚设置）和"内容管理"组扩展（产品分类/产品管理/招聘管理）

#### F5. 功能验证清单

1. **页面管理**：网站管理 → 页面管理 → 列表显示种子页面（首页/关于我们/产品中心/新闻中心/招聘中心/联系我们）
2. **设计器**：点击"装修" → 进入 `/page/design/{id}` → 左侧组件库可见 → 拖入 Banner → 右侧属性面板出现 → 修改 height → 画布预览更新 → 点击"保存草稿" → 弹出"保存成功"
3. **预览**：点击"预览" → 新窗口打开 `/page/preview/{id}` → 显示组件内容
4. **发布**：点击"发布" → 弹出"发布成功" → 列表中该页 Status 变为"已发布"
5. **内容管理**：内容管理 → 产品管理 → 新增产品 → 上传封面 → 选择分类 → 保存 → 列表显示
6. **前台访问**：浏览器访问 `/` → 渲染已发布首页组件 → 顶部导航栏 → Banner 轮播 → 新闻列表（显示真实新闻数据） → 产品网格 → 页脚
7. **响应式**：DevTools 切换设备 → 产品网格 4→2→1、导航折叠
8. **权限**：用非超管账号登录 → "装修"按钮不可见（无 Design 权限时）
9. **审计**：系统管理 → 审计日志 → 看到"发布页面""装修页面"等记录

---

## 假设与决策

| 决策 | 理由 |
|---|---|
| 视图放 `Views/<ControllerName>/`（不加 `Admin/` 前缀） | 与现有 `Views/Article/`、`Views/Menu/` 一致；Razor 默认视图发现规则 |
| 所有 admin 视图 `Layout = null` | 与现有约定一致；layuiadmin iframe 标签页模式 |
| 新建 `_PublicLayout.cshtml` 而非修复 `_Layout.cshtml` | 不破坏既有引用（即使 `~/syle/` 路径暂时 404，admin 页面不受影响） |
| 组件 partial 内用 `@inject IRepository<T>` | 技能文档 §21 推荐的扩展方式；避免 HomeController 一次性预取所有数据 |
| Design 视图 AJAX 加载组件 | 已核实 PageController.Design 不通过 ViewBag 传 ComponentJson |
| Preview 视图直接渲染 `ViewBag.ComponentJson` | 已核实 Preview action 内联传入；不走数据库，纯配置预览 |
| SaveDraft 用 form 字段 `componentJson` 传字符串 | 已核实 action 签名 `(int id, string componentJson)`，简单类型绑定 |
| 发布每次创建新版本行 | 已核实 Publish 逻辑；提供审计历史，符合技能文档 §13 |
| `Views/Home/Article.cshtml` 简化渲染 | Article 详情页不走组件系统，避免 partial 内重复查询导航/页脚；后续可优化为组件化 |
| 不修改 `Startup.cs` 路由 | 9 个路由已存在；HomeController 1:1 匹配 action 名即可 |
| 不创建 `/api/...` REST 端点 | 与既有 `Article/GetList` 风格一致；前后台共用同一 controller |
| 不修复 `DataInitializer` 中 `/syle/images/` 引用 | 既有问题，与本任务无关 |
| 不实现页面版本回滚 UI | 技能文档 §20 明确排除；数据结构已支持，UI 延后 |
| 不实现像素级自由拖拽 | 技能文档 §15 明确 Phase 1 仅纵向块拖拽 |

## 文件清单（共 19 个新文件 + 0 修改）

**Part D — 后台视图（15 个）**：
1. `src/CIMC.WebSite/Views/SiteConfig/Edit.cshtml`
2. `src/CIMC.WebSite/Views/Page/Index.cshtml`
3. `src/CIMC.WebSite/Views/Page/Edit.cshtml`
4. `src/CIMC.WebSite/Views/Page/Design.cshtml` ← 核心设计器
5. `src/CIMC.WebSite/Views/Page/Preview.cshtml`
6. `src/CIMC.WebSite/Views/Navigation/Index.cshtml`
7. `src/CIMC.WebSite/Views/Navigation/Edit.cshtml`
8. `src/CIMC.WebSite/Views/Footer/Edit.cshtml`
9. `src/CIMC.WebSite/Views/Product/Index.cshtml`
10. `src/CIMC.WebSite/Views/Product/Edit.cshtml`
11. `src/CIMC.WebSite/Views/ProductCategory/Index.cshtml`
12. `src/CIMC.WebSite/Views/ProductCategory/Edit.cshtml`
13. `src/CIMC.WebSite/Views/Job/Index.cshtml`
14. `src/CIMC.WebSite/Views/Job/Edit.cshtml`

**Part E — 前台渲染（15 个）**：
15. `src/CIMC.WebSite/Controllers/HomeController.cs`
16. `src/CIMC.WebSite/Views/Shared/_PublicLayout.cshtml`
17. `src/CIMC.WebSite/Views/Home/Index.cshtml`
18. `src/CIMC.WebSite/Views/Home/Article.cshtml`
19. `src/CIMC.WebSite/Views/Home/ProductDetail.cshtml`
20. `src/CIMC.WebSite/Views/Home/Components/_Navigation.cshtml`
21. `src/CIMC.WebSite/Views/Home/Components/_Banner.cshtml`
22. `src/CIMC.WebSite/Views/Home/Components/_NewsList.cshtml`
23. `src/CIMC.WebSite/Views/Home/Components/_ProductList.cshtml`
24. `src/CIMC.WebSite/Views/Home/Components/_JobList.cshtml`
25. `src/CIMC.WebSite/Views/Home/Components/_Footer.cshtml`
26. `src/CIMC.WebSite/Views/Home/Components/_RichText.cshtml`
27. `src/CIMC.WebSite/Views/Home/Components/_Image.cshtml`
28. `src/CIMC.WebSite/Views/Home/Components/_Title.cshtml`
29. `src/CIMC.WebSite/wwwroot/site/css/site.css`
30. `src/CIMC.WebSite/wwwroot/site/js/site.js`

**Part F — 无新文件**，仅命令验证。

---

## 执行顺序建议

1. **Part D 顺序**：D1（SiteConfig）→ D8（Footer）→ D7（Navigation Edit）→ D6（Navigation Index）→ D3（Page Edit）→ D2（Page Index）→ D4（Page Design，最复杂，放最后单独调试）→ D5（Page Preview）→ D10/D9（Product）→ D12/D11（ProductCategory）→ D14/D13（Job）
2. **Part E 顺序**：E1（HomeController）→ E2（_PublicLayout）→ E3（Home/Index）→ E15（site.css）→ E16（site.js）→ E6-E14（9 个 partial）→ E4/E5（Article/ProductDetail 详情页）
3. **Part F 顺序**：F1（build）→ F2（migration add）→ F3（database update）→ F4（run）→ F5（功能验证）

每个 Part 内文件可并行创建（不互相依赖），但 D4 设计器需独立调试。
