// Dependency-free runtime regression tests: node scripts/site-builder-effects.test.cjs
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const source = fs.readFileSync(path.join(__dirname, '../src/CIMC.WebSite/wwwroot/site-builder/effects.js'), 'utf8');

function environment({ reduced = false, observer = true } = {}) {
    const elements = [], observers = [], frames = new Map(); let frameId = 0;
    const preference = { matches: reduced, addEventListener(_, handler) { this.change = handler; } };
    const window = {
        matchMedia: () => preference,
        getComputedStyle: element => ({ display: element.display || 'block' }),
        requestAnimationFrame(callback) { frames.set(++frameId, callback); return frameId; },
        cancelAnimationFrame(id) { frames.delete(id); }
    };
    if (observer) window.IntersectionObserver = class {
        constructor(callback) { this.callback = callback; observers.push(this); }
        observe(element) { this.element = element; }
        disconnect() { this.disconnected = true; }
        emit(visible) { if (!this.disconnected) this.callback([{ isIntersecting: visible }]); }
    };
    const document = { readyState: 'complete', querySelectorAll: () => elements };
    const context = { window, document, WeakMap, IntersectionObserver: window.IntersectionObserver };
    vm.runInNewContext(source, context);
    function node(style, options = {}) {
        const element = {
            dataset: { sbEffects: JSON.stringify(style) }, isConnected: true, calls: [], children: [], textContent: options.text || '',
            closest: () => options.canvas || false, display: options.display,
            animate(frames, timing) {
                const animation = { frames, timing, cancel() { this.cancelled = true; if (this.oncancel) this.oncancel(); } };
                this.calls.push(animation);
                return animation;
            }
        };
        elements.push(element);
        return element;
    }
    return { api: window.SiteBuilderEffects, elements, observers, preference, node, tick(time) { const callbacks=[...frames.values()];frames.clear();callbacks.forEach(callback=>callback(time)); } };
}

const env = environment();
const invalid = env.api.normalize({ effectEntrance: '<script>', effectHover: 'bad', effectDuration: Infinity, effectDelay: -100, effectDistance: 999, effectEasing: 'bad' });
assert.equal(invalid.entrance, '');
assert.equal(invalid.hover, '');
assert.equal(invalid.duration, 600);
assert.equal(invalid.delay, 0);
assert.equal(invalid.distance, 100);
assert.equal(invalid.easing, 'ease-out');
assert.equal(env.api.normalize({ effectDuration: '99999' }).duration, 3000);
assert.equal(env.api.normalize({ effectRepeat: 'false' }).repeat, false);
assert.equal(env.api.normalize({ effectRepeat: 'true' }).repeat, true);
assert.equal(env.api.normalize({ effectCounter: true }).counter, true);

for (const entrance of ['fade', 'fade-up', 'slide-left', 'slide-right', 'zoom']) {
    const element = env.node({});
    assert.equal(env.api.preview(element, { effectEntrance: entrance, effectDuration: 900, effectDelay: 200 }), true);
    const animation = element.calls[0];
    assert.equal(animation.timing.duration, 900);
    assert.equal(animation.timing.delay, 200);
    assert.equal(animation.timing.fill, 'backwards');
    assert.equal(animation.frames[0].opacity, 0);
    assert.equal(animation.frames[1].opacity, 1);
    assert.equal(animation.frames[0].transform, undefined); // Do not overwrite authored transforms.
    env.api.preview(element, { effectEntrance: entrance });
    assert.equal(animation.cancelled, true);
}
for (const hover of ['lift', 'zoom', 'shadow']) {
    const element = env.node({});
    assert.equal(env.api.preview(element, { effectHover: hover }), true);
    assert.equal(element.calls[0].frames.length, 3);
    assert.equal(element.calls[0].timing.duration, 440);
    assert.equal(element.calls[0].timing.delay, 0);
}

const scroll = environment();
const once = scroll.node({ effectEntrance: 'fade' });
const repeat = scroll.node({ effectEntrance: 'fade-up', effectRepeat: true });
const inline = scroll.node({ effectHover: 'zoom' }, { display: 'inline' });
const canvas = scroll.node({ effectEntrance: 'fade' }, { canvas: true });
const malformed = scroll.node({}); malformed.dataset.sbEffects = '{invalid';
scroll.api.init(); scroll.api.init();
assert.equal(scroll.observers.length, 2); // Initialization is idempotent; canvas is excluded.
assert.equal(inline.dataset.sbEffectsInline, 'true');
assert.equal(canvas.calls.length, 0);
assert.equal(once.calls.length, 0);
scroll.observers[0].emit(true); scroll.observers[0].emit(false); scroll.observers[0].emit(true);
assert.equal(once.calls.length, 1);
scroll.observers[1].emit(true); repeat.calls[0].onfinish();
scroll.observers[1].emit(false); scroll.observers[1].emit(true);
assert.equal(repeat.calls.length, 2);
scroll.preference.matches = true; scroll.preference.change();
assert.equal(repeat.calls[1].cancelled, true);
assert.equal(scroll.api.preview(repeat, { effectEntrance: 'fade' }), false);

const reduced = environment({ reduced: true });
const staticNode = reduced.node({ effectEntrance: 'fade' }); reduced.api.init();
assert.equal(staticNode.calls.length, 0);
const fallback = environment({ observer: false });
const fallbackNode = fallback.node({ effectEntrance: 'fade' }); fallback.api.init();
assert.equal(fallbackNode.calls.length, 1);
const countEnv=environment();const countNode=countEnv.node({effectCounter:true,effectDuration:1000},{text:'33.41万㎡'});countEnv.api.init();countEnv.observers[0].emit(true);countEnv.tick(1);countEnv.tick(501);assert.notEqual(countNode.textContent,'33.41万㎡');countEnv.tick(1001);assert.equal(countNode.textContent,'33.41万㎡');
console.log('PASS: normalization, five entrances, three hover previews, counter, repeat/once, canvas isolation, reduced motion, observer fallback');
