(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var R = root.Registry;
    function f(key, label, type, extra) { var x = { key: key, label: label, type: type || 'text' }; if (extra) Object.keys(extra).forEach(function (k) { x[k] = extra[k]; }); return x; }
    function reg(def) { R.register(def); }

    reg({ type: 'section', name: '区段', group: 'layout', icon: 'layui-icon-template-1', container: true, defaults: {}, styleDefaults: { paddingTop: '48px', paddingBottom: '48px', backgroundColor: '#ffffff' }, inspector: [] });
    reg({ type: 'container', name: '容器', group: 'layout', icon: 'layui-icon-screen-full', container: true, defaults: {}, styleDefaults: { maxWidth: '1200px', marginLeft: 'auto', marginRight: 'auto', paddingLeft: '20px', paddingRight: '20px' }, inspector: [] });
    reg({ type: 'grid', name: '网格', group: 'layout', icon: 'layui-icon-table', container: true, defaults: { columns: 2 }, styleDefaults: { gap: '24px' }, inspector: [f('columns', '列数', 'number', { min: 1, max: 6 })] });
    reg({ type: 'column', name: '列', group: 'layout', icon: 'layui-icon-tabs', container: true, defaults: {}, styleDefaults: { minHeight: '40px' }, inspector: [] });

    reg({ type: 'heading', name: '标题', group: 'basic', icon: 'layui-icon-fonts-strong', defaults: { text: '请输入标题', level: 2 }, inspector: [f('text', '标题文字'), f('level', '标题级别', 'select', { options: [{value:1,text:'H1'},{value:2,text:'H2'},{value:3,text:'H3'},{value:4,text:'H4'}] })] });
    reg({ type: 'text', name: '文本', group: 'basic', icon: 'layui-icon-edit', defaults: { text: '请输入文本内容' }, inspector: [f('text', '文本内容', 'textarea')] });
    reg({ type: 'image', name: '图片', group: 'basic', icon: 'layui-icon-picture', defaults: { src: '', alt: '', link: '' }, inspector: [f('src','图片','image'), f('alt','替代文本'), f('link','跳转链接')] });
    reg({ type: 'banner', name: 'Banner', group: 'basic', icon: 'layui-icon-carousel', defaults: { images: [], height: 420, interval: 5000, showArrows: true, showDots: true, objectFit: 'cover' }, inspector: [f('images','轮播图片','image-list'), f('height','高度(px)','number',{min:120,max:900}), f('interval','轮播间隔(ms)','number',{min:1000,max:30000}), f('objectFit','图片填充','select',{options:[{value:'cover',text:'覆盖裁剪'},{value:'contain',text:'完整显示'}]}), f('showArrows','显示左右箭头','checkbox'), f('showDots','显示圆点指示','checkbox')] });
    reg({ type: 'button', name: '按钮', group: 'basic', icon: 'layui-icon-link', defaults: { text: '了解更多', href: '#', target: '_self', variant: 'primary' }, inspector: [f('text','按钮文字'), f('href','跳转链接'), f('target','打开方式','select',{options:[{value:'_self',text:'当前窗口'},{value:'_blank',text:'新窗口'}]}), f('variant','按钮样式','select',{options:[{value:'primary',text:'主按钮'},{value:'outline',text:'描边按钮'},{value:'text',text:'文字按钮'}]})] });
    reg({ type: 'icon', name: '图标', group: 'basic', icon: 'layui-icon-star', defaults: { text: '★', size: 32 }, inspector: [f('text','图标/字符'), f('size','尺寸','number',{min:12,max:160})] });
    reg({ type: 'video', name: '视频', group: 'basic', icon: 'layui-icon-video', defaults: { src: '', poster: '', controls: true }, inspector: [f('src','视频地址'), f('poster','封面图片','image'), f('controls','显示控件','checkbox')] });
    reg({ type: 'divider', name: '分隔线', group: 'basic', icon: 'layui-icon-more', defaults: {}, styleDefaults: { borderTopWidth: '1px', borderTopStyle: 'solid', borderTopColor: '#e5e7eb', marginTop: '20px', marginBottom: '20px' }, inspector: [] });
    reg({ type: 'spacer', name: '间距', group: 'basic', icon: 'layui-icon-screen-full', defaults: { height: 40 }, inspector: [f('height','高度(px)','number',{min:4,max:400})] });

    reg({ type: 'articleList', name: '文章列表', group: 'data', icon: 'layui-icon-list', defaults: { categoryId: 0, pageSize: 6, columns: 3, showSummary: true, showDate: true }, inspector: [f('categoryId','分类ID','number',{min:0}), f('pageSize','显示数量','number',{min:1,max:50}), f('columns','列数','number',{min:1,max:6}), f('showSummary','显示摘要','checkbox'), f('showDate','显示日期','checkbox')] });
    reg({ type: 'productList', name: '产品列表', group: 'data', icon: 'layui-icon-component', defaults: { categoryId: 0, pageSize: 8, columns: 4, showSummary: false }, inspector: [f('categoryId','分类ID','number',{min:0}), f('pageSize','显示数量','number',{min:1,max:50}), f('columns','列数','number',{min:1,max:6}), f('showSummary','显示摘要','checkbox')] });
    reg({ type: 'jobList', name: '招聘列表', group: 'data', icon: 'layui-icon-friends', defaults: { categoryId: 0, pageSize: 10, showSalary: true, showLocation: true }, inspector: [f('categoryId','分类ID','number',{min:0}), f('pageSize','显示数量','number',{min:1,max:50}), f('showSalary','显示薪资','checkbox'), f('showLocation','显示地点','checkbox')] });

    reg({ type: 'logo', name: 'Logo', group: 'global', icon: 'layui-icon-picture-fine', defaults: { src: '', text: '企业名称', href: '/' }, inspector: [f('src','Logo','image'), f('text','站点名称'), f('href','首页链接')] });
    reg({ type: 'navigation', name: '导航菜单', group: 'global', icon: 'layui-icon-menu-fill', defaults: { direction: 'horizontal' }, inspector: [f('direction','排列方式','select',{options:[{value:'horizontal',text:'横向'},{value:'vertical',text:'纵向'}]})] });
    reg({ type: 'search', name: '搜索', group: 'global', icon: 'layui-icon-search', defaults: { placeholder: '搜索', action: '/search' }, inspector: [f('placeholder','提示文字'), f('action','搜索地址')] });
    reg({ type: 'language', name: '语言切换', group: 'global', icon: 'layui-icon-website', defaults: { text: '中文 / EN' }, inspector: [f('text','显示文字')] });
    reg({ type: 'contact', name: '联系方式', group: 'global', icon: 'layui-icon-cellphone', defaults: { phone: '', email: '', address: '' }, inspector: [f('phone','电话'),f('email','邮箱'),f('address','地址')] });
    reg({ type: 'social', name: '社交链接', group: 'global', icon: 'layui-icon-share', defaults: { text: '关注我们', links: '' }, inspector: [f('text','标题'),f('links','链接配置','textarea')] });
    reg({ type: 'copyright', name: '版权信息', group: 'global', icon: 'layui-icon-auz', defaults: { text: '© 2026 企业名称 版权所有' }, inspector: [f('text','版权文字','textarea')] });
})(window);
