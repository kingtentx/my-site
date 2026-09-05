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
    var canvasWidth = 0;
    var canvasResizeObserver = null;
    var libraryFilter = 'all';
    var libraryView = 'components';
    var lastSavedDocument = null;
    var inspectorNodeId = null;
    var lastRenderedDocument = null;
    var saving = false;
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
    function gridColumnCount(value) {
        return SB.clampGridColumns ? SB.clampGridColumns(value) : Math.max(1, Math.min(6, Math.round(Number(value || 2))));
    }
    function gridWidths(value, count) {
        return SB.normalizeGridWidths ? SB.normalizeGridWidths(value, count) : (function () {
            var result = [];
            for (var i = 0; i < count; i++) result.push(100 / count);
            return result;
        })();
    }
    function gridTemplate(widths) {
        return widths.map(function (item) { return Math.max(1, Number(item || 1)) + 'fr'; }).join(' ');
    }

    function ensureDesignerRuntimeAssets() {
        function addCss(id, href) {
            if (document.getElementById(id)) return;
            var link = document.createElement('link');
            link.id = id;
            link.rel = 'stylesheet';
            link.href = href;
            document.head.appendChild(link);
        }
        addCss('sbPublicSiteCss', '/site/css/site.css?v=2026090511');
        addCss('sbRuntimeCss', '/site-builder/runtime.css?v=2026090511');

        if (!document.getElementById('sbDesignerWysiwygCss')) {
            var style = document.createElement('style');
            style.id = 'sbDesignerWysiwygCss';
            style.textContent = [
                '#canvas.sb-runtime{max-width:none!important;padding:0!important;border-radius:0!important;background:#fff!important;box-shadow:none!important;overflow:visible!important}',
                '.sb-canvas-scale-frame{position:relative;margin:0 auto;min-height:200px}',
                '#canvas .sb-root-drop{min-height:500px;padding:0!important}',
                '#canvas .sb-node{position:relative;outline:1px solid transparent;cursor:grab}',
                '#canvas .sb-node:hover{outline:1px dashed #d7e6f8}',
                '#canvas .sb-node.selected{outline:2px solid #1677ff!important;outline-offset:1px}',
                '#canvas .sb-node.is-hidden{opacity:.42}',
                '#canvas .sb-children{min-height:0}',
                '#canvas .sb-leaf-node{min-width:0}',
                '#canvas .sb-leaf-content{min-width:0}',
                '#canvas .sb-grid>.sb-node{min-width:0}',
                '#canvas .sb-drop-empty{min-height:52px;border:1px dashed #cbd5e1;border-radius:4px;display:flex;align-items:center;justify-content:center;color:#94a3b8;background:rgba(248,250,252,.7)}',
                '#canvas .sb-public-search input,#canvas video{pointer-events:none}',
                '.sb-viewport-tools{display:flex;align-items:center;gap:5px;margin-right:5px;color:#cbd5e1;font-size:12px}',
                '.sb-viewport-tools select{height:30px;border:1px solid rgba(255,255,255,.22);border-radius:4px;background:#1f2937;color:#fff;padding:0 6px;outline:0}',
                '.sb-viewport-tools .sb-scale-label{min-width:38px;color:#93c5fd;text-align:right}',
                '.sb-wysiwyg-note{position:absolute;right:12px;bottom:10px;z-index:5;padding:5px 8px;border-radius:4px;background:rgba(17,24,39,.78);color:#fff;font-size:10px;pointer-events:none}'
            ].join('\n');
            document.head.appendChild(style);
        }

        $('#canvas').addClass('sb-runtime');
        addCss('sbEditorCss', '/site-builder/editor.css?v=2026090511');
    }

    function ensureCanvasViewport() {
        var $canvas = $('#canvas');
        if (!$canvas.parent().hasClass('sb-canvas-scale-frame')) {
            $canvas.wrap('<div class="sb-canvas-scale-frame"></div>');
            $canvas.after('<div class="sb-wysiwyg-note">页面按设计宽度居中显示；整页预览包含 Header / Footer 草稿，正式网站需分别发布</div>');
        }
        if (!$('#sbCanvasWidth').length) {
            var options = [1200, 1440, 1920];
            var nearest = Number(store.document.settings.designWidth) || 1200;
            var html = '<div class="sb-viewport-tools"><span>设计宽度</span><select id="sbCanvasWidth" title="随页面保存；预览和发布均按此宽度居中显示">';
            options.forEach(function (item) { html += '<option value="' + item + '"' + (item === nearest ? ' selected' : '') + '>' + item + 'px</option>'; });
            html += '</select><span class="sb-scale-label" id="sbCanvasScale"></span></div>';
            $('.designer-toolbar .title').after(html);
            canvasWidth = nearest;
        } else {
            canvasWidth = Number($('#sbCanvasWidth').val()) || 1440;
        }
        applyCanvasViewport();

        if (window.ResizeObserver && !canvasResizeObserver) {
            canvasResizeObserver = new ResizeObserver(function () { applyCanvasViewport(); });
            canvasResizeObserver.observe($('#canvas')[0]);
            var wrap = $('.designer-canvas-wrap')[0];
            if (wrap) canvasResizeObserver.observe(wrap);
        }
    }

    function applyCanvasViewport(heightOnly) {
        var $canvas = $('#canvas');
        var $frame = $canvas.parent('.sb-canvas-scale-frame');
        var $wrap = $('.designer-canvas-wrap');
        if (!$canvas.length || !$frame.length || !$wrap.length) return;
        var logicalWidth = Math.max(1200, Math.min(1920, Number(canvasWidth || 1440)));
        var available = Math.max(320, $wrap.innerWidth() - 44);
        var scale = Math.min(1, available / logicalWidth);
        if (!heightOnly) {
            $canvas.css({ width: logicalWidth + 'px', minWidth: logicalWidth + 'px', transform: 'scale(' + scale + ')', transformOrigin: 'top left', margin: 0 });
            $frame.css({ width: Math.ceil(logicalWidth * scale) + 'px' });
        }
        window.requestAnimationFrame(function () {
            var rawHeight = $canvas.outerHeight();
            $frame.css('height', Math.ceil(rawHeight * scale) + 'px');
            $('#sbCanvasScale').text(Math.round(scale * 100) + '%');
        });
    }

    function renderLibrary() {
        if (!SB.Registry || typeof SB.Registry.groups !== 'function') {
            showLibraryError('组件注册器加载失败，请检查 registry.js。');
            return false;
        }
        var groups = SB.Registry.groups(), order = ['layout','basic','data','global'];
        var html = '<div class="library-heading"><strong>组件库</strong><span>点击即可添加</span></div>'
            + '<div class="library-tabs"><button type="button" data-library-view="components" class="active">组件</button><button type="button" data-library-view="templates">组合模板</button><button type="button" data-library-view="structure">页面结构</button></div>'
            + '<label class="library-search"><i class="layui-icon layui-icon-search"></i><input id="componentSearch" type="search" placeholder="搜索组件或预设"></label>'
            + '<div class="library-filters"><button type="button" data-library-filter="all" class="active">全部</button><button type="button" data-library-filter="layout">布局</button><button type="button" data-library-filter="basic">基础</button><button type="button" data-library-filter="data">内容</button><button type="button" data-library-filter="global">全局</button></div>'
            + '<div class="lib-tip lib-workflow-tip">拖入组件开始编辑，或使用「组合模板」快速搭建整块内容。</div>';
        var componentCount = 0;
        order.forEach(function (key) {
            var items = (groups[key] || []).filter(function(def){return def.type !== 'column';});
            if (!items.length) return;
            componentCount += items.length;
            html += '<section class="lib-group" data-library-group="' + key + '"><div class="lib-title">' + groupName(key) + '<span>' + items.length + '</span></div><div class="lib-items">';
            items.forEach(function (def) {
                html += '<button type="button" class="lib-item" data-type="' + esc(def.type) + '" data-library-name="' + esc(def.name + ' ' + def.type) + '"><i class="layui-icon ' + (def.icon || 'layui-icon-component') + '"></i><span>' + esc(def.name) + '</span></button>';
            });
            html += '</div></section>';
        });
        if (SB.Presets && typeof SB.Presets.all === 'function') {
            var presets = SB.Presets.all() || [];
            if (presets.length) {
                html += '<section class="lib-group lib-preset-group" data-library-group="preset"><div class="lib-title">组合预设<span>' + presets.length + '</span></div><div class="lib-items">';
                presets.forEach(function (preset) {
                    html += '<button type="button" class="lib-item lib-preset" data-preset="' + esc(preset.key) + '" data-library-name="' + esc(preset.name + ' ' + preset.key) + '"><i class="layui-icon layui-icon-template"></i><span>' + esc(preset.name) + '</span></button>';
                });
                html += '</div></section>';
            }
        }
        if (!componentCount) html += '<div class="lib-tip" style="margin-top:12px;color:#d4380d">没有加载到任何组件定义，请检查 default-components.js。</div>';
        if (libraryElement) libraryElement.innerHTML = html;
        filterLibrary();
        return componentCount > 0;
    }

    function filterLibrary() {
        var keyword = String($('#componentSearch').val() || '').trim().toLowerCase();
        $('.library-search').toggle(libraryView !== 'structure');
        $('.library-filters').toggle(libraryView === 'components');
        $('.lib-workflow-tip').toggle(libraryView !== 'structure');
        $('.sb-outline').toggle(libraryView === 'structure');
        var totalVisible = 0;
        $('#componentLibrary .lib-group').each(function () {
            var $group = $(this), group = $group.attr('data-library-group');
            var groupAllowed = libraryView === 'templates' ? group === 'preset' : libraryView === 'components' && group !== 'preset' && (libraryFilter === 'all' || libraryFilter === group);
            var visible = 0;
            $group.find('.lib-item').each(function () {
                var matches = !keyword || String($(this).attr('data-library-name') || '').toLowerCase().indexOf(keyword) >= 0;
                this.style.setProperty('display', groupAllowed && matches ? 'flex' : 'none', 'important');
                if (groupAllowed && matches) visible++;
            });
            $group.toggle(visible > 0);
            totalVisible += visible;
        });
        $('#componentLibrary .library-empty').remove();
        if (!totalVisible && libraryView !== 'structure') $('#componentLibrary').append('<div class="library-empty">没有匹配的组件</div>');
    }

    function selectedParentId() {
        var node = store.selected();
        if (node && node.type === 'grid') node = (node.children || [])[0];
        if (node && !SB.Tree.canHaveChildren(node)) {
            var location = SB.Tree.locate(store.document.nodes, node.id);
            node = location && location.parent;
        }
        return node && SB.Tree.canHaveChildren(node) && !isLocked(node.id) ? node.id : null;
    }

    function isLocked(id) {
        var location = SB.Tree.locate(store.document.nodes, id);
        return !!(location && (location.node.locked || (location.parent && isLocked(location.parent.id))));
    }

    function accepts(type, parentId) {
        var parent = parentId ? SB.Tree.find(store.document.nodes, parentId) : null;
        return type !== 'column' && (!parent || (!isLocked(parentId) && parent.type !== 'grid' && SB.Tree.canHaveChildren(parent)));
    }

    function insertComponent(type, preset, parentId, index) {
        if (!accepts(type, parentId)) { message('请将内容拖入列中；列数可在网格设置中调整'); return null; }
        var node = preset ? SB.Presets.create(preset) : SB.Registry.create(type);
        if (!node) return null;
        if (node.type === 'grid') {
            var temporary = new SB.Store();
            temporary.load({schemaVersion:1,nodes:[node]});
            node = temporary.document.nodes[0];
        }
        store.change(function(doc) { SB.Tree.insert(doc.nodes, node, parentId, index); });
        store.select(node.id);
        return node;
    }

    function addComponent(type) {
        try {
            if (store.selected() && isLocked(store.selectedId)) { message('组件已锁定，请先解锁'); return; }
            var location = SB.Tree.locate(store.document.nodes, store.selectedId);
            var node = insertComponent(type, null, selectedParentId(), location && !SB.Tree.canHaveChildren(location.node) ? location.index + 1 : null);
            if (node) store.select(node.id);
        } catch (e) {
            if (window.console) console.error('[SiteBuilder] add component failed', e);
            message('添加组件失败：' + (e && e.message ? e.message : e), 2);
        }
    }

    function addPreset(key) {
        try {
            if (!SB.Presets || typeof SB.Presets.create !== 'function') return;
            if (store.selected() && isLocked(store.selectedId)) { message('组件已锁定，请先解锁'); return; }
            insertComponent('section', key, selectedParentId());
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
                group: { name: 'site-builder-tree', pull: true, put: function(to, from, dragged) {
                    return accepts($(dragged).attr('data-node-type') || $(dragged).attr('data-type') || 'section', $(to.el).attr('data-parent-id') || null);
                } },
                animation: 160,
                filter: function(evt, target) {
                    var nearest = $(evt.target).closest('.sb-node')[0];
                    return nearest !== target || isLocked($(target).attr('data-node-id')) || $(target).attr('data-node-type') === 'column' || $(evt.target).closest('.sb-grid-resize-handle').length > 0;
                },
                preventOnFilter: false,
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
                onMove: function (evt) {
                    var type = $(evt.dragged).attr('data-node-type') || $(evt.dragged).attr('data-type') || 'section';
                    var parentId = $(evt.to).attr('data-parent-id') || null;
                    var parent = parentId ? SB.Tree.find(store.document.nodes, parentId) : null;
                    return accepts(type, parentId);
                },
                onStart: function () { document.body.classList.add('sb-is-dragging'); },
                onAdd: function(evt) {
                    if (!$(evt.item).hasClass('lib-item')) return;
                    var type = $(evt.item).attr('data-type') || 'section', preset = $(evt.item).attr('data-preset');
                    var index = $(evt.item).prevAll('.sb-node').length;
                    var parentId = $(evt.to).attr('data-parent-id') || null;
                    $(evt.item).remove();
                    insertComponent(type, preset, parentId, index);
                },
                onEnd: function (evt) {
                    document.body.classList.remove('sb-is-dragging');
                    var id = $(evt.item).attr('data-node-id');
                    var parentId = $(evt.to).attr('data-parent-id') || null;
                    var newIndex = typeof evt.newDraggableIndex === 'number' ? evt.newDraggableIndex : $(evt.to).children('.sb-node').index(evt.item);
                    if (id && accepts($(evt.item).attr('data-node-type'), parentId) && !isLocked(id)) store.move(id, parentId, newIndex < 0 ? 0 : newIndex);
                    lastRenderedDocument = null;
                    render();
                }
            });
        });
    }

    function setupLibraryDrag() {
        if (!window.Sortable) return;
        $('#componentLibrary .lib-items').each(function() {
            window.Sortable.create(this, {
                group: { name:'site-builder-tree', pull:'clone', put:false }, sort:false,
                draggable:'.lib-item', forceFallback:true, fallbackOnBody:true, fallbackTolerance:5,
                ghostClass:'sb-sort-ghost', chosenClass:'sb-sort-chosen', dragClass:'sb-sort-drag',
                onStart:function(){document.body.classList.add('sb-is-dragging');},
                onEnd:function(){document.body.classList.remove('sb-is-dragging');}
            });
        });
    }

    function updateGridDom($grid, widths) {
        var total = widths.reduce(function (sum, item) { return sum + Number(item || 0); }, 0) || 100;
        var cumulative = 0;
        $grid.css('grid-template-columns', gridTemplate(widths));
        $grid.children('.sb-grid-resize-handle').each(function (index) {
            cumulative += Number(widths[index] || 0);
            $(this).css('left', (cumulative / total * 100) + '%');
        });
    }

    function beginGridResize(e, handle) {
        e.preventDefault(); e.stopPropagation();
        var $handle = $(handle);
        var gridId = $handle.attr('data-grid-id');
        var index = Number($handle.attr('data-index'));
        var node = SB.Tree.find(store.document.nodes, gridId);
        if (!node || node.type !== 'grid' || node.locked) return;
        var $grid = $handle.closest('.sb-grid');
        if (!$grid.length) return;
        var rect = $grid[0].getBoundingClientRect();
        if (!rect.width) return;
        var count = gridColumnCount((node.props || {}).columns || (node.children || []).length || 2);
        var startWidths = gridWidths((node.props || {}).columnWidths, count);
        if (index < 0 || index >= startWidths.length - 1) return;
        var current = startWidths.slice();
        var startX = e.clientX;
        var pairTotal = startWidths[index] + startWidths[index + 1];
        var minWidth = Math.min(8, Math.max(2, pairTotal / 3));
        store.select(gridId);
        document.body.classList.add('sb-is-resizing-grid');
        $handle.addClass('is-resizing');
        $(document).off('.siteBuilderGridResize')
            .on('mousemove.siteBuilderGridResize', function (moveEvent) {
                moveEvent.preventDefault();
                var delta = (moveEvent.clientX - startX) / rect.width * 100;
                var left = Math.max(minWidth, Math.min(pairTotal - minWidth, startWidths[index] + delta));
                var right = pairTotal - left;
                current[index] = Math.round(left * 10) / 10;
                current[index + 1] = Math.round(right * 10) / 10;
                updateGridDom($grid, current);
                $handle.attr('data-size-label', Math.round(current[index]) + '% / ' + Math.round(current[index + 1]) + '%');
            })
            .on('mouseup.siteBuilderGridResize', function () {
                $(document).off('.siteBuilderGridResize');
                document.body.classList.remove('sb-is-resizing-grid');
                $handle.removeClass('is-resizing');
                store.setGridWidths(gridId, current);
                store.select(gridId);
            });
    }

    function ensureMaterialDialog() {
        var $dialog = $('#sbMaterialDialog');
        if ($dialog.length) return $dialog;
        $('body').append('<div class="sb-material-mask" id="sbMaterialDialog" style="display:none"><div class="sb-material-dialog"><div class="sb-material-head"><strong id="sbMaterialTitle">选择图片素材</strong><button type="button" data-material-close>×</button></div><div class="sb-material-tools"><input type="text" id="sbMaterialKeywords" placeholder="搜索文件名"><button type="button" id="sbMaterialSearch">搜索</button></div><div class="sb-material-body" id="sbMaterialBody"></div><div class="sb-material-foot"><span id="sbMaterialCount"></span><div><span id="sbMaterialSelected" class="sb-material-selected"></span><button type="button" id="sbMaterialPrev">上一页</button><span id="sbMaterialPage"></span><button type="button" id="sbMaterialNext">下一页</button><button type="button" id="sbMaterialConfirm" class="sb-material-confirm" style="display:none">确定</button></div></div></div></div>');
        return $('#sbMaterialDialog');
    }
    function resetMaterialState() { materialState.target=null; materialState.mode='single'; materialState.nodeId=null; materialState.area='props'; materialState.key=null; materialState.selectedUrls=[]; }
    function closeImagePicker() { $('#sbMaterialDialog').hide(); resetMaterialState(); }
    function isMaterialSelected(url) { return materialState.selectedUrls.indexOf(url) >= 0; }
    function updateMaterialSelectionUi() {
        var multi = materialState.mode === 'multiple';
        $('#sbMaterialSelected').text(multi ? ('已选 ' + materialState.selectedUrls.length + ' 张') : '');
        $('#sbMaterialConfirm').toggle(multi).prop('disabled', !multi || materialState.selectedUrls.length === 0);
        if (multi) $('#sbMaterialBody .sb-material-item').each(function () { $(this).toggleClass('is-selected', isMaterialSelected($(this).attr('data-url') || '')); });
    }
    function renderMaterialItems(items) {
        var html = '';
        (items || []).forEach(function (item) {
            var url = item.url || item.Url || '', fileName = item.fileName || item.FileName || '';
            if (!url) return;
            var selectedClass = materialState.mode === 'multiple' && isMaterialSelected(url) ? ' is-selected' : '';
            html += '<button type="button" class="sb-material-item' + selectedClass + '" data-url="' + esc(url) + '" title="' + esc(fileName) + '"><span class="sb-material-check">✓</span><span class="sb-material-thumb"><img src="' + esc(url) + '" alt="' + esc(fileName) + '"></span><span class="sb-material-name">' + esc(fileName || url) + '</span></button>';
        });
        if (!html) html = '<div class="sb-material-empty">暂无图片素材，请先到“内容管理 → 素材管理”上传图片。</div>';
        $('#sbMaterialBody').html(html); updateMaterialSelectionUi();
    }
    function loadImageMaterials() {
        $('#sbMaterialBody').html('<div class="sb-material-empty">正在加载素材...</div>');
        $.get('/Images/GetList', { pageIndex:materialState.pageIndex, pageSize:materialState.pageSize, keywords:materialState.keywords || '' }).done(function (res) {
            if (!ok(res)) { $('#sbMaterialBody').html('<div class="sb-material-empty is-error">读取素材失败：' + esc((res && res.message) || '请确认当前账号拥有素材查看权限') + '</div>'); return; }
            materialState.count = Number(res.count || 0);
            var pages = Math.max(1, Math.ceil(materialState.count / materialState.pageSize));
            if (materialState.pageIndex > pages) materialState.pageIndex = pages;
            renderMaterialItems(res.data || []);
            $('#sbMaterialCount').text('共 ' + materialState.count + ' 张');
            $('#sbMaterialPage').text(materialState.pageIndex + ' / ' + pages);
            $('#sbMaterialPrev').prop('disabled', materialState.pageIndex <= 1);
            $('#sbMaterialNext').prop('disabled', materialState.pageIndex >= pages);
        }).fail(function () { $('#sbMaterialBody').html('<div class="sb-material-empty is-error">读取素材请求失败，请检查 /Images/GetList。</div>'); });
    }
    function openImagePicker(input) {
        resetMaterialState(); materialState.mode='single'; materialState.target=input; materialState.pageIndex=1; materialState.keywords='';
        materialState.nodeId=store.selectedId; materialState.area=$(input).attr('data-area'); materialState.key=$(input).attr('data-key');
        ensureMaterialDialog().show(); $('#sbMaterialTitle').text('选择图片素材'); $('#sbMaterialKeywords').val(''); $('#sbMaterialConfirm').hide(); $('#sbMaterialSelected').text(''); loadImageMaterials();
    }
    function openImageListPicker(nodeId, area, key, images) {
        resetMaterialState(); materialState.mode='multiple'; materialState.nodeId=nodeId; materialState.area=area||'props'; materialState.key=key; materialState.selectedUrls=normalizeImageList(images).slice(); materialState.pageIndex=1; materialState.keywords='';
        ensureMaterialDialog().show(); $('#sbMaterialTitle').text('选择 Banner 图片（可多选）'); $('#sbMaterialKeywords').val(''); updateMaterialSelectionUi(); loadImageMaterials();
    }
    function confirmImageListPicker() {
        if (materialState.mode !== 'multiple' || !materialState.nodeId || !materialState.key) return;
        var nodeId=materialState.nodeId, area=materialState.area, key=materialState.key, values=materialState.selectedUrls.slice();
        store.change(function (doc) { var node=SB.Tree.find(doc.nodes,nodeId); if(!node)return; node[area]=node[area]||{}; node[area][key]=values; });
        store.select(nodeId); closeImagePicker();
    }
    function updateImageList(nodeId, area, key, updater) {
        store.change(function (doc) { var node=SB.Tree.find(doc.nodes,nodeId); if(!node)return; node[area]=node[area]||{}; var list=normalizeImageList(node[area][key]).slice(); updater(list); node[area][key]=list; });
        store.select(nodeId);
    }

    function render() {
        if (renderPending) return;
        renderPending = true;
        window.requestAnimationFrame(function () {
            renderPending = false;
            try {
                var serialized = store.serialize();
                canvasWidth = Number(store.document.settings.designWidth) || 1200;
                $('#sbCanvasWidth').val(String(canvasWidth));
                applyCanvasViewport();
                if (serialized !== lastRenderedDocument) {
                    $('#canvas .sb-children').each(function(){if(this._siteBuilderSortable)this._siteBuilderSortable.destroy();});
                    $('#canvas').html(SB.DesignerRenderer.render(store.document, store.selectedId));
                    lastRenderedDocument = serialized;
                    setupSortable();
                } else {
                    $('#canvas .sb-node').each(function(){ $(this).toggleClass('selected', $(this).attr('data-node-id') === store.selectedId); });
                }
                var folds = [], sameNode = inspectorNodeId === store.selectedId;
                var panelScroll = $('#propsPanel').scrollTop();
                var activeField = document.activeElement, focus = null;
                if (sameNode && $(activeField).closest('#propsPanel').length && $(activeField).attr('data-key')) focus = {area:$(activeField).attr('data-area'),key:$(activeField).attr('data-key'),type:$(activeField).attr('type'),start:activeField.selectionStart,end:activeField.selectionEnd};
                if (sameNode) $('#propsPanel details').each(function(){folds.push(this.open);});
                $('#propsPanel').html(SB.Inspector.render(store.selected(), store.document));
                if (sameNode) $('#propsPanel details').each(function(index){if(index < folds.length)this.open = folds[index];});
                $('#propsPanel').scrollTop(sameNode ? panelScroll : 0);
                if (focus) $('#propsPanel [data-area][data-key]').filter(function(){return $(this).attr('data-area')===focus.area&&$(this).attr('data-key')===focus.key&&$(this).attr('type')===focus.type;}).first().each(function(){this.focus({preventScroll:true});if(focus.start!=null&&this.setSelectionRange)this.setSelectionRange(focus.start,focus.end);});
                inspectorNodeId = store.selectedId;
                if (window.layui && layui.form) layui.form.render();
                applyCanvasViewport(true);
                $('#btnUndo').prop('disabled', store.history.length <= 1);
                $('#btnRedo').prop('disabled', store.future.length === 0);
                var dirty = lastSavedDocument !== null && lastSavedDocument !== store.serialize();
                $('#designerSaveState').toggleClass('is-dirty', dirty).text(dirty ? '未保存' : '已保存');
                renderOutline();
            } catch (e) {
                if (window.console) console.error('[SiteBuilder] render failed', e);
                message('装修器渲染失败：' + (e && e.message ? e.message : e), 2);
            }
        });
    }

    function renderOutline() {
        if (!$('#pageOutline').length) $('#componentLibrary').append('<details class="sb-outline" open><summary>页面结构 · 选择嵌套组件</summary><div id="pageOutline"></div></details>');
        function branch(nodes, depth) {
            return (nodes || []).map(function(node){
                return '<button type="button" data-outline-id="' + esc(node.id) + '" class="' + (node.id === store.selectedId ? 'active' : '') + '" style="padding-left:' + (8 + depth * 12) + 'px">' + esc(node.name || node.type) + (node.visible === false ? ' · 已隐藏' : '') + (node.locked ? ' · 已锁定' : '') + '</button>' + branch(node.children, depth+1);
            }).join('');
        }
        $('#pageOutline').html(branch(store.document.nodes,0) || '<div class="props-hint">从左侧拖入模块开始设计</div>');
        $('.sb-outline').toggle(libraryView === 'structure');
    }

    function load() {
        $.get('/Home/BuilderNavigation', {path:String(config.pagePath || '').indexOf('/__global/') === 0 ? '/' : config.pagePath}).done(function(items){
            if (Array.isArray(items)) { SB.DesignerRenderer.setNavigation(items); lastRenderedDocument=null;render(); }
        }).fail(function(){message('导航加载失败，请刷新后重试');});
        $.get('/Page/GetComponentData', { pageId:config.pageId }).done(function (res) {
            if (!ok(res)) { message((res && res.message) || '读取页面结构失败；可直接用新版装修器覆盖旧数据。', 2); return; }
            try {
                store.load(res.document || {schemaVersion:1,name:config.pageName||'',nodes:[],settings:{}});
                lastSavedDocument = store.serialize();
                $('#designerPublishState').toggleClass('is-unpublished', Number(res.status) !== 1).text(Number(res.status) === 1 ? '已有发布版本' : '当前页面尚未发布');
            }
            catch (e) { message(e.message + ' 当前画布保持为空，可直接重新设计并保存。', 0); }
        }).fail(function () { message('读取页面结构失败', 2); });
    }

    function saveDraft(done) {
        if (saving) return;
        var active = document.activeElement;
        if (active && $(active).closest('#propsPanel').length) $(active).trigger('change');
        var submitted = store.serialize();
        saving=true;
        $('#btnSaveDraft,#btnPublish,#btnPreview').prop('disabled',true);
        $.ajax({type:'post',url:'/Page/SaveDraft',data:{id:config.pageId,documentJson:submitted}}).done(function (res) {
            if (!ok(res)) { message((res && res.message) || '保存失败', 2); return; }
            lastSavedDocument = submitted;
            render();
            message('草稿已保存', 1); if (done) done();
        }).fail(function () { message('保存失败', 2); }).always(function(){saving=false;$('#btnSaveDraft,#btnPublish,#btnPreview').prop('disabled',false);});
    }

    function previewUrl() {
        return '/Home/BuilderPreview?id=' + config.pageId;
    }

    function bindEvents() {
        $('#propsPanel').on('click.siteBuilder','[data-action]',function(e){if(store.selected()&&isLocked(store.selectedId)&&$(this).attr('data-action')!=='select-node'){e.preventDefault();e.stopImmediatePropagation();message('组件已锁定，请先解锁');}});
        $('#componentLibrary').off('.siteBuilder').on('click.siteBuilder','[data-type]',function(){addComponent($(this).attr('data-type'));});
        $('#componentLibrary').on('click.siteBuilder','[data-preset]',function(){addPreset($(this).attr('data-preset'));});
        $('#componentLibrary').on('input.siteBuilder','#componentSearch',filterLibrary);
        $('#componentLibrary').on('click.siteBuilder','[data-library-view]',function(){libraryView=$(this).attr('data-library-view');$('#componentLibrary [data-library-view]').removeClass('active');$(this).addClass('active');filterLibrary();});
        $('#componentLibrary').on('click.siteBuilder','[data-outline-id]',function(){store.select($(this).attr('data-outline-id'));});
        $('#componentLibrary').on('click.siteBuilder','[data-library-filter]',function(){libraryFilter=$(this).attr('data-library-filter')||'all';$('#componentLibrary [data-library-filter]').removeClass('active');$(this).addClass('active');filterLibrary();});
        $('#canvas').on('click.siteBuilder','.sb-node',function(e){if($(e.target).closest('.sb-grid-resize-handle').length)return;e.preventDefault();e.stopPropagation();store.select($(this).attr('data-node-id'));});
        $('#canvas').on('click.siteBuilder',function(e){if(e.target===this||$(e.target).hasClass('sb-root-drop'))store.select(null);});
        $('#canvas').on('click.siteBuilder','a,button',function(e){e.preventDefault();});
        $('#canvas').on('mousedown.siteBuilder','.sb-grid-resize-handle',function(e){beginGridResize(e,this);});
        $('#propsPanel').on('change.siteBuilder','[data-area][data-key]',function(){
            var node=store.selected();if(!node)return;var area=$(this).attr('data-area'),key=$(this).attr('data-key'),value=SB.Inspector.readValue(this);
            if(isLocked(node.id)&&!(area==='node'&&key==='locked')){message('组件已锁定，请先解锁');render();return;}
            if(node.type==='grid'&&area==='props'&&key==='columns'){store.setGridColumns(node.id,value);store.select(node.id);return;}store.update(node.id,area,key,value);
        });
        $('#propsPanel').on('click.siteBuilder','[data-action="set-grid-columns"]',function(e){e.preventDefault();var node=store.selected();if(!node||node.type!=='grid')return;store.setGridColumns(node.id,Number($(this).attr('data-columns')||2));store.select(node.id);});
        $('#propsPanel').on('click.siteBuilder','[data-action="equal-grid-columns"]',function(e){e.preventDefault();var node=store.selected();if(!node||node.type!=='grid')return;var count=gridColumnCount((node.props||{}).columns||(node.children||[]).length||2),widths=[];for(var i=0;i<count;i++)widths.push(100/count);store.setGridWidths(node.id,widths);store.select(node.id);});
        $('#propsPanel').on('click.siteBuilder','[data-action="set-grid-ratio"]',function(){var node=store.selected();if(node&&node.type==='grid')store.setGridWidths(node.id,$(this).attr('data-widths').split(',').map(Number));});
        $('#propsPanel').on('click.siteBuilder','[data-action="pick-image"]',function(e){e.preventDefault();var input=$(this).siblings('input[data-area][data-key]')[0];if(input)openImagePicker(input);});
        $('#propsPanel').on('click.siteBuilder','[data-action="pick-images"]',function(e){e.preventDefault();var node=store.selected();if(!node)return;var area=$(this).attr('data-area')||'props',key=$(this).attr('data-key'),current=node[area]&&key?node[area][key]:[];openImageListPicker(node.id,area,key,current);});
        $('#propsPanel').on('click.siteBuilder','[data-action="remove-list-image"]',function(e){e.preventDefault();var node=store.selected();if(!node)return;var index=Number($(this).attr('data-index')),area=$(this).attr('data-area')||'props',key=$(this).attr('data-key');updateImageList(node.id,area,key,function(list){if(index>=0&&index<list.length)list.splice(index,1);});});
        $('#propsPanel').on('click.siteBuilder','[data-action="clear-list-images"]',function(e){e.preventDefault();var node=store.selected();if(!node)return;var area=$(this).attr('data-area')||'props',key=$(this).attr('data-key');updateImageList(node.id,area,key,function(list){list.splice(0,list.length);});});
        $('#propsPanel').on('click.siteBuilder','[data-action="move-list-image"]',function(e){e.preventDefault();if($(this).prop('disabled'))return;var node=store.selected();if(!node)return;var index=Number($(this).attr('data-index')),direction=$(this).attr('data-direction'),area=$(this).attr('data-area')||'props',key=$(this).attr('data-key');updateImageList(node.id,area,key,function(list){var target=direction==='up'?index-1:index+1;if(index<0||index>=list.length||target<0||target>=list.length)return;var item=list.splice(index,1)[0];list.splice(target,0,item);});});
        $('#propsPanel').on('click.siteBuilder','[data-action="select-node"]',function(){store.select($(this).attr('data-node-id'));});
        $('#propsPanel').on('click.siteBuilder','[data-action="duplicate-node"]',function(){var node=store.selected();if(node&&!isLocked(node.id)&&node.type!=='column')store.duplicate(node.id);});
        $('#propsPanel').on('click.siteBuilder','[data-action="delete-node"]',function(){var node=store.selected();if(node&&!isLocked(node.id)&&node.type!=='column')store.remove(node.id);});
        $('#propsPanel').on('click.siteBuilder','[data-action="move-node"]',function(){var node=store.selected(),location=node&&SB.Tree.locate(store.document.nodes,node.id);if(!location||isLocked(node.id)||node.type==='column')return;var index=location.index+Number($(this).attr('data-direction'));if(index>=0&&index<location.collection.length)store.move(node.id,location.parent?location.parent.id:null,index);});
        $('#propsPanel').on('click.siteBuilder','[data-action="clear-color"]',function(){var node=store.selected();if(node)store.update(node.id,'style',$(this).attr('data-key'),'');});
        $('#propsPanel').on('click.siteBuilder','[data-action="set-spacing"]',function(){var node=store.selected(),value=$(this).attr('data-value');if(!node||node.locked)return;store.change(function(doc){var target=SB.Tree.find(doc.nodes,node.id);if(!target)return;target.style=target.style||{};target.style.paddingTop=value;target.style.paddingRight=value;target.style.paddingBottom=value;target.style.paddingLeft=value;target.style.gap=value;});store.select(node.id);});

        $(document).off('.siteBuilderMaterial')
            .on('click.siteBuilderMaterial','[data-material-close]',function(){closeImagePicker();})
            .on('click.siteBuilderMaterial','#sbMaterialDialog',function(e){if(e.target===this)closeImagePicker();})
            .on('click.siteBuilderMaterial','.sb-material-item',function(){var url=$(this).attr('data-url')||'';if(!url)return;if(materialState.mode==='multiple'){var index=materialState.selectedUrls.indexOf(url);if(index>=0)materialState.selectedUrls.splice(index,1);else materialState.selectedUrls.push(url);updateMaterialSelectionUi();return;}if(!materialState.nodeId||isLocked(materialState.nodeId))return;store.update(materialState.nodeId,materialState.area,materialState.key,url);closeImagePicker();})
            .on('click.siteBuilderMaterial','#sbMaterialConfirm',function(){confirmImageListPicker();})
            .on('click.siteBuilderMaterial','#sbMaterialSearch',function(){materialState.keywords=$('#sbMaterialKeywords').val()||'';materialState.pageIndex=1;loadImageMaterials();})
            .on('keydown.siteBuilderMaterial','#sbMaterialKeywords',function(e){if(e.keyCode===13){e.preventDefault();materialState.keywords=$(this).val()||'';materialState.pageIndex=1;loadImageMaterials();}})
            .on('click.siteBuilderMaterial','#sbMaterialPrev',function(){if(materialState.pageIndex>1){materialState.pageIndex--;loadImageMaterials();}})
            .on('click.siteBuilderMaterial','#sbMaterialNext',function(){var pages=Math.max(1,Math.ceil(materialState.count/materialState.pageSize));if(materialState.pageIndex<pages){materialState.pageIndex++;loadImageMaterials();}});

        $(document).on('change.siteBuilderViewport','#sbCanvasWidth',function(){var width=Number($(this).val())||1200;store.change(function(doc){doc.settings.designWidth=width;});});
        $(window).on('resize.siteBuilderViewport',function(){applyCanvasViewport();});
        $('#btnUndo').on('click.siteBuilder',function(){store.undo();});
        $('#btnRedo').on('click.siteBuilder',function(){store.redo();});
        $('#btnSaveDraft').on('click.siteBuilder',function(){saveDraft();});
        $('#btnPreview').on('click.siteBuilder',function(){if(saving)return;var preview=window.open('about:blank','_blank');saveDraft(function(){if(preview)preview.location.href=previewUrl();else message('请允许浏览器打开预览窗口');});});
        $('#btnPublish').on('click.siteBuilder',function(){saveDraft(function(){$.post('/Page/Publish',{id:config.pageId}).done(function(res){if(ok(res)){$('#designerPublishState').removeClass('is-unpublished').text('已有发布版本');message('当前页面已发布；Header / Footer 请在全局区域设计中分别发布',1);}else message((res&&res.message)||'发布失败',2);}).fail(function(){message('发布失败，请重试',2);});});});
        $(document).on('keydown.siteBuilderShortcuts',function(e){
            var tag=String(e.target && e.target.tagName || '').toLowerCase(), editing=tag==='input'||tag==='textarea'||tag==='select'||e.target.isContentEditable;
            if ($('#sbMaterialDialog:visible').length) {if(e.key==='Escape')closeImagePicker();return;}
            if((e.ctrlKey||e.metaKey)&&String(e.key).toLowerCase()==='s'){e.preventDefault();saveDraft();return;}
            if((e.ctrlKey||e.metaKey)&&String(e.key).toLowerCase()==='d'&&!editing){e.preventDefault();var node=store.selected();if(node&&!isLocked(node.id)&&node.type!=='column')store.duplicate(node.id);return;}
            if((e.ctrlKey||e.metaKey)&&String(e.key).toLowerCase()==='z'&&!editing){e.preventDefault();if(e.shiftKey)store.redo();else store.undo();return;}
            if(e.key==='Delete'&&!editing){var selected=store.selected();if(selected&&!isLocked(selected.id)&&selected.type!=='column'){e.preventDefault();store.remove(selected.id);}return;}
            if(e.key==='Escape'&&!editing)store.select(null);
        });
    }

    function initDesigner() {
        if (initialized) return;
        initialized = true;
        ensureDesignerRuntimeAssets();
        ensureCanvasViewport();
        bindEvents();
        var hasComponents = renderLibrary();
        setupLibraryDrag();
        if (!hasComponents && window.console) console.error('[SiteBuilder] no registered component definitions');
        store.subscribe(render);
        render();
        load();
    }

    try { initDesigner(); }
    catch (e) { if (window.console) console.error('[SiteBuilder] initialize failed', e); showLibraryError('组件库初始化异常：' + (e && e.message ? e.message : e)); }

    window.setTimeout(function(){if(!libraryElement)return;if(!libraryElement.querySelector('.lib-item')){try{renderLibrary();}catch(e){if(window.console)console.error('[SiteBuilder] component library recovery failed',e);showLibraryError('组件库恢复失败：'+(e&&e.message?e.message:e));}}},300);
    if (window.layui && typeof layui.use === 'function') layui.use(['layer','form'],function(){layer=layui.layer||null;if(layui.form)layui.form.render();});
})(window, document, window.jQuery);
