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
        Object.keys(map).forEach(function (key) { if (style[key] !== undefined && style[key] !== null && style[key] !== '') parts.push(map[key] + ':' + style[key]); });
        return parts.join(';');
    }

    function bannerPreview(p) {
        var images = Array.isArray(p.images) ? p.images.filter(function (x) { return !!safeUrl(x); }) : [];
        if (!images.length) return '<div class="sb-placeholder">请选择 Banner 图片，可多选；2 张及以上自动轮播</div>';
        var height = Math.max(120, Math.min(900, Number(p.height || 420)));
        var previewHeight = Math.min(320, height);
        var fit = p.objectFit === 'contain' ? 'contain' : 'cover';
        var html = '<div class="sb-banner-preview" style="height:' + previewHeight + 'px">'
            + '<img src="' + esc(safeUrl(images[0])) + '" alt="Banner" style="width:100%;height:100%;object-fit:' + fit + ';display:block">'
            + '<span class="sb-banner-badge">' + images.length + ' 张' + (images.length > 1 ? ' · 自动轮播' : '') + '</span>';
        if (images.length > 1) {
            html += '<div class="sb-banner-dots">';
            images.forEach(function (_, index) { html += '<i class="' + (index === 0 ? 'active' : '') + '"></i>'; });
            html += '</div>';
        }
        return html + '</div>';
    }

    function leafPreview(node) {
        var p = node.props || {};
        switch (node.type) {
            case 'heading': var level = Math.max(1, Math.min(4, Number(p.level || 2))); return '<h' + level + '>' + esc(p.text || '标题') + '</h' + level + '>';
            case 'text': return '<p>' + esc(p.text || '文本内容').replace(/\n/g,'<br>') + '</p>';
            case 'image': var src = safeUrl(p.src); return src ? '<img src="' + esc(src) + '" alt="' + esc(p.alt || '') + '" style="max-width:100%;display:block">' : '<div class="sb-placeholder">请选择图片</div>';
            case 'banner': return bannerPreview(p);
            case 'button': return '<span class="sb-button sb-button-' + esc(p.variant || 'primary') + '">' + esc(p.text || '按钮') + '</span>';
            case 'icon': return '<div style="font-size:' + Number(p.size || 32) + 'px">' + esc(p.text || '★') + '</div>';
            case 'video': return '<div class="sb-placeholder">视频：' + esc(p.src || '尚未配置') + '</div>';
            case 'divider': return '<div style="height:1px"></div>';
            case 'spacer': return '<div class="sb-spacer" style="height:' + Math.max(4, Number(p.height || 40)) + 'px"></div>';
            case 'articleList': return '<div class="sb-placeholder">文章列表 · ' + Number(p.pageSize || 6) + ' 条 · ' + Number(p.columns || 3) + ' 列</div>';
            case 'productList': return '<div class="sb-placeholder">产品列表 · ' + Number(p.pageSize || 8) + ' 条 · ' + Number(p.columns || 4) + ' 列</div>';
            case 'jobList': return '<div class="sb-placeholder">招聘列表 · ' + Number(p.pageSize || 10) + ' 条</div>';
            case 'logo': return p.src ? '<img src="' + esc(safeUrl(p.src)) + '" alt="' + esc(p.text || 'Logo') + '" style="max-height:48px;max-width:180px">' : '<strong>' + esc(p.text || '企业名称') + '</strong>';
            case 'navigation': return '<div class="sb-nav-preview"><span>首页</span><span>关于我们</span><span>产品中心</span><span>新闻中心</span></div>';
            case 'search': return '<div class="sb-search-preview">' + esc(p.placeholder || '搜索') + ' <span>⌕</span></div>';
            case 'language': return '<span>' + esc(p.text || '中文 / EN') + '</span>';
            case 'contact': return '<div>' + esc(p.phone || '联系电话') + '<br>' + esc(p.email || '') + '<br>' + esc(p.address || '') + '</div>';
            case 'social': return '<div><strong>' + esc(p.text || '关注我们') + '</strong><div class="sb-muted">社交链接</div></div>';
            case 'copyright': return '<div>' + esc(p.text || '版权信息') + '</div>';
            default: return '<div class="sb-placeholder">' + esc(node.type) + '</div>';
        }
    }

    function nodeHtml(node, selectedId) {
        var def = Registry.get(node.type) || { name: node.type, icon: 'layui-icon-component' };
        var cls = 'sb-node' + (node.id === selectedId ? ' selected' : '') + (node.visible === false ? ' is-hidden' : '') + (def.container ? ' sb-container-node' : '');
        var body = '';
        if (def.container) {
            var children = (node.children || []).map(function (child) { return nodeHtml(child, selectedId); }).join('');
            var innerClass = 'sb-children';
            if (node.type === 'grid') innerClass += ' sb-grid';
            body = '<div class="' + innerClass + '" data-parent-id="' + esc(node.id) + '"' + (node.type === 'grid' ? ' style="grid-template-columns:repeat(' + Math.max(1, Number((node.props || {}).columns || 2)) + ',minmax(0,1fr))"' : '') + '>' + children + (children ? '' : '<div class="sb-drop-empty">拖入组件</div>') + '</div>';
        } else {
            body = leafPreview(node);
        }
        return '<section class="' + cls + '" data-node-id="' + esc(node.id) + '" data-node-type="' + esc(node.type) + '">' +
            '<div class="sb-node-toolbar"><span class="sb-drag"><i class="layui-icon ' + esc(def.icon || 'layui-icon-component') + '"></i> ' + esc(node.name || def.name) + '</span><span class="sb-node-type">' + esc(node.type) + '</span><span class="sb-node-actions"><button data-action="duplicate">复制</button><button data-action="toggle">' + (node.visible === false ? '显示' : '隐藏') + '</button><button data-action="delete">删除</button></span></div>' +
            '<div class="sb-node-content" style="' + esc(styleText(node.style)) + '">' + body + '</div></section>';
    }

    function render(documentModel, selectedId) {
        var nodes = documentModel && documentModel.nodes ? documentModel.nodes : [];
        if (!nodes.length) return '<div class="sb-root-drop sb-children" data-parent-id=""><div class="designer-empty">从左侧添加布局或预设开始设计页面</div></div>';
        return '<div class="sb-root-drop sb-children" data-parent-id="">' + nodes.map(function (node) { return nodeHtml(node, selectedId); }).join('') + '</div>';
    }

    root.DesignerRenderer = { render: render, styleText: styleText, escapeHtml: esc };
})(window);
