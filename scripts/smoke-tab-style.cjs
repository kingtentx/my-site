// 无头冒烟测试：校验三个列表控件的 Tab 样式字段与预览 class 输出
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

// 1. 三个控件都有 tabStyle / tabAlign 字段与默认值
['articleList', 'productList', 'jobList'].forEach(type => {
  const def = R.get ? R.get(type) : null;
  if (!def) { check(type + ' 注册存在', false); return; }
  const keys = (def.inspector || []).map(x => x.key);
  check(type + ' 含 tabStyle 字段', keys.includes('tabStyle'), keys.join(','));
  check(type + ' 含 tabAlign 字段', keys.includes('tabAlign'), keys.join(','));
  check(type + ' 默认 tabStyle=pill', def.defaults.tabStyle === 'pill', def.defaults.tabStyle);
  check(type + ' 默认 tabAlign=left', def.defaults.tabAlign === 'left', def.defaults.tabAlign);
  const styleField = (def.inspector || []).find(x => x.key === 'tabStyle');
  check(type + ' tabStyle 选项数=5', styleField.options.length === 5, String(styleField.options.length));
});

// 2. 预览渲染出的 Tab 容器 class
sandbox.SiteBuilder.CategoryData = {
  article: { rootId: 10, options: [{ value: 19, text: '公司新闻', parentId: 10 }, { value: 22, text: '行业资讯', parentId: 10 }] }
};

function renderArticle(props) {
  const node = { type: 'articleList', id: 'n1', props: props, style: {} };
  return Renderer.render({ nodes: [node] });
}

const cases = [
  [{}, 'sb-content-tabs is-tab-pill align-left'],
  [{ tabStyle: 'underline', tabAlign: 'center' }, 'sb-content-tabs is-tab-underline align-center'],
  [{ tabStyle: 'segmented', tabAlign: 'right' }, 'sb-content-tabs is-tab-segmented align-right'],
  [{ tabStyle: 'card' }, 'sb-content-tabs is-tab-card align-left'],
  [{ tabStyle: 'plain', tabAlign: 'center' }, 'sb-content-tabs is-tab-plain align-center'],
  [{ tabStyle: '不存在的样式', tabAlign: 'xx' }, 'sb-content-tabs is-tab-pill align-left']
];

cases.forEach(([props, expected]) => {
  const html = renderArticle(Object.assign({ pageSize: 3 }, props));
  check('预览 class ' + JSON.stringify(props), html.indexOf('class="' + expected + '"') >= 0, html.slice(0, 160));
});

// 3. 关闭 Tab 时不输出容器
const noTab = renderArticle({ showTabs: false, pageSize: 3 });
check('showTabs=false 不渲染 Tab', noTab.indexOf('sb-content-tabs') < 0);

console.log(fail === 0 ? '\nALL PASS' : '\nFAILED: ' + fail);
process.exit(fail === 0 ? 0 : 1);
