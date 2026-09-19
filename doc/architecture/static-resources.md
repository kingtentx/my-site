# 静态资源目录规范

应用自带的静态资源统一放在 `src/CIMC.WebSite/wwwroot/static/`，资源 URL 从 `/static/` 开始。

| 目录 | 内容 | 典型 URL |
| --- | --- | --- |
| `layuiadmin/` | 后台框架、后台自有的 CSS、JS 和图片 | `/static/layuiadmin/layui/layui.js` |
| `site-builder/` | 后台装修编辑器、组件定义、属性面板与设计器预览代码 | `/static/site-builder/page-designer.js` |
| `site/` | 前台样式、脚本、Builder 运行时及图片；原 `syle/` 归入 `site/legacy/` | `/static/site/css/builder-runtime.css` |
| `plugin/` | 外部类库，按库及版本保留原有内部结构 | `/static/plugin/ssi-uploader/js/ssi-uploader.js` |

第三方库包括 `layui-v2.6.8`、`ssi-uploader`、`wangEditor-4.7.13`、`wangeditor-4.7.9`，以及 jQuery、Sortable 等独立插件。后台框架内置的 `layuiadmin/layui/` 随框架整体保留，避免破坏其相对路径。新增第三方库应优先放入 `plugin/`，不要与站点自有代码混放。

`wwwroot/upload/` 是运行时产生的用户文件，不属于应用静态包。上传配置、数据库中的文件 URL、图片删除逻辑都依赖 `/upload/`，因此保留原目录和 URL，不随静态资源迁移。旧 `/site/`、`/site-builder/`、`/resource/`、`/syle/`、`/layui-v2.6.8/` 及 `/favicon.ico` 请求仍兼容，以保护已发布内容；新代码统一引用 `/static/`。
