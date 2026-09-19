(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var R = root.Registry;
    function f(key, label, type, extra) { var x = { key: key, label: label, type: type || 'text' }; if (extra) Object.keys(extra).forEach(function (k) { x[k] = extra[k]; }); return x; }
    function reg(def) { R.register(def); }

    // 样式能力令牌：组件声明自己支持哪些样式维度，属性面板据此裁剪；
    // 未声明的组件按“全量”处理，保证新增组件不会因为忘记声明而丢失样式项。
    var SCOPE_BOX = ['background', 'size', 'spacing', 'border', 'position'];
    var SCOPE_TEXT = ['background', 'size', 'typography', 'spacing', 'border', 'position'];
    var SCOPE_MEDIA = ['background', 'size', 'fit', 'spacing', 'border', 'position'];
    var SCOPE_PLAIN = ['background', 'size', 'spacing', 'border', 'position'];
    var SCOPE_GRID = ['background', 'size', 'gap', 'grid', 'spacing', 'border', 'position'];
    // Banner 的高度、图片填充属于组件语义参数，统一放在「内容」里配置，样式区不再重复出现同名字段。
    var SCOPE_BANNER = ['background', 'spacing', 'border', 'position'];
    // 间距组件的高度由「内容 · 高度」决定。
    var SCOPE_SPACER = ['background', 'spacing', 'border', 'position'];
    var SCOPE_ICON = ['background', 'typography', 'spacing', 'border', 'position'];

    // 文章 / 产品 / 招聘列表共用的分类 Tab 排列展示样式。
    // 三个控件必须使用同一套取值，服务端 _ArticleList / _ProductList / _JobList 与
    // designer-renderer 的 categoryTabPreview 都按这里的 value 生成 class。
    var TAB_STYLE_OPTIONS = [
        { value: 'pill', text: '胶囊按钮（默认）' },
        { value: 'underline', text: '下划线导航' },
        { value: 'segmented', text: '分段控件' },
        { value: 'card', text: '卡片式' },
        { value: 'plain', text: '纯文字' }
    ];
    var TEXT_WRAP_OPTIONS = [
        { value: 'auto', text: '自动换行（自适应宽度）' },
        { value: 'nowrap', text: '不换行（超出省略号）' },
        { value: 'break', text: '强制断行' }
    ];
    /** 文本/标题共用：换行方式。默认 auto = 在所在元素宽度内自动折行，永不溢出。 */
    function textWrapField() {
        return f('textWrap', '换行方式', 'select', {
            options: TEXT_WRAP_OPTIONS,
            hint: '自动换行：随所在列宽度自动折行；不换行：始终单行，超出部分显示省略号；强制断行：长串可在任意字符处断开'
        });
    }

    var TAB_ALIGN_OPTIONS = [
        { value: 'left', text: '左对齐' },
        { value: 'center', text: '居中' },
        { value: 'right', text: '右对齐' }
    ];
    // 导航菜单主导航条目风格（与 designer-renderer.js 的 NAV_ITEM_STYLES、_Node.cshtml 白名单三处同步）
    var NAV_ITEM_STYLE_OPTIONS = [
        { value: 'underline', text: '下划线（默认）' },
        { value: 'pill', text: '胶囊高亮' },
        { value: 'block', text: '色块条' },
        { value: 'left-bar', text: '左侧竖条' },
        { value: 'plain', text: '纯文字' }
    ];
    function tabStyleField() { return f('tabStyle', 'Tab 展示样式', 'select', { options: TAB_STYLE_OPTIONS, hint: '分类 Tab 的外观，需开启「显示分类 Tab」' }); }
    function tabAlignField() { return f('tabAlign', 'Tab 排列位置', 'select', { options: TAB_ALIGN_OPTIONS }); }

    reg({ type: 'section', name: '区段', group: 'layout', icon: 'layui-icon-template-1', desc: '页面大区块，通常一个页面由多个区段组成', container: true, styleScope: SCOPE_BOX, defaults: {}, styleDefaults: { paddingTop: '48px', paddingBottom: '48px', backgroundColor: '#ffffff' }, inspector: [] });
    reg({ type: 'container', name: '容器', group: 'layout', icon: 'layui-icon-screen-full', desc: '按设计宽度居中并控制左右留白', container: true, styleScope: SCOPE_BOX, defaults: {}, styleDefaults: { maxWidth: '1200px', marginLeft: 'auto', marginRight: 'auto', paddingLeft: '20px', paddingRight: '20px' }, inspector: [] });
    reg({ type: 'grid', name: '网格', group: 'layout', icon: 'layui-icon-table', desc: '1~6 列布局，列宽可在画布中拖动调整', container: true, styleScope: SCOPE_GRID, defaults: { columns: 2, columnWidths: [50, 50] }, styleDefaults: { gap: '24px' }, inspector: [f('columns', '网格列数', 'grid-columns', { min: 1, max: 6 })] });
    reg({ type: 'column', name: '列', group: 'layout', icon: 'layui-icon-tabs', desc: '网格内的列容器', container: true, styleScope: SCOPE_BOX, defaults: {}, styleDefaults: { minHeight: '40px' }, inspector: [] });

    reg({ type: 'heading', name: '标题', group: 'basic', icon: 'layui-icon-fonts-strong', desc: 'H1~H4 级标题', styleScope: SCOPE_TEXT, defaults: { text: '请输入标题', level: 2, textWrap: 'auto' }, inspector: [f('text', '标题文字'), f('level', '标题级别', 'select', { options: [{ value: 1, text: 'H1' }, { value: 2, text: 'H2' }, { value: 3, text: 'H3' }, { value: 4, text: 'H4' }] }), textWrapField()] });
    reg({ type: 'text', name: '文本', group: 'basic', icon: 'layui-icon-edit', desc: '普通多行文本，通过字体排版设置外观', styleScope: SCOPE_TEXT, defaults: { text: '请输入文本内容', textWrap: 'auto' }, inspector: [f('text', '文本内容', 'textarea', { rows: 6 }), textWrapField()] });
    reg({ type: 'richText', name: '富文本', group: 'basic', icon: 'layui-icon-fonts-html', desc: '使用 wangEditor 编辑文字、图片与表格', styleScope: [], hideStyle: true, defaults: { html: '<p>请输入富文本内容</p>' }, inspector: [f('html', '富文本内容', 'textarea', { rows: 8, richText: true })] });
    reg({ type: 'image', name: '图片', group: 'basic', icon: 'layui-icon-picture', desc: '单张图片，可设置跳转链接', styleScope: SCOPE_MEDIA, defaults: { src: '', alt: '', link: '' }, inspector: [f('src', '图片', 'image'), f('alt', '替代文本', 'text', { hint: '用于 SEO 与图片加载失败提示' }), f('link', '跳转链接', 'text', { placeholder: '留空表示不可点击' })] });
    reg({ type: 'banner', name: 'Banner', group: 'basic', icon: 'layui-icon-carousel', desc: '多图轮播，2 张及以上自动播放', styleScope: SCOPE_BANNER, defaults: { images: [], height: 420, interval: 5000, showArrows: true, showDots: true, objectFit: 'cover' }, inspector: [f('images', '轮播图片', 'image-list', { hint: '选择 2 张及以上时前台自动轮播' }), f('height', 'Banner 高度(px)', 'number', { min: 120, max: 900, default: 420 }), f('interval', '轮播间隔(ms)', 'number', { min: 1000, max: 30000, step: 500, default: 5000 }), f('objectFit', '图片填充', 'select', { options: [{ value: 'cover', text: '覆盖裁剪' }, { value: 'contain', text: '完整显示' }] }), f('showArrows', '显示左右箭头', 'checkbox'), f('showDots', '显示圆点指示', 'checkbox'), f('title', '主标题'), f('description', '介绍文字', 'textarea'), f('buttonText', '按钮文字'), f('buttonLink', '跳转链接', 'link')] });
    reg({ type: 'button', name: '按钮', group: 'basic', icon: 'layui-icon-link', desc: '行动按钮，支持跳转与打开方式', styleScope: SCOPE_TEXT, defaults: { text: '了解更多', link: { type: 'none' }, target: '_self', variant: 'primary' }, inspector: [f('text', '按钮文字'), f('link', '跳转链接', 'link'), f('target', '打开方式', 'select', { options: [{ value: '_self', text: '当前窗口' }, { value: '_blank', text: '新窗口' }] }), f('variant', '按钮样式', 'select', { options: [{ value: 'primary', text: '主按钮' }, { value: 'outline', text: '描边按钮' }, { value: 'text', text: '文字按钮' }] })] });
    reg({ type: 'icon', name: '图标', group: 'basic', icon: 'layui-icon-star', desc: '单字符图标或符号', styleScope: SCOPE_ICON, defaults: { text: '★', size: 32 }, inspector: [f('text', '图标/字符'), f('size', '尺寸(px)', 'number', { min: 12, max: 160, default: 32 })] });
    reg({ type: 'video', name: '视频', group: 'basic', icon: 'layui-icon-video', desc: 'mp4 等直链视频', styleScope: SCOPE_MEDIA, defaults: { src: '', poster: '', controls: true }, inspector: [f('src', '视频地址', 'text', { placeholder: '如 /upload/demo.mp4' }), f('poster', '封面图片', 'image'), f('controls', '显示播放控件', 'checkbox')] });
    reg({ type: 'divider', name: '分隔线', group: 'basic', icon: 'layui-icon-more', desc: '横向分隔线', styleScope: SCOPE_PLAIN, defaults: {}, styleDefaults: { borderTopWidth: '1px', borderTopStyle: 'solid', borderTopColor: '#e5e7eb', marginTop: '20px', marginBottom: '20px' }, inspector: [] });
    reg({ type: 'spacer', name: '间距', group: 'basic', icon: 'layui-icon-screen-full', desc: '纯占位空白，用于拉开区块距离', styleScope: SCOPE_SPACER, defaults: { height: 40 }, inspector: [f('height', '高度(px)', 'number', { min: 4, max: 400, default: 40 })] });

    reg({ type: 'articleList', name: '文章列表', group: 'data', icon: 'layui-icon-list', desc: '绑定后台新闻数据，支持多种展示样式、分类切换与分页', styleScope: SCOPE_BOX, defaults: { categoryIds: [], showTabs: true, pageSize: 6, columns: 3, showSummary: true, showDate: true, showImage: true, enablePagination: true, layout: 'card', tabStyle: 'pill', tabAlign: 'left' }, inspector: [f('categoryIds', '选择分类', 'categories', { contentType: 'article', allText: '全部文章', hint: '点「选择分类」从新闻分类中勾选；不勾选 = 全部文章，可多选' }), f('showTabs', '显示分类 Tab', 'checkbox', { hint: '选择「全部」或多个分类时，前台列表上方会生成一组可切换的分类 Tab；只选一个分类时不显示' }), tabStyleField(), tabAlignField(), f('layout', '展示样式', 'select', { options: [{ value: 'card', text: '网格卡片' }, { value: 'list', text: '纵向列表' }, { value: 'timeline', text: '时间轴' }, { value: 'editorial', text: '企业新闻（大图首条）' }] }), f('pageSize', '每页数量', 'number', { min: 1, max: 100 }), f('columns', '列数', 'number', { min: 1, max: 6, hint: '仅在网格卡片样式生效' }), f('showSummary', '显示摘要', 'checkbox'), f('showDate', '显示日期', 'checkbox'), f('showImage', '显示封面图', 'checkbox'), f('enablePagination', '开启分页', 'checkbox')] });
    reg({ type: 'productList', name: '产品列表', group: 'data', icon: 'layui-icon-component', desc: '绑定后台产品数据，支持分类切换与分页', styleScope: SCOPE_BOX, defaults: { categoryIds: [], showTabs: true, pageSize: 8, columns: 4, showSummary: false, enablePagination: true, tabStyle: 'pill', tabAlign: 'left' }, inspector: [f('categoryIds', '选择分类', 'categories', { contentType: 'product', allText: '全部产品', hint: '点「选择分类」从产品分类中勾选；不勾选 = 全部产品，可多选' }), f('showTabs', '显示分类 Tab', 'checkbox', { hint: '选择「全部」或多个分类时，列表上方生成可切换的分类 Tab；只选一个分类时不显示' }), tabStyleField(), tabAlignField(), f('pageSize', '每页数量', 'number', { min: 1, max: 50 }), f('columns', '列数', 'number', { min: 1, max: 6 }), f('showSummary', '显示摘要', 'checkbox'), f('enablePagination', '开启分页', 'checkbox')] });
    reg({ type: 'jobList', name: '招聘列表', group: 'data', icon: 'layui-icon-friends', desc: '绑定后台招聘数据，支持分类切换与分页', styleScope: SCOPE_BOX, defaults: { categoryIds: [], showTabs: true, pageSize: 10, showSalary: true, showLocation: true, enablePagination: true, tabStyle: 'pill', tabAlign: 'left' }, inspector: [f('categoryIds', '选择分类', 'categories', { contentType: 'job', allText: '全部招聘', hint: '点「选择分类」从招聘分类中勾选；不勾选 = 全部招聘，可多选' }), f('showTabs', '显示分类 Tab', 'checkbox', { hint: '选择「全部」或多个分类时，列表上方生成可切换的分类 Tab；只选一个分类时不显示' }), tabStyleField(), tabAlignField(), f('pageSize', '每页数量', 'number', { min: 1, max: 50 }), f('showSalary', '显示薪资', 'checkbox'), f('showLocation', '显示地点', 'checkbox'), f('layout', '列表样式', 'select', { options: [{ value: '', text: '标准列表' }, { value: 'compact', text: '简洁岗位列表' }] }), f('enablePagination', '开启分页', 'checkbox')] });

    reg({ type: 'logo', name: 'Logo', group: 'global', icon: 'layui-icon-picture-fine', desc: '站点 Logo 或站点名称', styleScope: SCOPE_TEXT, defaults: { src: '', text: '企业名称', href: '/', logoWidth: 200, logoMaxHeight: 56 }, inspector: [
        f('src', 'Logo', 'image', { hint: '未选择时显示下面的站点名称' }),
        f('text', '站点名称'),
        f('href', '首页链接', 'text', { placeholder: '如 /' }),
        f('logoWidth', 'Logo 宽度(px)', 'number', { min: 24, max: 800, step: 1, default: 200, hint: '图片会保持原始比例，并且不会撑出所在列；需要更宽时可同时拖动画布中的列宽分隔线' }),
        f('logoMaxHeight', 'Logo 最大高度(px)', 'number', { min: 16, max: 240, step: 1, default: 56, hint: '用于限制页头高度；宽度和最大高度会共同约束 Logo' })
    ] });
    reg({ type: 'navigation', name: '导航菜单', group: 'global', icon: 'layui-icon-menu-fill', desc: '取自已发布页面树', styleScope: SCOPE_TEXT, defaults: { direction: 'horizontal', alignment: 'center', itemGap: 16, itemPaddingX: 10, itemPaddingY: 8, itemStyle: 'underline', submenuEffect: 'slide-down', submenuItemEffect: 'background', submenuDuration: 200, submenuAccentColor: '#0054a6' }, inspector: [
        f('direction', '排列方式', 'select', { options: [{ value: 'horizontal', text: '横向' }, { value: 'vertical', text: '纵向' }], hint: '菜单内容来自「页面管理」中已发布且开启导航的页面' }),
        f('alignment', '菜单对齐', 'select', { options: [{ value: 'left', text: '左对齐' }, { value: 'center', text: '居中' }, { value: 'right', text: '右对齐' }] }),
        f('itemGap', '条目间距(px)', 'number', { min: 0, max: 80, step: 1, default: 16, hint: '导航固定单行；单行容纳不下时可减小此值，或调宽导航所在列' }),
        f('itemPaddingX', '条目左右内边距(px)', 'number', { min: 0, max: 40, step: 1, default: 10, hint: '导航固定单行；减小此值可以容纳更多菜单项' }),
        f('itemPaddingY', '条目上下内边距(px)', 'number', { min: 0, max: 24, step: 1, default: 8 }),
        f('itemStyle', '导航栏风格', 'select', { options: NAV_ITEM_STYLE_OPTIONS, hint: '横向主导航条目的悬停/选中样式；强调色跟随下方「强调颜色」' }),
        f('submenuEffect', '二级菜单展开', 'select', { options: [{ value: 'slide-down', text: '下滑淡入' }, { value: 'fade', text: '淡入' }, { value: 'zoom', text: '缩放淡入' }, { value: 'none', text: '无动画' }] }),
        f('submenuItemEffect', '二级条目悬停/选中', 'select', { options: [{ value: 'background', text: '背景高亮' }, { value: 'shift', text: '向右滑动' }, { value: 'underline', text: '底部线条' }, { value: 'left-bar', text: '左侧色条' }, { value: 'none', text: '无特效' }], hint: '鼠标滑过、键盘聚焦和当前页面使用同一种强调效果' }),
        f('submenuDuration', '特效时长(ms)', 'number', { min: 100, max: 1000, step: 50, default: 200 }),
        f('submenuAccentColor', '强调颜色', 'color', { default: '#0054a6' })
    ] });
    reg({ type: 'search', name: '搜索', group: 'global', icon: 'layui-icon-search', desc: '站内搜索框', styleScope: SCOPE_TEXT, defaults: { placeholder: '搜索', action: '/search' }, inspector: [f('placeholder', '提示文字'), f('action', '搜索地址', 'text', { placeholder: '如 /search' })] });
    reg({ type: 'language', name: '语言切换', group: 'global', icon: 'layui-icon-website', desc: '多语言入口占位', styleScope: SCOPE_TEXT, defaults: { text: '中文 / EN' }, inspector: [f('text', '显示文字')] });
    reg({ type: 'contact', name: '联系方式', group: 'global', icon: 'layui-icon-cellphone', desc: '电话 / 邮箱 / 地址', styleScope: SCOPE_TEXT, defaults: { phone: '', email: '', address: '' }, inspector: [f('phone', '电话'), f('email', '邮箱'), f('address', '地址')] });
    reg({ type: 'contactForm', name: '在线留言', group: 'basic', icon: 'layui-icon-form', desc: '可提交并保存到后台的在线留言表单', styleScope: SCOPE_BOX, defaults: { namePlaceholder: '姓名', phonePlaceholder: '联系电话', emailPlaceholder: '电子邮箱', messagePlaceholder: '留言内容', buttonText: '提交留言', action: '/home/message', showCaptcha: true }, inspector: [f('namePlaceholder', '姓名提示'), f('phonePlaceholder', '电话提示'), f('emailPlaceholder', '邮箱提示'), f('messagePlaceholder', '留言提示'), f('buttonText', '按钮文字'), f('showCaptcha', '显示验证码', 'checkbox'), f('action', '提交地址', 'text', { placeholder: '默认 /home/message', hint: '使用默认地址时，留言会写入后台「在线留言」' })] });
    reg({ type: 'social', name: '社交链接', group: 'global', icon: 'layui-icon-share', desc: '社交账号入口', styleScope: SCOPE_TEXT, defaults: { text: '关注我们', links: '' }, inspector: [f('text', '标题'), f('links', '链接配置', 'textarea', { hint: '每行一个，格式：名称|链接' })] });
    reg({ type: 'copyright', name: '版权信息', group: 'global', icon: 'layui-icon-auz', desc: '页脚版权文字', styleScope: SCOPE_TEXT, defaults: { text: '© 2026 企业名称 版权所有' }, inspector: [f('text', '版权文字', 'textarea')] });

    // Link and Banner child effects share the same inspector controls.
    ['button', 'banner'].forEach(function (type) {
        var def = R.get(type), key = type === 'banner' ? 'buttonLink' : 'link';
        def.defaults[key] = { type: 'none' };
        if (type === 'banner') {
            ['title', 'description', 'button'].forEach(function (part, index) {
                def.defaults[part + 'Effects'] = {};
                def.inspector.push(f(part + 'Effects', ['主标题特效', '介绍文字特效', '按钮特效'][index], 'effects', { target: part }));
            });
            def.defaults.buttonTarget = '_self';
            def.inspector.push(f('buttonTarget', '按钮打开方式', 'select', { options: [{value:'_self',text:'当前窗口'},{value:'_blank',text:'新窗口'}] }));
        }
    });

    // 装修富文本的安全转换：与服务端 RichTextSanitizer 保持一致。
    // wangEditor 实例的创建和销毁在 richtext-editor.js 中处理。
    (function () {
        var allowed = { p:1, br:1, strong:1, b:1, em:1, i:1, u:1, s:1, strike:1, ul:1, ol:1, li:1, blockquote:1, h1:1, h2:1, h3:1, h4:1, a:1, span:1, table:1, thead:1, tbody:1, tr:1, th:1, td:1, img:1 };
        var blocked = { script:1, style:1, iframe:1, object:1, embed:1, svg:1, math:1, form:1, input:1, button:1, textarea:1, select:1, option:1 };
        var colors = { '#d92d20':'sb-rte-color-red', '#1677ff':'sb-rte-color-blue', '#15803d':'sb-rte-color-green' };
        var sizes = { '2':'sb-rte-size-sm', '3':'sb-rte-size-md', '5':'sb-rte-size-lg', '13px':'sb-rte-size-sm', '16px':'sb-rte-size-md', '24px':'sb-rte-size-lg' };
        var alignments = { left:'sb-rte-align-left', center:'sb-rte-align-center', right:'sb-rte-align-right', justify:'sb-rte-align-justify' };
        var alignable = { p:1, h1:1, h2:1, h3:1, h4:1, blockquote:1, li:1, th:1, td:1 };
        var inlineClasses = { 'sb-rte-size-sm':1, 'sb-rte-size-md':1, 'sb-rte-size-lg':1, 'sb-rte-color-red':1, 'sb-rte-color-blue':1, 'sb-rte-color-green':1 };
        var blockClasses = { 'sb-rte-align-left':1, 'sb-rte-align-center':1, 'sb-rte-align-right':1, 'sb-rte-align-justify':1 };

        function colorClass(value) {
            var color = String(value || '').toLowerCase().replace(/\s+/g, '');
            var rgb = /^rgb\((\d{1,3}),(\d{1,3}),(\d{1,3})\)$/.exec(color);
            if (rgb) {
                var channels = rgb.slice(1).map(Number);
                if (channels.some(function (channel) { return channel > 255; })) return '';
                color = '#' + channels.map(function (channel) { return channel.toString(16).padStart(2, '0'); }).join('');
            }
            return colors[color] || '';
        }

        function safeClasses(source, tag) {
            var inline = tag === 'span' || tag === 'font';
            var permitted = inline ? inlineClasses : alignable[tag] ? blockClasses : {};
            var classes = String(source.getAttribute('class') || '').split(/\s+/).filter(function (name) { return permitted[name]; });
            if (inline) {
                var color = colorClass(source.getAttribute('color') || source.style.color);
                var size = sizes[String(source.getAttribute('size') || source.style.fontSize || '').toLowerCase()] || '';
                if (color) classes = classes.filter(function (name) { return name.indexOf('sb-rte-color-') !== 0; }).concat(color);
                if (size) classes = classes.filter(function (name) { return name.indexOf('sb-rte-size-') !== 0; }).concat(size);
            } else if (alignable[tag]) {
                var alignment = alignments[String(source.style.textAlign || '').toLowerCase()];
                if (alignment) classes = classes.filter(function (name) { return name.indexOf('sb-rte-align-') !== 0; }).concat(alignment);
            }
            return classes.filter(function (name, index) { return classes.indexOf(name) === index; });
        }

        function safeHref(value) {
            var text = String(value == null ? '' : value).trim();
            if (!text || /^\/\//.test(text)) return '';
            if (/^(#|\/|\.\/|\.\.\/)/.test(text)) return text;
            if (/^(https?:|mailto:|tel:)/i.test(text)) return text;
            return '';
        }

        function safeImageSrc(value) {
            var text = String(value == null ? '' : value).trim();
            return text && !/^\/\//.test(text) && (/^\/(?!\/)/.test(text) || /^\.\.?\//.test(text) || /^https?:\/\//i.test(text)) ? text : '';
        }

        function sanitize(value) {
            value = String(value == null ? '' : value);
            if (!value.trim()) return '';

            var template = document.createElement('template');
            template.innerHTML = value;
            var output = document.createElement('div');

            function appendNode(source, target) {
                if (source.nodeType === 3) {
                    target.appendChild(document.createTextNode(source.nodeValue || ''));
                    return;
                }
                if (source.nodeType !== 1) return;

                var tag = String(source.tagName || '').toLowerCase();
                if (blocked[tag]) return;

                if (!allowed[tag] && tag !== 'font') {
                    Array.prototype.slice.call(source.childNodes || []).forEach(function (child) { appendNode(child, target); });
                    return;
                }

                var classes = safeClasses(source, tag);
                if ((tag === 'span' || tag === 'font') && !classes.length) {
                    Array.prototype.slice.call(source.childNodes || []).forEach(function (child) { appendNode(child, target); });
                    return;
                }
                var element = document.createElement(tag === 'font' ? 'span' : tag);
                if (classes.length) element.setAttribute('class', classes.join(' '));
                if (tag === 'img') {
                    var src = safeImageSrc(source.getAttribute('src'));
                    if (!src) return;
                    element.setAttribute('src', src);
                    element.setAttribute('alt', source.getAttribute('alt') || '');
                    target.appendChild(element);
                    return;
                }
                if (tag === 'td' || tag === 'th') {
                    ['colspan', 'rowspan'].forEach(function (name) {
                        var count = Number(source.getAttribute(name));
                        if (Number.isInteger(count) && count >= 2 && count <= 12) element.setAttribute(name, String(count));
                    });
                }
                if (tag === 'a') {
                    var href = safeHref(source.getAttribute('href'));
                    if (href) element.setAttribute('href', href);
                    if (source.getAttribute('target') === '_blank') {
                        element.setAttribute('target', '_blank');
                        element.setAttribute('rel', 'noopener noreferrer');
                    }
                }
                Array.prototype.slice.call(source.childNodes || []).forEach(function (child) { appendNode(child, element); });
                target.appendChild(element);
            }

            Array.prototype.slice.call(template.content.childNodes || []).forEach(function (child) { appendNode(child, output); });
            return output.innerHTML;
        }

        root.RichText = {
            sanitize: sanitize,
            safeHref: safeHref,
            safeImageSrc: safeImageSrc
        };

        // 富文本编辑器实例由 richtext-editor.js 在属性面板渲染后创建。
    })();
})(window);
