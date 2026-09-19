(function (window, document) {
    'use strict';
    var entrances = ['fade', 'fade-up', 'slide-left', 'slide-right', 'zoom'];
    var hovers = ['lift', 'zoom', 'shadow'];
    var easing = ['ease-out', 'ease-in-out', 'linear'];
    var running = new WeakMap();
    var counters = new WeakMap();
    var preference = window.matchMedia('(prefers-reduced-motion: reduce)');
    function number(value, fallback, min, max) {
        if (value == null || value === '') return fallback;
        value = Number(value);
        return Number.isFinite(value) ? Math.min(max, Math.max(min, value)) : fallback;
    }
    function normalize(style) {
        style = style || {};
        return {
            entrance: entrances.indexOf(style.effectEntrance) >= 0 ? style.effectEntrance : '',
            hover: hovers.indexOf(style.effectHover) >= 0 ? style.effectHover : '',
            duration: number(style.effectDuration, 600, 100, 3000),
            delay: number(style.effectDelay, 0, 0, 3000),
            distance: number(style.effectDistance, 24, 0, 100),
            repeat: style.effectRepeat === true || style.effectRepeat === 'true',
            easing: easing.indexOf(style.effectEasing) >= 0 ? style.effectEasing : 'ease-out',
            counter: style.effectCounter === true || style.effectCounter === 'true'
        };
    }
    function playCounter(element, settings) {
        if (!settings.counter || preference.matches || !element.isConnected || element.children.length) return false;
        var original = element.dataset.sbCounterText || element.textContent;
        var match = String(original).match(/^(\s*)(\d+(?:\.\d+)?)(.*)$/s);
        if (!match) return false;
        var previous = counters.get(element);
        if (previous) window.cancelAnimationFrame(previous.frame);
        element.dataset.sbCounterText = original;
        var target = Number(match[2]);
        var decimals = (match[2].split('.')[1] || '').length;
        var start = 0;
        var state = { frame: 0 };
        function step(timestamp) {
            if (!start) start = timestamp;
            if (preference.matches || !element.isConnected) {
                element.textContent = original;
                counters.delete(element);
                return;
            }
            var progress = Math.min((timestamp - start) / settings.duration, 1);
            var value = target * (1 - Math.pow(1 - progress, 3));
            element.textContent = match[1] + (decimals ? value.toFixed(decimals) : String(Math.round(value))) + match[3];
            if (progress < 1) state.frame = window.requestAnimationFrame(step);
            else { element.textContent = original; counters.delete(element); }
        }
        state.frame = window.requestAnimationFrame(step);
        counters.set(element, state);
        return true;
    }
    function play(element, settings, preview) {
        var previous = running.get(element);
        if (previous) previous.cancel();
        if (preference.matches || !element.animate || !element.isConnected) return false;
        var from = { opacity: 0 }, to = { opacity: 1 };
        if (settings.entrance === 'fade-up') from.translate = '0 ' + settings.distance + 'px';
        if (settings.entrance === 'slide-left') from.translate = '-' + settings.distance + 'px 0';
        if (settings.entrance === 'slide-right') from.translate = settings.distance + 'px 0';
        if (from.translate) to.translate = '0 0';
        if (settings.entrance === 'zoom') { from.scale = 0.9; to.scale = 1; }
        var frames = [from, to];
        if (!settings.entrance) {
            if (!preview || !settings.hover) return false;
            frames = settings.hover === 'shadow'
                ? [{boxShadow:'0 0 0 transparent'},{boxShadow:'0 14px 32px rgba(15,23,42,.18)'},{boxShadow:'0 0 0 transparent'}]
                : settings.hover === 'lift' ? [{translate:'0 0'},{translate:'0 -6px'},{translate:'0 0'}]
                : [{scale:1},{scale:1.035},{scale:1}];
        }
        // Hover preview goes out and back using the same 220 ms transition as the public CSS.
        var animation = element.animate(frames, {duration: settings.entrance ? settings.duration : 440, delay: settings.entrance ? settings.delay : 0, easing: settings.entrance ? settings.easing : 'ease', fill:'backwards'});
        running.set(element, animation);
        function clear() { if (running.get(element) === animation) running.delete(element); }
        animation.onfinish = clear;
        animation.oncancel = clear;
        return true;
    }
    function init(root) {
        (root || document).querySelectorAll('[data-sb-effects]').forEach(function (element) {
            if (element.closest('#canvas') || element.dataset.sbEffectsReady) return;
            var raw;
            try { raw = JSON.parse(element.dataset.sbEffects); } catch (_) { return; }
            var settings = normalize(raw);
            element.dataset.sbEffectsReady = 'true';
            // Inline text/link roots need a transformable box, without adding a wrapper.
            if ((settings.entrance || settings.hover || settings.counter) && window.getComputedStyle(element).display === 'inline') element.dataset.sbEffectsInline = 'true';
            if (settings.hover) element.dataset.sbHover = settings.hover;
            if ((!settings.entrance && !settings.counter) || preference.matches) return;
            if (!window.IntersectionObserver) { play(element, settings); playCounter(element, settings); return; }
            var entered = false;
            var observer = new IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    if (!element.isConnected || preference.matches) { observer.disconnect(); return; }
                    if (!entry.isIntersecting) { entered = false; return; }
                    if (entered || running.has(element)) return;
                    entered = true;
                    play(element, settings);
                    playCounter(element, settings);
                    if (!settings.repeat) observer.disconnect();
                });
            }, { threshold: 0 });
            observer.observe(element);
        });
    }
    function reduceMotion() {
        if (preference.matches) document.querySelectorAll('[data-sb-effects], #canvas .sb-node').forEach(function (element) {
            var animation = running.get(element);
            if (animation) animation.cancel();
            var counter = counters.get(element);
            if (counter) {
                window.cancelAnimationFrame(counter.frame);
                if (element.dataset.sbCounterText) element.textContent = element.dataset.sbCounterText;
                counters.delete(element);
            }
        });
    }
    if (preference.addEventListener) preference.addEventListener('change', reduceMotion);
    window.SiteBuilderEffects = { normalize: normalize, init: init, preview: function(element, style) { var settings = normalize(style); var animated = play(element, settings, true); var counted = playCounter(element, settings); return animated || counted; } };
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', function () { init(); });
    else init();
})(window, document);
