(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;

    function esc(value) {
        return String(value == null ? '' : value).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#39;');
    }

    function safeUrl(value) {
        var v = String(value == null ? '' : value).trim();
        return /^(javascript|vbscript|data):/i.test(v) ? '' : v;
    }

    function styleText(style) {
        style = style || {};
        var map = {
            paddingTop:'padding-top', paddingRight:'padding-right', paddingBottom:'padding-bottom', paddingLeft:'padding-left',
            marginTop:'margin-top', marginRight:'margin-right', marginBottom:'margin-bottom', marginLeft:'margin-left',
            backgroundColor:'background-color', color:'color', maxWidth:'max-width', width:'width', minHeight:'min-height',
            gap:'gap', borderRadius:'border-radius', textAlign:'text-align', borderTopWidth:'border-top-width',
            borderTopStyle:'border-top-style', borderTopColor:'border-top-color', position:'position', top:'top',
            zIndex:'z-index', boxShadow:'box-shadow'
        };
        var parts = [];
        Object.keys(map).forEach(function (key) {
            if (style[key] !== undefined && style[key] !== null && style[key] !== '') parts.push(map[key] + ':' + style[key]);
        });
        return parts.join(';');
    }

    function normalizeGridWidths(node) {
        var p = node.props || {};
        var count = root.clampGridColumns ? root.clampGridColumns(p.columns || (node.children || []).length || 2) : Math.max(1, Math.min(6, Number(p.columns || 2)));
        return root.normalizeGridWidths ? root.normalizeGridWidths(p.columnWidths, count) : (function () {
            var result = [];
            for (var i = 0; i < count; i++) result.push(100 / count);
            return result;
        })();
    }

    function gridTemplate(widths) {
        return widths.map(function (width) { return Math.max(1, Number(width || 1)) + 'fr'; }).join(' ');
    }

    function gridResizeHandles(node, widths) {
        if (!widths || widths.length <= 1) return '';
        var html = '';
        var total = widths.reduce(function (sum, item) { return sum + Number(item || 0); }, 0) || 100;
        var cumulative = 0;
        for (var i = 0; i < widths.length - 1; i++) {
            cumulative += Number(widths[i] || 0);
            var left = Math.max(0, Math.min(100, cumulative / total * 100));
            html += '<button type="button" class="sb-grid-resize-handle" data-grid-id="' + esc(node.id) + '" data-index="' + i + '" style="left:' + left + '%" title="拖动调整列宽" aria-label="调整第' + (i + 1) + '列和第' + (i + 2) + '列宽度"><span></span></button>';
        }
        return html;
    }

    function toolbar(node, def) {
        return '<div class="sb-node-toolbar">'
            + '<span class="sb-drag"><i class="layui-icon ' + esc(def.icon || 'layui-icon-component') + '"></i> ' + esc(node.name || def.name) + '</span>'
            + '<span class="sb-node-type">' + esc(node.type) + '</span>'
            + '<span class="sb-node-actions"><button data-action="duplicate">复制</button><button data-action="toggle">' + (node.visible === false ? '显示' : '隐藏') + '</button><button data-action="delete">删除</button></span>'
            + '</div>';
    }

    function bannerPreview(p, css) {
        var images = Array.isArray(p.images) ? p.images.map(safeUrl).filter(Boolean) : [];
        if (!images.length) return '<div class="sb-placeholder" style="' + esc(css) + '">请选择 Banner 图片，可多选；2 张及以上自动轮播</div>';
        var height = Math.max(120, Math.min(900, Number(p.height || 420)));
        var fit = p.objectFit === 'contain' ? 'contain' : 'cover';
        var html = '<div class="banner sb-public-banner" style="height:' + height + 'px;' + esc(css) + '"><div class="banner-slides">';
        images.forEach(function (url, index) {
            html += '<div class="banner-slide' + (index === 0 ? ' active' : '') + '"><img src="' + esc(url) + '" alt="Banner ' + (index + 1) + '" style="width:100%;height:100%;object-fit:' + fit + '"></div>';
        });
        html += '</div>';
        if (images.length > 1 && p.showArrows !== false) {
            html += '<button type="button" class="banner-arrow prev">‹</button><button type="button" class="banner-arrow next">›</button>';
        }
        if (images.length > 1 && p.showDots !== false) {
            html += '<div class="banner-dots">';
            images.forEach(function (_, index) { html += '<span class="banner-dot' + (index === 0 ? ' active' : '') + '"></span>'; });
            html += '</div>';
        }
        return html + '</div>';
    }

    function leafPreview(node) {
        var p = node.props || {};
        var css = styleText(node.style);
        var styleAttr = esc(css);
        switch (node.type) {
            case 'heading': {
                var level = Math.max(1, Math.min(4, Number(p.level || 2)));
                return '<h' + level + ' class="sb-heading" style="' + styleAttr + '">' + esc(p.text || '标题') + '</h' + level + '>';
            }
            case 'text':
                return '<p class="sb-text" style="' + styleAttr + '">' + esc(p.text || '文本内容').replace(/\n/g,'<br>') + '</p>';
            case 'image': {
                var src = safeUrl(p.src);
                if (!src) return '<div class="sb-placeholder" style="' + styleAttr + '">请选择图片</div>';
                return '<img class="sb-image" src="' + esc(src) + '" alt="' + esc(p.alt || '') + '" style="' + styleAttr + '">';
            }
            case 'banner':
                return bannerPreview(p, css);
            case 'button':
                return '<a class="sb-public-button sb-public-button-' + esc(p.variant || 'primary') + '" href="#" style="' + styleAttr + '">' + esc(p.text || '按钮') + '</a>';
            case 'icon':
                return '<span class="sb-public-icon" style="font-size:' + Math.max(12, Math.min(160, Number(p.size || 32))) + 'px;' + styleAttr + '">' + esc(p.text || '★') + '</span>';
            case 'video': {
                var videoSrc = safeUrl(p.src);
                var poster = safeUrl(p.poster);
                return videoSrc
                    ? '<video class="sb-public-video" src="' + esc(videoSrc) + '" poster="' + esc(poster) + '" style="' + styleAttr + '" controls></video>'
                    : '<div class="sb-placeholder" style="' + styleAttr + '">视频：尚未配置</div>';
            }
            case 'divider':
                return '<hr class="sb-public-divider" style="' + styleAttr + '">';
            case 'spacer':
                return '<div aria-hidden="true" style="height:' + Math.max(4, Number(p.height || 40)) + 'px;' + styleAttr + '"></div>';
            case 'articleList':
                return '<div class="sb-placeholder" style="' + styleAttr + '">文章列表 · ' + Number(p.pageSize || 6) + ' 条 · ' + Number(p.columns || 3) + ' 列</div>';
            case 'productList':
                return '<div class="sb-placeholder" style="' + styleAttr + '">产品列表 · ' + Number(p.pageSize || 8) + ' 条 · ' + Number(p.columns || 4) + ' 列</div>';
            case 'jobList':
                return '<div class="sb-placeholder" style="' + styleAttr + '">招聘列表 · ' + Number(p.pageSize || 10) + ' 条</div>';
            case 'logo': {
                var logoSrc = safeUrl(p.src);
                return '<a class="sb-public-logo" href="#" style="' + styleAttr + '">' + (logoSrc ? '<img src="' + esc(logoSrc) + '" alt="' + esc(p.text || 'Logo') + '">' : '<strong>' + esc(p.text || '企业名称') + '</strong>') + '</a>';
            }
            case 'navigation': {
                var vertical = p.direction === 'vertical';
                return '<nav class="sb-public-nav ' + (vertical ? 'is-vertical' : 'is-horizontal') + '" style="' + styleAttr + '"><span>首页</span><span>关于我们</span><span>产品中心</span><span>新闻中心</span></nav>';
            }
            case 'search':
                return '<form class="sb-public-search" style="' + styleAttr + '"><input placeholder="' + esc(p.placeholder || '搜索') + '"><button type="button">⌕</button></form>';
            case 'language':
                return '<span class="sb-public-language" style="' + styleAttr + '">' + esc(p.text || '中文 / EN') + '</span>';
            case 'contact':
                return '<div class="sb-public-contact" style="' + styleAttr + '">' + esc(p.phone || '联系电话') + (p.email ? '<br>' + esc(p.email) : '') + (p.address ? '<br>' + esc(p.address) : '') + '</div>';
            case 'social':
                return '<div class="sb-public-social" style="' + styleAttr + '"><strong>' + esc(p.text || '关注我们') + '</strong><div class="sb-muted">' + esc(p.links || '社交链接') + '</div></div>';
            case 'copyright':
                return '<div class="sb-public-copyright" style="' + styleAttr + '">' + esc(p.text || '版权信息') + '</div>';
            default:
                return '<div class="sb-placeholder" style="' + styleAttr + '">' + esc(node.type) + '</div>';
        }
    }

    function layoutClass(type) {
        if (type === 'section') return 'sb-public-section';
        if (type === 'container') return 'sb-public-container';
        if (type === 'grid') return 'sb-public-grid sb-grid';
        if (type === 'column') return 'sb-public-column';
        return '';
    }

    function nodeHtml(node, selectedId) {
        var def = Registry.get(node.type) || { name: node.type, icon: 'layui-icon-component' };
        var selected = node.id === selectedId ? ' selected' : '';
        var hidden = node.visible === false ? ' is-hidden' : '';

        if (def.container) {
            var children = (node.children || []).map(function (child) { return nodeHtml(child, selectedId); }).join('');
            var classes = 'sb-node sb-container-node sb-children ' + layoutClass(node.type) + selected + hidden;
            var css = styleText(node.style);
            var attributes = ' data-node-id="' + esc(node.id) + '" data-node-type="' + esc(node.type) + '" data-parent-id="' + esc(node.id) + '"';
            var handles = '';
            if (node.type === 'grid') {
                var widths = normalizeGridWidths(node);
                css = 'grid-template-columns:' + gridTemplate(widths) + ';' + css;
                attributes += ' data-grid-id="' + esc(node.id) + '" data-grid-columns="' + widths.length + '"';
                handles = gridResizeHandles(node, widths);
            }
            var empty = children ? '' : '<div class="sb-drop-empty">拖入组件</div>';
            return '<section class="' + classes + '"' + attributes + ' style="' + esc(css) + '">'
                + toolbar(node, def) + children + handles + empty + '</section>';
        }

        var leafClass = 'sb-node sb-leaf-node' + selected + hidden;
        return '<section class="' + leafClass + '" data-node-id="' + esc(node.id) + '" data-node-type="' + esc(node.type) + '">'
            + toolbar(node, def) + '<div class="sb-leaf-content">' + leafPreview(node) + '</div></section>';
    }

    function render(documentModel, selectedId) {
        var nodes = documentModel && documentModel.nodes ? documentModel.nodes : [];
        if (!nodes.length) return '<div class="sb-root-drop sb-children" data-parent-id=""><div class="designer-empty">从左侧添加布局或预设开始设计页面</div></div>';
        return '<div class="sb-root-drop sb-children" data-parent-id="">' + nodes.map(function (node) { return nodeHtml(node, selectedId); }).join('') + '</div>';
    }

    root.DesignerRenderer = { render: render, styleText: styleText, escapeHtml: esc };
})(window);
