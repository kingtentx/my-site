// 无头冒烟测试：文本/标题「换行方式」textWrap —— 自适应所在元素宽度
const fs = require('fs');
const vm = require('vm');
const path = require('path');

const base = 'D:/MyProject/my-site/src/CIMC.WebSite/wwwroot/site-builder';
const sandbox = { console, setTimeout: () => 0, clearTimeout: () => {} };
sandbox.window = sandbox;
sandbox.document = {
  createElement: () => ({ style: {}, setAttribute() {}, appendChild() {} }),
  getElementById: () => null,
  querySelector: () => null,
  querySelectorAll: () => [],
  head: { appendChild() {} },
  body: {}
};
vm.createContext(sandbox);

['core/registry.js', 'core/store.js', 'components/default-components.js', 'renderer/designer-renderer.js'].forEach(f => {
  vm.runInContext(fs.readFileSync(path.join(base, f), 'utf8'), sandbox, { filename: f });
});

const R = sandbox.SiteBuilder.Registry;
const Renderer = sandbox.SiteBuilder.DesignerRenderer;
let fail = 0;
function check(name, cond, extra) {
  if (cond) console.log('PASS ' + name);
  else { console.log('FAIL ' + name + (extra ? ' -> ' + extra : '')); fail++; }
}

function render(type, props) {
  return Renderer.render({ nodes: [{ type: type, id: 'x1', props: props || {}, style: {} }] });
}
function wrapClass(type, props) {
  const html = render(type, props);
  const m = html.match(/class="sb-(?:text|heading) is-wrap-[a-z]+"/);
  return m ? m[0] : html.slice(0, 160);
}

// 1. 组件定义：text / heading 都有 textWrap，默认 auto，3 个选项
['text', 'heading'].forEach(type => {
  const comp = R.get(type);
  const keys = (comp.inspector || []).map(x => x.key);
  check(type + ' 含 textWrap 字段', keys.includes('textWrap'), keys.join(','));
  check(type + ' 默认 textWrap=auto', comp.defaults.textWrap === 'auto', String(comp.defaults.textWrap));
  const field = (comp.inspector || []).find(x => x.key === 'textWrap');
  check(type + ' textWrap 选项数=3', !!field && field.options.length === 3, String(field && field.options.length));
  check(type + ' 选项含 auto/nowrap/break',
    !!field && ['auto', 'nowrap', 'break'].every(v => field.options.some(o => o.value === v)),
    field ? field.options.map(o => o.value).join(',') : '');
});

// 2. 渲染 class 输出
check('text 默认 is-wrap-auto', wrapClass('text', {}).includes('is-wrap-auto'), wrapClass('text', {}));
check('text nowrap', wrapClass('text', { textWrap: 'nowrap' }).includes('is-wrap-nowrap'), wrapClass('text', { textWrap: 'nowrap' }));
check('text break', wrapClass('text', { textWrap: 'break' }).includes('is-wrap-break'), wrapClass('text', { textWrap: 'break' }));
check('text 非法值回落 auto', wrapClass('text', { textWrap: 'xxx' }).includes('is-wrap-auto'), wrapClass('text', { textWrap: 'xxx' }));
check('heading 默认 is-wrap-auto', wrapClass('heading', {}).includes('is-wrap-auto'), wrapClass('heading', {}));
check('heading nowrap', wrapClass('heading', { textWrap: 'nowrap' }).includes('is-wrap-nowrap'), wrapClass('heading', { textWrap: 'nowrap' }));
check('heading 非法值回落 auto', wrapClass('heading', { textWrap: 123 }).includes('is-wrap-auto'), wrapClass('heading', { textWrap: 123 }));

// 3. 换行与转义仍然正确（nowrap 下 \n 依旧转 <br>，只是不再自动折行）
check('nowrap 下字面 \\n 仍转 <br>',
  render('text', { text: '13800138000\\n13800138001', textWrap: 'nowrap' }).includes('13800138000<br>13800138001'), '');
check('HTML 仍被转义', render('text', { text: '<b>x</b>' }).includes('&lt;b&gt;'), '');

// 4. CSS：三档策略 + 宽度约束
const css = fs.readFileSync(path.join(base, 'runtime.css'), 'utf8');
check('CSS .sb-text 有 max-width:100%', /\.sb-runtime \.sb-text \{[^}]*max-width:\s*100%/.test(css), '');
check('CSS .sb-text 有 min-width:0', /\.sb-runtime \.sb-text \{[^}]*min-width:\s*0/.test(css), '');
check('CSS nowrap 有 ellipsis', /is-wrap-nowrap[^{]*\{[^}]*text-overflow:\s*ellipsis/.test(css), '');
check('CSS break 有 word-break:break-all', /is-wrap-break[^{]*\{[^}]*word-break:\s*break-all/.test(css), '');
check('CSS grid 子项 min-width:0', /sb-public-grid > \* \{ min-width: 0/.test(css), '');

// 5. 服务端 _Node.cshtml 有同款白名单
const nodeCshtml = fs.readFileSync('D:/MyProject/my-site/src/CIMC.WebSite/Views/Shared/SiteBuilder/_Node.cshtml', 'utf8');
check('_Node.cshtml 定义 TextWrapClass', nodeCshtml.includes('string TextWrapClass()'), '');
check('_Node.cshtml 白名单含三档', /"auto", "nowrap", "break"/.test(nodeCshtml), '');
check('_Node.cshtml text 分支用 TextWrapClass', /class="sb-text @TextWrapClass\(\)"/.test(nodeCshtml), '');
check('_Node.cshtml heading 分支用 TextWrapClass', /class="sb-heading @headWrap"/.test(nodeCshtml), '');

console.log(fail ? ('FAILED ' + fail) : 'ALL PASS');
process.exit(fail ? 1 : 0);
