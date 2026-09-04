(function (window, $) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;
    var esc = root.DesignerRenderer.escapeHtml;

    var commonStyleFields = [
        { key:'backgroundColor', label:'背景色', type:'color' },
        { key:'color', label:'文字颜色', type:'color' },
        { key:'textAlign', label:'对齐', type:'select', options:[{value:'',text:'默认'},{value:'left',text:'左'},{value:'center',text:'居中'},{value:'right',text:'右'}] },
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

    root.Inspector = { render: render, readValue: readValue };
})(window, window.jQuery);