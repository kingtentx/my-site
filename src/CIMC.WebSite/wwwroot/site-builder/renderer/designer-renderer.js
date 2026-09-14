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

    var RICH_TEXT_TAGS = { p:1, br:1, strong:1, b:1, em:1, i:1, u:1, s:1, strike:1, ul:1, ol:1, li:1, blockquote:1, h1:1, h2:1, h3:1, h4:1, a:1 };
    var RICH_TEXT_MARKUP = /<\/?(?:p|br|strong|b|em|i|u|s|strike|ul|ol|li|blockquote|h[1-4]|a)\b/i;

    function safeRichTextUrl(value) {
        var url = String(value == null ? '' : value).trim();
        if (!url) return '';
        if (/^(#|\/|\.\/|\.\.\/)/.test(url)) return url;
        return /^(https?:|mailto:|tel:)/i.test(url) ? url : '';
    }

    function plainTextToRichHtml(value) {
        var text = String(value == null ? '' : value).replace(/\r\n?/g, '\n');
        if (!text) return '';
        var box = document.createElement('div');
        text.split('\n').forEach(function (line) {
            var p = document.createElement('p');
            if (line) p.textContent = line;
            else p.appendChild(document.createElement('br'));
            box.appendChild(p);
        });
        return box.innerHTML;
    }

    function sanitizeRichText(value) {
        var html = String(value == null ? '' : value);
        if (!html.trim()) return '';
        if (!RICH_TEXT_MARKUP.test(html)) return plainTextToRichHtml(html);

        var template = document.createElement('template');
        template.innerHTML = html;
        var output = document.createElement('div');

        function copyChildren(source, target) {
            Array.prototype.slice.call(source.childNodes || []).forEach(function (child) {
                if (child.nodeType === 3) {
                    target.appendChild(document.createTextNode(child.nodeValue || ''));
                    return;
                }
                if (child.nodeType !== 1) return;

                var sourceTag = String(child.tagName || '').toLowerCase();
                if (sourceTag === 'div') sourceTag = 'p';
                if (sourceTag === 'span' || !RICH_TEXT_TAGS[sourceTag]) {
                    copyChildren(child, target);
                    return;
                }

                var safe = document.createElement(sourceTag === 'strike' ? 's' : sourceTag);
                if (sourceTag === 'a') {
                    var href = safeRichTextUrl(child.getAttribute('href'));
                    if (href) safe.setAttribute('href', href);
                    if (child.getAttribute('target') === '_blank') {
                        safe.setAttribute('target', '_blank');
                        safe.setAttribute('rel', 'noopener noreferrer');
                    }
                }
                if (sourceTag !== 'br') copyChildren(child, safe);
                target.appendChild(safe);
            });
        }

        copyChildren(template.content, output);
        return output.innerHTML;
    }

    function styleText(style) {
        style = style || {};
        var map = {
            paddingTop:'padding-top', paddingRight:'padding-right', paddingBottom:'padding-bottom', paddingLeft:'padding-left',
            marginTop:'margin-top', marginRight:'margin-right', marginBottom:'margin-bottom', marginLeft:'margin-left',
            backgroundColor:'background-color', color:'color', maxWidth:'max-width', width:'width', minHeight:'min-height',
            gap:'gap', borderRadius:'border-radius', textAlign:'text-align', borderTopWidth:'border-top-width',
            borderTopStyle:'border-top-style', borderTopColor:'border-top-color', position:'position', top:'top',
            zIndex:'z-index', boxShadow:'box-shadow', fontSize:'font-size', fontWeight:'font-weight',
            lineHeight:'line-height', letterSpacing:'letter-spacing', alignItems:'align-items', height:'height', objectFit:'object-fit'
        };
        var parts = [];
        Object.keys(map).forEach(function (key) {
            if (style[key] !== undefined && style[key] !== null && style[key] !== '') parts.push(map[key] + ':' + style[key]);
        });
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
            html += '<div class="sb-banner-shade"></div><div class="sb-banner-copy"><h1>' + esc(p.title || '') + '</h1><p>' + esc(p.description || '') + '</p>';
            if(p.buttonText) html += '<a class="sb-public-button" href="' + esc(safeUrl(p.buttonHref || '#')) + '">' + esc(p.buttonText) + '</a>';
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
                return '<div class="sb-text sb-rich-text" style="white-space:normal;' + styleAttr + '">' + sanitizeRichText(p.text == null ? '文本内容' : p.text) + '</div>';
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
                return '<a class="sb-public-button sb-public-button-' + esc(p.variant || 'primary') + '" href="' + esc(safeUrl(p.href || '#')) + '" target="' + (p.target === '_blank' ? '_blank' : '_self') + '" style="' + styleAttr + '">' + esc(p.text || '按钮') + '</a>';
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
                return '<div class="sb-placeholder" style="' + styleAttr + '">文章列表 · ' + Number(p.pageSize || 6) + ' 条 · ' + Number(p.columns || 3) + ' 列</div>';
            case 'productList':
                return '<div class="sb-placeholder" style="' + styleAttr + '">产品列表 · ' + Number(p.pageSize || 8) + ' 条 · ' + Number(p.columns || 4) + ' 列</div>';
            case 'jobList':
                return '<div class="sb-placeholder" style="' + styleAttr + '">招聘列表 · ' + Number(p.pageSize || 10) + ' 条</div>';
            case 'logo': {
                var logoSrc = safeUrl(p.src);
                return '<a class="sb-public-logo" href="' + esc(safeUrl(p.href || '/')) + '" style="' + styleAttr + '">' + (logoSrc ? '<img src="' + esc(logoSrc) + '" alt="' + esc(p.text || 'Logo') + '">' : '<strong>' + esc(p.text || '企业名称') + '</strong>') + '</a>';
            }
            case 'navigation': {
                var vertical = p.direction === 'vertical';
                return '<nav class="sb-public-nav ' + (vertical ? 'is-vertical' : 'is-horizontal') + '" style="' + styleAttr + '">' + navigationHtml(navigation) + '</nav>';
            }
            case 'search':
                return '<form class="sb-public-search" action="' + esc(safeUrl(p.action || '/search')) + '" method="get" onsubmit="return false" style="' + styleAttr + '"><input name="q" placeholder="' + esc(p.placeholder || '搜索') + '"><button type="button">⌕</button></form>';
            case 'language':
                return '<span class="sb-public-language" style="' + styleAttr + '">' + esc(p.text || '中文 / EN') + '</span>';
            case 'contact':
                return '<div class="sb-public-contact" style="' + styleAttr + '">' + ['phone','email','address'].map(function(key){return p[key] ? '<div>' + esc(p[key]) + '</div>' : '';}).join('') + '</div>';
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

    root.DesignerRenderer = { render: render, styleText: styleText, escapeHtml: esc, sanitizeRichText: sanitizeRichText, setNavigation: function(items) { navigation = items || []; } };

    // 文本属性仍保留 textarea 作为无脚本降级；装修器运行后将它增强为轻量富文本编辑器。
    function initRichTextInspector() {
        if (!root.Inspector || !document.getElementById('propsPanel')) return;

        if (!root.Inspector._richTextReadPatched) {
            var baseReadValue = root.Inspector.readValue;
            root.Inspector.readValue = function (el) {
                if (el && el.classList && el.classList.contains('sb-rich-editor-content')) {
                    return sanitizeRichText(el.innerHTML);
                }
                return baseReadValue(el);
            };
            root.Inspector._richTextReadPatched = true;
        }

        if (!document.getElementById('sbRichTextEditorCss')) {
            var style = document.createElement('style');
            style.id = 'sbRichTextEditorCss';
            style.textContent = [
                '.sb-rich-editor{width:100%;border:1px solid #dbe3ec;border-radius:6px;background:#fff;overflow:hidden}',
                '.sb-rich-editor:focus-within{border-color:#1677ff;box-shadow:0 0 0 2px rgba(22,119,255,.08)}',
                '.sb-rich-editor-toolbar{display:flex;flex-wrap:wrap;gap:4px;padding:6px;border-bottom:1px solid #edf1f5;background:#f8fafc}',
                '.sb-rich-editor-toolbar button{height:25px;min-width:27px;padding:0 7px;border:1px solid #dbe3ec;border-radius:4px;background:#fff;color:#475569;font-size:11px;cursor:pointer}',
                '.sb-rich-editor-toolbar button:hover{border-color:#1677ff;color:#1677ff;background:#eff6ff}',
                '.sb-rich-editor-toolbar .is-wide{min-width:38px}',
                '.sb-rich-editor-separator{width:1px;height:20px;margin:2px 1px;background:#e2e8f0}',
                '.sb-rich-editor-content{min-height:150px;max-height:340px;overflow:auto;padding:10px 11px;outline:0;color:#334155;font-size:13px;line-height:1.75;white-space:normal}',
                '.sb-rich-editor-content:empty:before{content:attr(data-placeholder);color:#94a3b8;pointer-events:none}',
                '.sb-rich-editor-content p{margin:0 0 8px}.sb-rich-editor-content p:last-child{margin-bottom:0}',
                '.sb-rich-editor-content h1,.sb-rich-editor-content h2,.sb-rich-editor-content h3,.sb-rich-editor-content h4{margin:5px 0 8px;line-height:1.35}',
                '.sb-rich-editor-content ul,.sb-rich-editor-content ol{margin:5px 0 8px;padding-left:22px}',
                '.sb-rich-editor-content blockquote{margin:6px 0;padding:5px 9px;border-left:3px solid #cbd5e1;background:#f8fafc;color:#64748b}',
                '.sb-rich-editor-content a{color:#1677ff;text-decoration:underline}',
                '.sb-field-row.is-tall .sb-rich-editor{flex:1;min-width:0}',
                '.sb-field-row.is-tall>textarea[data-rich-text-source]{display:none!important}'
            ].join('\n');
            document.head.appendChild(style);
        }

        function selectionInside(editor) {
            var selection = window.getSelection && window.getSelection();
            if (!selection || !selection.rangeCount) return null;
            var range = selection.getRangeAt(0);
            return editor.contains(range.commonAncestorContainer) ? range.cloneRange() : null;
        }
        function rememberSelection(editor) {
            var range = selectionInside(editor);
            if (range) editor._sbRichRange = range;
        }
        function restoreSelection(editor) {
            var range = editor._sbRichRange;
            var selection = window.getSelection && window.getSelection();
            if (!range || !selection) return;
            selection.removeAllRanges();
            selection.addRange(range);
        }
        function dispatchChange(editor) {
            editor.dispatchEvent(new Event('change', { bubbles:true }));
        }
        function runCommand(editor, command, value) {
            editor.focus({ preventScroll:true });
            restoreSelection(editor);
            try { document.execCommand(command, false, value == null ? null : value); } catch (e) { }
            rememberSelection(editor);
        }
        function enhance() {
            var panel = document.getElementById('propsPanel');
            if (!panel) return;
            var title = panel.querySelector('.props-title strong');
            if (!title || String(title.textContent || '').trim() !== '文本') return;
            var textarea = panel.querySelector('textarea[data-area="props"][data-key="text"]');
            if (!textarea || textarea.getAttribute('data-rich-text-source') === '1') return;

            var area = textarea.getAttribute('data-area') || 'props';
            var key = textarea.getAttribute('data-key') || 'text';
            var id = textarea.id || ('field-' + area + '-' + key);
            textarea.setAttribute('data-rich-text-source', '1');
            textarea.removeAttribute('data-area');
            textarea.removeAttribute('data-key');
            textarea.removeAttribute('id');
            textarea.setAttribute('aria-hidden', 'true');

            var editorBox = document.createElement('div');
            editorBox.className = 'sb-rich-editor';
            editorBox.innerHTML = '<div class="sb-rich-editor-toolbar" role="toolbar" aria-label="富文本工具栏">'
                + '<button type="button" class="is-wide" data-rich-cmd="formatBlock" data-rich-value="p" title="正文">正文</button>'
                + '<button type="button" data-rich-cmd="formatBlock" data-rich-value="h2" title="二级标题">H2</button>'
                + '<button type="button" data-rich-cmd="formatBlock" data-rich-value="h3" title="三级标题">H3</button>'
                + '<span class="sb-rich-editor-separator"></span>'
                + '<button type="button" data-rich-cmd="bold" title="加粗"><b>B</b></button>'
                + '<button type="button" data-rich-cmd="italic" title="斜体"><i>I</i></button>'
                + '<button type="button" data-rich-cmd="underline" title="下划线"><u>U</u></button>'
                + '<button type="button" data-rich-cmd="strikeThrough" title="删除线"><s>S</s></button>'
                + '<span class="sb-rich-editor-separator"></span>'
                + '<button type="button" data-rich-cmd="insertUnorderedList" title="无序列表">• 列表</button>'
                + '<button type="button" data-rich-cmd="insertOrderedList" title="有序列表">1. 列表</button>'
                + '<button type="button" data-rich-cmd="formatBlock" data-rich-value="blockquote" title="引用">❝</button>'
                + '<span class="sb-rich-editor-separator"></span>'
                + '<button type="button" data-rich-cmd="createLink" title="添加链接">链接</button>'
                + '<button type="button" data-rich-cmd="unlink" title="移除链接">取消链接</button>'
                + '<button type="button" data-rich-cmd="removeFormat" title="清除格式">清格式</button>'
                + '</div>';

            var editor = document.createElement('div');
            editor.className = 'sb-rich-editor-content';
            editor.id = id;
            editor.contentEditable = 'true';
            editor.setAttribute('role', 'textbox');
            editor.setAttribute('aria-multiline', 'true');
            editor.setAttribute('data-placeholder', '请输入文本内容');
            editor.setAttribute('data-area', area);
            editor.setAttribute('data-key', key);
            editor.innerHTML = sanitizeRichText(textarea.value || '');
            editorBox.appendChild(editor);
            textarea.parentNode.insertBefore(editorBox, textarea.nextSibling);

            editor.addEventListener('keyup', function () { rememberSelection(editor); });
            editor.addEventListener('mouseup', function () { rememberSelection(editor); });
            editor.addEventListener('input', function () { rememberSelection(editor); });
            editor.addEventListener('blur', function () { dispatchChange(editor); });
            editor.addEventListener('paste', function (event) {
                event.preventDefault();
                var clipboard = event.clipboardData || window.clipboardData;
                var text = clipboard ? clipboard.getData('text/plain') : '';
                runCommand(editor, 'insertText', text);
            });

            editorBox.querySelectorAll('[data-rich-cmd]').forEach(function (button) {
                button.addEventListener('mousedown', function (event) {
                    event.preventDefault();
                    rememberSelection(editor);
                });
                button.addEventListener('click', function (event) {
                    event.preventDefault();
                    var command = button.getAttribute('data-rich-cmd');
                    var value = button.getAttribute('data-rich-value');
                    if (command === 'createLink') {
                        restoreSelection(editor);
                        var href = window.prompt('请输入链接地址（http(s)、mailto、tel 或站内相对地址）', 'https://');
                        if (href == null) return;
                        href = safeRichTextUrl(href);
                        if (!href) { window.alert('链接地址格式不安全或不受支持'); return; }
                        runCommand(editor, 'createLink', href);
                        var selection = window.getSelection && window.getSelection();
                        var anchor = selection && selection.anchorNode ? (selection.anchorNode.nodeType === 1 ? selection.anchorNode : selection.anchorNode.parentElement) : null;
                        if (anchor && anchor.closest) {
                            anchor = anchor.closest('a');
                            if (anchor && editor.contains(anchor)) { anchor.target = '_blank'; anchor.rel = 'noopener noreferrer'; }
                        }
                        return;
                    }
                    runCommand(editor, command, value);
                });
            });
        }

        enhance();
        var panel = document.getElementById('propsPanel');
        if (window.MutationObserver && panel && !panel._sbRichTextObserver) {
            panel._sbRichTextObserver = new MutationObserver(function () { enhance(); });
            panel._sbRichTextObserver.observe(panel, { childList:true, subtree:true });
        }
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initRichTextInspector);
    else window.setTimeout(initRichTextInspector, 0);
})(window);
