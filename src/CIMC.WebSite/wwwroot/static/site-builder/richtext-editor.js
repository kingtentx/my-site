(function (window, document) {
    'use strict';

    var siteBuilder = window.SiteBuilder = window.SiteBuilder || {};
    var active = null;

    function flush(commit) {
        if (!active) return;
        var current = active;
        var html = siteBuilder.RichText.sanitize(current.editor.txt.html());
        var previousHtml = current.lastHtml;
        var changed = html !== previousHtml;
        current.source.value = html;
        current.lastHtml = html;
        if ((changed || commit) && typeof siteBuilder.RichTextEditor.onChange === 'function') {
            siteBuilder.RichTextEditor.onChange(current.nodeId, html, !!commit, previousHtml);
        }
    }

    function destroy() {
        if (!active) return;
        var current = active;
        active = null;
        if (current.resizeObserver) current.resizeObserver.disconnect();
        if (current.alignDropdowns) window.removeEventListener('resize', current.alignDropdowns);
        current.editor.config.onchange = null;
        current.editor.config.onblur = null;
        if (typeof current.editor.destroy === 'function') current.editor.destroy();
    }

    function mount(nodeId, locked) {
        var panel = document.getElementById('propsPanel');
        var source = panel && panel.querySelector('textarea[data-richtext="true"]');
        if (!source) { destroy(); return; }
        if (active && active.source === source && active.nodeId === nodeId) return;
        destroy();
        if (!window.wangEditor) {
            source.insertAdjacentHTML('beforebegin', '<div class="props-hint">wangEditor 未加载，暂时可用下方 HTML 输入框编辑。</div>');
            return;
        }

        var container = document.createElement('div');
        container.className = 'sb-wangeditor';
        container.innerHTML = siteBuilder.RichText.sanitize(source.value) || '<p><br></p>';
        source.parentNode.insertBefore(container, source);

        var editor = new window.wangEditor(container);
        editor.config.menus = [
            'head', 'bold', 'fontSize', 'italic', 'underline', 'strikeThrough',
            'foreColor', 'link', 'list', 'justify', 'quote', 'image', 'table', 'undo', 'redo'
        ];
        editor.config.fontSizes = {
            small: { name: '13px', value: '2' },
            normal: { name: '16px', value: '3' },
            'x-large': { name: '24px', value: '5' }
        };
        editor.config.colors = ['#d92d20', '#1677ff', '#15803d'];
        editor.config.showLinkImg = false;
        editor.config.uploadFileName = 'file';
        editor.config.uploadImgServer = '/Upload/UploadImage';
        editor.config.uploadImgHooks = {
            customInsert: function (insert, result) {
                if (result && result.url && siteBuilder.RichText.safeImageSrc(result.url)) insert(result.url);
            }
        };
        editor.config.onchangeTimeout = 100;
        editor.config.onchange = function () {
            if (active && active.editor === editor) flush(false);
        };
        editor.config.onblur = function () {
            window.setTimeout(function () {
                if (active && active.editor === editor && !container.contains(document.activeElement)) flush(true);
            }, 0);
        };

        active = { nodeId: nodeId, source: source, container: container, editor: editor, lastHtml: source.value };
        try {
            editor.create();
            var toolbar = container.querySelector('.w-e-toolbar');
            if (toolbar) {
                var alignDropdowns = function () {
                    var bottom = toolbar.getBoundingClientRect().bottom;
                    Array.prototype.forEach.call(toolbar.querySelectorAll('.w-e-menu'), function (menu) {
                        menu.style.setProperty('--sb-menu-dropdown-top', Math.ceil(bottom - menu.getBoundingClientRect().top) + 'px');
                    });
                };
                active.alignDropdowns = alignDropdowns;
                alignDropdowns();
                if (window.ResizeObserver) {
                    active.resizeObserver = new window.ResizeObserver(alignDropdowns);
                    active.resizeObserver.observe(toolbar);
                }
                window.addEventListener('resize', alignDropdowns);
            }
            if (locked) {
                container.classList.add('is-locked');
                var content = container.querySelector('.w-e-text');
                if (content) content.setAttribute('contenteditable', 'false');
            }
            source.classList.add('sb-richtext-source');
            source.style.display = 'none';
        } catch (error) {
            active = null;
            container.remove();
            if (window.console) console.error('[SiteBuilder] wangEditor initialization failed', error);
            source.insertAdjacentHTML('beforebegin', '<div class="props-hint">wangEditor 初始化失败，暂时可用下方 HTML 输入框编辑。</div>');
        }
    }

    siteBuilder.RichTextEditor = { mount: mount, flush: flush, destroy: destroy, onChange: null };
})(window, document);
