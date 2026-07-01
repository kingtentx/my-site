# MySite 通用可视化建站系统

这是一个基于 ASP.NET Core MVC 的通用 PC 门户网站框架，支持在管理后台通过可视化方式拖拽配置页面模块。

## 技术栈

- .NET 8
- ASP.NET Core MVC / Razor
- EF Core + SQLite 默认持久化
- 原生 JavaScript 可视化设计器
- 保留 `CIMC.Core`、`CIMC.Data`、`CIMC.EntityFramework`、`CIMC.Helper` 分层

## 分层说明

```text
src
├── CIMC.Core             # 通用常量、枚举、返回模型
├── CIMC.Data             # 通用实体：用户、角色、菜单、权限、审计、页面、区块
├── CIMC.EntityFramework  # DbContext、Repository、初始化种子数据
├── CIMC.Helper           # 密码、JSON 等工具
└── MySite.Web            # MVC 后台、前台渲染、页面设计器、权限过滤、审计中间件
```

## 保留的后台基础功能

- 菜单管理
- 角色管理
- 角色菜单权限：查看、新增、编辑、删除
- 审计日志
- 后台登录认证
- 可视化页面设计器

## 已删除的旧门户业务内容

已移除与特定企业门户强绑定的文章、产品、招聘、相册、留言、附件、旧前台页面和旧静态资源。保留的是通用建站底座和后台基础权限体系。

## 运行

```bash
dotnet run --project src/MySite.Web/MySite.Web.csproj
```

访问：

- 前台：首页 `/`
- 后台：`/Admin`
- 登录：`/Account/Login`

默认后台账号由 `src/MySite.Web/appsettings.json` 的 `Admin:Password` 初始化，生产环境请立即修改。默认数据库文件路径为 `src/MySite.Web/App_Data/mysite.db`。
