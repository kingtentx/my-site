(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;
    var Tree = root.Tree;

    function createDocument(name) {
        return { schemaVersion: 1, name: name || '', nodes: [], settings: { designWidth: 1200 } };
    }

    function pick(object, lower, upper, fallback) {
        if (object && object[lower] !== undefined && object[lower] !== null) return object[lower];
        if (object && object[upper] !== undefined && object[upper] !== null) return object[upper];
        return fallback;
    }

    function clampGridColumns(value) {
        var count = Math.round(Number(value || 2));
        if (!isFinite(count)) count = 2;
        return Math.max(1, Math.min(6, count));
    }

    function normalizeGridWidths(value, count) {
        count = clampGridColumns(count);
        var source = Array.isArray(value) ? value : [];
        var widths = source.map(function (item) { return Number(item); });
        var valid = widths.length === count && widths.every(function (item) { return isFinite(item) && item > 0; });
        if (!valid) {
            widths = [];
            for (var i = 0; i < count; i++) widths.push(100 / count);
        }
        var total = widths.reduce(function (sum, item) { return sum + item; }, 0) || 100;
        return widths.map(function (item) { return Math.round((item / total) * 1000) / 10; });
    }

    function reconcileGridNode(node, requestedCount) {
        if (!node || node.type !== 'grid') return node;
        node.props = node.props || {};
        node.children = Array.isArray(node.children) ? node.children : [];
        var count = clampGridColumns(requestedCount == null ? node.props.columns : requestedCount);
        var columns = [];
        var looseChildren = [];

        node.children.forEach(function (child) {
            if (child && child.type === 'column') columns.push(child);
            else if (child) looseChildren.push(child);
        });

        if (!columns.length) columns.push(Registry.create('column'));
        if (looseChildren.length) {
            columns[0].children = columns[0].children || [];
            Array.prototype.push.apply(columns[0].children, looseChildren);
        }

        while (columns.length < count) columns.push(Registry.create('column'));
        if (columns.length > count) {
            var target = columns[Math.max(0, count - 1)];
            target.children = target.children || [];
            columns.slice(count).forEach(function (extra) {
                if (extra && Array.isArray(extra.children) && extra.children.length) {
                    Array.prototype.push.apply(target.children, extra.children);
                }
            });
            columns = columns.slice(0, count);
        }

        node.children = columns;
        node.props.columns = count;
        node.props.columnWidths = normalizeGridWidths(node.props.columnWidths, count);
        return node;
    }

    function normalizeNode(source) {
        source = source || {};
        var type = pick(source, 'type', 'Type', '');
        var def = Registry.get(type);
        if (!def) throw new Error('页面中存在未知组件: ' + type);
        var children = pick(source, 'children', 'Children', []);
        var slots = pick(source, 'slots', 'Slots', {}) || {};
        var node = {
            id: pick(source, 'id', 'Id', '') || Registry.uid(type),
            type: type,
            version: Number(pick(source, 'version', 'Version', def.version || 1)) || 1,
            name: pick(source, 'name', 'Name', def.name || type),
            visible: pick(source, 'visible', 'Visible', true) !== false,
            locked: pick(source, 'locked', 'Locked', false) === true,
            props: pick(source, 'props', 'Props', {}) || {},
            style: pick(source, 'style', 'Style', {}) || {},
            bindings: pick(source, 'bindings', 'Bindings', {}) || {},
            actions: pick(source, 'actions', 'Actions', {}) || {},
            children: Array.isArray(children) ? children.map(normalizeNode) : [],
            slots: {}
        };
        Object.keys(slots).forEach(function (key) {
            var values = slots[key];
            node.slots[key] = Array.isArray(values) ? values.map(normalizeNode) : [];
        });
        if (node.type === 'grid') reconcileGridNode(node, node.props.columns);
        return node;
    }

    function Store(name) {
        this.document = createDocument(name);
        this.selectedId = null;
        this.history = [];
        this.future = [];
        this.listeners = [];
        this.snapshot();
    }

    Store.prototype.load = function (value) {
        if (!value || Array.isArray(value) || typeof value !== 'object') throw new Error('旧版数组页面结构已不再支持，请新建页面结构。');
        if (Number(pick(value, 'schemaVersion', 'SchemaVersion', 0)) !== 1) throw new Error('不支持的页面结构版本。');
        var nodes = pick(value, 'nodes', 'Nodes', []);
        this.document = {
            schemaVersion: 1,
            name: pick(value, 'name', 'Name', ''),
            nodes: Array.isArray(nodes) ? nodes.map(normalizeNode) : [],
            settings: pick(value, 'settings', 'Settings', {}) || {}
        };
        var width = Number(this.document.settings.designWidth);
        this.document.settings.designWidth = [1200, 1440, 1920].indexOf(width) >= 0 ? width : 1200;
        this.selectedId = null;
        this.history = [];
        this.future = [];
        this.snapshot();
        this.emit();
    };

    Store.prototype.snapshot = function () {
        var json = JSON.stringify(this.document);
        if (this.history.length && this.history[this.history.length - 1] === json) return;
        this.history.push(json);
        if (this.history.length > 80) this.history.shift();
        this.future = [];
    };
    Store.prototype.change = function (fn) { fn(this.document); this.snapshot(); this.emit(); };
    Store.prototype.emit = function () { var self=this; this.listeners.forEach(function (fn) { fn(self); }); };
    Store.prototype.subscribe = function (fn) { this.listeners.push(fn); };
    Store.prototype.select = function (id) { this.selectedId=id||null; this.emit(); };
    Store.prototype.selected = function () { return Tree.find(this.document.nodes, this.selectedId); };
    Store.prototype.add = function (type, parentId) {
        var self=this, node=Registry.create(type);
        if (type === 'grid') reconcileGridNode(node, node.props.columns);
        this.change(function (doc) { if (!Tree.insert(doc.nodes,node,parentId||null)) Tree.insert(doc.nodes,node,null); });
        self.selectedId=node.id; self.emit(); return node;
    };
    Store.prototype.remove = function (id) { var self=this; this.change(function (doc) { Tree.remove(doc.nodes,id); }); if(this.selectedId===id)this.selectedId=null; self.emit(); };
    Store.prototype.duplicate = function (id) { var copy=null,self=this; this.change(function(doc){copy=Tree.duplicate(doc.nodes,id);}); if(copy)this.selectedId=copy.id; self.emit(); return copy; };
    Store.prototype.move = function (id,parentId,index) {
        var moved = false;
        this.change(function(doc){ moved = Tree.move(doc.nodes,id,parentId||null,index); });
        return moved;
    };
    Store.prototype.update = function (id,area,key,value) { this.change(function(doc){var node=Tree.find(doc.nodes,id);if(!node)return;if(area==='node')node[key]=value;else{node[area]=node[area]||{};node[area][key]=value;}}); };
    Store.prototype.setGridColumns = function (id, count) {
        var changed = false;
        this.change(function (doc) {
            var node = Tree.find(doc.nodes, id);
            if (!node || node.type !== 'grid') return;
            reconcileGridNode(node, count);
            changed = true;
        });
        return changed;
    };
    Store.prototype.setGridWidths = function (id, widths) {
        var changed = false;
        this.change(function (doc) {
            var node = Tree.find(doc.nodes, id);
            if (!node || node.type !== 'grid') return;
            node.props = node.props || {};
            var count = clampGridColumns(node.props.columns || (node.children || []).length || 2);
            node.props.columnWidths = normalizeGridWidths(widths, count);
            changed = true;
        });
        return changed;
    };
    Store.prototype.undo = function () { if(this.history.length<=1)return;this.future.push(this.history.pop());this.document=JSON.parse(this.history[this.history.length-1]);this.selectedId=null;this.emit(); };
    Store.prototype.redo = function () { if(!this.future.length)return;var json=this.future.pop();this.history.push(json);this.document=JSON.parse(json);this.selectedId=null;this.emit(); };
    Store.prototype.serialize = function () { return JSON.stringify(this.document); };

    root.Store = Store;
    root.createDocument = createDocument;
    root.normalizeGridWidths = normalizeGridWidths;
    root.clampGridColumns = clampGridColumns;
})(window);
