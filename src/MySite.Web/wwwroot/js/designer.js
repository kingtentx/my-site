const state = {
  pageKey: window.siteDesignerPageKey || 'home',
  page: null,
  templates: [],
  selectedIndex: -1,
  dragIndex: null
};

const $ = (selector) => document.querySelector(selector);
const clone = (value) => JSON.parse(JSON.stringify(value || {}));
const uid = () => (crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(16).slice(2));

function toast(message) {
  const old = document.querySelector('.toast');
  if (old) old.remove();
  const el = document.createElement('div');
  el.className = 'toast';
  el.textContent = message;
  document.body.appendChild(el);
  setTimeout(() => el.remove(), 2200);
}

async function requestJson(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || response.statusText);
  }
  return response.json();
}

async function loadDesigner() {
  state.pageKey = ($('#pageKey').value || 'home').trim().replace(/^\/+|\/+$/g, '') || 'home';
  state.templates = await requestJson('/api/site-builder/templates');
  state.page = await requestJson(`/api/site-builder/page/${encodeURIComponent(state.pageKey)}`);
  state.page.sections = state.page.sections || [];
  $('#pageTitle').value = state.page.title || '';
  state.selectedIndex = state.page.sections.length ? 0 : -1;
  renderAll();
}

function newSection(template) {
  const section = clone(template.defaultSection);
  section.id = uid();
  section.component = template.key;
  section.name = section.name || template.name;
  section.title = section.title || template.name;
  section.isEnabled = true;
  section.images = section.images || [];
  section.settings = section.settings || {};
  return section;
}

function renderPalette() {
  $('#componentPalette').innerHTML = state.templates.map(t => `
    <div class="component-item" draggable="true" data-component="${t.key}">
      <b>${escapeHtml(t.name)}</b>
      <p>${escapeHtml(t.description || '')}</p>
    </div>`).join('');

  document.querySelectorAll('.component-item').forEach(item => {
    item.addEventListener('dragstart', e => {
      e.dataTransfer.setData('component', item.dataset.component);
    });
    item.addEventListener('dblclick', () => addComponent(item.dataset.component));
  });
}

function renderCanvas() {
  const canvas = $('#canvas');
  const sections = state.page.sections || [];
  if (!sections.length) {
    canvas.innerHTML = '<div class="canvas-empty">从左侧拖入组件，或双击组件添加到页面</div>';
  } else {
    canvas.innerHTML = sections.map((s, i) => `
      <div class="section-node ${i === state.selectedIndex ? 'selected' : ''} ${s.isEnabled ? '' : 'disabled'}" draggable="true" data-index="${i}">
        <h3>${escapeHtml(s.title || s.name || s.component)}</h3>
        <p>组件：${escapeHtml(s.component)} ｜ 名称：${escapeHtml(s.name || '')}</p>
        <div class="node-actions">
          <button class="icon-btn" data-action="up">上移</button>
          <button class="icon-btn" data-action="down">下移</button>
          <button class="icon-btn" data-action="copy">复制</button>
          <button class="icon-btn" data-action="toggle">${s.isEnabled ? '停用' : '启用'}</button>
          <button class="icon-btn" data-action="remove">删除</button>
        </div>
      </div>`).join('');
  }

  canvas.ondragover = e => e.preventDefault();
  canvas.ondrop = e => {
    e.preventDefault();
    const component = e.dataTransfer.getData('component');
    if (component) {
      addComponent(component);
    }
  };

  document.querySelectorAll('.section-node').forEach(node => {
    node.addEventListener('click', e => {
      const index = Number(node.dataset.index);
      const action = e.target.dataset.action;
      if (action) {
        handleAction(action, index);
        e.stopPropagation();
        return;
      }
      state.selectedIndex = index;
      renderAll();
    });
    node.addEventListener('dragstart', () => { state.dragIndex = Number(node.dataset.index); });
    node.addEventListener('dragover', e => e.preventDefault());
    node.addEventListener('drop', e => {
      e.preventDefault();
      const targetIndex = Number(node.dataset.index);
      moveSection(state.dragIndex, targetIndex);
    });
  });
}

function renderProps() {
  const props = $('#props');
  const section = state.page.sections[state.selectedIndex];
  if (!section) {
    props.innerHTML = '<p style="color:#667085;line-height:1.8">请选择画布中的模块进行配置。</p>';
    return;
  }

  props.innerHTML = `
    <label>组件类型</label>
    <input value="${escapeAttr(section.component)}" disabled />
    <label>后台名称</label>
    <input data-field="name" value="${escapeAttr(section.name)}" />
    <label>标题</label>
    <input data-field="title" value="${escapeAttr(section.title)}" />
    <label>副标题</label>
    <textarea data-field="subTitle">${escapeHtml(section.subTitle || '')}</textarea>
    <label>按钮文字</label>
    <input data-field="linkText" value="${escapeAttr(section.linkText)}" />
    <label>按钮链接</label>
    <input data-field="linkUrl" value="${escapeAttr(section.linkUrl)}" placeholder="/about 或 https://..." />
    <label>图片地址（一行一个）</label>
    <textarea id="imagesInput">${escapeHtml((section.images || []).join('\n'))}</textarea>
    <label>扩展配置 JSON</label>
    <textarea id="settingsInput" class="code">${escapeHtml(JSON.stringify(section.settings || {}, null, 2))}</textarea>
    <div style="display:flex;gap:10px;margin-top:12px">
      <button class="btn small" id="applySettings" type="button">应用 JSON</button>
      <button class="btn ghost small" id="formatSettings" type="button">格式化</button>
    </div>`;

  props.querySelectorAll('[data-field]').forEach(input => {
    input.addEventListener('input', () => {
      section[input.dataset.field] = input.value;
      renderCanvas();
      renderProps();
    });
  });

  $('#imagesInput').addEventListener('input', e => {
    section.images = e.target.value.split(/[\n,;]+/).map(x => x.trim()).filter(Boolean);
  });

  $('#applySettings').addEventListener('click', () => {
    const parsed = parseSettings();
    if (parsed) {
      section.settings = parsed;
      toast('JSON 已应用');
    }
  });

  $('#formatSettings').addEventListener('click', () => {
    const parsed = parseSettings();
    if (parsed) $('#settingsInput').value = JSON.stringify(parsed, null, 2);
  });
}

function renderAll() {
  renderPalette();
  renderCanvas();
  renderProps();
}

function addComponent(component) {
  const template = state.templates.find(x => x.key === component);
  if (!template) return;
  state.page.sections.push(newSection(template));
  state.selectedIndex = state.page.sections.length - 1;
  renderAll();
}

function handleAction(action, index) {
  if (action === 'up') moveSection(index, Math.max(0, index - 1));
  if (action === 'down') moveSection(index, Math.min(state.page.sections.length - 1, index + 1));
  if (action === 'copy') {
    const copied = clone(state.page.sections[index]);
    copied.id = uid();
    copied.name = `${copied.name || copied.component} - 复制`;
    state.page.sections.splice(index + 1, 0, copied);
    state.selectedIndex = index + 1;
    renderAll();
  }
  if (action === 'toggle') {
    state.page.sections[index].isEnabled = !state.page.sections[index].isEnabled;
    renderAll();
  }
  if (action === 'remove' && confirm('确认删除这个模块？')) {
    state.page.sections.splice(index, 1);
    state.selectedIndex = state.page.sections.length ? Math.min(index, state.page.sections.length - 1) : -1;
    renderAll();
  }
}

function moveSection(from, to) {
  if (from === null || from === undefined || from === to) return;
  const list = state.page.sections;
  const item = list.splice(from, 1)[0];
  list.splice(to, 0, item);
  state.selectedIndex = to;
  renderAll();
}

function parseSettings() {
  try {
    return JSON.parse($('#settingsInput').value || '{}');
  } catch (e) {
    toast(`JSON 格式错误：${e.message}`);
    return null;
  }
}

async function savePage() {
  const selected = state.page.sections[state.selectedIndex];
  if (selected && $('#settingsInput')) {
    const parsed = parseSettings();
    if (!parsed) return;
    selected.settings = parsed;
  }

  state.page.pageKey = ($('#pageKey').value || 'home').trim().replace(/^\/+|\/+$/g, '') || 'home';
  state.page.title = $('#pageTitle').value || state.page.title || state.page.pageKey;
  state.page.sections.forEach((s, i) => s.sort = (i + 1) * 10);

  await requestJson('/api/site-builder/page', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(state.page)
  });

  toast('保存成功');
  setTimeout(() => location.href = `/Admin/Designer?pageKey=${encodeURIComponent(state.page.pageKey)}`, 500);
}

function escapeHtml(value) {
  return String(value ?? '').replace(/[&<>"']/g, s => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[s]));
}

function escapeAttr(value) {
  return escapeHtml(value).replace(/`/g, '&#96;');
}

$('#btnReload').addEventListener('click', loadDesigner);
$('#btnSave').addEventListener('click', savePage);
$('#pageKey').addEventListener('change', loadDesigner);
loadDesigner().catch(err => toast(err.message || '加载失败'));
