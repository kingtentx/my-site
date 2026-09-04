(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;
    var Tree = root.Tree;

    function createDocument(name) {
        return { schemaVersion: 1, name: name || '', nodes: [], settings: {} };
    }

    function normalizeNode(node) {
        var def = Registry.get(node.type);
        if (!def) throw new Error('页面中存在未知组件: ' + node.type);
        node.id = node.id || Registry.uid(node.type);
        node.version = node.version || def.version || 1;
        node.name = node.name || def.name || node.type;
        node.visible = node.visible !== false;
        node.locked = node.locked === true;
        node.props = node.props || {};
        node.style = node.style || {};
        node.bindings = node.bindings || {};
        node.actions = node.actions || {};
        node.children = Array.isArray(node.children) ? node.children : [];
        node.slots = node.slots || {};
        node.children.forEach(normalizeNode);
        Object.keys(node.slots).forEach(function (key) {
            node.slots[key] = Array.isArray(node.slots[key]) ? node.slots[key] : [];
            node.slots[key].forEach(normalizeNode);
        });
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
        if (!value || Array.isArray(value) || typeof value !== 'object') {
            throw new Error('旧版数组页面结构已不再支持，请新建页面结构。');
        }
        if (Number(value.schemaVersion || value.SchemaVersion) !== 1) {
            throw new Error('不支持的页面结构版本。');
        }
        var nodes = value.nodes || value.Nodes;
        this.document = {
            schemaVersion: 1,
            name: value.name || value.Name || '',
            nodes: Array.isArray(nodes) ? nodes : [],
            settings: value.settings || value.Settings || {}
        };
        this.document.nodes.forEach(normalizeNode);
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

    Store.prototype.change = function (fn) {
        fn(this.document);
        this.snapshot();
        this.emit();
    };

    Store.prototype.emit = function () {
        var self = this;
        this.listeners.forEach(function (fn) { fn(self); });
    };

    Store.prototype.subscribe = function (fn) { this.listeners.push(fn); };
    Store.prototype.select = function (id) { this.selectedId = id || null; this.emit(); };
    Store.prototype.selected = function () { return Tree.find(this.document.nodes, this.selectedId); };

    Store.prototype.add = function (type, parentId) {
        var self = this, node = Registry.create(type);
        this.change(function (doc) {
            if (!Tree.insert(doc.nodes, node, parentId || null)) Tree.insert(doc.nodes, node, null);
        });
        self.selectedId = node.id;
        self.emit();
        return node;
    };

    Store.prototype.remove = function (id) {
        var self = this;
        this.change(function (doc) { Tree.remove(doc.nodes, id); });
        if (this.selectedId === id) this.selectedId = null;
        self.emit();
    };

    Store.prototype.duplicate = function (id) {
        var copy = null, self = this;
        this.change(function (doc) { copy = Tree.duplicate(doc.nodes, id); });
        if (copy) this.selectedId = copy.id;
        self.emit();
        return copy;
    };

    Store.prototype.move = function (id, parentId, index) {
        this.change(function (doc) { Tree.move(doc.nodes, id, parentId || null, index); });
    };

    Store.prototype.update = function (id, area, key, value) {
        this.change(function (doc) {
            var node = Tree.find(doc.nodes, id);
            if (!node) return;
            if (area === 'node') node[key] = value;
            else {
                node[area] = node[area] || {};
                node[area][key] = value;
            }
        });
    };

    Store.prototype.undo = function () {
        if (this.history.length <= 1) return;
        this.future.push(this.history.pop());
        this.document = JSON.parse(this.history[this.history.length - 1]);
        this.selectedId = null;
        this.emit();
    };

    Store.prototype.redo = function () {
        if (!this.future.length) return;
        var json = this.future.pop();
        this.history.push(json);
        this.document = JSON.parse(json);
        this.selectedId = null;
        this.emit();
    };

    Store.prototype.serialize = function () { return JSON.stringify(this.document); };

    root.Store = Store;
    root.createDocument = createDocument;
})(window);