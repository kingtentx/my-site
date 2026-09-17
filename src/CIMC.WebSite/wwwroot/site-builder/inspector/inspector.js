(function (window, $) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;
    var esc = root.DesignerRenderer.escapeHtml;

    // 样式字段按「能力令牌」声明归属：组件在 default-components.js 里用 styleScope 声明自己支持哪些令牌，
    // 面板据此裁剪，避免在文本组件上出现「图片填充」、在非容器组件上出现「子项间距」这类无意义项。
    var styleGroups = [
        { key:'layout', title:'背景与尺寸', fields:[
            { key:'backgroundColor', label:'背景色', type:'color', scope:'background' },
            { key:'backgroundImage', label:'背景图片', type:'image', scope:'background' },
            { key:'backgroundOverlay', label:'背景遮罩', type:'number', min:0, max:1, step:0.05, scope:'background', hint:'0~1，数值越大遮罩越深' },
            { key:'maxWidth', label:'最大宽度', type:'length', units:['px','%','vw','auto'], scope:'size' },
            { key:'minHeight', label:'最小高度', type:'length', units:['px','vh','auto'], scope:'size' },
            { key:'height', label:'固定高度', type:'length', units:['px','vh','auto'], scope:'size' },
            { key:'objectFit', label:'图片填充', type:'select', options:[{value:'',text:'默认'},{value:'cover',text:'裁剪铺满'},{value:'contain',text:'完整显示'}], scope:'fit' },
            { key:'textAlign', label:'内容对齐', type:'select', options:[{value:'',text:'默认'},{value:'left',text:'左对齐'},{value:'center',text:'居中'},{value:'right',text:'右对齐'}], scope:'align' },
            { key:'color', label:'文字颜色', type:'color', scope:'color' },
            { key:'alignItems', label:'列的垂直对齐', type:'select', options:[{value:'',text:'默认（拉伸）'},{value:'start',text:'顶部对齐'},{value:'center',text:'垂直居中'},{value:'end',text:'底部对齐'}], scope:'grid' }
        ]},
        { key:'typography', title:'字体排版', fields:[
            { key:'fontSize', label:'字号', type:'length', units:['px','rem','em'], scope:'typography' },
            { key:'fontWeight', label:'字重', type:'select', options:[{value:'',text:'默认'},{value:'400',text:'常规'},{value:'500',text:'中等'},{value:'600',text:'半粗'},{value:'700',text:'加粗'}], scope:'typography' },
            { key:'lineHeight', label:'行高', type:'text', placeholder:'如 1.8 或 24px', scope:'typography' },
            { key:'letterSpacing', label:'字间距', type:'length', units:['px','em'], scope:'typography' }
        ]},
        { key:'spacing', title:'间距', fields:[
            { key:'paddingTop', label:'上内边距', type:'length', scope:'spacing' },
            { key:'paddingRight', label:'右内边距', type:'length', scope:'spacing' },
            { key:'paddingBottom', label:'下内边距', type:'length', scope:'spacing' },
            { key:'paddingLeft', label:'左内边距', type:'length', scope:'spacing' },
            { key:'marginTop', label:'上外边距', type:'length', scope:'spacing' },
            { key:'marginRight', label:'右外边距', type:'length', scope:'spacing' },
            { key:'marginBottom', label:'下外边距', type:'length', scope:'spacing' },
            { key:'marginLeft', label:'左外边距', type:'length', scope:'spacing' },
            { key:'gap', label:'子项间距', type:'length', scope:'gap', hint:'作用于网格列之间' }
        ]},
        { key:'appearance', title:'边框与阴影', fields:[
            { key:'borderWidth', label:'边框宽度', type:'length', units:['px'], scope:'border', hint:'设置为 0 或留空时不显示边框' },
            { key:'borderStyle', label:'边框样式', type:'select', options:[{value:'',text:'默认'},{value:'solid',text:'实线'},{value:'dashed',text:'虚线'},{value:'dotted',text:'点线'},{value:'double',text:'双线'},{value:'none',text:'无边框'}], scope:'border' },
            { key:'borderColor', label:'边框颜色', type:'color', scope:'border' },
            { key:'borderRadius', label:'圆角', type:'length', units:['px','%'], scope:'border' },
            { key:'boxShadow', label:'阴影', type:'text', placeholder:'如 0 2px 8px rgba(0,0,0,.08)', scope:'border' }
        ]},
        { key:'effects', title:'动画与特效', fields:[
            {key:'effectEntrance',label:'滚动入场',type:'select',options:[{value:'',text:'无动画'},{value:'fade',text:'淡入'},{value:'fade-up',text:'上移淡入'},{value:'slide-left',text:'从左滑入'},{value:'slide-right',text:'从右滑入'},{value:'zoom',text:'缩放入场'}]},
            {key:'effectHover',label:'悬停效果',type:'select',options:[{value:'',text:'无特效'},{value:'lift',text:'轻微上浮'},{value:'zoom',text:'轻微放大'},{value:'shadow',text:'浮起阴影'}]},
            {key:'effectDuration',label:'时长（毫秒）',type:'number',min:100,max:3000,step:100,placeholder:'默认 600'},
            {key:'effectDelay',label:'延迟（毫秒）',type:'number',min:0,max:3000,step:100,placeholder:'默认 0'},
            {key:'effectDistance',label:'位移（px）',type:'number',min:0,max:100,placeholder:'默认 24'},
            {key:'effectEasing',label:'播放节奏',type:'select',options:[{value:'',text:'默认（自然减速）'},{value:'ease-out',text:'自然减速'},{value:'ease-in-out',text:'平缓起止'},{value:'linear',text:'匀速'}]},
            {key:'effectRepeat',label:'重复入场',type:'checkbox',hint:'开启后，每次滚出再滚入屏幕时播放；默认仅播放一次。'},
            {key:'effectCounter',label:'数字递增',type:'checkbox',scope:'typography',hint:'适合“25万”“33.41万㎡”等以数字开头的标题或文字。'}
        ]},
        { key:'advanced', title:'高级定位', fields:[
            { key:'position', label:'定位方式', type:'select', options:[{value:'',text:'默认'},{value:'relative',text:'相对定位'},{value:'static',text:'普通流'},{value:'sticky',text:'吸顶（随页面滚动固定）'},{value:'fixed',text:'固定'}], scope:'position' },
            { key:'top', label:'顶部距离', type:'length', units:['px','vh'], scope:'position' },
            { key:'zIndex', label:'层级', type:'number', min:0, max:99999, scope:'position' }
        ]}
    ];

    var ALL_SCOPES = ['background','size','fit','align','color','grid','typography','spacing','gap','border','position'];

    function fieldDef(key, label, type, extra) { var x = { key:key, label:label, type:type || 'text' }; if (extra) Object.keys(extra).forEach(function (k) { x[k] = extra[k]; }); return x; }
    function attr(value) { return esc(value == null ? '' : value); }
    function scopesOf(node) {
        var def = node && Registry.get(node.type);
        var list = def && def.styleScope;
        return Array.isArray(list) && list.length ? list : ALL_SCOPES;
    }
    function fieldApplies(field, scopes) { return !field.scope || scopes.indexOf(field.scope) >= 0; }

    function normalizeImageList(value) {
        if (Array.isArray(value)) return value.filter(function (x) { return !!x; });
        if (typeof value === 'string' && value.trim()) {
            try {
                var parsed = JSON.parse(value);
                if (Array.isArray(parsed)) return parsed.filter(function (x) { return !!x; });
            } catch (e) { }
        }
        return [];
    }

    // ── 分类 ID 归一化：兼容数组 / JSON 字符串 / 「1,2」/ 单个数字 ──────────────
    function normalizeCategoryIds(value) {
        var result = [];
        function push(item) {
            var id = Number(item);
            if (isFinite(id) && id > 0 && result.indexOf(Math.round(id)) < 0) result.push(Math.round(id));
        }
        if (Array.isArray(value)) { value.forEach(push); return result; }
        var text = String(value == null ? '' : value).trim();
        if (!text) return result;
        if (text.charAt(0) === '[') {
            try {
                var parsed = JSON.parse(text);
                if (Array.isArray(parsed)) { parsed.forEach(push); return result; }
            } catch (e) { /* 非法 JSON 时退回按逗号分隔解析 */ }
        }
        text.split(/[,，;；\s]+/).forEach(push);
        return result;
    }

    // ── CSS 长度值：文本输入 + 单位下拉，避免用户手打「24px」写错格式 ───────────────
    var UNIT_KEYWORDS = ['auto', 'none', 'inherit', 'initial', 'unset'];
    var CUSTOM_UNIT = 'custom';
    function parseLength(value, defaultUnit) {
        var text = String(value == null ? '' : value).trim();
        var matched = /^(-?\d+(?:\.\d+)?)\s*(px|%|rem|em|vh|vw|pt|auto|none)?$/i.exec(text);
        if (matched) {
            var unit = (matched[2] || defaultUnit || 'px').toLowerCase();
            if (UNIT_KEYWORDS.indexOf(unit) >= 0) return { number:'', unit:unit };
            return { number: matched[1], unit: unit };
        }
        // calc()/min()/… 等自定义表达式原样保留，避免读取时把它改写成非法值
        if (text) return { number: text, unit: CUSTOM_UNIT };
        return { number:'', unit: defaultUnit || 'px' };
    }
    function composeLength(raw, unit) {
        var text = String(raw == null ? '' : raw).trim();
        var normalizedUnit = String(unit || 'px').toLowerCase();
        if (normalizedUnit === CUSTOM_UNIT) return text;
        if (!text) return UNIT_KEYWORDS.indexOf(normalizedUnit) >= 0 ? normalizedUnit : '';
        if (UNIT_KEYWORDS.indexOf(text.toLowerCase()) >= 0) return text.toLowerCase();
        if (/^-?\d+(?:\.\d+)?$/.test(text)) return text + normalizedUnit;
        if (/^-?\d+(\.\d+)?\s*(px|%|rem|em|vh|vw|pt)$/i.test(text)) return text.replace(/\s+/g, '');
        return null;                                     // 非法值：交给调用方拦截，不写入文档
    }

    function renderLength(field, value, area) {
        var units = field.units || ['px', '%'];
        var parsed = parseLength(value, units[0]);
        var options = units.slice();
        if (parsed.unit === CUSTOM_UNIT) options.push(CUSTOM_UNIT);
        else if (units.indexOf(parsed.unit) < 0) options.push(parsed.unit);
        var html = '<div class="sb-length-control">'
            + '<input class="layui-input sb-length-value" type="text" data-area="' + area + '" data-key="' + attr(field.key) + '" value="' + attr(parsed.number) + '" placeholder="留空=默认">'
            + '<select class="sb-length-unit" data-area="' + area + '" data-key="' + attr(field.key) + '" data-unit-select="1">';
        options.forEach(function (unit) {
            html += '<option value="' + attr(unit) + '"' + (unit === parsed.unit ? ' selected' : '') + '>' + esc(unit === CUSTOM_UNIT ? '自定义' : unit) + '</option>';
        });
        html += '</select></div>';
        return html;
    }

    function renderImageList(field, value, area) {
        var images = normalizeImageList(value);
        var html = '<div class="sb-image-list-field">'
            + '<div class="sb-image-list-toolbar">'
            + '<button type="button" class="sb-image-pick-btn" data-action="pick-images" data-area="' + area + '" data-key="' + attr(field.key) + '">从素材库选择多图</button>';
        if (images.length) {
            html += '<button type="button" class="sb-image-list-clear" data-action="clear-list-images" data-area="' + area + '" data-key="' + attr(field.key) + '">清空</button>';
        }
        html += '<span class="sb-image-list-count">已选 ' + images.length + ' 张</span></div>';

        if (!images.length) {
            html += '<div class="sb-image-list-empty">未选择图片。选择 2 张及以上时，前台会自动轮播。</div>';
        } else {
            html += '<div class="sb-image-list">';
            images.forEach(function (url, index) {
                html += '<div class="sb-image-list-item">'
                    + '<img src="' + attr(url) + '" alt="Banner ' + (index + 1) + '">'
                    + '<div class="sb-image-list-meta"><span>' + (index + 1) + '</span><div>'
                    + '<button type="button" data-action="move-list-image" data-direction="up" data-index="' + index + '" data-area="' + area + '" data-key="' + attr(field.key) + '"' + (index === 0 ? ' disabled' : '') + '>↑</button>'
                    + '<button type="button" data-action="move-list-image" data-direction="down" data-index="' + index + '" data-area="' + area + '" data-key="' + attr(field.key) + '"' + (index === images.length - 1 ? ' disabled' : '') + '>↓</button>'
                    + '<button type="button" data-action="remove-list-image" data-index="' + index + '" data-area="' + area + '" data-key="' + attr(field.key) + '">删除</button>'
                    + '</div></div></div>';
            });
            html += '</div>';
        }
        return html + '</div>';
    }

    function gridWidthsOf(node, count) {
        var raw = node && node.props ? node.props.columnWidths : null;
        if (typeof raw === 'string') {
            try { raw = JSON.parse(raw); } catch (e) { raw = raw.split(/[,，;；\s]+/); }
        }
        var widths = Array.isArray(raw) ? raw.map(Number).filter(function (x) { return isFinite(x) && x > 0; }) : [];
        if (widths.length !== count) widths = Array.apply(null, {length:count}).map(function () { return 100 / count; });
        var total = widths.reduce(function (sum, x) { return sum + x; }, 0) || 100;
        return widths.map(function (x) { return x / total * 100; });
    }
    function sameGridWidths(left, right) {
        return left.length === right.length && left.every(function (value, index) { return Math.abs(value - right[index]) < 0.25; });
    }
    function gridWidthLabel(value) {
        var rounded = Math.round(value * 10) / 10;
        return String(rounded).replace(/\.0$/, '');
    }
    function selectedOptionClass(selected) { return selected ? ' class="active" aria-pressed="true"' : ' aria-pressed="false"'; }
    function optionCheck() { return '<span class="sb-option-check" aria-hidden="true">✓</span>'; }

    function renderGridColumns(field, value, area, node) {
        var count = Math.max(1, Math.min(6, Math.round(Number(value || 2))));
        var currentWidths = gridWidthsOf(node, count);
        var equalWidths = Array.apply(null, {length:count}).map(function () { return 100 / count; });
        var isEqual = sameGridWidths(currentWidths, equalWidths);
        var currentPreset = false;
        var html = '<div class="sb-grid-column-control">'
            + '<input class="layui-input sb-grid-column-number" type="number" data-area="' + area + '" data-key="' + attr(field.key) + '" value="' + count + '" min="1" max="6">'
            + '<div class="sb-grid-column-buttons">';
        for (var i = 1; i <= 6; i++) {
            html += '<button type="button" data-action="set-grid-columns" data-columns="' + i + '"' + selectedOptionClass(i === count) + '>' + optionCheck() + i + '列</button>';
        }
        html += '</div>';
        if (count === 2 || count === 3) {
            html += '<div class="sb-grid-option-label">常用列宽比例</div><div class="sb-grid-column-tools sb-ratio-presets">';
            (count === 2 ? [[50,50],[30,70],[70,30]] : [[25,50,25],[20,60,20],[33.3,33.4,33.3]]).forEach(function(widths){
                var selected = sameGridWidths(currentWidths, widths);
                if (selected) currentPreset = true;
                html += '<button type="button" data-action="set-grid-ratio" data-widths="' + widths.join(',') + '"' + selectedOptionClass(selected) + '>' + optionCheck() + widths.join(' / ') + '</button>';
            });
            html += '</div>';
        }
        html += '<div class="sb-grid-current-ratio"><span>当前列宽</span><strong>' + esc(currentWidths.map(gridWidthLabel).join(' / ')) + '</strong>' + (currentPreset ? '' : '<em>' + (isEqual ? '平均' : '自定义') + '</em>') + '</div>'
            + '<div class="sb-grid-column-tools"><button type="button" class="sb-grid-equal-action" data-action="equal-grid-columns">平均分配列宽</button></div>'
            + '<div class="sb-grid-column-tip">可直接选择 1~6 列；在画布中拖动列之间的蓝色分隔线，可自由调整每列宽度。减少列数时，原列内容会合并到相邻列。</div></div>';
        return html;
    }

    // ── 分类选择器 ────────────────────────────────────────────────────────────
    // 数据来源：后台 /contentcategory/getoptions（文章 / 产品 / 招聘共用一套分类树）。
    // 两种形态：
    //   1. select[data-category-type]  单选下拉（保留给旧组件）；
    //   2. .sb-category-field          多选，点「选择分类」弹窗勾选，值写成 id 数组。
    var categoryCache = {};

    function renderCategory(field, value, area) {
        var contentType = field.contentType || 'article';
        var selected = String(value == null ? 0 : value);
        return '<select lay-ignore data-area="' + area + '" data-key="' + attr(field.key) + '" data-category-type="' + attr(contentType) + '" data-category-value="' + attr(selected) + '">'
            + '<option value="0">' + esc(field.allText || '全部分类') + '</option>'
            + '</select>';
    }

    /** 多选分类：显示已选摘要 + 「选择分类」按钮，点按钮弹窗勾选。 */
    function renderCategories(field, value, area) {
        var contentType = field.contentType || 'article';
        var ids = normalizeCategoryIds(value);
        var html = '<div class="sb-category-field" data-category-type="' + attr(contentType) + '" data-category-value="' + attr(ids.join(',')) + '">'
            + '<div class="sb-category-toolbar">'
            + '<button type="button" class="sb-category-pick-btn" data-action="pick-categories" data-area="' + area + '" data-key="' + attr(field.key) + '" data-category-type="' + attr(contentType) + '">选择分类</button>'
            + '<span class="sb-category-count"></span>';
        if (ids.length) {
            html += '<button type="button" class="sb-category-clear" data-action="clear-categories" data-area="' + area + '" data-key="' + attr(field.key) + '">清空</button>';
        }
        html += '</div><div class="sb-category-names"></div>'
            + '<div class="sb-field-hint sb-category-tip">分类来自后台数据库；不勾选表示显示全部，可同时选择多个分类。</div>'
            + '</div>';
        return html;
    }

    function categoryNameOf(contentType, id) {
        var data = categoryCache[contentType];
        if (!data || !data.options) return '';
        var name = '';
        (data.options || []).forEach(function (opt) {
            if (Number(opt.value) === Number(id)) name = String(opt.text || '').replace(/^[\s　]+/, '');
        });
        return name;
    }

    /** 分类树加载完成后回填「已选 N 个 / 分类名称」，未加载时只显示条数。 */
    function refreshCategoryFields() {
        $('#propsPanel .sb-category-field').each(function () {
            var $field = $(this);
            var contentType = $field.attr('data-category-type');
            var ids = normalizeCategoryIds($field.attr('data-category-value'));
            var data = categoryCache[contentType];
            var allText = (data && data.allText) || '全部';
            $field.find('.sb-category-count').text(ids.length ? ('已选 ' + ids.length + ' 个分类') : allText);
            if (!data) { $field.find('.sb-category-names').text(''); return; }
            var names = ids.map(function (id) { return categoryNameOf(contentType, id) || ('#' + id); });
            $field.find('.sb-category-names').text(names.length ? names.join('、') : '');
        });
    }

    function fillCategoryOptions(contentType, data) {
        var allText = (data && data.allText) || '全部分类';
        var options = (data && data.options) || [];
        $('#propsPanel select[data-category-type="' + contentType + '"]').each(function () {
            var $sel = $(this);
            var current = String($sel.attr('data-category-value') || $sel.val() || '0');
            var html = '<option value="0">' + esc(allText) + '</option>';
            options.forEach(function (opt) {
                html += '<option value="' + attr(opt.value) + '">' + esc(opt.text) + '</option>';
            });
            $sel.html(html).val(current);
        });
    }

    /**
     * 拉取分类树并回填。
     * onLoaded 只在真正发起网络请求并拿到数据时回调一次（后续走缓存不再触发），
     * 调用方可借此重绘画布，让设计器预览里的分类 Tab 显示真实名称。
     */
    function populateCategories(onLoaded) {
        var $targets = $('#propsPanel select[data-category-type], #propsPanel .sb-category-field[data-category-type]');
        if (!$targets.length) return;
        var types = {};
        $targets.each(function () { types[$(this).attr('data-category-type')] = true; });
        Object.keys(types).forEach(function (contentType) {
            if (categoryCache[contentType]) { fillCategoryOptions(contentType, categoryCache[contentType]); refreshCategoryFields(); return; }
            $.get('/contentcategory/getoptions', { contentType: contentType }).done(function (res) {
                var code = res ? Number(res.code) : -1;
                if (code !== 0 && code !== 200) return;   // 后台成功码是 200，兼容 0
                categoryCache[contentType] = res.data || {};
                root.CategoryData = root.CategoryData || {};
                root.CategoryData[contentType] = res.data || {};
                fillCategoryOptions(contentType, categoryCache[contentType]);
                refreshCategoryFields();
                if (typeof onLoaded === 'function') onLoaded(contentType);
            }).fail(function () {
                $('#propsPanel select[data-category-type="' + contentType + '"]').append('<option value="" disabled>分类加载失败</option>');
            });
        });
    }

    function renderField(field, value, area, node) {
        var key = field.key, type = field.type || 'text';
        var blockTypes = ['image-list', 'grid-columns', 'categories'];
        var canReset = !!node && area !== 'node' && blockTypes.indexOf(type) < 0;
        var wrap = canReset;                       // 需要重置按钮时才套一层行容器，保证按钮与控件同行且不遮挡输入
        var html = '<div class="layui-form-item"><label class="layui-form-label" for="field-' + area + '-' + key + '">' + esc(field.label || key) + '</label><div class="layui-input-block">'
            + (wrap ? '<div class="sb-field-row' + (type === 'textarea' ? ' is-tall' : '') + '">' : '');
        if (type === 'textarea') {
            html += '<textarea class="layui-textarea" data-area="' + area + '" data-key="' + key + '" rows="' + (field.rows || 3) + '" placeholder="' + attr(field.placeholder || '') + '">' + esc(value || '') + '</textarea>';
        } else if (type === 'select') {
            html += '<select lay-ignore data-area="' + area + '" data-key="' + key + '">';
            (field.options || []).forEach(function (item) { html += '<option value="' + attr(item.value) + '"' + (String(item.value) === String(value == null ? '' : value) ? ' selected' : '') + '>' + esc(item.text) + '</option>'; });
            html += '</select>';
        } else if (type === 'checkbox') {
            html += '<label class="sb-checkbox-row"><input type="checkbox" lay-ignore data-area="' + area + '" data-key="' + key + '"' + (value ? ' checked' : '') + '><span>开启</span></label>';
        } else if (type === 'color') {
            html += '<div class="sb-color-control"><input type="color" aria-label="选择' + attr(field.label) + '" data-area="' + area + '" data-key="' + key + '" value="' + attr(/^#[0-9a-f]{6}$/i.test(value || '') ? value : '#ffffff') + '"><input type="text" class="layui-input" data-area="' + area + '" data-key="' + key + '" value="' + attr(value || '') + '" placeholder="留空=默认 / #ffffff"></div>';
        } else if (type === 'image') {
            html += '<div class="sb-image-input-row">'
                + '<input class="layui-input" type="text" data-area="' + area + '" data-key="' + key + '" value="' + attr(value == null ? '' : value) + '" placeholder="图片地址或从素材库选择">'
                + '<button type="button" class="sb-image-pick-btn" data-action="pick-image">素材库</button>'
                + '</div>';
            if (value) html += '<div class="sb-image-preview"><img src="' + attr(value) + '" alt="预览"></div>';
        } else if (type === 'image-list') {
            html += renderImageList(field, value, area);
        } else if (type === 'grid-columns') {
            html += renderGridColumns(field, value, area, node);
        } else if (type === 'category') {
            html += renderCategory(field, value, area);
        } else if (type === 'categories') {
            html += renderCategories(field, value, area);
        } else if (type === 'length') {
            html += renderLength(field, value, area);
        } else {
            var step = field.step != null ? ' step="' + field.step + '"' : '';
            html += '<input class="layui-input" type="' + (type === 'number' ? 'number' : 'text') + '" data-area="' + area + '" data-key="' + key + '" value="' + attr(value == null ? '' : value) + '" placeholder="' + attr(field.placeholder || '') + '"' + (field.min != null ? ' min="' + field.min + '"' : '') + (field.max != null ? ' max="' + field.max + '"' : '') + step + '>';
        }
        html = html.replace(/<(input|textarea|select)\b/g, (function () {
            var seq = 0;
            return function (matched, tag) { seq++; return '<' + tag + ' id="field-' + area + '-' + key + (seq > 1 ? '-' + seq : '') + '"'; };
        })());
        if (canReset) html += '<button type="button" class="sb-field-reset" data-action="reset-field" data-area="' + area + '" data-key="' + attr(key) + '" title="恢复默认值">↺</button>';
        if (wrap) html += '</div>';
        html += '</div></div>';
        if (field.hint) html += '<div class="sb-field-hint">' + esc(field.hint) + '</div>';
        return html;
    }

    function nodePath(nodes, nodeId) {
        var trail = [];
        function visit(items, ancestors) {
            (items || []).some(function (item) {
                var current = ancestors.concat([item]);
                if (item.id === nodeId) { trail = current; return true; }
                return visit(item.children, current);
            });
            return trail.length > 0;
        }
        visit(nodes || [], []);
        return trail;
    }

    function section(title, content, open, modifier, tools) {
        return '<details class="props-fold ' + (modifier || '') + '"' + (open ? ' open' : '') + '><summary><span class="props-fold-title">' + esc(title) + '</span>'
            + (tools || '') + '<i class="layui-icon layui-icon-down"></i></summary><div class="props-fold-body">' + content + '</div></details>';
    }

    function renderStyleGroups(node, contentExists) {
        var scopes = scopesOf(node);
        var style = node.style || {};
        var html = '';
        var first = true;
        styleGroups.forEach(function (group) {
            var fields = group.fields.filter(function (field) { return fieldApplies(field, scopes); });
            if (!fields.length) return;
            var body = fields.map(function (field) { return renderField(field, style[field.key], 'style', node); }).join('');
            if (group.key === 'effects') body += '<button type="button" class="sb-effects-preview" data-action="preview-effects">预览特效</button><div class="sb-effects-note">时长、延迟、位移和节奏用于滚动入场；悬停采用轻量过渡。系统启用“减少动态效果”时自动停用动画。</div>';
            if (group.key === 'spacing') {
                body = '<div class="props-quick-spacing"><span>快速设置</span><button type="button" data-action="set-spacing" data-value="12px">紧凑</button><button type="button" data-action="set-spacing" data-value="24px">舒适</button><button type="button" data-action="set-spacing" data-value="48px">宽松</button></div>' + body;
            }
            html += section(group.title, body, !contentExists && first, 'props-style-group props-style-' + group.key);
            first = false;
        });
        if (!html) html = '<div class="props-hint">该组件没有可调整的样式项。</div>';
        return html;
    }

    function styleSummary(node) {
        var style = node.style || {};
        var count = 0;
        styleGroups.forEach(function (group) { group.fields.forEach(function (field) { if (style[field.key] !== undefined && style[field.key] !== null && style[field.key] !== '') count++; }); });
        return count ? ('已设置 ' + count + ' 项') : '未设置样式';
    }

    function resetValue(node, area, key) {
        var def = (node && Registry.get(node.type)) || {};
        var source = area === 'style' ? (def.styleDefaults || {}) : (def.defaults || {});
        if (!Object.prototype.hasOwnProperty.call(source, key)) return '';
        var value = source[key];
        return value && typeof value === 'object' ? Registry.clone(value) : value;
    }

    function render(node, documentModel) {
        if (!node) return '<div class="props-empty"><strong>开始装修页面</strong>从左侧拖入组件或组合预设<br>点击画布内容，在这里修改配置<br>按住组件拖动即可调整位置</div><div class="props-hint">Ctrl+S 保存 · Ctrl+Z 撤销<br>Ctrl+D 复制 · Delete 删除<br>嵌套布局可从左侧「页面结构」选择</div>';
        var def = Registry.get(node.type);
        if (!def) return '<div class="props-empty">未知组件：' + esc(node.type) + '</div>';
        var path = nodePath(documentModel && documentModel.nodes, node.id);
        var breadcrumb = path.map(function (item, index) {
            var itemDef = Registry.get(item.type) || { name:item.type };
            return '<button type="button" data-action="select-node" data-node-id="' + attr(item.id) + '"' + (index === path.length - 1 ? ' class="is-current"' : '') + '>' + esc(item.name || itemDef.name) + '</button>';
        }).join('<span>›</span>');
        var isContainer = !!(def.container);
        var general = renderField(fieldDef('name', '组件名称', 'text', { hint: '仅用于后台管理，前台不显示' }), node.name, 'node', node)
            + renderField(fieldDef('visible', isContainer ? '是否显示内容' : '是否显示', 'checkbox'), node.visible !== false, 'node', node)
            + renderField(fieldDef('locked', '锁定组件', 'checkbox', { hint: '锁定后不能拖动或修改其属性' }), node.locked === true, 'node', node);
        var contentFields = (def.inspector || []).filter(function (field) { return field && field.key; });
        var contentExists = contentFields.length > 0;
        var location = root.Tree.locate(documentModel && documentModel.nodes, node.id);
        var blocked = path.some(function(item){return item.locked;}) || node.type === 'column';
        var html = '<div class="props-title"><div><strong>' + esc(def.name) + '</strong><small>' + esc(def.desc || node.type) + '</small></div><div class="props-title-actions">'
            + '<button type="button" data-action="move-node" data-direction="-1" title="上移"' + (blocked || !location || location.index === 0 ? ' disabled' : '') + '>↑</button>'
            + '<button type="button" data-action="move-node" data-direction="1" title="下移"' + (blocked || !location || location.index === location.collection.length-1 ? ' disabled' : '') + '>↓</button>'
            + '<button type="button" data-action="duplicate-node" title="复制组件"' + (blocked ? ' disabled' : '') + '><i class="layui-icon layui-icon-file"></i></button><button type="button" data-action="delete-node" title="删除组件"' + (blocked ? ' disabled' : '') + '><i class="layui-icon layui-icon-delete"></i></button></div></div>';
        if (node.type === 'column') html += '<div class="props-hint">选择上方网格可调整列数与比例；将内容拖入当前列即可添加。</div>';
        if (breadcrumb) html += '<div class="props-breadcrumb">' + breadcrumb + '</div>';
        html += '<form class="layui-form" onsubmit="return false">';
        html += section('组件设置', general, !contentExists, 'props-general');
        if (contentExists) html += section('内容', contentFields.map(function (field) {
            var currentProps = node.props || {};
            var value = Object.prototype.hasOwnProperty.call(currentProps, field.key) ? currentProps[field.key] : resetValue(node, 'props', field.key);
            return renderField(field, value, 'props', node);
        }).join(''), true, 'props-content');
        html += '<div class="props-style-heading"><span>样式 <em>按需展开</em></span><span class="props-style-tools"><em class="props-style-count">' + esc(styleSummary(node)) + '</em><button type="button" data-action="reset-style" title="清空本组件已设置的样式，回到组件默认外观">恢复默认样式</button></span></div>';
        html += renderStyleGroups(node, contentExists) + '</form>';
        return html;
    }

    function readValue(el) {
        var $el = $(el);
        var area = $el.attr('data-area'), key = $el.attr('data-key') || '';
        if ($el.attr('data-category-type')) {
            var v = Number($el.val());
            return isFinite(v) ? v : 0;
        }
        if ($el.attr('data-unit-select')) {
            var $number = $el.siblings('input[data-area][data-key]').first();
            return composeLength($number.val(), $el.val());
        }
        if ($el.hasClass('sb-length-value')) {
            return composeLength($el.val(), $el.siblings('select[data-unit-select]').val());
        }
        if ($el.attr('type') === 'checkbox') return $el.is(':checked');
        if ($el.attr('type') === 'number') {
            var raw = String($el.val() == null ? '' : $el.val()).trim();
            if (raw === '') return '';                       // 清空 = 恢复默认，不再强制写成 0
            var number = Number(raw);
            if (isFinite(number) && area === 'style' && key.indexOf('effect') === 0) {
                var effectField = styleGroups.filter(function(group){return group.key === 'effects';})[0].fields.filter(function(field){return field.key === key;})[0];
                if (effectField && effectField.type === 'number') number = Math.max(effectField.min, Math.min(effectField.max, number));
            }
            return isFinite(number) ? number : null;
        }
        void area;
        void key;
        return $el.val();
    }

    root.Inspector = {
        render: render,
        readValue: readValue,
        normalizeImageList: normalizeImageList,
        resetValue: resetValue,
        styleGroups: styleGroups,
        populateCategories: populateCategories,
        normalizeCategoryIds: normalizeCategoryIds,
        categoryNameOf: categoryNameOf,
        styleKeysFor: function (node) {
            var scopes = scopesOf(node);
            var keys = [];
            styleGroups.forEach(function (group) {
                group.fields.forEach(function (field) { if (fieldApplies(field, scopes)) keys.push(field.key); });
            });
            return keys;
        }
    };
})(window, window.jQuery);
