# 删除 10 个实体及其关联代码

## Context（背景）

用户要求删除 `CIMC.Data/Model` 下 10 个实体及其所有关联代码：

`Tag`、`SiteModule`、`SiteInfo`、`Navigation`、`MessageBoard`、`Job`、`Attachments`、`FooterInfo`、`Album`、`VideoMedia`

这些实体被站点代码深度引用：7 个后台控制器、`HomeController`（整个公开站点）、`DataInitializer`、`AuditLogFilter`、多个 ViewModel 与视图。删除它们需同步清理所有引用，否则无法编译。

附带问题：最初"项目编译异常"的根因是 `MySite.sln` 引用了已失效的 `MySite.Web` 项目（仅剩 obj 残留和报错的 Razor 视图，实际项目为 `CIMC.WebSite`）。方案一并处理。

保留的实体：`Admin`、`Role`、`Menu`、`RoleMenu`、`Article`、`Images`、`AuditLog`。

## 判断依据（用户跳过了确认问题，按"删除实体及关联代码"的字面意图取最一致的处理）

- **HomeController + Views/Home**：每个 Action 都依赖待删实体 → 整体删除
- **DataInitializer**：保留仅依赖 `Admin`/`Menu`/`Article` 的部分，删除依赖待删实体的方法
- **EF Migrations**：不动（基于字符串引用，不影响编译；保留 DB 演进历史）

## 一、删除文件

### 1. 实体（10）
- `src/CIMC.Data/Model/Tag.cs`
- `src/CIMC.Data/Model/SiteModule.cs`
- `src/CIMC.Data/Model/SiteInfo.cs`
- `src/CIMC.Data/Model/Navigation.cs`
- `src/CIMC.Data/Model/MessageBoard.cs`
- `src/CIMC.Data/Model/Job.cs`
- `src/CIMC.Data/Model/Attachments.cs`
- `src/CIMC.Data/Model/FooterInfo.cs`
- `src/CIMC.Data/Model/Album.cs`
- `src/CIMC.Data/Model/VideoMedia.cs`

### 2. 后台控制器（7，专门服务于待删实体）
- `src/CIMC.WebSite/Controllers/Admin/TagController.cs`
- `src/CIMC.WebSite/Controllers/Admin/SiteModuleController.cs`
- `src/CIMC.WebSite/Controllers/Admin/NavigationController.cs`
- `src/CIMC.WebSite/Controllers/Admin/MessageController.cs`
- `src/CIMC.WebSite/Controllers/Admin/JobController.cs`
- `src/CIMC.WebSite/Controllers/Admin/AlbumController.cs`
- `src/CIMC.WebSite/Controllers/Admin/AttachmentsController.cs`

### 3. 公开站点控制器 + 视图（依赖待删实体，无法独立运作）
- `src/CIMC.WebSite/Controllers/HomeController.cs`
- `src/CIMC.WebSite/Views/Home/`（Index、Products、ProductDetail、News、Article、Jobs、About、Contact 共 8 个 cshtml）

### 4. 后台视图（对应被删控制器 + AdminController 被删 Action）
- `Views/Tag/`（Index、Edit）
- `Views/SiteModule/`（Index、Edit）
- `Views/Navigation/`（Index、Edit）
- `Views/Message/`（Index）
- `Views/Job/`（Index、Edit）
- `Views/Album/`（Index、Edit、BatchUpload）
- `Views/Attachments/`（Index）
- `Views/Admin/SiteInfo.cshtml`、`Views/Admin/FooterInfo.cshtml`

### 5. ViewModel/Model 类（仅被已删代码引用）
- `Models/PageCt/TagModel.cs`
- `Models/Album/AlbumModel.cs`
- `Models/JobModel.cs`
- `Models/MessageBoardModel.cs`
- `Models/VideoMediaModel.cs`
- `Models/Pages/SiteInfoModel.cs`
- `Models/Pages/FooterModel.cs`
- `Models/Navigation/NavigationModel.cs`
- `Models/UploadModel/AttachmentsModel.cs`
- `Models/Web/AlbumOutput.cs`
- `Models/Web/JobOutput.cs`
- `Models/Web/PublicSiteViewModels.cs`（仅 HomeController + Views/Home 用）
- `Models/Web/SiteViewModel.cs`（仅 HomeController + Views/Home 用）

> 执行前对每个 Model 类做一次 Grep 确认无残留引用后再删。

## 二、修改文件

### 1. `src/CIMC.EntityFramework/EntityFramework/AppDbContext.cs`
删除 10 个 DbSet：`Attachments`、`Job`、`MessageBoard`、`Album`、`SiteModule`、`VideoMedia`、`SiteInfo`、`FooterInfo`、`Navigation`、`Tag`。保留 `Admin`、`Role`、`Menu`、`RoleMenu`、`Images`、`Article`、`AuditLog`。

### 2. `src/CIMC.EntityFramework/DataInitializer.cs`
- `Create()` 仅保留 `InitUser`、`InitMenu`、`InitArticles`、`RepairSiteAssetPaths`
- 删除方法：`InitTags`、`InitSiteContent`、`InitJobs`、`InitProducts`、`InitCertificates`、`InitPartners`、`InitSiteModules`、`UpdateProductDetails`、`EnsureSiteModuleTable`、`EnsureAlbumColumns`、`RemoveDuplicateAlbums`、`Module`、`Product`、`GetTagId`、`GetNewsTagId`、`AddColumnIfMissing`（仅被 EnsureSiteModuleTable/EnsureAlbumColumns 用）
- `InitArticles`：移除 `GetNewsTagId` 调用，`TagId` 赋 0
- `InitSiteMenus`：移除为 `Site_Info`、`Site_Footer`、`Site_Message`、`Content_Job`、`Content_Album`、`Content_Module` 创建菜单的 `EnsureMenu` 调用
- 删除内部类 `JobSeedItem`（仅 InitJobs 用）；保留 `NewsSeedItem`

### 3. `src/CIMC.WebSite/Controllers/Admin/AdminController.cs`
- 删除字段 `_siteInfoRepository`、`_footerInfoRepository` 及对应构造参数
- 删除 `SiteInfo`（GET/POST）、`FooterInfo`（GET/POST）共 4 个 Action
- 保留 Login/ReLogin/Logout/Index/Main/Password/UpdatePassword/ImageSelector/Error 及工具方法

### 4. `src/CIMC.WebSite/Controllers/Admin/ArticleController.cs` + `ContentControllerBase.cs`
- `ContentControllerBase`：删除 `IRepository<Tag>` 字段/构造参数、`GetTags`、`GetTagName`；基类变为空壳 → 直接删除该文件，让 `ArticleController` 改为继承 `AdminBaseController`
- `ArticleController`：构造参数移除 `IRepository<Tag>`；`Index` 不再传 `GetTags(...)`（返回空 View 或移除标签 ViewData）；`Edit` 中 `TagsList` 赋值删除；`GetList` 中 `TagName = GetTagName(p.TagId)` 改为空字符串

### 5. `src/CIMC.WebSite/Models/Article/ArticleModel.cs`
删除 `TagsList`（List<TagModel>）属性和 `TagName` 计算属性（二者依赖已删的 `TagModel`）。保留 `TagId`、`TagType`（plain int，Article 实体本身就有）。

### 6. `src/CIMC.WebSite/Filters/AuditLogFilter.cs`
在 `GetEntityType`、`GetOperationTable`、`BuildOperationDesc` 三个 switch 中删除 case：`Album`、`Job`、`Tag`、`Navigation`、`SiteModule`、`Attachments`、`Message`。

### 7. `src/CIMC.WebSite/MapperConfig/AutomapperConfig.cs`
删除映射：`Album`、`Attachments`、`Job`、`MessageBoard`、`VideoMedia`、`SiteInfo`、`FooterInfo`。保留 `Admin`、`Menu`、`Article`。

### 8. `src/CIMC.WebSite/Common/CacheKey.cs`
删除常量：`Navigation`、`SiteInfo`、`FooterInfo`。

### 9. `src/CIMC.WebSite/Startup.cs`（第 387 行附近）
保留 `new DataInitializer().Create(dbContext)` 调用（Create 签名不变）。检查路由/区域注册是否引用被删控制器（MVC 约定路由无需改）。

## 三、处理失效的 MySite.Web 项目（解决最初编译异常）

- `src/MySite.Web/` 仅剩 `obj/` 残留和报错的 Views，已无源码
- 从 `MySite.sln` 移除 `MySite.Web` 项目节点（Project + 对应配置节）
- 删除 `src/MySite.Web/` 残留目录

## 四、不改动

- `src/CIMC.EntityFramework/Migrations/`：历史迁移与 ModelSnapshot 基于字符串引用实体名，不影响编译；保留 DB 演进历史
- `MenuCode` 枚举/常量定义：清理引用后保留定义本身（无害）
- `CIMC.Core/Enums/TagsType.cs`、`ControlType.cs` 等：`TagType` 枚举仍被 `Article.TagType` 使用，保留

## 五、验证

1. `dotnet build MySite.sln` —— 0 错误 0 警告
2. 启动站点（`dotnet run --project src/CIMC.WebSite`）：
   - 后台登录正常（Admin/Menu 种子数据生效）
   - 文章管理（Article）列表/编辑可用（标签分类 UI 已移除，TagId 字段保留）
   - 已删控制器路由（/album/index、/job/index 等）返回 404
3. 检查无残留引用：`Grep "\b(Album|Tag|Job|SiteModule|Navigation|SiteInfo|FooterInfo|MessageBoard|Attachments|VideoMedia)\b"` 在 `src/CIMC.WebSite` 与 `src/CIMC.EntityFramework` 中仅命中迁移文件/无关词
