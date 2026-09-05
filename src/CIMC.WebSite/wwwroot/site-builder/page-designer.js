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
    var materialState = {
        pageIndex: 1,
        pageSize: 24,
        keywords: '',
        target: null,
        count: 0,
        mode: 'single',
        nodeId: null,
        area: 'props',
        key: null,
        selectedUrls: []
    };

    function ok(res) { var code = res ? Number(res.code) : -1; return code === 200 || code === 0; }
    function message(text, icon) {
        if (layer) layer.msg(text, icon ? { icon: icon } : undefined);
        else if (window.console) console.log('[SiteBuilder]', text);
    }
    function groupName(key) { return { layout:'布局组件', basic:'基础组件', data:'内容组件', global:'全局组件' }[key] || key; }
    function esc(value) { return SB.DesignerRenderer.escapeHtml(value == null ? '' : value); }
    function normalizeImageList(value) {
        return SB.Inspector && typeof SB.Inspector.normalizeImageList === 'function'
            ? SB.Inspector.normalizeImageList(value)
            : (Array.isArray(value) ? value.filter(Boolean) : []);
    }

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
                html += '<button type="button" class="lib-item" data-type="' + esc(def.type) + '"><i class="layui-icon ' + (def.icon || 'layui-icon-component') + '"></i><span>' + esc(def.name) + '</span></button>';
            });
        });

        if (SB.Presets && typeof SB.Presets.all === 'function') {
            var presets = SB.Presets.all() || [];
            if (presets.length) {
                html += '<div class="lib-title">组合预设</div>';
                presets.forEach(function (preset) {
                    html += '<button type="button" class="lib-item lib-preset" data-preset="' + esc(preset.key) + '"><i class="layui-icon layui-icon-template"></i><span>' + esc(preset.name) + '</span></button>';
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
        if (!window.Sortable || typeof window.Sortable.create !== 'function') {
            if (window.console) console.warn('[SiteBuilder] Sortable 未加载，拖动排序不可用');
            return;
        }

        $('.sb-children').each(function () {
            var container = this;
            if (container._siteBuilderSortable) return;
            container._siteBuilderSortable = window.Sortable.create(container, {
                group: { name: 'site-builder-tree', pull: true, put: true },
                animation: 160,
                handle: '.sb-drag',
                draggable: '.sb-node',
                forceFallback: true,
                fallbackOnBody: true,
                fallbackTolerance: 3,
                swapThreshold: 0.65,
                emptyInsertThreshold: 14,
                scroll: true,
                bubbleScroll: true,
                ghostClass: 'sb-sort-ghost',
                chosenClass: 'sb-sort-chosen',
                dragClass: 'sb-sort-drag',
                onStart: function () {
                    document.body.classList.add('sb-is-dragging');
                },
                onEnd: function (evt) {
                    document.body.classList.remove('sb-is-dragging');
                    var id = $(evt.item).attr('data-node-id');
                    var parentId = $(evt.to).attr('data-parent-id') || null;
                    var newIndex = typeof evt.newDraggableIndex === 'number'
                        ? evt.newDraggableIndex
                        : $(evt.to).children('.sb-node').index(evt.item);
                    if (!id || !store.move(id, parentId, newIndex < 0 ? 0 : newIndex)) {
                        render();
                    }
                }
            });
        });
    }

    function ensureMaterialDialog() {
        var $dialog = $('#sbMaterialDialog');
        if ($dialog.length) return $dialog;
        $('body').append(
            '<div class="sb-material-mask" id="sbMaterialDialog" style="display:none">' +
                '<div class="sb-material-dialog">' +
                    '<div class="sb-material-head"><strong id="sbMaterialTitle">选择图片素材</strong><button type="button" data-material-close>×</button></div>' +
                    '<div class="sb-material-tools"><input type="text" id="sbMaterialKeywords" placeholder="搜索文件名"><button type="button" id="sbMaterialSearch">搜索</button></div>' +
                    '<div class="sb-material-body" id="sbMaterialBody"></div>' +
                    '<div class="sb-material-foot"><span id="sbMaterialCount"></span><div><span id="sbMaterialSelected" class="sb-material-selected"></span><button type="button" id="sbMaterialPrev">上一页</button><span id="sbMaterialPage"></span><button type="button" id="sbMaterialNext">下一页</button><button type="button" id="sbMaterialConfirm" class="sb-material-confirm" style="display:none">确定</button></div></div>' +
                '</div>' +
            '</div>');
        return $('#sbMaterialDialog');
    }

    function resetMaterialState() {
        materialState.target = null;
        materialState.mode = 'single';
        materialState.nodeId = null;
        materialState.area = 'props';
        materialState.key = null;
        materialState.selectedUrls = [];
    }

    function closeImagePicker() {
        $('#sbMaterialDialog').hide();
        resetMaterialState();
    }

    function isMaterialSelected(url) {
        return materialState.selectedUrls.indexOf(url) >= 0;
    }

    function updateMaterialSelectionUi() {
        var multi = materialState.mode === 'multiple';
        $('#sbMaterialSelected').text(multi ? ('已选 ' + materialState.selectedUrls.length + ' 张') : '');
        $('#sbMaterialConfirm').toggle(multi).prop('disabled', !multi || materialState.selectedUrls.length === 0);
        if (multi) {
            $('#sbMaterialBody .sb-material-item').each(function () {
                $(this).toggleClass('is-selected', isMaterialSelected($(this).attr('data-url') || ''));
            });
        }
    }

    function renderMaterialItems(items) {
        var html = '';
        (items || []).forEach(function (item) {
            var url = item.url || item.Url || '';
            var fileName = item.fileName || item.FileName || '';
            if (!url) return;
            var selectedClass = materialState.mode === 'multiple' && isMaterialSelected(url) ? ' is-selected' : '';
            html += '<button type="button" class="sb-material-item' + selectedClass + '" data-url="' + esc(url) + '" title="' + esc(fileName) + '">' +
                '<span class="sb-material-check">✓</span>' +
                '<span class="sb-material-thumb"><img src="' + esc(url) + '" alt="' + esc(fileName) + '"></span>' +
                '<span class="sb-material-name">' + esc(fileName || url) + '</span>' +
                '</button>';
        });
        if (!html) html = '<div class="sb-material-empty">暂无图片素材，请先到“内容管理 → 素材管理”上传图片。</div>';
        $('#sbMaterialBody').html(html);
        updateMaterialSelectionUi();
    }

    function loadImageMaterials() {
        $('#sbMaterialBody').html('<div class="sb-material-empty">正在加载素材...</div>');
        $.get('/Images/GetList', {
            pageIndex: materialState.pageIndex,
            pageSize: materialState.pageSize,
            keywords: materialState.keywords || ''
        }).done(function (res) {
            if (!ok(res)) {
                $('#sbMaterialBody').html('<div class="sb-material-empty is-error">读取素材失败：' + esc((res && res.message) || '请确认当前账号拥有素材查看权限') + '</div>');
                return;
            }
            materialState.count = Number(res.count || 0);
            var pages = Math.max(1, Math.ceil(materialState.count / materialState.pageSize));
            if (materialState.pageIndex > pages) materialState.pageIndex = pages;
            renderMaterialItems(res.data || []);
            $('#sbMaterialCount').text('共 ' + materialState.count + ' 张');
            $('#sbMaterialPage').text(materialState.pageIndex + ' / ' + pages);
            $('#sbMaterialPrev').prop('disabled', materialState.pageIndex <= 1);
            $('#sbMaterialNext').prop('disabled', materialState.pageIndex >= pages);
        }).fail(function () {
            $('#sbMaterialBody').html('<div class="sb-material-empty is-error">读取素材请求失败，请检查 /Images/GetList。</div>');
        });
    }

    function openImagePicker(input) {
        resetMaterialState();
        materialState.mode = 'single';
        materialState.target = input;
        materialState.pageIndex = 1;
        materialState.keywords = '';
        ensureMaterialDialog().show();
        $('#sbMaterialTitle').text('选择图片素材');
        $('#sbMaterialKeywords').val('');
        $('#sbMaterialConfirm').hide();
        $('#sbMaterialSelected').text('');
        loadImageMaterials();
    }

    function openImageListPicker(nodeId, area, key, images) {
        resetMaterialState();
        materialState.mode = 'multiple';
        materialState.nodeId = nodeId;
        materialState.area = area || 'props';
        materialState.key = key;
        materialState.selectedUrls = normalizeImageList(images).slice();
        materialState.pageIndex = 1;
        materialState.keywords = '';
        ensureMaterialDialog().show();
        $('#sbMaterialTitle').text('选择 Banner 图片（可多选）');
        $('#sbMaterialKeywords').val('');
        updateMaterialSelectionUi();
        loadImageMaterials();
    }

    function confirmImageListPicker() {
        if (materialState.mode !== 'multiple' || !materialState.nodeId || !materialState.key) return;
        var nodeId = materialState.nodeId;
        var area = materialState.area;
        var key = materialState.key;
        var values = materialState.selectedUrls.slice();
        store.change(function (doc) {
            var node = SB.Tree.find(doc.nodes, nodeId);
            if (!node) return;
            node[area] = node[area] || {};
            node[area][key] = values;
        });
        store.select(nodeId);
        closeImagePicker();
    }

    function updateImageList(nodeId, area, key, updater) {
        store.change(function (doc) {
            var node = SB.Tree.find(doc.nodes, nodeId);
            if (!node) return;
            node[area] = node[area] || {};
            var list = normalizeImageList(node[area][key]).slice();
            updater(list);
            node[area][key] = list;
        });
        store.select(nodeId);
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
        $('#propsPanel').on('click.siteBuilder', '[data-action="pick-image"]', function (e) {
            e.preventDefault();
            var input = $(this).siblings('input[data-area][data-key]')[0];
            if (input) openImagePicker(input);
        });
        $('#propsPanel').on('click.siteBuilder', '[data-action="pick-images"]', function (e) {
            e.preventDefault();
            var node = store.selected();
            if (!node) return;
            var area = $(this).attr('data-area') || 'props';
            var key = $(this).attr('data-key');
            var current = node[area] && key ? node[area][key] : [];
            openImageListPicker(node.id, area, key, current);
        });
        $('#propsPanel').on('click.siteBuilder', '[data-action="remove-list-image"]', function (e) {
            e.preventDefault();
            var node = store.selected(); if (!node) return;
            var index = Number($(this).attr('data-index'));
            var area = $(this).attr('data-area') || 'props';
            var key = $(this).attr('data-key');
            updateImageList(node.id, area, key, function (list) { if (index >= 0 && index < list.length) list.splice(index, 1); });
        });
        $('#propsPanel').on('click.siteBuilder', '[data-action="clear-list-images"]', function (e) {
            e.preventDefault();
            var node = store.selected(); if (!node) return;
            var area = $(this).attr('data-area') || 'props';
            var key = $(this).attr('data-key');
            updateImageList(node.id, area, key, function (list) { list.splice(0, list.length); });
        });
        $('#propsPanel').on('click.siteBuilder', '[data-action="move-list-image"]', function (e) {
            e.preventDefault();
            if ($(this).prop('disabled')) return;
            var node = store.selected(); if (!node) return;
            var index = Number($(this).attr('data-index'));
            var direction = $(this).attr('data-direction');
            var area = $(this).attr('data-area') || 'props';
            var key = $(this).attr('data-key');
            updateImageList(node.id, area, key, function (list) {
                var target = direction === 'up' ? index - 1 : index + 1;
                if (index < 0 || index >= list.length || target < 0 || target >= list.length) return;
                var item = list.splice(index, 1)[0];
                list.splice(target, 0, item);
            });
        });

        $(document).off('.siteBuilderMaterial')
            .on('click.siteBuilderMaterial', '[data-material-close]', function () { closeImagePicker(); })
            .on('click.siteBuilderMaterial', '#sbMaterialDialog', function (e) { if (e.target === this) closeImagePicker(); })
            .on('click.siteBuilderMaterial', '.sb-material-item', function () {
                var url = $(this).attr('data-url') || '';
                if (!url) return;
                if (materialState.mode === 'multiple') {
                    var index = materialState.selectedUrls.indexOf(url);
                    if (index >= 0) materialState.selectedUrls.splice(index, 1);
                    else materialState.selectedUrls.push(url);
                    updateMaterialSelectionUi();
                    return;
                }
                if (!materialState.target) return;
                $(materialState.target).val(url).trigger('change');
                closeImagePicker();
            })
            .on('click.siteBuilderMaterial', '#sbMaterialConfirm', function () { confirmImageListPicker(); })
            .on('click.siteBuilderMaterial', '#sbMaterialSearch', function () {
                materialState.keywords = $('#sbMaterialKeywords').val() || '';
                materialState.pageIndex = 1;
                loadImageMaterials();
            })
            .on('keydown.siteBuilderMaterial', '#sbMaterialKeywords', function (e) {
                if (e.keyCode === 13) {
                    e.preventDefault();
                    materialState.keywords = $(this).val() || '';
                    materialState.pageIndex = 1;
                    loadImageMaterials();
                }
            })
            .on('click.siteBuilderMaterial', '#sbMaterialPrev', function () {
                if (materialState.pageIndex > 1) { materialState.pageIndex--; loadImageMaterials(); }
            })
            .on('click.siteBuilderMaterial', '#sbMaterialNext', function () {
                var pages = Math.max(1, Math.ceil(materialState.count / materialState.pageSize));
                if (materialState.pageIndex < pages) { materialState.pageIndex++; loadImageMaterials(); }
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

    window.setTimeout(function () {
        if (!libraryElement) return;
        if (!libraryElement.querySelector('.lib-item')) {
            try { renderLibrary(); }
            catch (e) {
                if (window.console) console.error('[SiteBuilder] component library recovery failed', e);
                showLibraryError('组件库恢复失败：' + (e && e.message ? e.message : e));
            }
        }
    }, 300);

    if (window.layui && typeof layui.use === 'function') {
        layui.use(['layer','form'], function () {
            layer = layui.layer || null;
            if (layui.form) layui.form.render();
        });
    }
})(window, document, window.jQuery);
