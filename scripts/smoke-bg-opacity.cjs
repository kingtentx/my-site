// 无头冒烟测试：校验背景不透明度 bgOpacity 在 styleText 中的 rgba 转换
const fs = require('fs');
const vm = require('vm');
const path = require('path');

const base = 'D:/MyProject/my-site/src/CIMC.WebSite/wwwroot/static/site-builder';
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

['core/registry.js', 'components/default-components.js', 'renderer/designer-renderer.js'].forEach(f => {
  vm.runInContext(fs.readFileSync(path.join(base, f), 'utf8'), sandbox, { filename: f });
});

const R = sandbox.SiteBuilder.Registry;
const Renderer = sandbox.SiteBuilder.DesignerRenderer;
let fail = 0;
function check(name, cond, extra) {
  if (cond) console.log('PASS ' + name);
  else { console.log('FAIL ' + name + (extra ? ' -> ' + extra : '')); fail++; }
}

// 1. inspector 背景组含 bgOpacity 字段；样式字段按 styleScope 裁剪，验证组件声明了 background 令牌
const section = R.get('section');
const scopes = section.styleScope || section.defaults && section.defaults.styleScope || [];
const hasBgScope = Array.isArray(scopes) ? scopes.includes('background') : String(section.styleScope).includes('background');
check('section 声明 background 样式令牌', hasBgScope, JSON.stringify(section.styleScope || scopes));

// 2. styleText 转换规则
function st(style) { return Renderer.styleText(style); }

check('6位hex+0.5 → rgba', st({ backgroundColor: '#1677ff', bgOpacity: 0.5 }).includes('background-color:rgba(22,119,255,0.5)'), st({ backgroundColor: '#1677ff', bgOpacity: 0.5 }));
check('3位hex+0.3 → rgba', st({ backgroundColor: '#fff', bgOpacity: 0.3 }).includes('background-color:rgba(255,255,255,0.3)'), st({ backgroundColor: '#fff', bgOpacity: 0.3 }));
check('bgOpacity=1 保持原样', st({ backgroundColor: '#1677ff', bgOpacity: 1 }).includes('background-color:#1677ff'), st({ backgroundColor: '#1677ff', bgOpacity: 1 }));
check('无 bgOpacity 保持原样', st({ backgroundColor: '#1677ff' }).includes('background-color:#1677ff'), '');
check('bgOpacity 缺省值不输出', !st({ backgroundColor: '#1677ff' }).includes('bgOpacity'), '');
check('bgOpacity 非法值忽略', st({ backgroundColor: '#1677ff', bgOpacity: 'abc' }).includes('background-color:#1677ff'), '');
check('bgOpacity 超界(>1)忽略', st({ backgroundColor: '#1677ff', bgOpacity: 1.5 }).includes('background-color:#1677ff'), '');
check('非hex背景色原样', st({ backgroundColor: 'red', bgOpacity: 0.5 }).includes('background-color:red'), '');
check('仅透明度无背景色不输出', !st({ bgOpacity: 0.5 }).includes('background-color'), st({ bgOpacity: 0.5 }));
check('背景图+半透明背景色共存', (() => {
  const s = st({ backgroundColor: '#000', bgOpacity: 0.4, backgroundImage: '/a.png' });
  return s.includes('background-color:rgba(0,0,0,0.4)') && s.includes('background-image');
})(), '');

// 3. 真实渲染路径：节点 style 走 styleText
const node = { type: 'section', id: 'n1', props: {}, style: { backgroundColor: '#ffffff', bgOpacity: 0.7 }, children: [] };
const html = Renderer.render({ nodes: [node] });
check('render 输出含 rgba 背景', /background-color:rgba\(255,255,255,0\.7\)/.test(html), html.slice(0, 200));

console.log(fail ? ('FAILED ' + fail) : 'ALL PASS');
process.exit(fail ? 1 : 0);
