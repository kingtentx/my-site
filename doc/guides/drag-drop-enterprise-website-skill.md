# 拖拉拽企业建站 Skill

## 1. Skill 名称

拖拉拽企业网站可视化建站系统开发 Skill

## 2. 使用场景

当需要开发一个企业网站可视化建站系统时，使用本 Skill。

系统目标是让管理员无需编写代码，通过后台拖拉拽方式完成企业官网页面搭建、栏目配置、内容管理和页面发布。

适用网站类型包括：

* 企业官网
* 产品展示网站
* 新闻资讯网站
* 招聘官网
* 集团门户网站
* 项目宣传网站
* 品牌展示网站

## 3. 核心目标

开发一个支持拖拉拽装修页面的企业建站系统，后台管理员可以通过可视化方式配置页面结构、页面组件、导航菜单、Banner、新闻、产品、招聘、页脚等内容。

系统应支持：

* 页面可视化设计
* 页面组件拖拉拽
* 企业内容管理
* 首页装修
* 栏目页装修
* 详情页装修
* 页面预览
* 页面发布
* 多端适配
* SEO 基础配置
* 后台权限控制

## 4. 推荐技术定位

后台管理端建议沿用现有管理后台框架风格，例如 layuiAdmin、Layui、Vue Admin 或其他已有后台框架。

前台展示端建议采用独立前台项目，负责根据页面配置 JSON 渲染页面内容。

整体建议采用以下结构：

```text
WebSite
├── Admin               后台管理系统
├── WebFront            企业网站前台展示
├── Api                 后端接口服务
├── Database            数据库脚本
└── Docs                文档说明
```

## 5. 系统模块

### 5.1 站点管理

用于管理企业网站的基础信息。

功能包括：

* 站点名称
* 站点 Logo
* 浏览器标题
* 网站关键词
* 网站描述
* ICP 备案号
* 公安备案号
* 联系电话
* 联系邮箱
* 公司地址
* 版权信息
* 默认主题
* 默认语言
* 网站状态：启用、停用

### 5.2 页面管理

用于管理网站页面，例如首页、关于我们、产品中心、新闻中心、招聘中心、联系我们等。

功能包括：

* 页面新增
* 页面编辑
* 页面复制
* 页面删除
* 页面预览
* 页面发布
* 设置首页
* 页面排序
* 页面状态管理
* 页面 SEO 配置
* 页面路径配置

页面字段建议包括：

```text
Id
SiteId
PageName
PageCode
PagePath
PageTitle
SeoKeywords
SeoDescription
LayoutJson
ComponentJson
Status
IsHome
Sort
CreateTime
UpdateTime
PublishTime
```

### 5.3 可视化装修

这是系统核心功能。

管理员可以在后台进入页面设计器，通过拖拉拽方式完成页面装修。

核心能力包括：

* 左侧组件库
* 中间页面画布
* 右侧属性面板
* 顶部工具栏
* 组件拖入画布
* 组件上下排序
* 组件复制
* 组件删除
* 组件隐藏
* 组件锁定
* 组件属性编辑
* 页面实时预览
* 保存草稿
* 发布页面

页面设计器布局建议：

```text
┌──────────────────────────────────────────────┐
│ 顶部工具栏：保存 / 预览 / 发布 / 撤销 / 重做 │
├──────────────┬────────────────┬──────────────┤
│ 左侧组件库   │ 中间页面画布   │ 右侧属性面板 │
│ 布局组件     │ 页面实时设计区 │ 样式/数据配置 │
│ 内容组件     │                │              │
└──────────────┴────────────────┴──────────────┘
```

## 6. 页面组件

### 6.1 布局组件

布局组件用于控制页面结构。

应支持：

* 单列布局
* 双列布局
* 三列布局
* 栅格布局
* 容器布局
* 分栏布局
* 卡片布局
* 通栏布局
* 留白间距
* 分割线

布局配置项包括：

* 宽度模式：固定宽度、全屏宽度
* 背景颜色
* 背景图片
* 内边距
* 外边距
* 圆角
* 阴影
* 对齐方式
* 响应式显示控制

### 6.2 导航组件

导航组件用于配置网站顶部导航。

应支持：

* Logo 显示
* 一级导航
* 二级导航
* 外部链接
* 页面链接
* 栏目链接
* 高亮当前菜单
* 固定顶部
* 透明导航
* 下拉菜单
* 移动端菜单

导航配置项包括：

```text
Logo
导航样式
导航数据
是否固定顶部
背景颜色
文字颜色
选中颜色
菜单间距
跳转方式
```

### 6.3 Banner 组件

Banner 用于首页或栏目页顶部展示。

应支持：

* 单图 Banner
* 多图轮播 Banner
* 视频 Banner
* 标题文字
* 副标题文字
* 按钮链接
* 图片跳转
* 自动轮播
* 轮播间隔
* 遮罩层
* 文字位置设置

Banner 配置项包括：

```text
图片列表
标题
副标题
按钮文字
按钮链接
高度
轮播速度
是否自动播放
文字颜色
遮罩透明度
```

### 6.4 新闻组件

新闻组件用于展示企业新闻、行业动态、公告信息等。

应支持：

* 新闻列表
* 新闻分类
* 新闻推荐
* 新闻置顶
* 图片新闻
* 新闻详情跳转
* 分页加载
* 按发布时间排序
* 显示摘要
* 显示封面图

新闻组件配置项包括：

```text
新闻分类
显示数量
显示样式
是否显示封面
是否显示摘要
是否显示日期
是否显示更多按钮
更多按钮链接
```

新闻数据字段建议：

```text
Id
Title
CategoryId
CoverImage
Summary
Content
Author
Source
Tags
IsTop
IsRecommend
Status
ViewCount
PublishTime
CreateTime
UpdateTime
```

### 6.5 产品组件

产品组件用于展示企业产品、解决方案、服务项目等。

应支持：

* 产品分类
* 产品列表
* 产品详情
* 产品图片
* 产品参数
* 产品特点
* 产品推荐
* 产品排序
* 产品上下架
* 产品搜索
* 多图展示

产品组件配置项包括：

```text
产品分类
显示数量
显示样式
是否显示图片
是否显示简介
是否显示参数
是否显示更多按钮
每行显示数量
```

产品数据字段建议：

```text
Id
ProductName
CategoryId
CoverImage
ImageList
Summary
Description
Specification
Feature
Sort
IsRecommend
Status
CreateTime
UpdateTime
```

### 6.6 招聘组件

招聘组件用于展示企业招聘岗位。

应支持：

* 招聘列表
* 岗位分类
* 工作地点
* 招聘人数
* 薪资范围
* 岗位职责
* 任职要求
* 在线投递入口
* 岗位详情
* 招聘状态

招聘组件配置项包括：

```text
岗位分类
显示数量
显示工作地点
显示薪资
显示招聘人数
显示发布时间
是否显示投递按钮
```

招聘数据字段建议：

```text
Id
JobTitle
Department
WorkLocation
SalaryRange
RecruitCount
JobType
Responsibilities
Requirements
ContactName
ContactPhone
ContactEmail
Status
PublishTime
CreateTime
UpdateTime
```

### 6.7 页脚组件

页脚组件用于展示企业底部信息。

应支持：

* 公司 Logo
* 公司简介
* 联系电话
* 联系邮箱
* 公司地址
* 快捷导航
* 友情链接
* 二维码
* ICP 备案号
* 公安备案号
* 版权信息

页脚配置项包括：

```text
Logo
公司名称
公司简介
联系电话
联系邮箱
公司地址
二维码图片
备案信息
版权信息
友情链接
背景颜色
文字颜色
```

### 6.8 其他常用组件

建议补充以下企业官网常用组件：

* 图文组件
* 标题组件
* 富文本组件
* 图片组件
* 视频组件
* 按钮组件
* 联系我们组件
* 地图组件
* 友情链接组件
* 案例展示组件
* 荣誉资质组件
* 合作客户组件
* 数据统计组件
* 时间轴组件
* 表单组件

## 7. 组件数据结构

页面组件建议统一使用 JSON 结构保存。

示例：

```json
{
  "id": "banner_001",
  "type": "banner",
  "name": "首页 Banner",
  "sort": 1,
  "visible": true,
  "locked": false,
  "props": {
    "height": 520,
    "autoplay": true,
    "interval": 5000,
    "items": [
      {
        "title": "专注企业数字化建设",
        "subtitle": "为企业提供专业的网站建设与数字化解决方案",
        "image": "/uploads/banner/banner1.jpg",
        "buttonText": "了解更多",
        "buttonLink": "/about"
      }
    ]
  },
  "style": {
    "backgroundColor": "#ffffff",
    "paddingTop": 0,
    "paddingBottom": 0,
    "marginTop": 0,
    "marginBottom": 0
  }
}
```

页面整体 JSON 示例：

```json
{
  "pageId": 1,
  "pageName": "首页",
  "pagePath": "/",
  "layout": {
    "width": "full",
    "theme": "default"
  },
  "components": [
    {
      "id": "nav_001",
      "type": "navigation",
      "name": "顶部导航"
    },
    {
      "id": "banner_001",
      "type": "banner",
      "name": "首页 Banner"
    },
    {
      "id": "news_001",
      "type": "news",
      "name": "新闻中心"
    },
    {
      "id": "product_001",
      "type": "product",
      "name": "产品中心"
    },
    {
      "id": "footer_001",
      "type": "footer",
      "name": "页脚"
    }
  ]
}
```

## 8. 内容管理功能

### 8.1 新闻管理

功能包括：

* 新闻分类管理
* 新闻新增
* 新闻编辑
* 新闻删除
* 新闻发布
* 新闻下架
* 新闻置顶
* 新闻推荐
* 新闻封面上传
* 新闻富文本编辑
* 新闻 SEO 设置

### 8.2 产品管理

功能包括：

* 产品分类管理
* 产品新增
* 产品编辑
* 产品删除
* 产品上下架
* 产品推荐
* 产品图片上传
* 产品参数维护
* 产品详情编辑
* 产品排序

### 8.3 招聘管理

功能包括：

* 岗位新增
* 岗位编辑
* 岗位删除
* 岗位发布
* 岗位关闭
* 岗位分类
* 工作地点维护
* 简历投递记录
* 投递状态管理

## 9. 后台菜单建议

后台管理菜单建议如下：

```text
网站管理
├── 站点设置
├── 页面管理
├── 页面装修
├── 导航管理
├── Banner管理
├── 页脚设置

内容管理
├── 新闻分类
├── 新闻管理
├── 产品分类
├── 产品管理
├── 招聘管理
├── 简历投递

资源管理
├── 图片素材
├── 文件素材
├── 视频素材

系统管理
├── 用户管理
├── 角色管理
├── 权限管理
├── 操作日志
```

## 10. 权限设计

建议按功能模块配置权限。

权限编码示例：

```text
Website.Site.View
Website.Site.Edit

Website.Page.View
Website.Page.Create
Website.Page.Edit
Website.Page.Delete
Website.Page.Design
Website.Page.Publish

Website.News.View
Website.News.Create
Website.News.Edit
Website.News.Delete
Website.News.Publish

Website.Product.View
Website.Product.Create
Website.Product.Edit
Website.Product.Delete
Website.Product.Publish

Website.Job.View
Website.Job.Create
Website.Job.Edit
Website.Job.Delete
Website.Job.Publish

Website.Material.View
Website.Material.Upload
Website.Material.Delete
```

## 11. 接口设计建议

### 11.1 页面接口

```text
GET    /api/pages
GET    /api/pages/{id}
POST   /api/pages
PUT    /api/pages/{id}
DELETE /api/pages/{id}
POST   /api/pages/{id}/copy
POST   /api/pages/{id}/publish
POST   /api/pages/{id}/preview
PUT    /api/pages/{id}/design
```

### 11.2 站点接口

```text
GET  /api/site/config
PUT  /api/site/config
```

### 11.3 新闻接口

```text
GET    /api/news
GET    /api/news/{id}
POST   /api/news
PUT    /api/news/{id}
DELETE /api/news/{id}
POST   /api/news/{id}/publish
POST   /api/news/{id}/offline
```

### 11.4 产品接口

```text
GET    /api/products
GET    /api/products/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}
POST   /api/products/{id}/online
POST   /api/products/{id}/offline
```

### 11.5 招聘接口

```text
GET    /api/jobs
GET    /api/jobs/{id}
POST   /api/jobs
PUT    /api/jobs/{id}
DELETE /api/jobs/{id}
POST   /api/jobs/{id}/publish
POST   /api/jobs/{id}/close
```

### 11.6 素材接口

```text
GET    /api/materials
POST   /api/materials/upload
DELETE /api/materials/{id}
```

## 12. 数据库表建议

建议至少包含以下表：

```text
website_site_config       站点配置表
website_page              页面表
website_page_version      页面版本表
website_navigation        导航表
website_banner            Banner表
website_footer            页脚配置表

content_news_category     新闻分类表
content_news              新闻表

content_product_category  产品分类表
content_product           产品表
content_product_image     产品图片表

content_job               招聘岗位表
content_job_apply         简历投递表

material_file             素材文件表
system_operation_log      操作日志表
```

## 13. 页面发布机制

页面设计建议区分草稿和发布版本。

流程如下：

```text
编辑页面
    ↓
保存草稿
    ↓
后台预览
    ↓
确认发布
    ↓
生成发布版本
    ↓
前台读取发布版本渲染
```

页面版本表建议保存：

```text
Id
PageId
VersionNo
DraftJson
PublishJson
Status
CreateTime
PublishTime
CreateUserId
```

好处：

* 支持草稿编辑
* 不影响线上页面
* 支持回滚历史版本
* 支持发布审核扩展
* 降低误操作风险

## 14. 前台渲染规则

前台根据页面路径获取页面配置 JSON，然后按组件类型动态渲染。

渲染流程：

```text
访问页面路径
    ↓
根据 path 获取页面配置
    ↓
读取 PublishJson
    ↓
遍历 components
    ↓
根据 type 匹配组件
    ↓
渲染页面
```

组件类型映射示例：

```text
navigation -> NavigationComponent
banner     -> BannerComponent
news       -> NewsListComponent
product    -> ProductListComponent
job        -> JobListComponent
footer     -> FooterComponent
richText   -> RichTextComponent
image      -> ImageComponent
video      -> VideoComponent
```

## 15. 可视化编辑器交互要求

页面设计器应支持以下操作：

* 从组件库拖入组件
* 组件拖拽排序
* 点击组件选中
* 右侧编辑组件属性
* 组件复制
* 组件删除
* 组件隐藏
* 组件锁定
* 组件上移
* 组件下移
* 撤销
* 重做
* 保存草稿
* 页面预览
* 页面发布

编辑器应避免复杂化，第一阶段优先实现纵向组件拖拽装修，不建议一开始实现过复杂的自由画布绝对定位。

推荐第一阶段设计模式：

```text
组件按块排列
上下拖拽排序
每个组件内部通过属性配置控制样式
不支持任意像素级拖动
```

这样更适合企业官网，也更容易保证响应式效果。

## 16. 响应式适配

组件应支持 PC、平板、手机端显示。

每个组件建议支持：

```text
showOnPc
showOnTablet
showOnMobile
pcStyle
tabletStyle
mobileStyle
```

常用配置：

* PC 每行 4 个产品
* 平板每行 2 个产品
* 手机每行 1 个产品
* Banner 高度自动适配
* 导航移动端折叠菜单
* 页脚移动端上下排列

## 17. SEO 基础功能

页面应支持：

* 页面标题
* 页面关键词
* 页面描述
* 自定义 URL
* 友情链接
* 图片 alt
* sitemap 扩展
* robots.txt 扩展

页面 SEO 字段：

```text
PageTitle
SeoKeywords
SeoDescription
CanonicalUrl
```

## 18. 文件上传要求

素材上传应支持：

* 图片上传
* 视频上传
* PDF 上传
* Word 上传
* Excel 上传
* 文件分类
* 文件预览
* 文件删除
* 文件大小限制
* 文件类型限制

图片建议支持：

* jpg
* jpeg
* png
* webp
* gif
* svg

上传路径建议：

```text
/uploads/website/yyyyMM/dd/filename.ext
```

## 19. 安全要求

系统应注意以下安全点：

* 后台接口必须登录访问
* 按钮级权限控制
* 文件上传类型校验
* 文件大小限制
* 富文本内容 XSS 过滤
* 页面路径防重复
* 删除操作二次确认
* 发布操作记录日志
* 防止越权修改页面
* 前台只读取已发布内容

## 20. 开发阶段建议

### 第一阶段：企业官网基础功能

完成：

* 站点设置
* 页面管理
* 页面装修
* 导航组件
* Banner 组件
* 新闻组件
* 产品组件
* 招聘组件
* 页脚组件
* 前台页面渲染
* 页面预览
* 页面发布

### 第二阶段：增强装修能力

完成：

* 页面模板
* 组件模板
* 页面复制
* 页面版本回滚
* 多主题切换
* 移动端样式配置
* 素材库管理

### 第三阶段：企业门户增强

完成：

* 多语言
* 表单组件
* 在线留言
* 招聘投递
* 访问统计
* SEO 自动生成
* 静态化发布
* CDN 资源适配

## 21. 开发约束

开发时应遵守：

* 不要把页面内容硬编码到前台
* 页面配置必须保存在数据库
* 页面装修数据必须使用 JSON 保存
* 组件类型要可扩展
* 组件属性要统一结构
* 后台和前台分离
* 前台只负责渲染发布后的页面
* 后台负责编辑、预览、发布
* 重要操作需要记录日志
* 删除数据建议使用软删除

## 22. 推荐默认页面

企业官网默认创建以下页面：

```text
首页 /
关于我们 /about
新闻中心 /news
新闻详情 /news/detail/{id}
产品中心 /products
产品详情 /products/detail/{id}
招聘中心 /jobs
招聘详情 /jobs/detail/{id}
联系我们 /contact
```

## 23. 推荐默认栏目

默认栏目建议：

```text
首页
关于我们
产品中心
新闻中心
招聘中心
联系我们
```

导航数据示例：

```json
[
  {
    "title": "首页",
    "path": "/",
    "children": []
  },
  {
    "title": "关于我们",
    "path": "/about",
    "children": []
  },
  {
    "title": "产品中心",
    "path": "/products",
    "children": []
  },
  {
    "title": "新闻中心",
    "path": "/news",
    "children": []
  },
  {
    "title": "招聘中心",
    "path": "/jobs",
    "children": []
  },
  {
    "title": "联系我们",
    "path": "/contact",
    "children": []
  }
]
```

## 24. 首页推荐组件顺序

企业官网首页推荐默认结构：

```text
顶部导航
Banner
公司简介
产品中心
解决方案
新闻中心
合作客户
招聘信息
联系我们
页脚
```

## 25. 验收标准

系统完成后应满足以下验收标准：

* 管理员可以创建页面
* 管理员可以进入页面装修
* 管理员可以拖入组件
* 管理员可以配置组件属性
* 管理员可以保存草稿
* 管理员可以预览页面
* 管理员可以发布页面
* 前台可以正常访问发布页面
* 新闻组件可以读取新闻数据
* 产品组件可以读取产品数据
* 招聘组件可以读取岗位数据
* 导航组件可以正确跳转
* Banner 可以正常轮播
* 页脚信息可以后台配置
* 页面支持 PC 和手机端基础适配
* 后台接口具备权限控制
* 文件上传具备安全校验

## 26. 生成代码时的要求

当根据本 Skill 生成代码时，应优先完成可运行的最小闭环。

最小闭环包括：

```text
页面管理
页面设计器
组件 JSON 保存
前台动态渲染
导航组件
Banner 组件
新闻组件
产品组件
招聘组件
页脚组件
发布功能
```

不要一开始实现过度复杂的低代码能力，例如：

* 任意像素级自由拖动
* 复杂动画编排
* 多人协同编辑
* 在线代码编辑
* 复杂工作流审批

第一版应重点保证：

* 能装修
* 能保存
* 能预览
* 能发布
* 前台能渲染
* 新闻、产品、招聘能展示

## 27. 开发提示词

在开发该系统时，可以使用以下提示词：

```text
请根据拖拉拽企业建站 Skill，开发一个企业网站可视化建站系统。

要求：
1. 后台支持站点管理、页面管理、页面装修、新闻管理、产品管理、招聘管理、素材管理。
2. 页面装修采用组件化 JSON 方案。
3. 页面组件包括布局、导航、Banner、新闻、产品、招聘、页脚、图文、富文本、图片、视频。
4. 后台页面设计器采用左侧组件库、中间画布、右侧属性面板结构。
5. 页面支持保存草稿、预览、发布。
6. 前台根据发布后的 JSON 动态渲染页面。
7. 新闻、产品、招聘组件需要从对应内容表读取数据。
8. 后台接口必须有权限控制。
9. 文件上传需要限制类型和大小。
10. 第一版只做纵向组件拖拽排序，不做复杂自由画布。
```

## 28. 重点实现原则

本系统不是简单 CMS，也不是复杂低代码平台，而是企业官网可视化装修系统。

核心原则：

```text
内容由 CMS 管理
页面由组件装修
组件由 JSON 配置
前台由 JSON 渲染
发布版本和草稿版本分离
```

最终目标是让企业管理员可以通过后台完成网站页面搭建和内容发布，降低企业官网维护成本，提高页面更新效率。
