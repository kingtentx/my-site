(function (window, document, $) {
    'use strict';
    if (!$ || !window.pageDesignerConfig) return;

    var config = window.pageDesignerConfig;
    var layer = null, form = null, richEditor = null;
    var state = { components: [] };
    var selectedIndex = -1;
    var history = [], historyIndex = -1;
    var categoryOptions = { article: null, product: null, job: null };

    var definitions = {
        banner: { name: 'Banner图', icon: 'layui-icon-picture', defaults: { height: 400, autoplay: true, interval: 5000, items: [] } },
        richText: { name: '富文本', icon: 'layui-icon-edit', defaults: { html: '', paddingTop: 20, paddingBottom: 20 } },
        image: { name: '图片', icon: 'layui-icon-image', defaults: { src: '', alt: '', width: '100%', align: 'center', link: '' } },
        title: { name: '标题', icon: 'layui-icon-fonts', defaults: { text: '标题', level: 2, align: 'center', color: '#333333', subtitle: '' } },
        button: { name: '按钮', icon: 'layui-icon-button', defaults: { text: '了解更多', link: '#', align: 'center', size: 'medium', styleType: 'primary', newWindow: false } },
        divider: { name: '分隔线', icon: 'layui-icon-more', defaults: { color: '#e8e8e8', width: 1, lineStyle: 'solid', marginTop: 20, marginBottom: 20 } },
        spacer: { name: '间距', icon: 'layui-icon-screen-full', defaults: { height: 40 } },
        video: { name: '视频', icon: 'layui-icon-video', defaults: { src: '', poster: '', height: 480, controls: true, autoplay: false, muted: false } },
        iconText: { name: '图文卡片', icon: 'layui-icon-template-1', defaults: { icon: '★', title: '卡片标题', text: '在这里填写说明内容', link: '', align: 'center', backgroundColor: '#ffffff' } },
        news: { name: '文章列表', icon: 'layui-icon-list', defaults: { categoryId: 0, pageSize: 6, enablePagination: true, showStyle: 'list', showCover: true, showSummary: true, showDate: true, moreLink: '/news' } },
        product: { name: '产品列表', icon: 'layui-icon-component', defaults: { categoryId: 0, pageSize: 8, enablePagination: true, colsPerRow: 4, showImage: true, showSummary: false, moreLink: '/products' } },
        job: { name: '招聘列表', icon: 'layui-icon-friends', defaults: { categoryId: 0, pageSize: 10, enablePagination: true, showLocation: true, showSalary: true, showCount: true, showPublishTime: true } }
    };

    function clone(value) { return JSON.parse(JSON.stringify(value == null ? null : value)); }
    function escapeHtml(value) { return String(value == null ? '' : value).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;'); }
    function escapeAttr(value) { return escapeHtml(value).replace(/`/g, '&#96;'); }
    function safeUrl(value) { var v = String(value == null ? '' : value).trim(); return /^(javascript|vbscript|data):/i.test(v) ? '' : v; }
    function getProp(component, key, fallback) { var value = component && component.props ? component.props[key] : undefined; return value === undefined || value === null ? fallback : value; }
    function setProp(component, key, value) { component.props = component.props || {}; component.props[key] = value; }
    function newId(type) { return type + '_' + Date.now() + '_' + Math.floor(Math.random() * 10000); }

    function normalize(list) {
        var result = [];
        (Array.isArray(list) ? list : []).forEach(function (item) {
            if (!item || !definitions[item.type] || item.type === 'navigation' || item.type === 'footer') return;
            item.id = item.id || newId(item.type);
            item.name = item.name || definitions[item.type].name;
            item.visible = item.visible !== false;
            item.props = $.extend(true, {}, definitions[item.type].defaults, item.props || {});
            item.style = item.style || {};
            item.sort = result.length + 1;
            result.push(item);
        });
        return result;
    }

    function recordHistory() {
        history = history.slice(0, historyIndex + 1);
        history.push(clone(state.components));
        if (history.length > 60) history.shift();
        historyIndex = history.length - 1;
    }

    function restoreHistory() {
        state.components = clone(history[historyIndex]) || [];
        selectedIndex = -1;
        renderCanvas();
        renderProperties();
    }

    function afterChange(keepSelection) {
        state.components.forEach(function (item, index) { item.sort = index + 1; });
        if (!keepSelection && selectedIndex >= state.components.length) selectedIndex = -1;
        recordHistory();
        renderCanvas();
        renderProperties();
    }

    function addComponent(type) {
        var def = definitions[type];
        if (!def) return;
        state.components.push({ id: newId(type), type: type, name: def.name, sort: state.components.length + 1, visible: true, locked: false, props: clone(def.defaults), style: {} });
        selectedIndex = state.components.length - 1;
        afterChange(true);
    }

    function preview(component) {
        var type = component.type;
        if (type === 'banner') {
            var items = getProp(component, 'items', []);
            if (!Array.isArray(items) || !items.length) return empty('尚未配置 Banner 图片');
            var item = items[0] || {}, image = safeUrl(item.image || '');
            return '<div style="height:120px;background:' + (image ? 'url(\'' + escapeAttr(image) + '\') center/cover' : '#eef3f8') + ';display:flex;align-items:center;justify-content:center;flex-direction:column;color:' + (image ? '#fff' : '#666') + '"><strong>' + escapeHtml(item.title || 'Banner 标题') + '</strong><span>' + escapeHtml(item.subtitle || '') + '</span></div>';
        }
        if (type === 'richText') return getProp(component, 'html', '') ? '<div class="designer-rich-preview">' + getProp(component, 'html', '') + '</div>' : empty('点击右侧编辑富文本');
        if (type === 'image') { var src = safeUrl(getProp(component, 'src', '')); return src ? '<div style="text-align:' + escapeAttr(getProp(component, 'align', 'center')) + '"><img src="' + escapeAttr(src) + '" style="max-width:100%;max-height:160px"></div>' : empty('请选择图片'); }
        if (type === 'title') return '<div style="text-align:' + escapeAttr(getProp(component, 'align', 'center')) + ';color:' + escapeAttr(getProp(component, 'color', '#333')) + '"><strong style="font-size:22px">' + escapeHtml(getProp(component, 'text', '标题')) + '</strong><div>' + escapeHtml(getProp(component, 'subtitle', '')) + '</div></div>';
        if (type === 'button') return '<div style="text-align:' + escapeAttr(getProp(component, 'align', 'center')) + '"><span class="layui-btn">' + escapeHtml(getProp(component, 'text', '按钮')) + '</span></div>';
        if (type === 'divider') return '<hr style="border:0;border-top:' + Number(getProp(component, 'width', 1)) + 'px ' + escapeAttr(getProp(component, 'lineStyle', 'solid')) + ' ' + escapeAttr(getProp(component, 'color', '#eee')) + '">';
        if (type === 'spacer') return '<div style="height:' + Math.min(Number(getProp(component, 'height', 40)), 160) + 'px;background:repeating-linear-gradient(45deg,#fafafa,#fafafa 8px,#f3f3f3 8px,#f3f3f3 16px)"></div>';
        if (type === 'video') return '<div style="height:120px;background:#222;color:#fff;display:flex;align-items:center;justify-content:center"><i class="layui-icon layui-icon-video" style="font-size:34px"></i>&nbsp;' + escapeHtml(getProp(component, 'src', '') || '尚未配置视频地址') + '</div>';
        if (type === 'iconText') return '<div style="padding:24px;text-align:' + escapeAttr(getProp(component, 'align', 'center')) + ';background:' + escapeAttr(getProp(component, 'backgroundColor', '#fff')) + '"><div style="font-size:32px">' + escapeHtml(getProp(component, 'icon', '★')) + '</div><strong>' + escapeHtml(getProp(component, 'title', '卡片标题')) + '</strong><p>' + escapeHtml(getProp(component, 'text', '')) + '</p></div>';
        if (type === 'news') return empty('文章范围：' + categoryText('article', getProp(component, 'categoryId', 0)) + '；每页 ' + getProp(component, 'pageSize', 6) + ' 条');
        if (type === 'product') {
            var cols = Math.max(1, Number(getProp(component, 'colsPerRow', 4)) || 4), html = '<div style="display:grid;grid-template-columns:repeat(' + cols + ',1fr);gap:8px">';
            for (var i = 0; i < Math.min(cols * 2, Number(getProp(component, 'pageSize', 8)) || 8); i++) html += '<div style="height:70px;background:#f1f3f5;border-radius:3px"></div>';
            return html + '</div><div class="preview-caption">' + escapeHtml(categoryText('product', getProp(component, 'categoryId', 0))) + '</div>';
        }
        if (type === 'job') return empty('招聘范围：' + categoryText('job', getProp(component, 'categoryId', 0)) + '；每页 ' + getProp(component, 'pageSize', 10) + ' 条');
        return empty('暂不支持的组件');
    }

    function empty(text) { return '<div class="designer-preview-empty">' + escapeHtml(text) + '</div>'; }

    function renderCanvas() {
        if (!state.components.length) { $('#canvas').html('<div class="designer-empty">从左侧拖入或点击组件开始装修</div>'); return; }
        var html = '';
        state.components.forEach(function (component, index) {
            var def = definitions[component.type] || { name: component.type, icon: 'layui-icon-component' };
            html += '<div class="designer-block' + (index === selectedIndex ? ' selected' : '') + (component.visible === false ? ' hidden-block' : '') + '" data-index="' + index + '">';
            html += '<div class="designer-block-header drag-handle"><span class="block-name"><i class="layui-icon ' + def.icon + '"></i> ' + escapeHtml(component.name || def.name) + '</span><span class="block-type">' + escapeHtml(component.type) + '</span><span class="block-actions">';
            html += '<button type="button" data-action="up" title="上移">↑</button><button type="button" data-action="down" title="下移">↓</button><button type="button" data-action="copy" title="复制">复制</button><button type="button" data-action="toggle" title="显示/隐藏">' + (component.visible === false ? '显示' : '隐藏') + '</button><button type="button" data-action="delete" class="danger">删除</button>';
            html += '</span></div><div class="designer-block-body">' + preview(component) + '</div></div>';
        });
        $('#canvas').html(html);
    }

    function fieldText(key, label, value, placeholder) { return '<div class="layui-form-item"><label class="layui-form-label">' + label + '</label><div class="layui-input-block"><input type="text" class="layui-input" data-prop="' + key + '" value="' + escapeAttr(value || '') + '" placeholder="' + escapeAttr(placeholder || '') + '"></div></div>'; }
    function fieldNumber(key, label, value, min) { return '<div class="layui-form-item"><label class="layui-form-label">' + label + '</label><div class="layui-input-block"><input type="number" class="layui-input" data-prop="' + key + '" min="' + (min == null ? 0 : min) + '" value="' + escapeAttr(value) + '"></div></div>'; }
    function fieldCheckbox(key, label, checked) { return '<div class="layui-form-item"><label class="layui-form-label">' + label + '</label><div class="layui-input-block"><input type="checkbox" data-prop="' + key + '" lay-skin="switch" lay-text="是|否"' + (checked ? ' checked' : '') + '></div></div>'; }
    function fieldColor(key, label, value) { return '<div class="layui-form-item"><label class="layui-form-label">' + label + '</label><div class="layui-input-block"><input type="color" data-prop="' + key + '" value="' + escapeAttr(value || '#000000') + '" style="width:100%;height:32px"></div></div>'; }
    function fieldSelect(key, label, options, value) {
        var html = '<div class="layui-form-item"><label class="layui-form-label">' + label + '</label><div class="layui-input-block"><select data-prop="' + key + '">';
        options.forEach(function (option) { html += '<option value="' + escapeAttr(option.value) + '"' + (String(option.value) === String(value) ? ' selected' : '') + '>' + escapeHtml(option.text) + '</option>'; });
        return html + '</select></div></div>';
    }

    function categoryField(type, value) {
        var info = categoryOptions[type] || { allText: '全部内容', options: [] };
        var options = [{ value: 0, text: info.allText || '全部内容' }].concat(info.options || []);
        return fieldSelect('categoryId', '内容范围', options, value || 0);
    }

    function categoryText(type, value) {
        var info = categoryOptions[type];
        if (!value || !info) return info && info.allText ? info.allText : '全部内容';
        var item = (info.options || []).filter(function (option) { return Number(option.value) === Number(value); })[0];
        return item ? item.text : '指定分类 #' + value;
    }

    function renderProperties() {
        if (richEditor) { try { richEditor.destroy(); } catch (e) { } richEditor = null; }
        if (selectedIndex < 0 || !state.components[selectedIndex]) { $('#propsPanel').html('<div class="props-empty">请选择一个组件</div>'); return; }
        var component = state.components[selectedIndex], type = component.type;
        var html = '<div class="props-title">' + escapeHtml(definitions[type].name) + '设置</div><form class="layui-form" onsubmit="return false">';
        html += fieldText('__name', '组件名称', component.name, '') + fieldCheckbox('__visible', '是否显示', component.visible !== false);

        if (type === 'banner') {
            html += fieldNumber('height', '高度(px)', getProp(component, 'height', 400), 100) + fieldCheckbox('autoplay', '自动播放', getProp(component, 'autoplay', true)) + fieldNumber('interval', '切换间隔', getProp(component, 'interval', 5000), 1000);
            var items = getProp(component, 'items', []); if (!Array.isArray(items)) items = [];
            html += '<div class="props-subtitle">Banner 图片项</div>';
            items.forEach(function (item, index) {
                html += '<div class="banner-editor" data-item-index="' + index + '"><div class="banner-editor-title">第 ' + (index + 1) + ' 项 <a data-action="delete-banner">删除</a></div><button type="button" class="layui-btn layui-btn-primary layui-btn-xs" data-action="pick-banner">选择图片</button>';
                if (item.image) html += '<img src="' + escapeAttr(safeUrl(item.image)) + '" class="props-thumb">';
                html += '<input class="layui-input" data-item-field="title" placeholder="标题" value="' + escapeAttr(item.title || '') + '"><input class="layui-input" data-item-field="subtitle" placeholder="副标题" value="' + escapeAttr(item.subtitle || '') + '"><input class="layui-input" data-item-field="buttonText" placeholder="按钮文字" value="' + escapeAttr(item.buttonText || '') + '"><input class="layui-input" data-item-field="buttonLink" placeholder="按钮链接" value="' + escapeAttr(item.buttonLink || '') + '"></div>';
            });
            html += '<button type="button" class="layui-btn layui-btn-normal layui-btn-sm" data-action="add-banner">新增 Banner 项</button>';
        } else if (type === 'richText') {
            html += '<div class="layui-form-item"><label class="layui-form-label">内容</label><div class="layui-input-block"><div id="richEditor" class="rich-editor"></div></div></div>' + fieldNumber('paddingTop', '上边距', getProp(component, 'paddingTop', 20), 0) + fieldNumber('paddingBottom', '下边距', getProp(component, 'paddingBottom', 20), 0);
        } else if (type === 'image') {
            html += '<div class="layui-form-item"><label class="layui-form-label">图片</label><div class="layui-input-block"><button type="button" class="layui-btn layui-btn-sm" data-action="pick-image">选择图片</button>' + (getProp(component, 'src', '') ? '<img class="props-thumb" src="' + escapeAttr(safeUrl(getProp(component, 'src', ''))) + '">' : '') + '</div></div>';
            html += fieldText('alt', '替代文字', getProp(component, 'alt', ''), '') + fieldText('width', '宽度', getProp(component, 'width', '100%'), '100%') + fieldSelect('align', '对齐', alignOptions(), getProp(component, 'align', 'center')) + fieldText('link', '点击链接', getProp(component, 'link', ''), '');
        } else if (type === 'title') {
            html += fieldText('text', '标题', getProp(component, 'text', ''), '') + fieldText('subtitle', '副标题', getProp(component, 'subtitle', ''), '') + fieldSelect('level', '标题级别', [{value:1,text:'H1'},{value:2,text:'H2'},{value:3,text:'H3'},{value:4,text:'H4'}], getProp(component, 'level', 2)) + fieldSelect('align', '对齐', alignOptions(), getProp(component, 'align', 'center')) + fieldColor('color', '文字颜色', getProp(component, 'color', '#333333'));
        } else if (type === 'button') {
            html += fieldText('text', '按钮文字', getProp(component, 'text', ''), '') + fieldText('link', '跳转链接', getProp(component, 'link', ''), '') + fieldSelect('align', '对齐', alignOptions(), getProp(component, 'align', 'center')) + fieldSelect('size', '尺寸', [{value:'small',text:'小'},{value:'medium',text:'中'},{value:'large',text:'大'}], getProp(component, 'size', 'medium')) + fieldSelect('styleType', '样式', [{value:'primary',text:'主要按钮'},{value:'outline',text:'描边按钮'},{value:'text',text:'文字链接'}], getProp(component, 'styleType', 'primary')) + fieldCheckbox('newWindow', '新窗口', getProp(component, 'newWindow', false));
        } else if (type === 'divider') {
            html += fieldColor('color', '线条颜色', getProp(component, 'color', '#e8e8e8')) + fieldNumber('width', '线条粗细', getProp(component, 'width', 1), 1) + fieldSelect('lineStyle', '线型', [{value:'solid',text:'实线'},{value:'dashed',text:'虚线'},{value:'dotted',text:'点线'}], getProp(component, 'lineStyle', 'solid')) + fieldNumber('marginTop', '上边距', getProp(component, 'marginTop', 20), 0) + fieldNumber('marginBottom', '下边距', getProp(component, 'marginBottom', 20), 0);
        } else if (type === 'spacer') {
            html += fieldNumber('height', '高度(px)', getProp(component, 'height', 40), 0);
        } else if (type === 'video') {
            html += fieldText('src', '视频地址', getProp(component, 'src', ''), 'MP4 或可播放 URL') + fieldText('poster', '封面地址', getProp(component, 'poster', ''), '') + fieldNumber('height', '高度(px)', getProp(component, 'height', 480), 120) + fieldCheckbox('controls', '显示控制条', getProp(component, 'controls', true)) + fieldCheckbox('autoplay', '自动播放', getProp(component, 'autoplay', false)) + fieldCheckbox('muted', '静音', getProp(component, 'muted', false));
        } else if (type === 'iconText') {
            html += fieldText('icon', '图标/字符', getProp(component, 'icon', '★'), '') + fieldText('title', '标题', getProp(component, 'title', ''), '') + fieldText('text', '说明', getProp(component, 'text', ''), '') + fieldText('link', '跳转链接', getProp(component, 'link', ''), '') + fieldSelect('align', '对齐', alignOptions(), getProp(component, 'align', 'center')) + fieldColor('backgroundColor', '背景色', getProp(component, 'backgroundColor', '#ffffff'));
        } else if (type === 'news') {
            html += categoryField('article', getProp(component, 'categoryId', 0)) + fieldNumber('pageSize', '每页条数', getProp(component, 'pageSize', 6), 1) + fieldCheckbox('enablePagination', '显示分页', getProp(component, 'enablePagination', true)) + fieldSelect('showStyle', '展示样式', [{value:'list',text:'列表'},{value:'card',text:'卡片'}], getProp(component, 'showStyle', 'list')) + fieldCheckbox('showCover', '显示封面', getProp(component, 'showCover', true)) + fieldCheckbox('showSummary', '显示摘要', getProp(component, 'showSummary', true)) + fieldCheckbox('showDate', '显示日期', getProp(component, 'showDate', true)) + fieldText('moreLink', '更多链接', getProp(component, 'moreLink', '/news'), '');
        } else if (type === 'product') {
            html += categoryField('product', getProp(component, 'categoryId', 0)) + fieldNumber('pageSize', '每页条数', getProp(component, 'pageSize', 8), 1) + fieldCheckbox('enablePagination', '显示分页', getProp(component, 'enablePagination', true)) + fieldSelect('colsPerRow', '每行列数', [{value:1,text:'1列'},{value:2,text:'2列'},{value:3,text:'3列'},{value:4,text:'4列'}], getProp(component, 'colsPerRow', 4)) + fieldCheckbox('showImage', '显示图片', getProp(component, 'showImage', true)) + fieldCheckbox('showSummary', '显示摘要', getProp(component, 'showSummary', false)) + fieldText('moreLink', '更多链接', getProp(component, 'moreLink', '/products'), '');
        } else if (type === 'job') {
            html += categoryField('job', getProp(component, 'categoryId', 0)) + fieldNumber('pageSize', '每页条数', getProp(component, 'pageSize', 10), 1) + fieldCheckbox('enablePagination', '显示分页', getProp(component, 'enablePagination', true)) + fieldCheckbox('showLocation', '显示地点', getProp(component, 'showLocation', true)) + fieldCheckbox('showSalary', '显示薪资', getProp(component, 'showSalary', true)) + fieldCheckbox('showCount', '显示人数', getProp(component, 'showCount', true)) + fieldCheckbox('showPublishTime', '显示时间', getProp(component, 'showPublishTime', true));
        }

        html += '<div class="component-save-box"><button type="button" class="layui-btn layui-btn-normal layui-btn-fluid" data-action="save-component"><i class="layui-icon layui-icon-ok"></i> 仅临时保存当前组件</button><div class="component-save-tip" id="componentSaveTip">不会覆盖其他尚未保存的组件修改</div></div></form>';
        $('#propsPanel').html(html);
        if (form) form.render();
        if (type === 'richText') initRichEditor(component);
    }

    function alignOptions() { return [{value:'left',text:'左对齐'},{value:'center',text:'居中'},{value:'right',text:'右对齐'}]; }

    function initRichEditor(component) {
        if (!window.wangEditor) return;
        richEditor = new window.wangEditor('#richEditor');
        richEditor.config.uploadFileName = 'file';
        richEditor.config.uploadImgServer = '/Upload/UploadImage';
        richEditor.config.uploadImgHooks = { customInsert: function (insertImg, result) { if (result && result.url) insertImg(result.url); } };
        richEditor.config.height = 220;
        richEditor.config.onchange = function (html) { setProp(component, 'html', html); renderCanvas(); };
        richEditor.create();
        richEditor.txt.html(getProp(component, 'html', ''));
    }

    function openImageSelector(callback) {
        window.__pageDesignerImageCallback = callback;
        layer.open({ type: 2, title: '选择图片', area: ['900px', '680px'], content: '/Admin/ImageSelector' });
    }

    function saveCurrentComponent() {
        var component = state.components[selectedIndex];
        if (!component) return;
        var button = $('[data-action="save-component"]'); button.addClass('layui-btn-disabled').prop('disabled', true);
        $.ajax({
            type: 'post', url: '/PageEnhancement/SaveComponentDraft', dataType: 'json',
            data: { pageId: config.pageId, componentId: component.id, componentJson: JSON.stringify(component) },
            success: function (res) { if (res.code === 200) { $('#componentSaveTip').text('已临时保存 ' + (res.savedAt || '')).addClass('saved'); layer.msg(res.message, {icon:1}); } else layer.msg(res.message || '保存失败', {icon:5}); },
            error: function () { layer.msg('临时保存失败', {icon:5}); },
            complete: function () { button.removeClass('layui-btn-disabled').prop('disabled', false); }
        });
    }

    function saveDraft(callback) {
        var loading = layer.load(1, {shade:0.25});
        $.ajax({
            type: 'post', url: '/Page/SaveDraft?id=' + config.pageId, dataType: 'json',
            data: { componentJson: JSON.stringify(normalize(state.components)) },
            success: function (res) { layer.close(loading); if (res.code === 200) { layer.msg(res.message, {icon:1}); if (callback) callback(); } else layer.msg(res.message, {icon:5}); },
            error: function () { layer.close(loading); layer.msg('保存失败', {icon:5}); }
        });
    }

    function bindEvents() {
        $('.designer-lib').on('click', '.lib-item', function () { addComponent($(this).data('type')); });
        $('#canvas').on('click', '.designer-block', function (event) { if ($(event.target).closest('.block-actions').length) return; selectedIndex = Number($(this).data('index')); renderCanvas(); renderProperties(); });
        $('#canvas').on('click', '.block-actions button', function (event) {
            event.stopPropagation(); var index = Number($(this).closest('.designer-block').data('index')), action = $(this).data('action');
            if (!state.components[index]) return;
            if (action === 'up' && index > 0) { var up = state.components[index - 1]; state.components[index - 1] = state.components[index]; state.components[index] = up; selectedIndex = index - 1; afterChange(true); }
            else if (action === 'down' && index < state.components.length - 1) { var down = state.components[index + 1]; state.components[index + 1] = state.components[index]; state.components[index] = down; selectedIndex = index + 1; afterChange(true); }
            else if (action === 'copy') { var copy = clone(state.components[index]); copy.id = newId(copy.type); state.components.splice(index + 1, 0, copy); selectedIndex = index + 1; afterChange(true); }
            else if (action === 'toggle') { state.components[index].visible = state.components[index].visible === false; selectedIndex = index; afterChange(true); }
            else if (action === 'delete') layer.confirm('确认删除该组件？', function (i) { state.components.splice(index, 1); selectedIndex = -1; afterChange(false); layer.close(i); });
        });

        $('#propsPanel').on('input change', '[data-prop]', function () {
            var component = state.components[selectedIndex]; if (!component) return;
            var key = $(this).data('prop'), value = this.type === 'checkbox' ? this.checked : (this.type === 'number' ? Number(this.value || 0) : this.value);
            if (key === '__name') component.name = value;
            else if (key === '__visible') component.visible = value;
            else setProp(component, key, value);
            renderCanvas();
        });
        $('#propsPanel').on('click', '[data-action="save-component"]', saveCurrentComponent);
        $('#propsPanel').on('click', '[data-action="pick-image"]', function () { var component = state.components[selectedIndex]; openImageSelector(function (url) { setProp(component, 'src', url); renderProperties(); renderCanvas(); }); });
        $('#propsPanel').on('click', '[data-action="add-banner"]', function () { var component = state.components[selectedIndex], items = getProp(component, 'items', []); if (!Array.isArray(items)) items = []; items.push({title:'',subtitle:'',image:'',buttonText:'',buttonLink:''}); setProp(component, 'items', items); renderProperties(); renderCanvas(); });
        $('#propsPanel').on('click', '[data-action="delete-banner"]', function () { var component = state.components[selectedIndex], index = Number($(this).closest('.banner-editor').data('item-index')), items = getProp(component, 'items', []); items.splice(index, 1); setProp(component, 'items', items); renderProperties(); renderCanvas(); });
        $('#propsPanel').on('click', '[data-action="pick-banner"]', function () { var component = state.components[selectedIndex], index = Number($(this).closest('.banner-editor').data('item-index')); openImageSelector(function (url) { var items = getProp(component, 'items', []); items[index] = items[index] || {}; items[index].image = url; setProp(component, 'items', items); renderProperties(); renderCanvas(); }); });
        $('#propsPanel').on('input change', '[data-item-field]', function () { var component = state.components[selectedIndex], index = Number($(this).closest('.banner-editor').data('item-index')), items = getProp(component, 'items', []); items[index] = items[index] || {}; items[index][$(this).data('item-field')] = this.value; setProp(component, 'items', items); renderCanvas(); });

        $('#btnUndo').on('click', function () { if (historyIndex > 0) { historyIndex--; restoreHistory(); } });
        $('#btnRedo').on('click', function () { if (historyIndex < history.length - 1) { historyIndex++; restoreHistory(); } });
        $('#btnSaveDraft').on('click', function () { saveDraft(); });
        $('#btnPreview').on('click', function () { saveDraft(function () { window.open('/page/preview/' + config.pageId); }); });
        $('#btnPublish').on('click', function () { layer.confirm('将先保存当前全部组件，再发布页面。是否继续？', function (i) { layer.close(i); saveDraft(function () { $.post('/Page/Publish?id=' + config.pageId, function (res) { if (res.code === 200) layer.alert(res.message, function () { location.href = '/page/index'; }); else layer.msg(res.message, {icon:5}); }, 'json'); }); }); });

        window.addEventListener('message', function (event) { if (event.data && event.data.type === 'imageSelected' && window.__pageDesignerImageCallback) { window.__pageDesignerImageCallback(event.data.url || ''); window.__pageDesignerImageCallback = null; } });

        if (window.Sortable) {
            window.Sortable.create(document.getElementById('canvas'), {
                animation: 150, handle: '.drag-handle',
                onEnd: function () {
                    var reordered = [];
                    $('#canvas .designer-block').each(function () { var index = Number($(this).data('index')); if (state.components[index]) reordered.push(state.components[index]); });
                    if (reordered.length === state.components.length) { state.components = reordered; selectedIndex = -1; afterChange(false); }
                }
            });
        }
    }

    function loadCategoryOptions(done) {
        var requests = ['article', 'product', 'job'].map(function (type) {
            return $.get('/ContentCategory/GetOptions', {contentType:type}, function (res) { if (res && res.code === 200) categoryOptions[type] = res.data; }, 'json');
        });
        $.when.apply($, requests).always(done);
    }

    function loadPage() {
        var loading = layer.load(1, {shade:0.25});
        $.get('/Page/GetComponentData', {pageId:config.pageId}, function (res) {
            layer.close(loading);
            if (res.code !== 200) { layer.msg(res.message || '加载失败', {icon:5}); return; }
            state.components = normalize(res.components);
            history = [clone(state.components)]; historyIndex = 0;
            renderCanvas(); renderProperties(); bindEvents();
        }, 'json').fail(function () { layer.close(loading); layer.msg('加载页面失败', {icon:5}); });
    }

    layui.use(['form', 'layer'], function () {
        form = layui.form; layer = layui.layer;
        loadCategoryOptions(loadPage);
    });
})(window, document, window.jQuery);
