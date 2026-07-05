# Part F - 运行时问题修复计划

## 概述

接续 Part E（前台渲染）已完成的工作，本计划聚焦于 Part F 验证阶段发现的 3 个运行时问题。所有问题根因已通过日志分析和文件检查确认，不涉及架构改动。

## 当前状态分析

通过应用日志（`d:\MyProject\my-site\src\CIMC.WebSite\Logs\20260705.log`）和文件系统检查确认：

### 问题 1：About/Products/News/Jobs/Contact 返回 500

- **现象**：访问 `/products` 返回 500，错误日志：`The view 'Products' was not found. Searched locations: /Views/Home/Products.cshtml...`
- **根因**：`HomeController.cs` 的 `About()`、`Products()`、`News()`、`Jobs()`、`Contact()` 5 个 action 调用 `return View(model)`，ASP.NET Core 默认按 action 名查找视图文件，但 `Views/Home/` 下只有 `Index.cshtml`、`Article.cshtml`、`ProductDetail.cshtml`、`NotFound.cshtml`，没有 `About.cshtml` 等。
- **设计意图**：所有栏目页共用 `Index.cshtml` 渲染（partial 由 `Model.Components` 驱动），不需要为每个栏目建独立视图。
- **验证依据**：`Index.cshtml` 第 1-3 行明确为通用渲染入口，遍历 `Model.Components` 调用对应 partial；`_PublicLayout.cshtml` 是独立布局，不依赖 action 名。

### 问题 2：site.css / site.js 404

- **现象**：访问 `http://localhost:5077/site/css/site.css` 返回 404，日志：`The request path /site/css/site.css does not match an existing file`。
- **根因**：文件被创建在错误位置 `d:\MyProject\my-site\wwwroot\site\`（项目根目录），而 ASP.NET Core 的静态文件根目录是 `d:\MyProject\my-site\src\CIMC.WebSite\wwwroot\`（csproj 所在目录）。
- **验证依据**：
  - 文件存在确认：`D:\MyProject\my-site\wwwroot\site\css\site.css` 和 `D:\MyProject\my-site\wwwroot\site\js\site.js`（错误位置）
  - csproj 位置：`d:\MyProject\my-site\src\CIMC.WebSite\MySite.Web.csproj`，`Microsoft.NET.Sdk.Web` SDK 默认把 csproj 同级的 `wwwroot` 作为 WebRootPath
  - 错误位置的 `wwwroot` 不在任何项目内，不会被静态文件中间件识别

### 问题 3：首页（/）"超时"

- **现象**：首次访问 `http://localhost:5077/` 等待 15 秒未响应。
- **根因**：**不是真正的超时**。日志显示请求实际成功完成（HTTP 200），耗时 19212ms，其中 `Executed ViewResult - view Index executed in 18956.5122ms`。耗时来源是首次请求时 Razor 视图的 JIT 编译：
  - `_Navigation.cshtml` 编译 223ms
  - `_Banner.cshtml` 编译 151ms
  - `_RichText.cshtml` 编译 80ms
  - `_Footer.cshtml` 编译 150ms
  - `_PublicLayout.cshtml` 编译 113ms
  - 加上 Index.cshtml、_ProductList.cshtml、_NewsList.cshtml、_JobList.cshtml、_Image.cshtml、_Title.cshtml 等的编译和执行
- **结论**：开发环境下首次访问慢是 ASP.NET Core Razor 运行时编译的正常行为，编译结果会被缓存，后续请求会快。**不需要代码修复**，只需在验证时接受首次慢响应，或预热一次后再测。
- **生产优化（不在本计划范围）**：可通过 `RazorCompileOnPublish`、`MvcRazorRuntimeCompilation` 配置或预编译视图提升首屏速度。

## 待修复文件清单

### 1. 修改 `d:\MyProject\my-site\src\CIMC.WebSite\Controllers\HomeController.cs`

将以下 5 个 action 末尾的 `return View(model);` 改为 `return View("Index", model);`：

| 行号 | Action | 当前代码 | 修改后 |
|------|--------|----------|--------|
| 64 | About | `return View(model);` | `return View("Index", model);` |
| 97 | Products | `return View(model);` | `return View("Index", model);` |
| 145 | News | `return View(model);` | `return View("Index", model);` |
| 184 | Jobs | `return View(model);` | `return View("Index", model);` |
| 194 | Contact | `return View(model);` | `return View("Index", model);` |

**注意**：
- `Index()` 自身（行 54）保持 `return View(model);` 不变（它本来就要查 `Index.cshtml`）
- `ProductDetail()` 返回 `View(product)` 查 `ProductDetail.cshtml`，保持不变
- `Article()` 返回 `View(article)` 查 `Article.cshtml`，保持不变
- `ArticlePreview()` 返回 `View("Article", article)`，已显式指定，保持不变

### 2. 在 `d:\MyProject\my-site\src\CIMC.WebSite\wwwroot\site\css\site.css` 创建文件

将 `d:\MyProject\my-site\wwwroot\site\css\site.css` 的全部内容（490 行 CSS）复制到正确位置。

### 3. 在 `d:\MyProject\my-site\src\CIMC.WebSite\wwwroot\site\js\site.js` 创建文件

将 `d:\MyProject\my-site\wwwroot\site\js\site.js` 的全部内容（123 行 JS）复制到正确位置。

### 4. 删除错误位置的 `d:\MyProject\my-site\wwwroot\` 整个目录

删除 `d:\MyProject\my-site\wwwroot\` 目录及其下的 `site\css\site.css`、`site\js\site.js`。这个 wwwroot 不属于任何项目，会误导后续维护。

## 验证步骤

### 步骤 1：停止当前运行的应用

当前有一个 `MySite.Web` 进程（PID 49240）在运行，需要先停止：
```powershell
Stop-Process -Id 49240 -Force
```
同时停止所有相关的 `dotnet` 进程（保留 `dotnet build`/`dotnet run` 用到的，可在执行时判断）。

### 步骤 2：重新构建项目

```powershell
dotnet build "d:\MyProject\my-site\src\CIMC.WebSite\MySite.Web.csproj"
```
预期：Build succeeded，0 errors。

### 步骤 3：启动应用

```powershell
dotnet run --project "d:\MyProject\my-site\src\CIMC.WebSite\MySite.Web.csproj" --no-build
```
监听 `http://localhost:5077`。等启动日志输出 `Now listening on: http://localhost:5077` 后继续。

### 步骤 4：HTTP 路由验证

依次请求以下 URL（每个最多等待 30 秒），验证响应码和关键内容：

| URL | 预期状态码 | 验证点 |
|-----|-----------|--------|
| `http://localhost:5077/site/css/site.css` | 200 | 响应 Content-Type: `text/css`，body 包含 `.page-wrapper` |
| `http://localhost:5077/site/js/site.js` | 200 | 响应 Content-Type: `application/javascript`，body 包含 `initBanners` |
| `http://localhost:5077/` | 200 | body 包含 `<header class="site-header`、`<section class="banner`、`<footer class="site-footer`（首次可能慢 15-20s，是 Razor JIT 编译） |
| `http://localhost:5077/about` | 200 | body 包含 `class="rich-text-content"`（about 页第一个内容组件是 richText） |
| `http://localhost:5077/products` | 200 | body 包含 `class="product-grid"` 或 `product-section` |
| `http://localhost:5077/news` | 200 | body 包含 `class="news-grid"` 或 `news-section` |
| `http://localhost:5077/jobs` | 200 | body 包含 `class="job-table"` 或 `job-section` |
| `http://localhost:5077/contact` | 200 | body 包含 `site-footer` |
| `http://localhost:5077/admin` | 200 或 302 | 后台登录页或重定向到登录页 |
| `http://localhost:5077/news/info-1.html` | 200 或 404 | 若 Article 表有数据则 200，body 包含 `article-detail`；否则 404 |
| `http://localhost:5077/products/detail-1.html` | 200 或 404 | 同上，body 包含 `product-detail` |

### 步骤 5：二次访问首页验证缓存

再次请求 `http://localhost:5077/`，预期响应时间 < 500ms（Razor 已编译缓存）。

### 步骤 6：后台登录验证

- 访问 `http://localhost:5077/admin`
- 使用凭据 `admin` / `123qwe` 登录
- 验证登录后能进入 `/admin/index` 主页
- 验证左侧菜单出现"网站管理"（含"页面管理"、"导航管理"、"页脚设置"等子菜单）和"内容管理"（含"新闻管理"、"产品管理"等）
- 验证点击"页面管理"后能看到 6 个种子页面（首页、关于我们、产品中心、新闻中心、招聘中心、联系我们）

## 假设与决策

1. **不修改 Index.cshtml 的渲染逻辑**：当前 `try/catch` 包裹 `Html.PartialAsync` 的方式正确，能优雅处理个别组件渲染失败。
2. **不修改 BuildPage 的多查询逻辑**：每次 BuildPage 都加载 NewsList/ProductList/JobList（即使该页不显示这些组件）是冗余但无害的优化点，不在本计划范围。可在后续优化中按组件类型按需加载。
3. **不引入 Razor 预编译**：开发环境保持运行时编译即可，避免增加构建复杂度。
4. **错误位置的 wwwroot 目录直接删除**：已确认不在任何 csproj 引用范围内，删除是安全的。
5. **应用启动使用 `--no-build`**：避免 `dotnet run` 重新触发构建，确保使用步骤 2 构建的结果。但需先完成步骤 2 的 build。

## 风险与回滚

- **风险**：删除 `d:\MyProject\my-site\wwwroot\` 时若误删项目内的 wwwroot，会导致其他静态资源（如 layui、resource）丢失。
- **缓解**：删除前用 `Test-Path` 确认路径是 `d:\MyProject\my-site\wwwroot\`（项目根目录下），不是 `d:\MyProject\my-site\src\CIMC.WebSite\wwwroot\`。
- **回滚**：所有修改都是文件级，可通过 git checkout 恢复 HomeController.cs；wwwroot 下文件被删除可重新创建。
