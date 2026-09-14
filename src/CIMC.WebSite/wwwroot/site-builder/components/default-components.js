(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var R = root.Registry;
    function f(key, label, type, extra) { var x = { key: key, label: label, type: type || 'text' }; if (extra) Object.keys(extra).forEach(function (k) { x[k] = extra[k]; }); return x; }
    function reg(def) { R.register(def); }

    // 样式能力令牌：组件声明自己支持哪些样式维度，属性面板据此裁剪；
    // 未声明的组件按“全量”处理，保证新增组件不会因为忘记声明而丢失样式项。
    var SCOPE_BOX = ['background', 'size', 'align', 'color', 'spacing', 'border', 'position'];
    var SCOPE_TEXT = ['background', 'size', 'align', 'color', 'typography', 'spacing', 'border', 'position'];
    var SCOPE_MEDIA = ['background', 'size', 'fit', 'align', 'spacing', 'border', 'position'];
    var SCOPE_PLAIN = ['background', 'size', 'spacing', 'border', 'position'];
    var SCOPE_GRID = ['background', 'size', 'align', 'color', 'gap', 'grid', 'spacing', 'border', 'position'];
    // Banner 的高度、图片填充属于组件语义参数，统一放在「内容」里配置，样式区不再重复出现同名字段。
    var SCOPE_BANNER = ['background', 'align', 'color', 'spacing', 'border', 'position'];
    // 间距组件的高度由「内容 · 高度」决定。
    var SCOPE_SPACER = ['background', 'spacing', 'border', 'position'];
    var SCOPE_ICON = ['background', 'color', 'align', 'typography', 'spacing', 'border', 'position'];

    reg({ type: 'section', name: '区段', group: 'layout', icon: 'layui-icon-template-1', desc: '页面大区块，通常一个页面由多个区段组成', container: true, styleScope: SCOPE_BOX, defaults: {}, styleDefaults: { paddingTop: '48px', paddingBottom: '48px', backgroundColor: '#ffffff' }, inspector: [] });
    reg({ type: 'container', name: '容器', group: 'layout', icon: 'layui-icon-screen-full', desc: '按设计宽度居中并控制左右留白', container: true, styleScope: SCOPE_BOX, defaults: {}, styleDefaults: { maxWidth: '1200px', marginLeft: 'auto', marginRight: 'auto', paddingLeft: '20px', paddingRight: '20px' }, inspector: [] });
    reg({ type: 'grid', name: '网格', group: 'layout', icon: 'layui-icon-table', desc: '1~6 列布局，列宽可在画布中拖动调整', container: true, styleScope: SCOPE_GRID, defaults: { columns: 2, columnWidths: [50, 50] }, styleDefaults: { gap: '24px' }, inspector: [f('columns', '网格列数', 'grid-columns', { min: 1, max: 6 })] });
    reg({ type: 'column', name: '列', group: 'layout', icon: 'layui-icon-tabs', desc: '网格内的列容器', container: true, styleScope: SCOPE_BOX, defaults: {}, styleDefaults: { minHeight: '40px' }, inspector: [] });

    reg({ type: 'heading', name: '标题', group: 'basic', icon: 'layui-icon-fonts-strong', desc: 'H1~H4 级标题', styleScope: SCOPE_TEXT, defaults: { text: '请输入标题', level: 2 }, inspector: [f('text', '标题文字'), f('level', '标题级别', 'select', { options: [{ value: 1, text: 'H1' }, { value: 2, text: 'H2' }, { value: 3, text: 'H3' }, { value: 4, text: 'H4' }] })] });
    reg({ type: 'text', name: '文本', group: 'basic', icon: 'layui-icon-edit', desc: '富文本内容，支持段落、标题、加粗、列表、引用与链接', styleScope: SCOPE_TEXT, defaults: { text: '请输入文本内容' }, inspector: [f('text', '文本内容', 'textarea', { hint: '支持正文/标题、加粗、斜体、下划线、列表、引用与超链接；历史纯文本会自动兼容' })] });
    reg({ type: 'image', name: '图片', group: 'basic', icon: 'layui-icon-picture', desc: '单张图片，可设置跳转链接', styleScope: SCOPE_MEDIA, defaults: { src: '', alt: '', link: '' }, inspector: [f('src', '图片', 'image'), f('alt', '替代文本', 'text', { hint: '用于 SEO 与图片加载失败提示' }), f('link', '跳转链接', 'text', { placeholder: '留空表示不可点击' })] });
    reg({ type: 'banner', name: 'Banner', group: 'basic', icon: 'layui-icon-carousel', desc: '多图轮播，2 张及以上自动播放', styleScope: SCOPE_BANNER, defaults: { images: [], height: 420, interval: 5000, showArrows: true, showDots: true, objectFit: 'cover' }, inspector: [f('images', '轮播图片', 'image-list', { hint: '选择 2 张及以上时前台自动轮播' }), f('height', 'Banner 高度(px)', 'number', { min: 120, max: 900, default: 420 }), f('interval', '轮播间隔(ms)', 'number', { min: 1000, max: 30000, step: 500, default: 5000 }), f('objectFit', '图片填充', 'select', { options: [{ value: 'cover', text: '覆盖裁剪' }, { value: 'contain', text: '完整显示' }] }), f('showArrows', '显示左右箭头', 'checkbox'), f('showDots', '显示圆点指示', 'checkbox'), f('title', '主标题'), f('description', '介绍文字', 'textarea'), f('buttonText', '按钮文字'), f('buttonHref', '按钮链接', 'text', { placeholder: '留空则不显示按钮' })] });
    reg({ type: 'button', name: '按钮', group: 'basic', icon: 'layui-icon-link', desc: '行动按钮，支持跳转与打开方式', styleScope: SCOPE_TEXT, defaults: { text: '了解更多', href: '#', target: '_self', variant: 'primary' }, inspector: [f('text', '按钮文字'), f('href', '跳转链接', 'text', { placeholder: '如 /contact 或 https://…' }), f('target', '打开方式', 'select', { options: [{ value: '_self', text: '当前窗口' }, { value: '_blank', text: '新窗口' }] }), f('variant', '按钮样式', 'select', { options: [{ value: 'primary', text: '主按钮' }, { value: 'outline', text: '描边按钮' }, { value: 'text', text: '文字按钮' }] })] });
    reg({ type: 'icon', name: '图标', group: 'basic', icon: 'layui-icon-star', desc: '单字符图标或符号', styleScope: SCOPE_ICON, defaults: { text: '★', size: 32 }, inspector: [f('text', '图标/字符'), f('size', '尺寸(px)', 'number', { min: 12, max: 160, default: 32 })] });
    reg({ type: 'video', name: '视频', group: 'basic', icon: 'layui-icon-video', desc: 'mp4 等直链视频', styleScope: SCOPE_MEDIA, defaults: { src: '', poster: '', controls: true }, inspector: [f('src', '视频地址', 'text', { placeholder: '如 /upload/demo.mp4' }), f('poster', '封面图片', 'image'), f('controls', '显示播放控件', 'checkbox')] });
    reg({ type: 'divider', name: '分隔线', group: 'basic', icon: 'layui-icon-more', desc: '横向分隔线', styleScope: SCOPE_PLAIN, defaults: {}, styleDefaults: { borderTopWidth: '1px', borderTopStyle: 'solid', borderTopColor: '#e5e7eb', marginTop: '20px', marginBottom: '20px' }, inspector: [] });
    reg({ type: 'spacer', name: '间距', group: 'basic', icon: 'layui-icon-screen-full', desc: '纯占位空白，用于拉开区块距离', styleScope: SCOPE_SPACER, defaults: { height: 40 }, inspector: [f('height', '高度(px)', 'number', { min: 4, max: 400, default: 40 })] });

    reg({ type: 'articleList', name: '文章列表', group: 'data', icon: 'layui-icon-list', desc: '绑定后台新闻数据', styleScope: SCOPE_BOX, defaults: { categoryId: 0, pageSize: 6, columns: 3, showSummary: true, showDate: true }, inspector: [f('categoryId', '分类ID', 'number', { min: 0, hint: '0 表示全部分类' }), f('pageSize', '显示数量', 'number', { min: 1, max: 50 }), f('columns', '列数', 'number', { min: 1, max: 6 }), f('showSummary', '显示摘要', 'checkbox'), f('showDate', '显示日期', 'checkbox'), f('layout', '卡片样式', 'select', { options: [{ value: '', text: '标准卡片' }, { value: 'editorial', text: '企业新闻卡片' }] })] });
    reg({ type: 'productList', name: '产品列表', group: 'data', icon: 'layui-icon-component', desc: '绑定后台产品数据', styleScope: SCOPE_BOX, defaults: { categoryId: 0, pageSize: 8, columns: 4, showSummary: false }, inspector: [f('categoryId', '分类ID', 'number', { min: 0, hint: '0 表示全部分类' }), f('pageSize', '显示数量', 'number', { min: 1, max: 50 }), f('columns', '列数', 'number', { min: 1, max: 6 }), f('showSummary', '显示摘要', 'checkbox')] });
    reg({ type: 'jobList', name: '招聘列表', group: 'data', icon: 'layui-icon-friends', desc: '绑定后台招聘数据', styleScope: SCOPE_BOX, defaults: { categoryId: 0, pageSize: 10, showSalary: true, showLocation: true }, inspector: [f('categoryId', '分类ID', 'number', { min: 0, hint: '0 表示全部分类' }), f('pageSize', '显示数量', 'number', { min: 1, max: 50 }), f('showSalary', '显示薪资', 'checkbox'), f('showLocation', '显示地点', 'checkbox'), f('layout', '列表样式', 'select', { options: [{ value: '', text: '标准列表' }, { value: 'compact', text: '简洁岗位列表' }] })] });

    reg({ type: 'logo', name: 'Logo', group: 'global', icon: 'layui-icon-picture-fine', desc: '站点 Logo 或站点名称', styleScope: SCOPE_TEXT, defaults: { src: '', text: '企业名称', href: '/' }, inspector: [f('src', 'Logo', 'image', { hint: '未选择时显示下面的站点名称' }), f('text', '站点名称'), f('href', '首页链接', 'text', { placeholder: '如 /' })] });
    reg({ type: 'navigation', name: '导航菜单', group: 'global', icon: 'layui-icon-menu-fill', desc: '取自已发布页面树', styleScope: SCOPE_TEXT, defaults: { direction: 'horizontal' }, inspector: [f('direction', '排列方式', 'select', { options: [{ value: 'horizontal', text: '横向' }, { value: 'vertical', text: '纵向' }], hint: '菜单内容来自「页面管理」中已发布且开启导航的页面' })] });
    reg({ type: 'search', name: '搜索', group: 'global', icon: 'layui-icon-search', desc: '站内搜索框', styleScope: SCOPE_TEXT, defaults: { placeholder: '搜索', action: '/search' }, inspector: [f('placeholder', '提示文字'), f('action', '搜索地址', 'text', { placeholder: '如 /search' })] });
    reg({ type: 'language', name: '语言切换', group: 'global', icon: 'layui-icon-website', desc: '多语言入口占位', styleScope: SCOPE_TEXT, defaults: { text: '中文 / EN' }, inspector: [f('text', '显示文字')] });
    reg({ type: 'contact', name: '联系方式', group: 'global', icon: 'layui-icon-cellphone', desc: '电话 / 邮箱 / 地址', styleScope: SCOPE_TEXT, defaults: { phone: '', email: '', address: '' }, inspector: [f('phone', '电话'), f('email', '邮箱'), f('address', '地址')] });
    reg({ type: 'social', name: '社交链接', group: 'global', icon: 'layui-icon-share', desc: '社交账号入口', styleScope: SCOPE_TEXT, defaults: { text: '关注我们', links: '' }, inspector: [f('text', '标题'), f('links', '链接配置', 'textarea', { hint: '每行一个，格式：名称|链接' })] });
    reg({ type: 'copyright', name: '版权信息', group: 'global', icon: 'layui-icon-auz', desc: '页脚版权文字', styleScope: SCOPE_TEXT, defaults: { text: '© 2026 企业名称 版权所有' }, inspector: [f('text', '版权文字', 'textarea')] });
})(window);
