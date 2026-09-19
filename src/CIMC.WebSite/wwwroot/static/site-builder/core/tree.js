(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var Registry = root.Registry;

    function childCollections(node) {
        var result = [];
        if (node && Array.isArray(node.children)) result.push(node.children);
        if (node && node.slots) {
            Object.keys(node.slots).forEach(function (key) {
                if (Array.isArray(node.slots[key])) result.push(node.slots[key]);
            });
        }
        return result;
    }

    function walk(nodes, callback, parent) {
        (nodes || []).forEach(function (node, index) {
            callback(node, parent || null, index, nodes);
            childCollections(node).forEach(function (children) { walk(children, callback, node); });
        });
    }

    function find(nodes, id) {
        var found = null;
        walk(nodes, function (node) { if (!found && node.id === id) found = node; });
        return found;
    }

    function locate(nodes, id) {
        var location = null;
        walk(nodes, function (node, parent, index, collection) {
            if (!location && node.id === id) location = { node: node, parent: parent, index: index, collection: collection };
        });
        return location;
    }

    function remove(nodes, id) {
        var location = locate(nodes, id);
        if (!location) return null;
        return location.collection.splice(location.index, 1)[0];
    }

    function canHaveChildren(node) {
        var def = node && Registry.get(node.type);
        return !!(def && def.container);
    }

    function insert(nodes, node, parentId, index) {
        var target = nodes;
        if (parentId) {
            var parent = find(nodes, parentId);
            if (!parent || !canHaveChildren(parent)) return false;
            parent.children = parent.children || [];
            target = parent.children;
        }
        if (index == null || index < 0 || index > target.length) index = target.length;
        target.splice(index, 0, node);
        return true;
    }

    function contains(node, id) {
        var found = false;
        childCollections(node).forEach(function (children) {
            if (find(children, id)) found = true;
        });
        return found;
    }

    function move(nodes, id, parentId, index) {
        var location = locate(nodes, id);
        if (!location) return false;
        if (parentId && (parentId === id || contains(location.node, parentId))) return false;
        var node = location.node;
        location.collection.splice(location.index, 1);
        if (!insert(nodes, node, parentId, index)) {
            location.collection.splice(location.index, 0, node);
            return false;
        }
        return true;
    }

    function renewIds(node) {
        node.id = Registry.uid(node.type);
        childCollections(node).forEach(function (children) {
            children.forEach(renewIds);
        });
        return node;
    }

    function duplicate(nodes, id) {
        var location = locate(nodes, id);
        if (!location) return null;
        var copy = renewIds(Registry.clone(location.node));
        location.collection.splice(location.index + 1, 0, copy);
        return copy;
    }

    root.Tree = {
        walk: walk,
        find: find,
        locate: locate,
        remove: remove,
        insert: insert,
        move: move,
        duplicate: duplicate,
        canHaveChildren: canHaveChildren
    };
})(window);