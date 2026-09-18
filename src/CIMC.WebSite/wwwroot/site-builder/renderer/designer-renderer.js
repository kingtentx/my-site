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

    // 背景色 + 背景不透明度(bgOpacity 0~1) → rgba；仅处理 #rgb/#rrggbb，其余原样返回
    function bgColorValue(color, opacity) {
        if (!color) return '';
        var op = Number(opacity);
        if (!isFinite(op) || op < 0 || op >= 1) return color;
        var m = /^#([0-9a-f]{3}|[0-9a-f]{6})$/i.exec(String(color).trim());
        if (!m) return color;
        var hex = m[1];
        if (hex.length === 3) hex = hex.replace(/./g, function (c) { return c + c; });
        var r = parseInt(hex.slice(0, 2), 16), g = parseInt(hex.slice(2, 4), 16), b = parseInt(hex.slice(4, 6), 16);
        return 'rgba(' + r + ',' + g + ',' + b + ',' + op + ')';
    }

    function styleText(style) {
        style = style || {};
        var map = {
            paddingTop:'padding-top', paddingRight:'padding-right', paddingBottom:'padding-bottom', paddingLeft:'padding-left',
            marginTop:'margin-top', marginRight:'margin-right', marginBottom:'margin-bottom', marginLeft:'margin-left',
            color:'color', maxWidth:'max-width', width:'width', minHeight:'min-height',
            gap:'gap', borderWidth:'border-width', borderStyle:'border-style', borderColor:'border-color', borderRadius:'border-radius', textAlign:'text-align', borderTopWidth:'border-top-width',
            borderTopStyle:'border-top-style', borderTopColor:'border-top-color', position:'position', top:'top',
            zIndex:'z-index', boxShadow:'box-shadow', fontSize:'font-size', fontWeight:'font-weight',
            lineHeight:'line-height', letterSpacing:'letter-spacing', alignItems:'align-items', height:'height', objectFit:'object-fit'
        };
        var parts = [];
        Object.keys(map).forEach(function (key) {
            if (style[key] !== undefined && style[key] !== null && style[key] !== '') parts.push(map[key] + ':' + style[key]);
        });
        var bg = bgColorValue(style.backgroundColor, style.bgOpacity);
        if (bg) parts.push('background-color:' + bg);
        if(style.backgroundImage){
            var url=safeUrl(style.backgroundImage).replace(/["'()\\]/g,function(c){return '%'+c.charCodeAt(0).toString(16);});
            var shade=Math.max(0,Math.min(1,Number(style.backgroundOverlay)||0));
            if(url)parts.push('background-image:linear-gradient(rgba(0,0,0,'+shade+'),rgba(0,0,0,'+shade+')),url("'+url+'")','background-size:cover','background-position:center');
        }
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

    var navigation = [];
    function linkUrl(link) {
        if (!link || typeof link !== 'object') return '';
        if (link.type === 'page') {
            var page = (root.PageOptions || []).filter(function (item) { return Number(item.id) === Number(link.pageId); })[0];
            return page ? safeUrl(page.path) : '';
        }
        if (link.type === 'external' && /^https?:\/\//i.test(String(link.url || '').trim())) return safeUrl(link.url);
        return '';
    }
    function hrefAttribute(link) { var url = linkUrl(link); return url ? ' href="' + esc(url) + '"' : ''; }
    function bannerEffects(part, effects) { return ' data-banner-part="' + part + '" data-sb-effects="' + esc(JSON.stringify(effects || {})) + '"'; }
    function navigationHtml(items) {
        return (items || []).map(function (item) {
            return '<div class="sb-public-nav-item' + (item.isCurrent ? ' is-current' : '') + '"><a href="' + esc(safeUrl(item.path || '#')) + '">'
                + (item.icon ? '<i class="layui-icon ' + esc(item.icon) + '"></i>' : '') + '<span>' + esc(item.title) + '</span></a>'
                + (item.children && item.children.length ? '<div class="sb-public-subnav">' + navigationHtml(item.children) + '</div>' : '') + '</div>';
        }).join('');
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
        if (p.title || p.description || p.buttonText) {
            html += '<div class="sb-banner-shade"></div><div class="sb-banner-copy"><h1' + bannerEffects('title',p.titleEffects) + '>' + esc(p.title || '') + '</h1><p' + bannerEffects('description',p.descriptionEffects) + '>' + esc(p.description || '') + '</p>';
            if(p.buttonText) html += '<a class="sb-public-button"' + hrefAttribute(p.buttonLink) + bannerEffects('button',p.buttonEffects) + ' target="' + (p.buttonTarget === '_blank' ? '_blank' : '_self') + '">' + esc(p.buttonText) + '</a>';
            html += '</div>';
        }
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

    function normalizeIdList(value) {
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
            } catch (e) { /* 退回逗号分隔解析 */ }
        }
        text.split(/[,，;；\s]+/).forEach(push);
        return result;
    }

    // 分类 Tab 的排列展示样式：取值与 default-components.js 的 TAB_STYLE_OPTIONS /
    // TAB_ALIGN_OPTIONS 以及服务端 _ArticleList / _ProductList / _JobList 保持一致。
    var TAB_STYLES = ['pill', 'underline', 'segmented', 'card', 'plain'];
    var TAB_ALIGNS = ['left', 'center', 'right'];
    function tabWrapClass(p) {
        var style = String((p && p.tabStyle) || 'pill').toLowerCase();
        if (TAB_STYLES.indexOf(style) < 0) style = 'pill';
        var align = String((p && p.tabAlign) || 'left').toLowerCase();
        if (TAB_ALIGNS.indexOf(align) < 0) align = 'left';
        return 'sb-content-tabs is-tab-' + style + ' align-' + align;
    }

    /**
     * 分类切换 Tab 预览。分类名称由 inspector 拉取后写入 SiteBuilder.CategoryData，
     * 首次加载完成时 page-designer 会重绘画布，这里就能显示真实分类名。
     */
    function categoryTabPreview(p, contentType) {
        if (p.showTabs === false) return '';
        var data = (root.CategoryData && root.CategoryData[contentType]) || null;
        var options = (data && data.options) || [];
        var ids = normalizeIdList(p.categoryIds);
        var names = [];
        if (ids.length === 0) {
            if (!options.length) return '';
            options.forEach(function (opt) {
                if (Number(opt.parentId) === Number(data.rootId)) names.push(String(opt.text || '').replace(/^[\s　]+/, ''));
            });
        } else if (ids.length > 1) {
            ids.forEach(function (id) {
                var name = '';
                options.forEach(function (opt) { if (Number(opt.value) === id) name = String(opt.text || '').replace(/^[\s　]+/, ''); });
                names.push(name || ('分类 ' + id));
            });
        }
        if (!names.length) return '';
        var html = '<div class="' + esc(tabWrapClass(p)) + '"><a class="sb-content-tab is-active">全部</a>';
        names.forEach(function (name) { html += '<a class="sb-content-tab">' + esc(name) + '</a>'; });
        return html + '</div>';
    }

    function articleListPreview(p, css) {
        var layout = String(p.layout || 'card').toLowerCase();
        if (layout !== 'card' && layout !== 'list' && layout !== 'timeline' && layout !== 'editorial') layout = 'card';
        var columns = Math.max(1, Math.min(6, Number(p.columns || 3)));
        var showImage = p.showImage !== false;
        var showSummary = p.showSummary !== false;
        var showDate = p.showDate !== false;
        var pageSize = Math.max(1, Math.min(100, Number(p.pageSize || 6)));
        var count = Math.min(pageSize, 4);

        function mockCard(i, featured) {
            var thumb = showImage ? ('<a class="sb-thumb' + (featured ? ' is-featured' : '') + '"><div class="sb-thumb-placeholder">封面</div></a>') : '';
            var title = '<h3><a>文章标题示例 ' + (i + 1) + '</a></h3>';
            var summary = showSummary ? '<p>这是一段摘要示例，用于预览文章列表在实际页面中的排版效果。</p>' : '';
            var date = showDate ? '<time>2026-09-14</time>' : '';
            return '<article class="sb-content-card' + (featured ? ' is-featured' : '') + '">' + thumb + '<div class="sb-content-card-body">' + title + summary + date + '</div></article>';
        }
        function mockListItem(i) {
            var thumb = showImage ? '<a class="sb-article-list-thumb"><div class="sb-thumb-placeholder">封面</div></a>' : '';
            return '<article class="sb-article-list-item">' + thumb
                + '<div class="sb-article-list-content"><h3><a>文章标题示例 ' + (i + 1) + '</a></h3>'
                + (showSummary ? '<p>这是一段摘要示例，用于预览纵向列表排版效果。</p>' : '')
                + (showDate ? '<time>2026-09-14</time>' : '') + '</div></article>';
        }
        function mockTimelineItem(i) {
            return '<article class="sb-article-timeline-item">'
                + (showDate ? '<time class="sb-timeline-date" datetime="2026-09-14" aria-label="2026-09-14"><span class="sb-timeline-date-day">14</span><span class="sb-timeline-date-month">2026.09</span></time>' : '')
                + '<div class="sb-timeline-dot"></div>'
                + '<div class="sb-timeline-content">' + (showImage ? '<a><div class="sb-thumb-placeholder">封面</div></a>' : '')
                + '<h3><a>文章标题示例 ' + (i + 1) + '</a></h3>' + (showSummary ? '<p>这是一段摘要示例，用于预览时间轴排版效果。</p>' : '') + '</div></article>';
        }

        var html = '<div class="sb-content-list is-' + layout + ' sb-designer-article-list" style="' + esc(css) + '">';
        html += categoryTabPreview(p, 'article');
        if (layout === 'card') {
            html += '<div class="sb-article-grid" style="--sb-cols:' + columns + '">';
            for (var i = 0; i < count; i++) html += mockCard(i, false);
            html += '</div>';
        } else if (layout === 'list') {
            html += '<div class="sb-article-listview">';
            for (var i = 0; i < count; i++) html += mockListItem(i);
            html += '</div>';
        } else if (layout === 'timeline') {
            html += '<div class="sb-article-timeline' + (showDate ? '' : ' is-date-hidden') + '">';
            for (var i = 0; i < count; i++) html += mockTimelineItem(i);
            html += '</div>';
        } else if (layout === 'editorial') {
            html += '<div class="sb-article-editorial">' + mockCard(0, true)
                + '<div class="sb-article-editorial-list">';
            for (var i = 1; i < count; i++) html += mockCard(i, false);
            html += '</div></div>';
        }
        if (p.enablePagination !== false) {
            html += '<nav class="component-pagination"><a>上一页</a><span class="active">1</span><a>2</a><a>3</a><a>下一页</a></nav>';
        }
        return html + '</div>';
    }

    // Use the public component structure so layout options are visible before publishing.
    function previewCategoryNames(p, contentType) {
        var data = (root.CategoryData && root.CategoryData[contentType]) || {};
        var ids = normalizeIdList(p.categoryIds);
        return (data.options || []).filter(function (option) {
            return ids.length ? ids.indexOf(Number(option.value)) >= 0 : Number(option.parentId) === Number(data.rootId);
        }).map(function (option) { return String(option.text || '').trim(); });
    }

    function productListPreview(p, css) {
        var columns = Math.max(1, Math.min(6, Math.floor(Number(p.columns) || 4)));
        var count = Math.max(1, Math.min(50, Math.floor(Number(p.pageSize) || 8)));
        var names = previewCategoryNames(p, 'product');
        var html = '<div class="sb-content-list sb-designer-product-list" style="' + esc(css) + '">';
        html += categoryTabPreview(p, 'product');
        html += '<div class="sb-product-grid" style="--sb-cols:' + columns + '">';
        for (var i = 0; i < count; i++) {
            var name = names.length ? names[i % names.length] : '产品';
            html += '<article class="sb-content-card"><a><div class="sb-product-preview-image" role="img" aria-label="产品图片示例"><span>产品图片</span></div>'
                + '<div class="sb-content-card-body"><h3>' + esc(name) + '示例 ' + (i + 1) + '</h3>'
                + (p.showSummary === true ? '<p>产品介绍示例，展示产品特点、适用场景与服务优势。</p>' : '')
                + '</div></a></article>';
        }
        html += '</div>';
        if (p.enablePagination !== false) html += '<nav class="component-pagination"><a>上一页</a><span class="active">1</span><a>2</a><a>3</a><a>下一页</a></nav>';
        return html + '</div>';
    }

    function jobListPreview(p, css) {
        var count = Math.max(1, Math.min(50, Math.floor(Number(p.pageSize) || 10)));
        var names = previewCategoryNames(p, 'job');
        var titles = ['机械设计工程师', '生产管理专员', '质量工程师', '市场销售经理'];
        var html = '<div class="sb-content-list sb-designer-job-list' + (p.layout === 'compact' ? ' is-compact' : '') + '" style="' + esc(css) + '">';
        html += categoryTabPreview(p, 'job');
        html += '<div class="sb-job-list">';
        for (var i = 0; i < count; i++) {
            html += '<details class="sb-job-card"><summary><strong>' + esc(titles[i % titles.length]) + '（示例 ' + (i + 1) + '）</strong><span>' + esc(names.length ? names[i % names.length] : '业务部门') + '</span>'
                + (p.showLocation !== false ? '<span>上海</span>' : '')
                + (p.showSalary !== false ? '<span>8,000–15,000 元/月</span>' : '')
                + '</summary><div class="sb-job-detail"><h4>岗位职责</h4><div><p>参与业务项目实施，协同团队完成工作目标，持续提升产品与服务质量。</p></div>'
                + '<h4>任职要求</h4><div><p>具备相关专业知识与工作经验，善于沟通协作，有责任心和学习能力。</p></div></div></details>';
        }
        html += '</div>';
        if (p.enablePagination !== false) html += '<nav class="component-pagination"><a>上一页</a><span class="active">1</span><a>2</a><a>3</a><a>下一页</a></nav>';
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
            case 'text': {
                var value = p.text == null ? '文本内容' : p.text;
                return '<div class="sb-text" style="' + styleAttr + '">' + esc(value) + '</div>';
            }
            case 'richText':
                return '<div class="sb-richtext">' + root.RichText.sanitize(p.html || '') + '</div>';
            case 'image': {
                var src = safeUrl(p.src);
                if (!src) return '<div class="sb-placeholder" style="' + styleAttr + '">请选择图片</div>';
                var imageHtml = '<img class="sb-image" src="' + esc(src) + '" alt="' + esc(p.alt || '') + '" style="' + styleAttr + '">';
                var imageLink = safeUrl(p.link);
                // 与前台保持一致：配置了跳转链接时包一层 <a>，避免装修器里看不出效果
                return imageLink ? '<a class="sb-image-link" href="' + esc(imageLink) + '">' + imageHtml + '</a>' : imageHtml;
            }
            case 'banner':
                return bannerPreview(p, css);
            case 'button':
                return '<a class="sb-public-button sb-public-button-' + esc(p.variant || 'primary') + '"' + hrefAttribute(p.link) + ' target="' + (p.target === '_blank' ? '_blank' : '_self') + '" style="' + styleAttr + '">' + esc(p.text || '按钮') + '</a>';
            case 'icon':
                return '<span class="sb-public-icon" style="font-size:' + Math.max(12, Math.min(160, Number(p.size || 32))) + 'px;' + styleAttr + '">' + esc(p.text || '★') + '</span>';
            case 'video': {
                var videoSrc = safeUrl(p.src);
                var poster = safeUrl(p.poster);
                if (!videoSrc) return '<div class="sb-placeholder" style="' + styleAttr + '">视频：尚未配置</div>';
                // 与前台保持一致：未勾选「显示播放控件」时不输出 controls
                return '<video class="sb-public-video" src="' + esc(videoSrc) + '" poster="' + esc(poster) + '" style="' + styleAttr + '"' + (p.controls === false ? '' : ' controls') + '></video>';
            }
            case 'divider':
                return '<hr class="sb-public-divider" style="' + styleAttr + '">';
            case 'spacer':
                return '<div aria-hidden="true" style="height:' + Math.max(4, Math.min(400, Number(p.height || 40))) + 'px;' + styleAttr + '"></div>';
            case 'articleList':
                return articleListPreview(p, css);
            case 'productList':
                return productListPreview(p, css);
            case 'jobList':
                return jobListPreview(p, css);
            case 'logo': {
                var logoSrc = safeUrl(p.src);
                return '<a class="sb-public-logo" href="' + esc(safeUrl(p.href || '/')) + '" style="' + styleAttr + '">' + (logoSrc ? '<img src="' + esc(logoSrc) + '" alt="' + esc(p.text || 'Logo') + '">' : '<strong>' + esc(p.text || '企业名称') + '</strong>') + '</a>';
            }
            case 'navigation': {
                var vertical = p.direction === 'vertical';
                var submenuEffect = ['slide-down', 'fade', 'zoom', 'none'].indexOf(p.submenuEffect) >= 0 ? p.submenuEffect : 'slide-down';
                var submenuItemEffect = ['background', 'shift', 'underline', 'left-bar', 'none'].indexOf(p.submenuItemEffect) >= 0 ? p.submenuItemEffect : 'background';
                var submenuDuration = Math.max(100, Math.min(1000, Number(p.submenuDuration || 200)));
                var submenuAccentColor = /^#[0-9a-f]{6}$/i.test(String(p.submenuAccentColor || '')) ? p.submenuAccentColor : '#0054a6';
                return '<nav class="sb-public-nav ' + (vertical ? 'is-vertical' : 'is-horizontal') + '" data-subnav-effect="' + submenuEffect + '" data-subnav-item-effect="' + submenuItemEffect + '" style="' + styleAttr + ';--sb-subnav-duration:' + submenuDuration + 'ms;--sb-subnav-accent:' + submenuAccentColor + '">' + navigationHtml(navigation) + '</nav>';
            }
            case 'search':
                return '<form class="sb-public-search" action="' + esc(safeUrl(p.action || '/search')) + '" method="get" onsubmit="return false" style="' + styleAttr + '"><input name="q" placeholder="' + esc(p.placeholder || '搜索') + '"><button type="button">⌕</button></form>';
            case 'language':
                return '<span class="sb-public-language" style="' + styleAttr + '">' + esc(p.text || '中文 / EN') + '</span>';
            case 'contact':
                return '<div class="sb-public-contact" style="' + styleAttr + '">' + ['phone','email','address'].map(function(key){return p[key] ? '<div>' + esc(p[key]) + '</div>' : '';}).join('') + '</div>';
            case 'contactForm':
                return '<form class="sb-contact-form" style="' + styleAttr + '" onsubmit="return false"><div class="sb-contact-form-row"><input type="text" placeholder="' + esc(p.namePlaceholder || '姓名') + '"><input type="tel" placeholder="' + esc(p.phonePlaceholder || '联系电话') + '"></div><input type="email" placeholder="' + esc(p.emailPlaceholder || '电子邮箱') + '"><textarea rows="5" placeholder="' + esc(p.messagePlaceholder || '留言内容') + '"></textarea>' + (p.showCaptcha === false ? '' : '<div class="sb-contact-form-row sb-contact-captcha"><input type="text" placeholder="验证码"><span class="sb-captcha-preview">验证码图片</span></div>') + '<button type="button" class="sb-public-button sb-public-button-primary">' + esc(p.buttonText || '提交留言') + '</button></form>';
            case 'social':
                return '<div class="sb-public-social" style="' + styleAttr + '"><strong>' + esc(p.text == null ? '关注我们' : p.text) + '</strong><div>' + esc(p.links || '') + '</div></div>';
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
            var empty = children ? '' : '<div class="sb-drop-empty">拖入组件，或点击左侧添加</div>';
            return '<section class="' + classes + '"' + attributes + ' style="' + esc(css) + '">'
                + children + handles + empty + '</section>';
        }

        var leafClass = 'sb-node sb-leaf-node' + selected + hidden;
        // Put editing metadata on the rendered element itself: no toolbar or layout-changing wrapper.
        return leafPreview(node).replace(/^<([a-z0-9]+)([^>]*)>/i, function (_, tag, attrs) {
            if (/class="/.test(attrs)) attrs = attrs.replace('class="', 'class="' + leafClass + ' ');
            else attrs += ' class="' + leafClass + '"';
            return '<' + tag + attrs + ' data-node-id="' + esc(node.id) + '" data-node-type="' + esc(node.type) + '">';
        });
    }

    function render(documentModel, selectedId) {
        var nodes = documentModel && documentModel.nodes ? documentModel.nodes : [];
        if (!nodes.length) return '<div class="sb-root-drop sb-children" data-parent-id=""><div class="designer-empty">从左侧添加布局或预设开始设计页面</div></div>';
        return '<div class="sb-root-drop sb-children" data-parent-id="">' + nodes.map(function (node) { return nodeHtml(node, selectedId); }).join('') + '</div>';
    }

    root.DesignerRenderer = { render: render, styleText: styleText, escapeHtml: esc, setNavigation: function(items) { navigation = items || []; } };
})(window);
