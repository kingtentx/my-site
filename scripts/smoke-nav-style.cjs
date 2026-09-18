// 无头冒烟测试：导航条目风格 itemStyle + 文本换行渲染
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

// 1. 组件定义
const nav = R.get('navigation');
const keys = (nav.inspector || []).map(x => x.key);
check('navigation 含 itemStyle 字段', keys.includes('itemStyle'), keys.join(','));
check('默认 itemStyle=underline', nav.defaults.itemStyle === 'underline', String(nav.defaults.itemStyle));
const styleField = (nav.inspector || []).find(x => x.key === 'itemStyle');
check('itemStyle 选项数=5', styleField && styleField.options.length === 5, String(styleField && styleField.options.length));
['alignment', 'itemGap', 'itemPaddingX', 'itemPaddingY'].forEach(key => {
  check('navigation 含 ' + key + ' 字段', keys.includes(key), keys.join(','));
});
check('navigation 不再暴露 wrapMode', !keys.includes('wrapMode'), keys.join(','));

const logo = R.get('logo');
const logoKeys = (logo.inspector || []).map(x => x.key);
check('Logo 含宽度设置', logoKeys.includes('logoWidth'), logoKeys.join(','));
check('Logo 含最大高度设置', logoKeys.includes('logoMaxHeight'), logoKeys.join(','));

// 2. 渲染 class 输出
function renderNav(props) {
  const node = { type: 'navigation', id: 'n1', props: props, style: {} };
  return Renderer.render({ nodes: [node] });
}
sandbox.SiteBuilder.DesignerRenderer.setNavigation([{ title: '首页', path: '/', isCurrent: true }, { title: '关于我们', path: '/about' }]);
check('默认输出 is-item-underline', renderNav({}).includes('sb-public-nav is-item-underline is-horizontal'), (renderNav({}).match(/sb-public-nav[^"]*/) || [''])[0]);
check('pill', renderNav({ itemStyle: 'pill' }).includes('is-item-pill'), '');
check('block', renderNav({ itemStyle: 'block' }).includes('is-item-block'), '');
check('left-bar', renderNav({ itemStyle: 'left-bar' }).includes('is-item-left-bar'), '');
check('plain', renderNav({ itemStyle: 'plain' }).includes('is-item-plain'), '');
check('非法值回落 underline', renderNav({ itemStyle: 'xxx' }).includes('is-item-underline'), '');
check('纵向+plain 组合', renderNav({ direction: 'vertical', itemStyle: 'plain' }).includes('is-item-plain is-vertical'), '');
const compactNav = renderNav({ alignment: 'right', itemGap: 4, itemPaddingX: 3, itemPaddingY: 5 });
check('导航输出右对齐 class', compactNav.includes('align-right'), compactNav.slice(0, 240));
check('导航输出可调间距变量', compactNav.includes('--sb-nav-gap:4px;--sb-nav-padding-x:3px;--sb-nav-padding-y:5px'), compactNav.slice(0, 320));
check('导航不输出换行模式 class', !/\bis-(?:wrap|nowrap)\b/.test(compactNav), compactNav.slice(0, 240));

const logoHtml = Renderer.render({ nodes: [{ type: 'logo', id: 'logo1', props: { src: '/logo.png', logoWidth: 360, logoMaxHeight: 96 }, style: {} }] });
check('Logo 宽度作用于图片', logoHtml.includes('width:360px'), logoHtml);
check('Logo 最大高度作用于图片', logoHtml.includes('max-height:96px'), logoHtml);

// 3. 文本换行：字面 \n 与真实换行都转 <br>
const html = renderNav({});
const textNode = { type: 'text', id: 't1', props: { text: '13800138000\\n13800138001' }, style: {} };
const textHtml = Renderer.render({ nodes: [textNode] });
check('字面 \\n 转 <br>', textHtml.includes('13800138000<br>13800138001'), textHtml.slice(0, 200));
const textNode2 = { type: 'text', id: 't2', props: { text: 'a\nb' }, style: {} };
check('真实换行转 <br>', Renderer.render({ nodes: [textNode2] }).includes('a<br>b'), '');
const textNode3 = { type: 'text', id: 't3', props: { text: '<b>x</b>' }, style: {} };
check('HTML 仍被转义', Renderer.render({ nodes: [textNode3] }).includes('&lt;b&gt;'), '');

// 4. CSS / 服务端渲染保持一致
const css = fs.readFileSync(path.join(base, 'runtime.css'), 'utf8');
check('横向导航强制单行', /sb-public-nav\.is-horizontal\s*\{\s*flex-wrap:\s*nowrap/.test(css), '');
check('导航宽度受所在列约束', /sb-public-nav\s*\{[^}]*max-width:\s*100%/.test(css), '');
check('Logo 图片取消 200px 硬上限', !/sb-public-logo img[^}]*max-width:\s*200px/.test(css), '');

const nodeCshtml = fs.readFileSync('D:/MyProject/my-site/src/CIMC.WebSite/Views/Shared/SiteBuilder/_Node.cshtml', 'utf8');
check('服务端输出 Logo 尺寸', nodeCshtml.includes('IntProp("logoWidth", 200)') && nodeCshtml.includes('IntProp("logoMaxHeight", 56)'), '');
check('服务端输出导航间距变量', nodeCshtml.includes('--sb-nav-gap:@(itemGap)px'), '');

console.log(fail ? ('FAILED ' + fail) : 'ALL PASS');
process.exit(fail ? 1 : 0);
