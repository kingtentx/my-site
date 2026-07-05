# 测试数据种子补充方案

## Context

当前项目已实现 7 个业务模块的种子数据（Admin、Menu、Article、WebsiteSiteConfig、WebsiteFooter、WebsiteNavigation、WebsitePage），但有 6 个模块尚无任何数据：

- `ContentProductCategory`（产品分类）
- `ContentProduct`（产品）
- `ContentJob`（招聘岗位）
- `Images`（素材库）
- `Role`（角色）
- `RoleMenu`（角色权限映射）

此外，`Article`（新闻）模块的现有种子依赖 `wwwroot/syle/data/news-data.json` 文件，若该文件不存在则新闻列表完全为空，无法验证前台 `/news` 路由。

本次任务目标：在 [DataInitializer.cs](file:///d:/MyProject/my-site/src/CIMC.EntityFramework/DataInitializer.cs) 中新增 7 个幂等的种子方法，为上述缺失数据的模块补充合理、真实的中文测试数据，使后台各管理列表和前台各栏目页均可直接进行功能验证。

## 修改文件清单

仅修改 **1 个文件**：

- [DataInitializer.cs](file:///d:/MyProject/my-site/src/CIMC.EntityFramework/DataInitializer.cs) — 新增 7 个私有种子方法，并调整 `Create` 方法的调用序列

## 实施步骤

### 步骤 1：调整 `Create` 方法调用序列

在已有调用之后追加新方法调用，保持依赖顺序（Category 先于 Product，Role 先于 RoleMenu）：

```csharp
public void Create(AppDbContext context)
{
    // ===== 已有种子（保持不变）=====
    InitUser(context);
    InitMenu(context);
    InitArticles(context);
    InitArticlesFallback(context);  // 【新增】紧跟 InitArticles 之后
    RepairSiteAssetPaths(context);
    InitSiteConfig(context);
    InitFooter(context);
    InitNavigation(context);
    InitSitePages(context);

    // ===== 新增种子（按依赖顺序）=====
    InitRoles(context);
    InitRoleMenus(context);         // 依赖 Role.Id
    InitProductCategories(context);
    InitProducts(context);           // 依赖 Category.Id
    InitJobs(context);
    InitImages(context);
}
```

### 步骤 2：实现 `InitArticlesFallback` — 兜底新闻（6 条）

仅当 `context.Article.Any()` 为 `false` 时执行（即 `news-data.json` 未加载时）。
- 6 条企业官网场景新闻（获奖、产品发布、战略合作、行业活动、校招、资质认证）
- `TagType=1, TagId=0`（与现有 `InitArticles` 一致）
- 前 2 条 `IsHot=true` 用于验证热门新闻模块
- `ImageUrl` 用 `/syle/images/174376305.png` 兜底（与 `NormalizeSitePath` 默认值一致）
- `CreationTime` 用 `DateTime.Now.AddDays(-5/-10/-15/-20/-25/-30)` 形成时间梯度

### 步骤 3：实现 `InitRoles` — 3 个测试角色

幂等检查：`if (context.Role.Any(p => p.RoleName == "内容编辑" || p.RoleName == "内容审核" || p.RoleName == "运营管理员")) return;`

| RoleName | Description | RoleType |
|---|---|---|
| 内容编辑 | 负责新闻、产品、招聘内容的录入与维护 | 0 |
| 内容审核 | 负责内容审核与发布，仅查看权限 | 0 |
| 运营管理员 | 负责网站内容与站点配置的运营管理 | 0 |

### 步骤 4：实现 `InitRoleMenus` — 角色权限映射

依赖 `Role.Id` 与已存在 `Menu.PermissionKey`。

权限字符串命名规则（与 `MenuCode.cs` 一致）：
- 模块级：`Content`、`Site`
- 菜单级：`Content_Article`、`Content_Product`、`Content_ProductCategory`、`Content_Job`、`Content_Images`、`Site_Info`、`Website_Page`、`Site_Navigation`、`Site_Footer`
- 按钮级：菜单级 + `_Add` / `_Edit` / `_Delete` / `_Design` / `_Publish`

权限分配：
- **内容编辑**：内容管理全部模块的增删改查按钮权限（约 21 条）
- **内容审核**：仅菜单级查看权限，无按钮权限（约 6 条）
- **运营管理员**：内容管理全部 + 网站管理全部（约 35 条）

幂等：`if (context.RoleMenu.Any(p => roleIds.Contains(p.RoleId))) return;`

### 步骤 5：实现 `InitProductCategories` — 4 顶级 + 5 二级分类

幂等检查：`if (context.ContentProductCategory.Any(p => !p.IsDelete)) return;`

**分两次 SaveChanges**：先插入顶级分类获取自增 Id，再用这些 Id 作为二级分类的 Pid。

| 顶级（Pid=0） | 二级（Pid=父Id） |
|---|---|
| 智能硬件（Sort=1） | 智能终端、物联网设备 |
| 软件应用（Sort=2） | 企业管理软件、行业解决方案 |
| 云服务（Sort=3） | 云部署服务 |
| 数字化咨询（Sort=4） | （无二级） |

### 步骤 6：实现 `InitProducts` — 8 个产品

幂等检查：`if (context.ContentProduct.Any(p => !p.IsDelete)) return;`

8 个产品分布到各二级分类下：
1. 企业官网建站系统 → 企业管理软件（推荐，ViewCount=1280）
2. 电商小程序平台 → 企业管理软件（推荐，ViewCount=980）
3. ERP 企业资源管理系统 → 企业管理软件（ViewCount=654）
4. 智能客服平台 → 行业解决方案（推荐，ViewCount=1120）
5. 工业物联网网关 → 物联网设备（ViewCount=432）
6. 智能自助终端 → 智能终端（推荐，ViewCount=760）
7. 云原生部署服务 → 云部署服务（推荐，ViewCount=890）
8. 数字化战略咨询服务 → 数字化咨询（ViewCount=320）

字段填充要点：
- `ImageList` 必须为合法 JSON 数组字符串（`HomeController.ProductDetail` 会反序列化为 `List<string>`），空用 `"[]"` 而非 null
- `Summary` ≤ 1000 字符（实体 `Len_1000` 约束）
- `Description/Specification/Feature` 用简单 HTML：`<p>...</p>` / `<table>...</table>` / `<ul>...</ul>`
- `CreationTime` 用 `DateTime.Now.AddDays(-30/-28/-25/-20/-18/-15/-10/-5)` 形成时间梯度

### 步骤 7：实现 `InitJobs` — 6 个招聘岗位

幂等检查：`if (context.ContentJob.Any(p => !p.IsDelete)) return;`

6 个岗位覆盖不同部门和类型：
1. 高级 .NET 开发工程师 / 研发中心 / 上海 / 25k-40k / 全职
2. 前端工程师 / 研发中心 / 上海 / 18k-30k / 全职
3. 产品经理 / 产品中心 / 上海 / 20k-35k / 全职
4. UI 设计师 / 设计中心 / 上海 / 15k-25k / 全职
5. 销售经理 / 市场部 / 北京 / 12k-20k+提成 / 全职
6. 实习生（前端方向）/ 研发中心 / 上海 / 200元/天 / 实习

字段填充要点：
- `JobType` 取值 `全职/兼职/实习`（与 `JobController.Edit` 默认值一致）
- `IsActive=true` 且 `PublishTime` 非空（否则前台 `/jobs` 过滤 `IsActive` 时不会显示）
- `Responsibilities/Requirements` 用 `<p>1. xxx</p>` 编号列表格式

### 步骤 8：实现 `InitImages` — 8 个素材记录

幂等检查：`if (context.Images.Any()) return;`

8 个素材覆盖不同场景：
- 首页Banner、关于我们配图、产品封面图 ×2、新闻配图、招聘海报、二维码、公司Logo

字段填充要点：
- `Size` 用真实字节数（524288 = 512KB），`ImagesController.GetList` 会按 `Size/1024/1024` 转换显示
- `ExtensionName` 包含点号（如 `.jpg`、`.png`，与 `Path.GetExtension` 返回值一致）
- `Url` 用 `/uploads/seed/{name}.jpg` 前缀，便于在素材库识别为种子数据
- 物理文件可能不存在，但 `ImagesController.GetList` 仅查库不验证文件存在性，可安全存在

## 关键决策记录

| 决策 | 理由 |
|---|---|
| 不修改已有 7 个模块的种子方法 | 已有数据正确且完整，避免引入回归风险 |
| `InitArticlesFallback` 用 `Article.Any()` 触发 | 保护 `news-data.json` 已加载的数据，仅在表完全为空时兜底 |
| 产品分类分两次 SaveChanges | 二级分类的 `Pid` 依赖顶级分类已持久化的自增 Id |
| 不显式赋值 Id | 所有 `int Id` 是 `MySqlValueGenerationStrategy.IdentityColumn` 自增，硬编码会导致 Id 冲突 |
| RoleMenu.Permission 字符串严格匹配 MenuCode 命名 | 与 `PermissionService.GetRoleMenus` 解析逻辑兼容 |
| 种子图片 URL 用 `/uploads/seed/` 前缀 | 便于在素材库 UI 中识别为种子数据，与真实上传 `/uploads/yyyyMM/` 区分 |
| 角色权限分配区分增删改查 vs 仅查看 | 模拟真实业务场景，便于测试权限粒度 |

## 验证步骤

### 1. 编译验证
```powershell
dotnet build d:\MyProject\my-site\src\CIMC.WebSite\CIMC.WebSite.csproj
```
应无编译错误。

### 2. 启动验证
启动应用，`Startup.Configure` 第 387 行会自动调用 `new DataInitializer().Create(dbContext)`，观察控制台无异常。

### 3. SQL 验证
```sql
USE my_site;
SELECT COUNT(*) FROM Role;                           -- 应为 3
SELECT COUNT(*) FROM RoleMenu;                       -- 应为约 62（21+6+35）
SELECT COUNT(*) FROM ContentProductCategory WHERE NOT IsDelete;  -- 应为 9（4+5）
SELECT COUNT(*) FROM ContentProduct WHERE NOT IsDelete;          -- 应为 8
SELECT COUNT(*) FROM ContentJob WHERE NOT IsDelete;              -- 应为 6
SELECT COUNT(*) FROM Images;                          -- 应为 8
SELECT COUNT(*) FROM Article;                         -- 6（fallback 触发时）或更多（news-data.json 已加载）
```

### 4. 后台页面验证
登录 `admin/123qwe`：
- `/role/index` → 看到 3 个测试角色
- `/role/index` → 点"授权"按钮 → 看到对应角色已勾选的菜单与按钮
- `/productcategory/index` → 看到 4 顶级 + 5 二级分类
- `/product/index` → 看到 8 个产品，支持分类筛选、推荐标识
- `/job/index` → 看到 6 个岗位，覆盖全职/实习类型
- `/images/index` → 看到 8 个素材记录，大小按 KB/MB 正确显示

### 5. 前台页面验证（无需登录）
- `/products` → 产品列表，可按分类筛选
- `/products/智能硬件` → 路由按分类 Name 匹配，筛选出对应产品
- `/products/detail-1.html` → 产品详情，ImageList 多图正确反序列化
- `/jobs` → 看到 6 个招聘岗位
- `/news` → 若 `news-data.json` 不存在，看到 6 条兜底新闻

### 6. 幂等性验证
重启应用再次触发 `DataInitializer.Create`，数据库记录数应保持不变（重复执行上述 SQL 验证）。
