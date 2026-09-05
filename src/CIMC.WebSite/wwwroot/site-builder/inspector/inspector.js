(function (window, $) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;
    var esc = root.DesignerRenderer.escapeHtml;

    var commonStyleFields = [
        { key:'backgroundColor', label:'背景色', type:'color' },
        { key:'color', label:'文字颜色', type:'color' },
        { key:'textAlign', label:'对齐', type:'select', options:[{value:'',text:'默认'},{value:'left',text:'左'},{value:'center',text:'居中'},{value:'right',text:'右'}] },
        { key:'position', label:'定位', type:'select', options:[{value:'',text:'默认'},{value:'relative',text:'相对定位'},{value:'sticky',text:'吸顶'},{value:'fixed',text:'固定'}] },
        { key:'top', label:'顶部距离', type:'text', placeholder:'如 0px' },
        { key:'zIndex', label:'层级', type:'number', min:0, max:99999 },
        { key:'boxShadow', label:'阴影', type:'text', placeholder:'如 0 2px 8px rgba(0,0,0,.08)' },
        { key:'maxWidth', label:'最大宽度', type:'text', placeholder:'如 1200px' },
        { key:'minHeight', label:'最小高度', type:'text', placeholder:'如 200px' },
        { key:'paddingTop', label:'上内边距', type:'text', placeholder:'如 24px' },
        { key:'paddingRight', label:'右内边距', type:'text', placeholder:'如 20px' },
        { key:'paddingBottom', label:'下内边距', type:'text', placeholder:'如 24px' },
        { key:'paddingLeft', label:'左内边距', type:'text', placeholder:'如 20px' },
        { key:'marginTop', label:'上外边距', type:'text', placeholder:'如 0' },
        { key:'marginBottom', label:'下外边距', type:'text', placeholder:'如 0' },
        { key:'gap', label:'间距', type:'text', placeholder:'如 24px' },
        { key:'borderRadius', label:'圆角', type:'text', placeholder:'如 8px' }
    ];

    function attr(value) { return esc(value == null ? '' : value); }

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

    function renderGridColumns(field, value, area) {
        var count = Math.max(1, Math.min(6, Number(value || 2)));
        var html = '<div class="sb-grid-column-control">'
            + '<input class="layui-input sb-grid-column-number" type="number" data-area="' + area + '" data-key="' + attr(field.key) + '" value="' + count + '" min="1" max="6">'
            + '<div class="sb-grid-column-buttons">';
        for (var i = 1; i <= 6; i++) {
            html += '<button type="button" data-action="set-grid-columns" data-columns="' + i + '" class="' + (i === count ? 'active' : '') + '">' + i + '列</button>';
        }
        html += '</div><div class="sb-grid-column-tools"><button type="button" data-action="equal-grid-columns">平均分配列宽</button></div>'
            + '<div class="sb-grid-column-tip">可直接选择 1~6 列；在画布中拖动列之间的蓝色分隔线，可自由调整每列宽度。</div></div>';
        return html;
    }

    function renderField(field, value, area) {
        var key = field.key, type = field.type || 'text';
        var html = '<div class="layui-form-item"><label class="layui-form-label">' + esc(field.label || key) + '</label><div class="layui-input-block">';
        if (type === 'textarea') {
            html += '<textarea class="layui-textarea" data-area="' + area + '" data-key="' + key + '" placeholder="' + attr(field.placeholder || '') + '">' + esc(value || '') + '</textarea>';
        } else if (type === 'select') {
            html += '<select data-area="' + area + '" data-key="' + key + '">';
            (field.options || []).forEach(function (item) { html += '<option value="' + attr(item.value) + '"' + (String(item.value) === String(value == null ? '' : value) ? ' selected' : '') + '>' + esc(item.text) + '</option>'; });
            html += '</select>';
        } else if (type === 'checkbox') {
            html += '<input type="checkbox" data-area="' + area + '" data-key="' + key + '" lay-skin="switch" lay-text="是|否"' + (value ? ' checked' : '') + '>';
        } else if (type === 'color') {
            html += '<input type="color" data-area="' + area + '" data-key="' + key + '" value="' + attr(/^#[0-9a-f]{6}$/i.test(value || '') ? value : '#ffffff') + '" style="width:100%;height:34px">';
        } else if (type === 'image') {
            html += '<div class="sb-image-input-row">'
                + '<input class="layui-input" type="text" data-area="' + area + '" data-key="' + key + '" value="' + attr(value == null ? '' : value) + '" placeholder="图片地址或从素材库选择">'
                + '<button type="button" class="sb-image-pick-btn" data-action="pick-image">素材库</button>'
                + '</div>';
            if (value) html += '<div class="sb-image-preview"><img src="' + attr(value) + '" alt="预览"></div>';
        } else if (type === 'image-list') {
            html += renderImageList(field, value, area);
        } else if (type === 'grid-columns') {
            html += renderGridColumns(field, value, area);
        } else {
            html += '<input class="layui-input" type="' + (type === 'number' ? 'number' : 'text') + '" data-area="' + area + '" data-key="' + key + '" value="' + attr(value == null ? '' : value) + '" placeholder="' + attr(field.placeholder || '') + '"' + (field.min != null ? ' min="' + field.min + '"' : '') + (field.max != null ? ' max="' + field.max + '"' : '') + '>';
        }
        return html + '</div></div>';
    }

    function render(node) {
        if (!node) return '<div class="props-empty">请选择一个组件</div>';
        var def = Registry.get(node.type);
        if (!def) return '<div class="props-empty">未知组件：' + esc(node.type) + '</div>';
        var html = '<div class="props-title">' + esc(def.name) + '<small>' + esc(node.type) + '</small></div><form class="layui-form" onsubmit="return false">';
        html += '<div class="props-section-title">常规</div>';
        html += renderField({key:'name',label:'组件名称'}, node.name, 'node');
        html += renderField({key:'visible',label:'是否显示',type:'checkbox'}, node.visible !== false, 'node');
        html += renderField({key:'locked',label:'锁定组件',type:'checkbox'}, node.locked === true, 'node');
        if ((def.inspector || []).length) {
            html += '<div class="props-section-title">内容</div>';
            (def.inspector || []).forEach(function (field) { html += renderField(field, (node.props || {})[field.key], 'props'); });
        }
        html += '<div class="props-section-title">样式</div>';
        commonStyleFields.forEach(function (field) { html += renderField(field, (node.style || {})[field.key], 'style'); });
        html += '</form>';
        return html;
    }

    function readValue(el) {
        var $el = $(el);
        if ($el.attr('type') === 'checkbox') return $el.is(':checked');
        if ($el.attr('type') === 'number') return Number($el.val() || 0);
        return $el.val();
    }

    root.Inspector = { render: render, readValue: readValue, normalizeImageList: normalizeImageList };
})(window, window.jQuery);