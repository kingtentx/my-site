# MySite 通用可视化建站系统

这是一个基于 ASP.NET Core MVC 的通用 PC 门户网站框架，支持在管理后台通过可视化方式拖拽配置页面模块。

## 技术栈

- .NET 8
- ASP.NET Core MVC / Razor
- 原生 JavaScript 可视化设计器
- JSON 文件持久化，不依赖数据库，后续可替换为 EF Core / MySQL / SQL Server

## 运行

```bash
dotnet run --project src/MySite.Web/MySite.Web.csproj
```

访问：

- 前台：首页 `/`
- 后台：`/Admin`
- 登录：`/Account/Login`

默认后台账号在 `src/MySite.Web/appsettings.json` 中配置，生产环境请立即修改。

## 目录说明

```text
src/MySite.Web
├── Controllers       # 前台、后台、设计器 API
├── Models            # 通用页面、区块、模板模型
├── Services          # 页面配置持久化服务
├── Views             # Razor 页面
└── wwwroot           # 静态资源
```

## 已删除的旧门户冗余内容

本分支已移除原 PC 门户中与特定企业业务绑定的实体、控制器、视图和资源依赖，仅保留通用建站需要的页面、区块、组件、后台设计器和前台渲染能力。
