(function (window, $) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;
    var esc = root.DesignerRenderer.escapeHtml;

    var styleGroups = [
        { key:'layout', title:'布局与颜色', fields:[
            { key:'backgroundColor', label:'背景色', type:'color' },
            { key:'color', label:'文字颜色', type:'color' },
            { key:'textAlign', label:'内容对齐', type:'select', options:[{value:'',text:'默认'},{value:'left',text:'左对齐'},{value:'center',text:'居中'},{value:'right',text:'右对齐'}] },
            { key:'maxWidth', label:'最大宽度', type:'text', placeholder:'如 1200px' },
            { key:'minHeight', label:'最小高度', type:'text', placeholder:'如 200px' }
        ]},
        { key:'spacing', title:'间距', fields:[
            { key:'paddingTop', label:'上内边距', type:'text', placeholder:'如 24px' },
            { key:'paddingRight', label:'右内边距', type:'text', placeholder:'如 20px' },
            { key:'paddingBottom', label:'下内边距', type:'text', placeholder:'如 24px' },
            { key:'paddingLeft', label:'左内边距', type:'text', placeholder:'如 20px' },
            { key:'marginTop', label:'上外边距', type:'text', placeholder:'如 0' },
            { key:'marginRight', label:'右外边距', type:'text', placeholder:'如 0' },
            { key:'marginBottom', label:'下外边距', type:'text', placeholder:'如 0' },
            { key:'marginLeft', label:'左外边距', type:'text', placeholder:'如 0' },
            { key:'gap', label:'子项间距', type:'text', placeholder:'如 24px' }
        ]},
        { key:'appearance', title:'边框与阴影', fields:[
            { key:'borderRadius', label:'圆角', type:'text', placeholder:'如 8px' },
            { key:'boxShadow', label:'阴影', type:'text', placeholder:'如 0 2px 8px rgba(0,0,0,.08)' }
        ]},
        { key:'advanced', title:'高级定位', fields:[
            { key:'position', label:'定位', type:'select', options:[{value:'',text:'默认'},{value:'relative',text:'相对定位'},{value:'sticky',text:'吸顶'},{value:'fixed',text:'固定'}] },
            { key:'top', label:'顶部距离', type:'text', placeholder:'如 0px' },
            { key:'zIndex', label:'层级', type:'number', min:0, max:99999 }
        ]}
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
        html += '</div>';
        if (count === 2 || count === 3) {
            html += '<div class="sb-grid-column-tools sb-ratio-presets">';
            (count === 2 ? [[50,50],[30,70],[70,30]] : [[25,50,25],[20,60,20],[33.3,33.4,33.3]]).forEach(function(widths){
                html += '<button type="button" data-action="set-grid-ratio" data-widths="' + widths.join(',') + '">' + widths.join(' / ') + '</button>';
            });
            html += '</div>';
        }
        html += '<div class="sb-grid-column-tools"><button type="button" data-action="equal-grid-columns">平均分配列宽</button></div>'
            + '<div class="sb-grid-column-tip">可直接选择 1~6 列；在画布中拖动列之间的蓝色分隔线，可自由调整每列宽度。</div></div>';
        return html;
    }

    function renderField(field, value, area) {
        var key = field.key, type = field.type || 'text';
        var html = '<div class="layui-form-item"><label class="layui-form-label" for="field-' + area + '-' + key + '">' + esc(field.label || key) + '</label><div class="layui-input-block">';
        if (type === 'textarea') {
            html += '<textarea class="layui-textarea" data-area="' + area + '" data-key="' + key + '" placeholder="' + attr(field.placeholder || '') + '">' + esc(value || '') + '</textarea>';
        } else if (type === 'select') {
            html += '<select lay-ignore data-area="' + area + '" data-key="' + key + '">';
            (field.options || []).forEach(function (item) { html += '<option value="' + attr(item.value) + '"' + (String(item.value) === String(value == null ? '' : value) ? ' selected' : '') + '>' + esc(item.text) + '</option>'; });
            html += '</select>';
        } else if (type === 'checkbox') {
            html += '<input type="checkbox" lay-ignore style="display:inline-block;width:18px;height:18px;margin-top:7px;accent-color:#1677ff" data-area="' + area + '" data-key="' + key + '"' + (value ? ' checked' : '') + '>';
        } else if (type === 'color') {
            html += '<div class="sb-color-control"><input type="color" aria-label="选择' + attr(field.label) + '" data-area="' + area + '" data-key="' + key + '" value="' + attr(/^#[0-9a-f]{6}$/i.test(value || '') ? value : '#ffffff') + '"><input type="text" class="layui-input" data-area="' + area + '" data-key="' + key + '" value="' + attr(value || '') + '" placeholder="默认 / #ffffff"><button type="button" data-action="clear-color" data-key="' + key + '" title="恢复默认颜色">↺</button></div>';
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
        html = html.replace(/<(input|textarea|select)\b/, '<$1 id="field-' + area + '-' + key + '"');
        return html + '</div></div>';
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

    function section(title, content, open, modifier) {
        return '<details class="props-fold ' + (modifier || '') + '"' + (open ? ' open' : '') + '><summary>' + esc(title) + '<i class="layui-icon layui-icon-down"></i></summary><div class="props-fold-body">' + content + '</div></details>';
    }

    function renderStyleGroups(node, contentExists) {
        var style = node.style || {};
        var html = '';
        styleGroups.forEach(function (group, index) {
            var fields = group.fields.map(function (field) { return renderField(field, style[field.key], 'style'); }).join('');
            if (group.key === 'spacing') {
                fields = '<div class="props-quick-spacing"><span>快速设置</span><button type="button" data-action="set-spacing" data-value="12px">紧凑</button><button type="button" data-action="set-spacing" data-value="24px">舒适</button><button type="button" data-action="set-spacing" data-value="48px">宽松</button></div>' + fields;
            }
            html += section(group.title, fields, !contentExists && index === 0, 'props-style-group props-style-' + group.key);
        });
        return html;
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
        var general = renderField({key:'name',label:'组件名称'}, node.name, 'node')
            + renderField({key:'visible',label:'是否显示',type:'checkbox'}, node.visible !== false, 'node')
            + renderField({key:'locked',label:'锁定组件',type:'checkbox'}, node.locked === true, 'node');
        var contentExists = (def.inspector || []).length > 0;
        var location = root.Tree.locate(documentModel && documentModel.nodes, node.id);
        var blocked = path.some(function(item){return item.locked;}) || node.type === 'column';
        var html = '<div class="props-title"><div><strong>' + esc(def.name) + '</strong></div><div class="props-title-actions">'
            + '<button type="button" data-action="move-node" data-direction="-1" title="上移"' + (blocked || !location || location.index === 0 ? ' disabled' : '') + '>↑</button>'
            + '<button type="button" data-action="move-node" data-direction="1" title="下移"' + (blocked || !location || location.index === location.collection.length-1 ? ' disabled' : '') + '>↓</button>'
            + '<button type="button" data-action="duplicate-node" title="复制组件"' + (blocked ? ' disabled' : '') + '><i class="layui-icon layui-icon-file"></i></button><button type="button" data-action="delete-node" title="删除组件"' + (blocked ? ' disabled' : '') + '><i class="layui-icon layui-icon-delete"></i></button></div></div>';
        if (node.type === 'column') html += '<div class="props-hint">选择上方网格可调整列数与比例；将内容拖入当前列即可添加。</div>';
        if (breadcrumb) html += '<div class="props-breadcrumb">' + breadcrumb + '</div>';
        html += '<form class="layui-form" onsubmit="return false">';
        html += section('组件设置', general, !contentExists, 'props-general');
        if (contentExists) html += section('内容', (def.inspector || []).map(function (field) { return renderField(field, (node.props || {})[field.key], 'props'); }).join(''), true, 'props-content');
        html += '<div class="props-style-heading">样式 <span>按需展开</span></div>' + renderStyleGroups(node, contentExists);
        html += '</form>';
        return html;
    }

    function readValue(el) {
        var $el = $(el);
        if ($el.attr('type') === 'checkbox') return $el.is(':checked');
        if ($el.attr('type') === 'number') return Number($el.val() || 0);
        var value = $el.val();
        if ($el.attr('data-area') === 'style' && /^(padding|margin|gap|borderRadius|maxWidth|minHeight|top)/.test($el.attr('data-key') || '') && /^-?\d+(\.\d+)?$/.test(String(value).trim())) return String(value).trim() + 'px';
        return value;
    }

    root.Inspector = { render: render, readValue: readValue, normalizeImageList: normalizeImageList };
})(window, window.jQuery);
