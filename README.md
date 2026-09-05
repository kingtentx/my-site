# MySite.Web

MySite.Web 是一个基于 **ASP.NET Core 8 / MVC / Razor / EF Core / MySQL** 的企业官网内容管理与可视化建站项目。

当前 `agent/site-builder-enhancements` 分支已对页面装修体系进行重构，核心目标是：

- 页面管理与网站导航合并为一棵页面树；
- Header / Footer 与普通页面统一使用 Site Builder；
- 页面装修采用递归组件树，不再使用旧版平铺组件 JSON；
- 设计、预览、正式发布尽量共用同一套运行时样式，实现接近所见即所得；
- 删除旧 WebsiteNavigation、WebsiteFooter、旧 Header 样式配置等重复数据模型；
- 启动时只初始化系统运行必需的基础数据，不再写入大量演示数据。

---

## 1. 技术栈

| 类型 | 技术 |
| --- | --- |
| Runtime | .NET 8 |
| Web | ASP.NET Core MVC + Razor |
| ORM | Entity Framework Core 8 |
| Database | MySQL 8 / Pomelo.EntityFrameworkCore.MySql |
| JSON | Newtonsoft.Json |
| Cache | Redis（可选） |
| Logging | Serilog |
| Admin UI | Layui |
| Builder | JavaScript + SortableJS |
| API Docs | Swagger / Swashbuckle |

Web 主项目：

```text
src/CIMC.WebSite/MySite.Web.csproj
```

解决方案主要项目：

```text
src/
├─ CIMC.Core
├─ CIMC.Data
├─ CIMC.EntityFramework
├─ CIMC.Helper
└─ CIMC.WebSite          # MySite.Web
```

---

## 2. 主要功能

### 2.1 系统管理

- 管理员管理
- 角色管理
- 菜单管理
- 权限控制
- 审计日志

### 2.2 网站管理

新版网站管理只保留：

```text
网站管理
├─ 站点设置
├─ 页面管理
└─ 全局区域设计
```

#### 站点设置

只负责网站基础信息：

- 站点名称
- Logo
- 浏览器标题
- SEO 关键词
- 网站描述
- 站点启用状态

Header / Footer 的颜色、布局、定位等样式不再放在站点设置中，而是统一在“全局区域设计”中完成。

#### 页面管理

页面管理同时承担网站导航管理功能。

核心字段关系：

```text
WebsitePage.ParentId
    ↓
决定页面父子层级，同时决定网站导航层级

WebsitePage.Sort
    ↓
决定同级页面 / 导航排序

WebsitePage.ShowInNavigation
    ↓
决定页面是否进入前台导航

WebsitePage.NavigationTitle
    ↓
为空时使用 PageName，否则使用单独的导航名称
```

因此不再单独维护 WebsiteNavigation 表。

页面管理支持：

- 新建页面
- 新建子页面
- 页面树结构
- 页面路径
- SEO 设置
- 导航显示控制
- 页面排序
- 页面装修
- 草稿保存
- 发布
- 发布版本

### 2.3 全局区域设计

全局区域目前包含：

```text
Global Header
Global Footer
```

Header / Footer 与普通页面共用同一套 Site Builder 组件系统。

前台渲染顺序：

```text
Global Header
    ↓
当前页面 BuilderDocument
    ↓
Global Footer
```

### 2.4 内容管理

```text
内容管理
├─ 新闻管理
├─ 产品分类
├─ 产品管理
├─ 招聘管理
└─ 素材管理
```

素材管理中的图片可以直接在页面装修器中选择使用。

---

## 3. Site Builder 架构

新版页面装修不再使用旧的：

```text
ComponentModel[]
```

而是采用递归组件树：

```text
BuilderDocument
└─ Nodes[]
   └─ BuilderNode
      ├─ Id
      ├─ Type
      ├─ Version
      ├─ Props
      ├─ Style
      ├─ Children[]
      ├─ Slots
      ├─ Bindings
      ├─ Actions
      ├─ Visible
      └─ Locked
```

布局组件可以继续嵌套子组件，从而构建复杂页面。

### 3.1 布局组件

```text
区段 Section
容器 Container
网格 Grid
列 Column
```

Grid 支持：

- 1 ～ 6 列；
- 快速调整列数；
- 自定义列宽比例；
- 鼠标拖动列分隔线调整宽度；
- 列内继续放置其他组件；
- 减少列数时尽量保留原列内容。

示例：

```text
50 / 50
30 / 70
25 / 50 / 25
20 / 30 / 50
```

### 3.2 基础组件

目前包括：

- 标题
- 文本
- 图片
- Banner
- 按钮
- 图标
- 视频
- 分隔线
- 间距

### 3.3 Banner

Banner 支持：

- 从素材库多选图片；
- 单图静态显示；
- 多图自动轮播；
- 轮播间隔；
- Banner 高度；
- `cover / contain`；
- 左右箭头；
- 圆点指示器；
- 图片顺序调整。

### 3.4 内容组件

- 文章列表
- 产品列表
- 招聘列表

用于直接绑定后台内容数据。

### 3.5 全局组件

- Logo
- 导航菜单
- 搜索
- 语言切换
- 联系方式
- 社交链接
- 版权信息

---

## 4. 页面装修器

装修入口：

```text
后台 → 网站管理 → 页面管理 → 装修
```

装修界面分为三部分：

```text
左侧：组件库 / 组合预设
中间：可视化画布
右侧：组件属性与样式
```

支持：

- 点击添加组件；
- 拖动组件调整顺序；
- 跨容器拖动；
- Grid 内列排序；
- Grid 列宽拖动；
- 组件复制；
- 组件显示 / 隐藏；
- 组件锁定；
- 撤销 / 重做；
- 图片素材库选择；
- Banner 多图选择；
- 保存草稿；
- 保存并预览；
- 发布。

### 4.1 所见即所得

设计页、预览页和正式发布页共用：

```text
wwwroot/site-builder/runtime.css
```

该文件是 Site Builder 的统一运行时样式。

目标是避免：

```text
设计页 CSS
≠
预览页 CSS
≠
正式站点 CSS
```

设计器还支持按目标宽度查看页面，例如：

```text
1200px
1440px
1920px
```

用于减少后台编辑区域宽度与真实浏览器宽度差异带来的布局偏差。

---

## 5. 数据结构说明

### WebsitePage

页面及导航树核心表。

主要职责：

- 页面基础信息；
- 页面父子层级；
- 前台导航结构；
- BuilderDocument；
- 发布状态；
- SEO；
- 页面排序。

### WebsitePageVersion

保存页面草稿 / 发布版本。

### WebsiteSiteConfig

只保存站点基础设置。

### Menu

后台管理系统菜单和权限定义。

> 注意：`Menu` 是后台管理菜单；WebsitePage 页面树是前台网站导航，两者不是同一个概念。

### 已废弃结构

新版架构不再使用：

```text
WebsiteNavigation
WebsiteFooter
WebsitePage.LayoutJson
旧版 ComponentModel[] 页面 JSON
SiteConfig HeaderBgColor
SiteConfig HeaderTextColor
SiteConfig HeaderActiveColor
SiteConfig HeaderFixedTop
```

---

## 6. 启动数据初始化

当前 `DataInitializer` 只负责初始化系统必需数据：

```text
1. 超级管理员
2. 后台系统菜单
3. 网站管理菜单
4. 内容管理菜单
5. 站点基础配置
```

不再自动初始化：

- 旧 Footer；
- 旧 WebsiteNavigation；
- 旧数组页面；
- 演示新闻；
- 演示产品；
- 演示招聘；
- 演示素材；
- 演示角色和权限。

应用启动后会执行 EF Core Migration，再执行基础数据初始化。

---

## 7. 本地开发

### 7.1 环境要求

建议：

```text
.NET SDK 8.x
MySQL 8.x
Visual Studio 2022+ / VS Code / Rider
Redis（可选）
```

### 7.2 数据库配置

修改：

```text
src/CIMC.WebSite/appsettings.json
```

配置：

```json
{
  "ConnectionStrings": {
    "Default": "server=127.0.0.1;port=3306;database=my_site;user=YOUR_USER;password=YOUR_PASSWORD"
  }
}
```

生产环境不要把真实密码、JWT Secret、Redis 密码直接提交到仓库，建议使用：

- 环境变量；
- User Secrets；
- 独立生产配置文件；
- 密钥管理服务。

### 7.3 还原和编译

在仓库根目录执行：

```powershell
dotnet restore
dotnet build
```

运行 Web 项目：

```powershell
dotnet run --project .\src\CIMC.WebSite\MySite.Web.csproj
```

首次启动时会自动执行数据库 Migration 和基础数据初始化。

---

## 8. 数据库重置

如果数据库中仍保留重构前的页面装修、旧 Footer、旧 Navigation、旧 Menu 历史数据，可使用：

```text
scripts/site_builder_reset_v3.sql
```

该脚本属于**破坏性重置脚本**，会清理旧页面装修数据并重新初始化新版菜单。

执行前必须先备份数据库。

正常升级已有数据库时，优先使用 EF Core Migration，不要每次都执行重置脚本。

---

## 9. 页面发布流程

```text
编辑页面
   ↓
保存草稿
   ↓
WebsitePageVersion Draft
   ↓
预览
   ↓
发布
   ↓
WebsitePageVersion Publish
   ↓
前台读取已发布 BuilderDocument
```

正式前台只展示：

- 已发布；
- 已启用；
- 未删除；
- 非全局保留页；

的页面。

页面树中开启 `ShowInNavigation` 的已发布页面，会自动生成前台导航。

---

## 10. 关键目录

```text
src/CIMC.WebSite/
├─ Controllers/
│  ├─ Admin/                  # 后台控制器
│  └─ HomeController.cs       # 前台页面路由与渲染
├─ Models/
│  └─ Site/                   # Site Builder / 页面模型
├─ Views/
│  ├─ Page/                   # 页面管理 / 装修 / 预览
│  ├─ GlobalRegion/           # Header / Footer 全局区域
│  ├─ Home/                   # 前台页面
│  └─ Shared/SiteBuilder/     # Builder 服务端渲染
└─ wwwroot/
   └─ site-builder/
      ├─ core/                # Registry / Tree / Store
      ├─ components/          # 组件定义
      ├─ inspector/           # 属性面板
      ├─ renderer/            # 设计器渲染
      ├─ page-designer.js     # 装修器主逻辑
      ├─ presets.js           # 组合预设
      └─ runtime.css          # 设计 / 预览 / 发布统一运行时样式
```

---

## 11. 开发约定

### 页面与导航

不要重新增加独立 WebsiteNavigation 数据同步逻辑。

统一规则：

```text
页面树 = 网站导航树
```

### Header / Footer

不要重新增加独立 FooterController / WebsiteFooter 配置。

统一规则：

```text
Header / Footer = 全局 BuilderDocument
```

### 页面组件

新增组件时，需要同步检查：

```text
1. Registry 组件定义
2. Inspector 配置
3. Designer Renderer
4. Server Renderer (_Node.cshtml)
5. runtime.css
6. BuilderDocument 服务端校验
```

设计器和正式前台的 DOM / CSS 应尽量保持一致，避免重新产生两套渲染规则。

---

## 12. 当前开发分支

Site Builder 重构分支：

```text
agent/site-builder-enhancements
```

该分支重点包括：

- 页面树与导航合并；
- 新版递归 Site Builder；
- Header / Footer 全局 Builder；
- Grid 动态列数和列宽；
- 素材库选择；
- Banner 多图轮播；
- 设计 / 预览 / 发布统一运行时样式；
- 清理旧 Footer / Navigation / ComponentModel 代码。

---

## 13. 后续建议

后续开发建议优先继续完善：

- 设计器与前台 DOM 结构进一步统一；
- 响应式断点配置；
- 手机 / 平板 / PC 独立预览；
- 更多布局预设；
- 富文本组件；
- 表单组件；
- 内容列表更多模板；
- Builder Schema 版本升级策略；
- 页面导入 / 导出；
- 全局主题变量（颜色、字体、间距 Token）。

---

> 当前项目已从“固定页面模板 + 独立导航 / Footer 配置”逐步重构为“页面树 + 统一 Site Builder + 全局 Header / Footer”的企业官网可视化建站架构。
