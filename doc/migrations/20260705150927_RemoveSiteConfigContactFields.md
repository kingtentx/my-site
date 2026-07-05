# 迁移文档：RemoveSiteConfigContactFields

## 基本信息

| 属性 | 值 |
|---|---|
| 迁移 ID | `20260705150927_RemoveSiteConfigContactFields` |
| 创建时间 | 2026-07-05 15:09:27 |
| 操作类型 | Schema 变更（破坏性） |
| 影响表 | `WebsiteSiteConfig` |
| 影响字段数 | 6 |
| 数据风险 | ⚠️ **高** —— 字段及字段内数据将被永久删除 |

## 变更背景

### 业务原因

"网站管理 → 站点设置"页面中的"联系与备案"功能被移除。该区块包含 6 个字段：

| 字段名 | 类型 | 长度 | 用途 |
|---|---|---|---|
| `Phone` | varchar | 50 | 联系电话 |
| `Email` | varchar | 100 | 联系邮箱 |
| `Address` | varchar | 250 | 公司地址 |
| `IcpNo` | varchar | 100 | ICP 备案号 |
| `PoliceNo` | varchar | 100 | 公安备案号 |
| `Copyright` | varchar | 250 | 版权信息 |

### 功能迁移说明

经过代码考古确认：**前台页面显示的"联系与备案"信息（电话/邮箱/地址/ICP/版权等）实际数据源是 `WebsiteFooter` 表**（由"网站管理 → 页脚设置"维护），而不是 `WebsiteSiteConfig` 表。

`WebsiteSiteConfig` 中的这 6 个字段在前台视图中**无任何引用**（grep `SiteConfig.Phone` 等均无匹配），属于历史遗留的冗余字段。删除后不影响前台显示，且消除了两个表字段重复造成的维护混乱。

## Schema 变更明细

### Up（正向执行）

删除 `WebsiteSiteConfig` 表的 6 个字段：

```sql
ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `Address`;
ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `Copyright`;
ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `Email`;
ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `IcpNo`;
ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `Phone`;
ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `PoliceNo`;
```

### Down（回滚）

恢复 6 个字段（数据无法恢复，仅恢复结构）：

```sql
ALTER TABLE `WebsiteSiteConfig` ADD COLUMN `Address` varchar(250) NULL;
ALTER TABLE `WebsiteSiteConfig` ADD COLUMN `Copyright` varchar(250) NULL;
ALTER TABLE `WebsiteSiteConfig` ADD COLUMN `Email` varchar(100) NULL;
ALTER TABLE `WebsiteSiteConfig` ADD COLUMN `IcpNo` varchar(100) NULL;
ALTER TABLE `WebsiteSiteConfig` ADD COLUMN `Phone` varchar(50) NULL;
ALTER TABLE `WebsiteSiteConfig` ADD COLUMN `PoliceNo` varchar(100) NULL;
```

## 代码变更

### 1. 实体类（[WebsiteSiteConfig.cs](file:///d:/MyProject/my-site/src/CIMC.Data/Model/WebsiteSiteConfig.cs)）

删除 6 个属性：`Phone`、`Email`、`Address`、`IcpNo`、`PoliceNo`、`Copyright`。

### 2. 视图模型（[SiteConfigModel.cs](file:///d:/MyProject/my-site/src/CIMC.WebSite/Models/Site/SiteConfigModel.cs)）

删除对应的 6 个属性。

### 3. 视图层（[Views/SiteConfig/Index.cshtml](file:///d:/MyProject/my-site/src/CIMC.WebSite/Views/SiteConfig/Index.cshtml)）

删除"联系与备案"表单区块（含 6 个表单项），原"主题/语言/启用状态"区块改名为"主题与状态"。

### 4. 控制器

- [SiteConfigController.cs](file:///d:/MyProject/my-site/src/CIMC.WebSite/Controllers/Admin/SiteConfigController.cs)：`Edit` 和 `ToModel` 删除 6 个字段的赋值与映射
- [HomeController.cs](file:///d:/MyProject/my-site/src/CIMC.WebSite/Controllers/HomeController.cs)：`ToSiteConfigModel` 删除 6 个字段的映射

### 5. 种子数据（[DataInitializer.cs](file:///d:/MyProject/my-site/src/CIMC.EntityFramework/DataInitializer.cs)）

`InitSiteConfig` 方法移除 6 个字段的初始值设置。

## 不受影响的模块

### ✅ WebsiteFooter 实体（页脚管理）

`WebsiteFooter` 表保留所有字段不变，包括：`Phone`、`Email`、`Address`、`IcpNo`、`PoliceNo`、`Copyright`、`Logo`、`CompanyName`、`Intro`、`Qrcode`、`FriendLinks`、`BgColor`、`TextColor`、`IsActive`。

前台页脚渲染（[_Footer.cshtml](file:///d:/MyProject/my-site/src/CIMC.WebSite/Views/Home/Components/_Footer.cshtml)）继续从 `FooterModel` 读取这些字段，显示效果不受影响。

### ✅ 其他实体

`ContentJob` 的 `ContactPhone`、`ContactEmail` 字段与本次变更无关，不受影响。

## 应用方式

### 自动应用（推荐）

应用启动时 `Startup.Configure` 会自动调用 `Database.Migrate()`，新迁移会自动应用。下次启动应用即完成 schema 变更。

### 手动应用

```powershell
dotnet ef database update RemoveSiteConfigContactFields `
    --project d:\MyProject\my-site\src\CIMC.EntityFramework `
    --startup-project d:\MyProject\my-site\src\CIMC.WebSite
```

### 回滚

```powershell
# 回滚到上一个迁移（仅恢复字段结构，数据不可恢复）
dotnet ef database update AddSiteHeaderStyle `
    --project d:\MyProject\my-site\src\CIMC.EntityFramework `
    --startup-project d:\MyProject\my-site\src\CIMC.WebSite

# 完全移除迁移文件
dotnet ef migrations remove `
    --project d:\MyProject\my-site\src\CIMC.EntityFramework `
    --startup-project d:\MyProject\my-site\src\CIMC.WebSite
```

## 数据备份建议

由于此迁移会永久删除 `WebsiteSiteConfig` 表的 6 个字段及其数据，**执行前请务必备份数据库**：

```bash
mysqldump -h 127.0.0.1 -P 3306 -u root -p123qwe my_site > backup_before_remove_site_config_fields.sql
```

或仅备份受影响的字段：

```sql
USE my_site;
SELECT Id, Phone, Email, Address, IcpNo, PoliceNo, Copyright
INTO OUTFILE 'site_config_contact_backup.csv'
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
FROM WebsiteSiteConfig;
```

## 验证清单

### Schema 验证

```sql
USE my_site;
-- 应该返回 0 行
SHOW COLUMNS FROM WebsiteSiteConfig WHERE Field IN ('Phone', 'Email', 'Address', 'IcpNo', 'PoliceNo', 'Copyright');
```

### 功能验证

1. 访问"网站管理 → 站点设置" → 应只看到"基础信息"、"顶部导航样式"、"主题与状态"三个区块
2. 保存站点设置 → 应正常保存，不报错
3. 访问前台首页 → 页脚显示的联系/备案信息应正常（来自 WebsiteFooter）

### 编译验证

```powershell
dotnet build d:\MyProject\my-site\src\CIMC.WebSite\MySite.Web.csproj
```
应 0 错误。
