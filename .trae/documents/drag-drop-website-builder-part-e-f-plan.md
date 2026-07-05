# 拖拉拽企业建站 Part E + F 实施计划

## 摘要

基于前序已完成的 Part A（8 个实体）/B（权限菜单种子）/C（7 个 admin 控制器 + view models）/D（14 个后台视图），本计划覆盖剩余两项：

- **Part E（前台渲染）**：创建 `HomeController`（9 个 action）+ `_PublicLayout` + `Home/Index` + 9 个组件 partial + Article/ProductDetail 详情页 + 前台静态资源（site.css/site.js）
- **Part F（验证）**：先修复前序遗留的编译错误（删除冲突的 `Models/PageCt/PageModel.cs`），再 `dotnet build` → 启动应用验证

## 当前状态分析

### 已完成（无需重复）
- 数据层：`WebsiteSiteConfig` / `WebsitePage` / `WebsitePageVersion` / `WebsiteNavigation` / `WebsiteFooter` / `ContentProduct` / `ContentProductCategory` / `ContentJob` 均已定义于 `CIMC.Data\Model\`
- 迁移：`src\CIMC.EntityFramework\Migrations\20260704132231_init.cs` 已存在，Startup.cs 中 `dbContext.Database.Migrate()` 会在启动时自动应用
- 后台：7 个 admin 控制器全部就位于 `Controllers\Admin\`，14 个视图全部就位于 `Views\{SiteConfig,Footer,Navigation,Page,ProductCategory,Product,Job}\`
- `PageController` 已实现：`Index` / `Edit` / `Design` / `Preview` / `Publish` / `SaveDraft` / `GetComponentData` / `GetList` / `Delete` / `SetHome`
- 路由：Startup.cs 已配置 9 个 Home 路由（`""`→Index、`about`→About、`products/{category?}`→Products、`products/detail-{id}.html`→ProductDetail、`news/{category?}`→News、`news/info-{id}.html`→Article、`news/preview-{id}.html`→ArticlePreview、`jobs`→Jobs、`contact`→Contact）+ default `{controller=Admin}/{action=Index}/{id?}`
- 视图模型：`Models\Site\PageModel.cs` 已定义 `PageModel` / `ComponentModel`（含 Props/Style Dictionary）/ `PageRenderModel`（含 PageId/PageName/PagePath/PageTitle/SeoKeywords/SeoDescription/Components/SiteConfig/Navigation/Footer）

### 关键发现：编译错误根因
- 旧的 `Models\PageCt\PageModel.cs` 定义了 `class PageModel { List<PageControlModel> PageControlList }`，命名空间为 `MySite.Web.Models`
- 新的 `Models\Site\PageModel.cs`（Part C 创建）也定义了 `class PageModel`，同一命名空间 `MySite.Web.Models`
- csproj 使用 `Microsoft.NET.Sdk.Web` SDK，自动包含所有 .cs 文件，无法通过 csproj 排除
- **结果**：CS0101 重复定义编译错误（与 memory 中"User reported a project compilation exception"对应）
- 验证：`PageCt\PageModel.cs` 与 `Pages\PageConfigModel.cs` 仅互相引用（PageControlList/PageControlModel），无任何 Controller/View 引用 → 安全删除 `Models\PageCt\PageModel.cs`

### 路由约定
- Home 路由的 controller 名称是 `"Home"`（不是 `"Admin"`），需创建 `Controllers\HomeController.cs`（不在 `Admin\` 子目录）
- Home 控制器**不继承** `AdminBaseController`，**不加** `[Authorize]`，**不注入** `IPermissionService`
- Home 视图默认 Layout 由 `_ViewStart.cshtml` 指定为 `"_Layout"`（admin layout），需在每个 Home 视图显式 `Layout = "_PublicLayout"` 覆盖

### 静态资源约定
- `wwwroot\` 仅含 `layui-v2.6.8\` 和 `resource\`，需新建 `wwwroot\site\css\site.css` 和 `wwwroot\site\js\site.js`
- 旧的 `Views\Shared\_Layout.cshtml` 引用 `~/syle/site.css`（拼写错误，文件也不存在），本计划不动它（admin 仍在用），新建独立的 `_PublicLayout.cshtml` 用正确路径 `~/site/css/site.css`

## 实施步骤

### 步骤 0：修复编译错误（Part F 前置）
**文件**：删除 `d:\MyProject\my-site\src\CIMC.WebSite\Models\PageCt/PageModel.cs`
**理由**：该文件定义的 `PageModel` 与 Part C 创建的 `Models\Site\PageModel.cs` 同名同命名空间，导致 CS0101 重复定义。该文件仅与 `Models\Pages\PageConfigModel.cs` 互相引用，无任何业务代码依赖。
**保留**：`Models\PageCt\FormDataModel.cs` 与 `Models\Pages\PageConfigModel.cs`（虽然也是死代码，但不冲突，最小化变更范围，不在本计划中删除）

### 步骤 E1：HomeController（核心控制器）
**文件**：`d:\MyProject\my-site\src\CIMC.WebSite\Controllers\HomeController.cs`
**命名空间**：`MySite.Web.Controllers`
**继承**：`Microsoft.AspNetCore.Mvc.Controller`（不继承 AdminBaseController，不加 [Authorize]）
**注入依赖**：
```csharp
private readonly IRepository<WebsitePage> _pageRepository;
private readonly IRepository<WebsitePageVersion> _versionRepository;
private readonly IRepository<WebsiteSiteConfig> _siteConfigRepository;
private readonly IRepository<WebsiteNavigation> _navigationRepository;
private readonly IRepository<WebsiteFooter> _footerRepository;
private readonly IRepository<Article> _articleRepository;
private readonly IRepository<ContentProduct> _productRepository;
private readonly IRepository<ContentProductCategory> _productCategoryRepository;
private readonly IRepository<ContentJob> _jobRepository;
```
**9 个 action**（严格匹配 Startup.cs 路由）：
1. `Index()` → 调用 `BuildPage(p => p.IsHome && !p.IsDelete)`，返回 `View("Index", model)`
2. `About()` → `BuildPage(p => p.PagePath == "/about" && !p.IsDelete)`
3. `Products(string category)` → `BuildPage(p => p.PagePath == "/products" && !p.IsDelete)`；额外预加载 `ViewBag.ProductList`（按 category 过滤，仅 IsActive && !IsDelete，按 Sort 升序，最多 20 条），`ViewBag.Categories`（顶级分类列表）
4. `ProductDetail(int id)` → 加载 `ContentProduct`（含 Category），如果未找到返回 `NotFound()`；预加载 `SiteConfig`/`Navigation`/`Footer` 装入 `ViewBag`；返回 `View("ProductDetail", product)`
5. `News(string category)` → `BuildPage(p => p.PagePath == "/news" && !p.IsDelete)`；预加载 `ViewBag.NewsList`（IsActive 文章，按 CreationTime 降序，最多 10 条）
6. `Article(int id)` → 加载 `Article`（必须 IsActive && !IsDelete），返回 `View("Article", article)`；预加载 SiteConfig/Navigation/Footer 到 ViewBag
7. `ArticlePreview(int id)` → 加载 `Article`（不过滤状态，允许预览未发布），返回 `View("Article", article)`
8. `Jobs()` → `BuildPage(p => p.PagePath == "/jobs" && !p.IsDelete)`；预加载 `ViewBag.JobList`（IsActive 岗位，按 Sort 升序）
9. `Contact()` → `BuildPage(p => p.PagePath == "/contact" && !p.IsDelete)`

**私有方法 `BuildPage`**：
```csharp
private PageRenderModel BuildPage(Expression<Func<WebsitePage, bool>> predicate)
{
    var page = _pageRepository.GetOne(predicate);
    if (page == null || page.Status != 1) return null;

    // 取最新已发布版本的 PublishJson，回退到 page.ComponentJson
    var publishedVersion = _versionRepository.GetList(v => v.PageId == page.Id && v.Status == 1)
        .OrderByDescending(v => v.VersionNo).FirstOrDefault();
    var componentJson = publishedVersion?.PublishJson ?? page.ComponentJson ?? "[]";

    var components = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ComponentModel>>(componentJson);

    var siteConfig = _siteConfigRepository.GetOne(1);
    var footer = _footerRepository.GetOne(1);
    var navigations = _navigationRepository.GetList(n => !n.IsDelete && n.IsActive && n.IsShow,
        n => n.Sort, true);

    var model = new PageRenderModel
    {
        PageId = page.Id,
        PageName = page.PageName,
        PagePath = page.PagePath,
        PageTitle = page.PageTitle,
        SeoKeywords = page.SeoKeywords,
        SeoDescription = page.SeoDescription,
        Components = components ?? new List<ComponentModel>(),
        SiteConfig = ToSiteConfigModel(siteConfig),
        Navigation = navigations.Select(n => ToNavigationModel(n)).ToList(),
        Footer = ToFooterModel(footer)
    };

    // 预加载常用数据到 ViewBag（组件 partial 共用）
    ViewData["Title"] = page.PageTitle ?? siteConfig?.BrowserTitle ?? siteConfig?.SiteName;
    ViewBag.SiteConfig = model.SiteConfig;
    ViewBag.NavigationList = model.Navigation;
    ViewBag.Footer = model.Footer;
    ViewBag.NewsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive,
        a => a.CreationTime, false).Take(6).ToList();
    ViewBag.ProductList = _productRepository.GetList(p => !p.IsDelete && p.IsActive,
        p => p.Sort, true).Take(8).ToList();
    ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive,
        j => j.Sort, true).ToList();

    return model;
}
```
**说明**：在 action 中如果 `BuildPage` 返回 null（页面未发布或不存在），返回 `View("NotFound")` 或 `NotFound()`。

### 步骤 E2：_PublicLayout（前台母版）
**文件**：`d:\MyProject\my-site\src\CIMC.WebSite\Views\Shared\_PublicLayout.cshtml`
**结构**：
```html
@{
    var siteConfig = ViewBag.SiteConfig as MySite.Web.Models.SiteConfigModel;
    var title = ViewData["Title"] as string ?? siteConfig?.BrowserTitle ?? siteConfig?.SiteName ?? "企业官网";
    var keywords = ViewData["Keywords"] as string ?? siteConfig?.Keywords;
    var description = ViewData["Description"] as string ?? siteConfig?.Description;
}
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@title</title>
    @if (!string.IsNullOrEmpty(keywords)) { <meta name="keywords" content="@keywords" /> }
    @if (!string.IsNullOrEmpty(description)) { <meta name="description" content="@description" /> }
    <link rel="stylesheet" href="~/site/css/site.css" />
</head>
<body>
    @RenderBody()
    <script src="~/site/js/site.js" defer></script>
</body>
</html>
```

### 步骤 E3：Home/Index.cshtml（页面渲染入口）
**文件**：`d:\MyProject\my-site\src\CIMC.WebSite\Views\Home\Index.cshtml`
**结构**：
```html
@{
    Layout = "_PublicLayout";
}
@model MySite.Web.Models.PageRenderModel

<div class="page-wrapper">
    @if (Model?.Components != null)
    {
        foreach (var comp in Model.Components)
        {
            if (!comp.Visible) continue;
            var partialName = "_" + char.ToUpper(comp.Type[0]) + comp.Type.Substring(1);
            try { @await Html.PartialAsync("~/Views/Home/Components/" + partialName + ".cshtml", comp) }
            catch { <div class="component-error">未知组件类型：@comp.Type</div> }
        }
    }
    else
    {
        <div class="empty-page">页面尚未发布或未配置组件</div>
    }
</div>
```
**注意**：组件类型首字母大写（如 `banner` → `_Banner`），partial 文件名匹配。

### 步骤 E4-E12：9 个组件 partial
**目录**：`d:\MyProject\my-site\src\CIMC.WebSite\Views\Home\Components\`

每个 partial 接收 `@model MySite.Web.Models.ComponentModel`，通过 `ViewBag`/`ViewData` 访问预加载数据。

**E4. `_Navigation.cshtml`**：
- 从 `ViewBag.NavigationList` 读取 `List<NavigationModel>`
- 渲染：`<header class="site-header"><div class="container">` Logo + 导航列表 + 移动端汉堡按钮
- 支持 props：`bgColor` / `textColor` / `activeColor` / `fixed`（应用 inline style）

**E5. `_Banner.cshtml`**：
- 从 `Model.Props["items"]` 读取 banner 项数组（每项含 image/title/subtitle/buttonText/buttonLink）
- 渲染：`<section class="banner">` 单图或多图轮播（用纯 JS 实现，依赖 site.js 初始化）
- 支持属性：`height` / `autoplay` / `interval` / `textColor` / `overlayOpacity`

**E6. `_NewsList.cshtml`**：
- 从 `ViewBag.NewsList` 读取 `List<Article>`
- 支持属性：`count`（显示数量，默认 6）/ `showImage`（是否显示封面）/ `showDate` / `moreLink`
- 渲染：`<section class="news-section">` 卡片网格 + "查看更多"按钮

**E7. `_ProductList.cshtml`**：
- 从 `ViewBag.ProductList` 读取 `List<ContentProduct>`
- 支持属性：`count` / `columns`（每行数量，默认 4）/ `showImage` / `showSummary`
- 渲染：`<section class="product-section">` 卡片网格 + 详情链接 `/products/detail-{id}.html`

**E8. `_JobList.cshtml`**：
- 从 `ViewBag.JobList` 读取 `List<ContentJob>`
- 支持属性：`count` / `showSalary` / `showLocation` / `showCount`
- 渲染：`<section class="job-section">` 表格样式列表

**E9. `_Footer.cshtml`**：
- 从 `ViewBag.Footer` 读取 `FooterModel`
- 渲染：`<footer class="site-footer">` 4 列布局（公司信息 / 联系方式 / 快捷导航 / 二维码）+ 备案信息
- 应用 `BgColor` / `TextColor` inline style

**E10. `_RichText.cshtml`**：
- 从 `Model.Props["html"]` 读取富文本内容
- 渲染：`<section class="rich-text-section">` + `@Html.Raw(html)`
- 注意：`@Html.Raw` 输出富文本需注意 XSS，但本系统是后台管理员编辑的内容，来源可信

**E11. `_Image.cshtml`**：
- 从 `Model.Props["src"]` 读取图片 URL，`Model.Props["alt"]` / `width` / `height`
- 渲染：`<section class="image-section"><img src="..." alt="..." /></section>`

**E12. `_Title.cshtml`**：
- 从 `Model.Props["text"]` 读取标题，`level`（1-6，默认 2）/ `align`
- 渲染：`<h{n} class="title-component" style="text-align:...">text</h{n}>`

### 步骤 E13：Home/Article.cshtml（新闻详情）
**文件**：`d:\MyProject\my-site\src\CIMC.WebSite\Views\Home\Article.cshtml`
**模型**：`@model CIMC.Data.Article`
**结构**：
- `Layout = "_PublicLayout"`
- 顶部：导航（用 `ViewBag.NavigationList` 渲染，复用 `_Navigation` partial 或内联）
- 主体：文章标题 + 作者 + 发布时间 + 浏览量 + 内容（`@Html.Raw(Model.Detail)`）
- 侧边：返回列表按钮
- 底部：footer partial
**简化**：导航和 footer 直接 inline 渲染（避免与 Index 的循环 partial 重复），主体是文章内容

### 步骤 E14：Home/ProductDetail.cshtml（产品详情）
**文件**：`d:\MyProject\my-site\src\CIMC.WebSite\Views\Home\ProductDetail.cshtml`
**模型**：`@model CIMC.Data.ContentProduct`
**结构**：
- `Layout = "_PublicLayout"`
- 顶部：导航
- 主体：产品名称 + 封面图（含多图列表轮播）+ 摘要 + 详情（`@Html.Raw(Model.Description)`）+ 参数（`@Html.Raw(Model.Specification)`）+ 特点（`@Html.Raw(Model.Feature)`）
- 底部：footer
**简化**：与 Article 类似，inline 渲染导航和 footer

### 步骤 E15：site.css（前台样式）
**文件**：`d:\MyProject\my-site\wwwroot\site\css\site.css`
**覆盖**：
- Reset（box-sizing、margin/padding 0）
- 全局：`body { font-family: -apple-system, "Segoe UI", "Microsoft YaHei", sans-serif; color: #333; line-height: 1.6; }`
- `.container { max-width: 1200px; margin: 0 auto; padding: 0 20px; }`
- `.page-wrapper { min-height: 100vh; display: flex; flex-direction: column; }`
- `.site-header`：固定顶部 + Logo 左 + 导航右 + 移动端响应式汉堡
- `.banner`：相对定位 + 轮播项绝对定位 + 文字居中
- `.news-section` / `.product-section` / `.job-section`：卡片网格（grid-template-columns: repeat(auto-fill, minmax(280px, 1fr))）
- `.site-footer`：4 列 grid + 备案信息
- `.rich-text-section img` / `.image-section img`：max-width: 100%
- 响应式断点：768px（移动端单列、汉堡菜单展开）

### 步骤 E16：site.js（前台交互）
**文件**：`d:\MyProject\my-site\wwwroot\site\js\site.js`
**覆盖**：
- 移动端汉堡菜单 toggle
- Banner 轮播：找所有 `[data-banner]` 元素，按 `data-autoplay` / `data-interval` 属性自动切换
- 图片懒加载（可选，简单实现：`IntersectionObserver`）
- 平滑滚动锚点

### 步骤 F1：编译验证
**命令**（PowerShell）：
```powershell
dotnet build "d:\MyProject\my-site\src\CIMC.WebSite\MySite.Web.csproj"
```
**预期**：Build succeeded，0 errors。若仍有错误，按错误信息逐一修复。

### 步骤 F2：数据库迁移验证
**说明**：Startup.cs 已 `dbContext.Database.Migrate()`，启动应用时自动应用 `20260704132231_init` 迁移。无需手动 `dotnet ef database update`。
**命令**（PowerShell，后台启动应用）：
```powershell
dotnet run --project "d:\MyProject\my-site\src\CIMC.WebSite\MySite.Web.csproj"
```
**验证项**：
1. 应用启动无异常（监听 5000/5001 端口）
2. 浏览器访问 `http://localhost:5000/` → 显示首页（若数据库中无已发布首页，显示"页面尚未发布"提示）
3. 访问 `http://localhost:5000/admin` → 跳转到登录页（验证后台正常）
4. 登录后台 → 在"页面管理"创建一个首页 → 添加 Navigation + Banner + Footer 组件 → 保存草稿 → 发布 → 回到前台 `http://localhost:5000/` 验证渲染

## 关键技术决策

### 决策 1：前台组件渲染用 partial dispatch
不在 _PublicLayout 中渲染 nav/footer，而是完全由 Index.cshtml 的组件循环驱动。这匹配设计器中"用户选择是否添加 Navigation 组件"的逻辑。若用户没加 Navigation 组件，前台就没有顶部导航。这是 Phase 1 的简化方案。

### 决策 2：BuildPage 共享预加载策略
所有 BuildPage 调用都预加载 NewsList/ProductList/JobList 到 ViewBag，组件 partial 直接从 ViewBag 取数。这避免每个 partial 各自查数据库（会重复查询）。对于 Products/News/Jobs 等栏目页，action 内再覆盖 ViewBag.ProductList 等以应用栏目过滤。

### 决策 3：富文本用 @Html.Raw 不做 XSS 过滤
理由：内容来源是后台管理员通过 wangEditor 编辑（admin 已经过 [Authorize] 鉴权），可信度高。Phase 1 不引入 HTML 净化库（HtmlSanitizer）以降低复杂度。Phase 2 可加。

### 决策 4：路径匹配用 PagePath 精确匹配
Home 控制器通过 `p.PagePath == "/about"` 等精确路径加载页面。这要求后台创建页面时必须使用约定的路径（/, /about, /products, /news, /jobs, /contact）。DataInitializer 已在 Part B 种子数据中创建这些默认页面。

### 决策 5：删除而非重命名冲突文件
`Models\PageCt\PageModel.cs` 改名也可解决冲突，但删除更彻底（确认是死代码：仅被 `Models\Pages\PageConfigModel.cs` 引用 PageControlList，而 PageConfigModel 本身也无任何 Controller 引用）。删除一个文件比改 namespace 影响更小。

### 决策 6：Article/ProductDetail 详情页不用组件驱动
新闻详情和产品详情是固定结构（标题+正文），用独立 cshtml 而非走组件循环。导航和 footer 直接 inline 渲染（不复用 partial），避免与 Home/Index 的 partial dispatch 模式耦合。

## 假设与前提

1. MySQL 服务在 `127.0.0.1:3306` 运行，数据库 `my_site` 已创建，root 密码 `123qwe`（来自 appsettings.json）
2. DataInitializer 已在 Part B 创建默认页面（/, /about, /products, /news, /jobs, /contact）和默认导航菜单
3. 用户已配置 admin 账户能登录后台（DataInitializer 默认 superadmin/123qwe 已就位）
4. 现有 `Models\PageCt\PageModel.cs` 确实是死代码（已通过 Grep 验证仅被 `PageConfigModel.cs` 引用，而 PageConfigModel 本身无业务引用）
5. 现有迁移 `20260704132231_init.cs` 已包含所有 8 个新实体对应的表

## 验证清单

- [ ] `Models\PageCt\PageModel.cs` 已删除
- [ ] `dotnet build` 成功，0 errors
- [ ] `HomeController.cs` 已创建，包含 9 个 action
- [ ] `_PublicLayout.cshtml` 已创建，引用 `~/site/css/site.css` 和 `~/site/js/site.js`
- [ ] `Home\Index.cshtml` 已创建，遍历 `Model.Components` 渲染 partial
- [ ] 9 个组件 partial 全部创建于 `Views\Home\Components\`
- [ ] `Home\Article.cshtml` 与 `Home\ProductDetail.cshtml` 已创建
- [ ] `wwwroot\site\css\site.css` 与 `wwwroot\site\js\site.js` 已创建
- [ ] 应用启动无异常，访问 `http://localhost:5000/` 不抛 500 错误
- [ ] 后台创建并发布首页后，前台能正确渲染组件

## 执行顺序

1. 删除 `Models\PageCt\PageModel.cs`（步骤 0）
2. 创建 `HomeController.cs`（步骤 E1）
3. 创建 `_PublicLayout.cshtml`（步骤 E2）
4. 创建 `Home\Index.cshtml`（步骤 E3）
5. 创建 9 个组件 partial（步骤 E4-E12，可并行）
6. 创建 `Home\Article.cshtml`（步骤 E13）
7. 创建 `Home\ProductDetail.cshtml`（步骤 E14）
8. 创建 `wwwroot\site\css\site.css`（步骤 E15）
9. 创建 `wwwroot\site\js\site.js`（步骤 E16）
10. `dotnet build` 验证（步骤 F1）
11. `dotnet run` 启动应用并浏览器验证（步骤 F2）
