(function (window, document, $) {
    'use strict';

    var libraryElement = document.getElementById('componentLibrary');

    function showLibraryError(text) {
        if (!libraryElement) return;
        libraryElement.innerHTML = '<div class="lib-tip" style="color:#d4380d">' + String(text || '装修器组件库初始化失败') + '</div>';
    }

    if (!$ || !window.pageDesignerConfig || !window.SiteBuilder) {
        showLibraryError('装修器基础脚本加载失败，请按 Ctrl+F5 强制刷新，并检查 site-builder 静态脚本是否返回 200。');
        return;
    }

    var SB = window.SiteBuilder;
    var config = window.pageDesignerConfig;

    if (!SB.Registry || !SB.Store || !SB.Tree || !SB.DesignerRenderer || !SB.Inspector) {
        showLibraryError('装修器模块未完整加载。Registry / Store / Tree / Renderer / Inspector 中至少有一个缺失。');
        if (window.console) console.error('[SiteBuilder] modules missing', {
            Registry: !!SB.Registry,
            Store: !!SB.Store,
            Tree: !!SB.Tree,
            DesignerRenderer: !!SB.DesignerRenderer,
            Inspector: !!SB.Inspector
        });
        return;
    }

    var store = new SB.Store(config.pageName || '');
    var layer = null;
    var renderPending = false;
    var initialized = false;

    function ok(res) { var code = res ? Number(res.code) : -1; return code === 200 || code === 0; }
    function message(text, icon) {
        if (layer) layer.msg(text, icon ? { icon: icon } : undefined);
        else if (window.console) console.log('[SiteBuilder]', text);
    }
    function groupName(key) { return { layout:'布局组件', basic:'基础组件', data:'内容组件', global:'全局组件' }[key] || key; }

    function renderLibrary() {
        if (!SB.Registry || typeof SB.Registry.groups !== 'function') {
            showLibraryError('组件注册器加载失败，请检查 registry.js。');
            return false;
        }

        var groups = SB.Registry.groups(), order = ['layout','basic','data','global'];
        var html = '<div class="lib-tip">新版装修器使用树形结构。先添加区段/容器/网格/列，再把内容组件放入布局；Header 与 Footer 也使用同一套组件模型。</div>';
        var componentCount = 0;

        order.forEach(function (key) {
            var items = groups[key] || [];
            if (!items.length) return;
            componentCount += items.length;
            html += '<div class="lib-title">' + groupName(key) + '</div>';
            items.forEach(function (def) {
                html += '<button type="button" class="lib-item" data-type="' + SB.DesignerRenderer.escapeHtml(def.type) + '"><i class="layui-icon ' + (def.icon || 'layui-icon-component') + '"></i><span>' + SB.DesignerRenderer.escapeHtml(def.name) + '</span></button>';
            });
        });

        if (SB.Presets && typeof SB.Presets.all === 'function') {
            var presets = SB.Presets.all() || [];
            if (presets.length) {
                html += '<div class="lib-title">组合预设</div>';
                presets.forEach(function (preset) {
                    html += '<button type="button" class="lib-item lib-preset" data-preset="' + SB.DesignerRenderer.escapeHtml(preset.key) + '"><i class="layui-icon layui-icon-template"></i><span>' + SB.DesignerRenderer.escapeHtml(preset.name) + '</span></button>';
                });
            }
        }

        if (!componentCount) {
            html += '<div class="lib-tip" style="margin-top:12px;color:#d4380d">没有加载到任何组件定义，请检查 default-components.js 是否成功加载。</div>';
        }

        if (libraryElement) libraryElement.innerHTML = html;
        return componentCount > 0;
    }

    function selectedParentId() {
        var node = store.selected();
        return node && SB.Tree.canHaveChildren(node) && !node.locked ? node.id : null;
    }

    function addComponent(type) {
        try {
            var node = store.add(type, selectedParentId());
            if (!node) return;
            if (type === 'grid') {
                store.change(function () {
                    node.children.push(SB.Registry.create('column'));
                    node.children.push(SB.Registry.create('column'));
                });
                store.select(node.id);
            }
        } catch (e) {
            if (window.console) console.error('[SiteBuilder] add component failed', e);
            message('添加组件失败：' + (e && e.message ? e.message : e), 2);
        }
    }

    function addPreset(key) {
        try {
            if (!SB.Presets || typeof SB.Presets.create !== 'function') return;
            var node = SB.Presets.create(key);
            if (!node) return;
            var parentId = selectedParentId();
            store.change(function (doc) { if (!SB.Tree.insert(doc.nodes, node, parentId)) SB.Tree.insert(doc.nodes, node, null); });
            store.select(node.id);
        } catch (e) {
            if (window.console) console.error('[SiteBuilder] add preset failed', e);
            message('添加预设失败：' + (e && e.message ? e.message : e), 2);
        }
    }

    function setupSortable() {
        if (!window.Sortable || typeof window.Sortable.create !== 'function') return;

        $('.sb-children').each(function () {
            var container = this;
            if (container._siteBuilderSortable) return;
            container._siteBuilderSortable = window.Sortable.create(container, {
                group: { name: 'site-builder-tree', pull: true, put: true },
                animation: 120,
                handle: '.sb-drag',
                draggable: ':scope > .sb-node',
                fallbackOnBody: true,
                swapThreshold: 0.65,
                onEnd: function (evt) {
                    var id = $(evt.item).attr('data-node-id');
                    var parentId = $(evt.to).attr('data-parent-id') || null;
                    var newIndex = $(evt.to).children('.sb-node').index(evt.item);
                    store.move(id, parentId, newIndex < 0 ? 0 : newIndex);
                }
            });
        });
    }

    function render() {
        if (renderPending) return;
        renderPending = true;
        window.requestAnimationFrame(function () {
            renderPending = false;
            try {
                $('#canvas').html(SB.DesignerRenderer.render(store.document, store.selectedId));
                $('#propsPanel').html(SB.Inspector.render(store.selected()));
                if (window.layui && layui.form) layui.form.render();
                setupSortable();
                $('#btnUndo').prop('disabled', store.history.length <= 1);
                $('#btnRedo').prop('disabled', store.future.length === 0);
            } catch (e) {
                if (window.console) console.error('[SiteBuilder] render failed', e);
                message('装修器渲染失败：' + (e && e.message ? e.message : e), 2);
            }
        });
    }

    function load() {
        $.get('/Page/GetComponentData', { pageId: config.pageId }).done(function (res) {
            if (!ok(res)) { message((res && res.message) || '读取页面结构失败；可直接用新版装修器覆盖旧数据。', 2); return; }
            try {
                var doc = res.document || { schemaVersion:1, name:config.pageName || '', nodes:[], settings:{} };
                store.load(doc);
            } catch (e) {
                message(e.message + ' 当前画布保持为空，可直接重新设计并保存。', 0);
            }
        }).fail(function () { message('读取页面结构失败', 2); });
    }

    function saveDraft(done) {
        $.ajax({ type:'post', url:'/Page/SaveDraft', data:{ id:config.pageId, documentJson:store.serialize() } }).done(function (res) {
            if (!ok(res)) { message((res && res.message) || '保存失败', 2); return; }
            message('草稿已保存', 1); if (done) done();
        }).fail(function () { message('保存失败', 2); });
    }

    function bindEvents() {
        $('#componentLibrary').off('.siteBuilder').on('click.siteBuilder', '[data-type]', function () { addComponent($(this).attr('data-type')); });
        $('#componentLibrary').on('click.siteBuilder', '[data-preset]', function () { addPreset($(this).attr('data-preset')); });
        $('#canvas').on('click.siteBuilder', '.sb-node', function (e) { if ($(e.target).closest('.sb-node-actions').length) return; e.stopPropagation(); store.select($(this).attr('data-node-id')); });
        $('#canvas').on('click.siteBuilder', function (e) { if (e.target === this || $(e.target).hasClass('sb-root-drop')) store.select(null); });
        $('#canvas').on('click.siteBuilder', '.sb-node-actions button', function (e) {
            e.preventDefault(); e.stopPropagation();
            var id = $(this).closest('.sb-node').attr('data-node-id'), node = SB.Tree.find(store.document.nodes, id);
            if (!node) return;
            var action = $(this).attr('data-action');
            if (node.locked && action !== 'toggle') { message('组件已锁定，请先在右侧取消锁定'); return; }
            if (action === 'delete') store.remove(id);
            else if (action === 'duplicate') store.duplicate(id);
            else if (action === 'toggle') store.update(id, 'node', 'visible', node.visible === false);
        });
        $('#propsPanel').on('change.siteBuilder', '[data-area][data-key]', function () {
            var node = store.selected(); if (!node) return;
            store.update(node.id, $(this).attr('data-area'), $(this).attr('data-key'), SB.Inspector.readValue(this));
        });
        $('#btnUndo').on('click.siteBuilder', function () { store.undo(); });
        $('#btnRedo').on('click.siteBuilder', function () { store.redo(); });
        $('#btnSaveDraft').on('click.siteBuilder', function () { saveDraft(); });
        $('#btnPreview').on('click.siteBuilder', function () { saveDraft(function () { window.open('/Page/Preview?id=' + config.pageId, '_blank'); }); });
        $('#btnPublish').on('click.siteBuilder', function () {
            saveDraft(function () {
                $.post('/Page/Publish', { id:config.pageId }).done(function (res) { if (ok(res)) message('页面已发布', 1); else message((res && res.message) || '发布失败', 2); });
            });
        });
    }

    function initDesigner() {
        if (initialized) return;
        initialized = true;
        bindEvents();
        var hasComponents = renderLibrary();
        if (!hasComponents && window.console) console.error('[SiteBuilder] no registered component definitions');
        store.subscribe(render);
        render();
        load();
    }

    try {
        initDesigner();
    } catch (e) {
        if (window.console) console.error('[SiteBuilder] initialize failed', e);
        showLibraryError('组件库初始化异常：' + (e && e.message ? e.message : e));
    }

    // 防止浏览器缓存旧脚本或第三方脚本意外清空组件区。
    window.setTimeout(function () {
        if (!libraryElement) return;
        if (!libraryElement.querySelector('.lib-item')) {
            try {
                renderLibrary();
            } catch (e) {
                if (window.console) console.error('[SiteBuilder] component library recovery failed', e);
                showLibraryError('组件库恢复失败：' + (e && e.message ? e.message : e));
            }
        }
    }, 300);

    // Layui 仅负责增强提示层和表单渲染；即使 Layui 模块加载失败，也不影响左侧组件库。
    if (window.layui && typeof layui.use === 'function') {
        layui.use(['layer','form'], function () {
            layer = layui.layer || null;
            if (layui.form) layui.form.render();
        });
    }
})(window, document, window.jQuery);