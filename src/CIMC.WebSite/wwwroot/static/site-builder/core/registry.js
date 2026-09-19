(function (window) {
    'use strict';

    var root = window.SiteBuilder = window.SiteBuilder || {};
    var definitions = {};

    function clone(value) {
        return JSON.parse(JSON.stringify(value == null ? null : value));
    }

    function uid(type) {
        return (type || 'node') + '_' + Date.now().toString(36) + '_' + Math.random().toString(36).slice(2, 8);
    }

    function register(definition) {
        if (!definition || !definition.type) throw new Error('组件定义必须包含 type');
        definitions[definition.type] = definition;
        return definition;
    }

    function get(type) {
        return definitions[type] || null;
    }

    function all() {
        return Object.keys(definitions).map(function (key) { return definitions[key]; });
    }

    function groups() {
        var result = {};
        all().forEach(function (item) {
            var group = item.group || 'other';
            result[group] = result[group] || [];
            result[group].push(item);
        });
        return result;
    }

    function create(type, overrides) {
        var def = get(type);
        if (!def) throw new Error('未注册组件: ' + type);
        var node = {
            id: uid(type),
            type: type,
            version: def.version || 1,
            name: def.name || type,
            visible: true,
            locked: false,
            props: clone(def.defaults || {}),
            style: clone(def.styleDefaults || {}),
            bindings: {},
            actions: {},
            children: [],
            slots: {}
        };
        if (overrides) {
            Object.keys(overrides).forEach(function (key) { node[key] = clone(overrides[key]); });
        }
        return node;
    }

    root.Registry = {
        register: register,
        get: get,
        all: all,
        groups: groups,
        create: create,
        clone: clone,
        uid: uid
    };
})(window);