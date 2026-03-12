(function () {
  if (window.__gLovRuntimeReady) return;
  window.__gLovRuntimeReady = true;

  const state = {
    title: '',
    apiUrl: '',
    columns: [],
    dataFields: [],
    targetInputs: {},
    selectedValue: {},
    currentRow: null,
    formatDisplay: null,
    onConfirmCallback: null,
    onSelectCallback: null,
    columnMatches: [],
    filterItems: [],
    page: 1,
    pageSize: 50,
    bufferView: true,
    sortEnabled: false,
    sortKey: '',
    sortDir: 'asc',
    rows: [],
    hasMore: true,
    loading: false,
    query: '',
    sourceInputId: '',
    suppressAutoSearchUntil: 0
  };

  const dom = {
    modal: null,
    title: null,
    input: null,
    thead: null,
    tbody: null,
    gridWrap: null,
    btnSearch: null,
    btnOk: null,
    btnCancel: null,
    btnClose: null
  };

  function ensureDom() {
    if (document.getElementById('genericLovModal')) {
      bindDomRefs();
      return;
    }

    const hostEl = document.getElementById('gLovHost');
    const host = (!hostEl || hostEl.classList.contains('hidden')) ? document.body : hostEl;
    const wrapper = document.createElement('div');
    wrapper.innerHTML = `
<div id="genericLovModal" class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm hidden z-[950] items-center justify-center p-4 transition-opacity duration-200">
  <div class="bg-white rounded-lg shadow-2xl w-full max-w-2xl flex flex-col overflow-hidden border border-slate-300">
    <div class="bg-blue-600 text-white px-4 py-2 flex justify-between items-center cursor-move" id="genericLovTitleBar">
      <h3 class="text-sm font-bold flex items-center gap-2" id="genericLovTitle"></h3>
      <button type="button" id="genericLovCloseBtn" class="p-1 rounded text-white hover:bg-white/20 transition-colors">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
      </button>
    </div>

    <div class="p-4 flex-1 flex flex-col bg-slate-50">
      <div class="flex gap-2 items-center mb-4">
        <label class="text-sm font-bold text-slate-700 whitespace-nowrap">查詢 %</label>
        <input type="text" id="genericLovSearchInput" class="flex-1 border border-slate-300 px-3 py-1.5 rounded-sm shadow-inner text-sm focus:outline-none focus:border-blue-500" placeholder="輸入關鍵字後按 Enter 或查詢">
        <button type="button" id="genericLovSearchBtn" class="bg-white border border-slate-300 px-4 py-1.5 rounded-sm text-sm font-bold text-slate-700 hover:bg-slate-50 transition-colors shadow-sm">查詢</button>
      </div>

      <div id="genericLovGridWrap" class="border border-slate-300 bg-white flex-1 overflow-auto rounded-sm min-h-[300px] max-h-[420px]">
        <table class="w-full text-left text-sm border-collapse">
          <thead class="bg-slate-100 text-slate-700 sticky top-0 shadow-sm" id="genericLovThead"><tr></tr></thead>
          <tbody id="genericLovTableBody"></tbody>
        </table>
      </div>
    </div>

    <div class="bg-slate-100 border-t border-slate-300 px-4 py-3 flex justify-center gap-4">
      <button type="button" id="genericLovOkBtn" class="bg-blue-600 text-white border border-blue-700 px-6 py-1.5 rounded-sm shadow-sm text-sm font-bold hover:bg-blue-700 transition-colors">確定</button>
      <button type="button" id="genericLovCancelBtn" class="bg-white border text-slate-700 border-slate-400 px-6 py-1.5 rounded-sm shadow-sm text-sm font-bold hover:bg-slate-50 transition-colors">取消</button>
    </div>
  </div>
</div>`;

    while (wrapper.firstChild) host.appendChild(wrapper.firstChild);
    bindDomRefs();

    dom.btnClose.addEventListener('click', closeGenericLov);
    dom.btnCancel.addEventListener('click', closeGenericLov);
    dom.btnOk.addEventListener('click', confirmGenericLovSelection);
    dom.btnSearch.addEventListener('click', () => fetchGenericLovData(false));

    dom.input.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        fetchGenericLovData(false);
      }
    });

    dom.modal.addEventListener('click', (e) => {
      if (e.target === dom.modal) closeGenericLov();
    });

    dom.gridWrap.addEventListener('scroll', () => {
      const nearBottom = dom.gridWrap.scrollTop + dom.gridWrap.clientHeight >= dom.gridWrap.scrollHeight - 40;
      if (state.bufferView && nearBottom && state.hasMore && !state.loading) {
        fetchGenericLovData(true);
      }
    });

    window.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && dom.modal && dom.modal.style.display === 'flex') closeGenericLov();
    });
  }

  function bindDomRefs() {
    dom.modal = document.getElementById('genericLovModal');
    dom.title = document.getElementById('genericLovTitle');
    dom.input = document.getElementById('genericLovSearchInput');
    dom.thead = document.getElementById('genericLovThead');
    dom.tbody = document.getElementById('genericLovTableBody');
    dom.gridWrap = document.getElementById('genericLovGridWrap');
    dom.btnSearch = document.getElementById('genericLovSearchBtn');
    dom.btnOk = document.getElementById('genericLovOkBtn');
    dom.btnCancel = document.getElementById('genericLovCancelBtn');
    dom.btnClose = document.getElementById('genericLovCloseBtn');
  }

  function renderTitle(title) {
    dom.title.innerHTML = `
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
      ${escapeHtml(title || '查詢')}`;
  }

  function renderHeader() {
    const tr = dom.thead.querySelector('tr');
    tr.innerHTML = '';

    state.columns.forEach((col, idx) => {
      const key = state.dataFields[idx];
      const sortable = state.sortEnabled && !!key;
      const isCurrent = state.sortKey === key;
      const indicator = !sortable ? '' : (isCurrent ? (state.sortDir === 'asc' ? '▲' : '▼') : '↕');
      const th = document.createElement('th');
      th.className = `border-b border-r border-slate-300 px-3 py-2 font-bold whitespace-nowrap ${sortable ? 'cursor-pointer select-none' : ''}`;
      th.innerHTML = `<span class="inline-flex items-center gap-1"><span>${escapeHtml(col)}</span>${sortable ? `<span class="text-[11px] ${isCurrent ? 'text-blue-600' : 'text-slate-400'}">${indicator}</span>` : ''}</span>`;
      if (sortable) {
        th.addEventListener('click', () => {
          if (state.sortKey === key) state.sortDir = state.sortDir === 'asc' ? 'desc' : 'asc';
          else {
            state.sortKey = key;
            state.sortDir = 'asc';
          }
          renderHeader();
          renderRows();
        });
      }
      tr.appendChild(th);
    });
  }

  function openGenericLov(title, apiUrl, columns, dataFields, targetInputs, formatDisplay, onConfirmCallback, options) {
    ensureDom();

    if (title && typeof title === 'object' && !Array.isArray(title)) {
      const cfg = title;
      return openGenericLov(
        cfg.title || cfg.lovTitle || '查詢',
        cfg.api || cfg.apiUrl || cfg.lovApi || '',
        Array.isArray(cfg.columns) ? cfg.columns : (Array.isArray(cfg.lovColumns) ? cfg.lovColumns : []),
        Array.isArray(cfg.fields) ? cfg.fields : (Array.isArray(cfg.dataFields) ? cfg.dataFields : []),
        cfg.map || cfg.targetInputs || {},
        cfg.formatDisplay || cfg.formatter || null,
        cfg.onConfirm || cfg.onConfirmCallback || null,
        Object.assign({}, cfg.options || {}, {
          onSelect: cfg.onSelect || null,
          columnMatches: cfg.columnMatches || [],
          filterItems: cfg.filterItems || []
        })
      );
    }

    const opts = options || {};
    const pageSize = Number(opts.pageSize) > 0 ? Number(opts.pageSize) : 50;
    const bufferView = opts.bufferView !== false;
    const sortEnabled = opts.sortEnabled === true;
    const sourceInputId = typeof opts.sourceInputId === 'string' ? opts.sourceInputId : '';
    const initialQuery = typeof opts.initialQuery === 'string' ? opts.initialQuery : '';

    state.title = title || '查詢';
    state.apiUrl = typeof apiUrl === 'string' ? apiUrl : '';
    state.columns = Array.isArray(columns) ? columns : [];
    state.dataFields = Array.isArray(dataFields) ? dataFields : [];
    state.targetInputs = targetInputs || {};
    state.formatDisplay = formatDisplay;
    state.onConfirmCallback = onConfirmCallback;
    state.onSelectCallback = typeof opts.onSelect === 'function' ? opts.onSelect : null;
    state.columnMatches = Array.isArray(opts.columnMatches) ? opts.columnMatches : [];
    state.filterItems = Array.isArray(opts.filterItems) ? opts.filterItems : [];
    state.page = 1;
    state.pageSize = pageSize;
    state.bufferView = bufferView;
    state.sortEnabled = sortEnabled;
    state.sortKey = '';
    state.sortDir = 'asc';
    state.rows = [];
    state.hasMore = true;
    state.loading = false;
    state.query = '';
    state.sourceInputId = sourceInputId;
    state.selectedValue = {};
    state.currentRow = null;

    renderTitle(state.title);
    renderHeader();

    let firstQuery = (initialQuery || '').trim();
    if (!firstQuery && sourceInputId) {
      const sourceEl = document.getElementById(sourceInputId);
      if (sourceEl && typeof sourceEl.value !== 'undefined') {
        firstQuery = String(sourceEl.value || '').trim();
      }
    }
    dom.input.value = firstQuery;
    dom.tbody.innerHTML = '';
    dom.modal.classList.remove('hidden');
    dom.modal.style.display = 'flex';

    setTimeout(() => {
      dom.input.focus();
      fetchGenericLovData(false);
    }, 0);
  }

  function closeGenericLov() {
    if (!dom.modal) return;
    dom.modal.classList.add('hidden');
    dom.modal.style.display = 'none';
    state.selectedValue = {};
    state.currentRow = null;
  }

  function suppressAutoSearch(ms) {
    const hold = Number(ms) > 0 ? Number(ms) : 600;
    state.suppressAutoSearchUntil = Date.now() + hold;
  }

  async function fetchGenericLovData(loadMore) {
    if (!dom.tbody) return;
    if (state.loading) return;

    const query = (dom.input?.value || '').trim();
    if (!state.apiUrl) {
      dom.tbody.innerHTML = `<tr><td colspan="${Math.max(state.columns.length, 1)}" class="text-center py-4 text-red-500">LOV API 未設定</td></tr>`;
      return;
    }

    if (!loadMore || state.query !== query) {
      state.page = 1;
      state.hasMore = true;
      state.query = query;
      state.currentRow = null;
      dom.tbody.innerHTML = `<tr><td colspan="${Math.max(state.columns.length, 1)}" class="text-center py-4 text-slate-500">載入中...</td></tr>`;
    } else if (!state.bufferView || !state.hasMore) {
      return;
    } else {
      const loadingRow = document.createElement('tr');
      loadingRow.id = 'genericLovLoadingRow';
      loadingRow.innerHTML = `<td colspan="${Math.max(state.columns.length, 1)}" class="text-center py-2 text-slate-400 text-xs">載入更多...</td>`;
      dom.tbody.appendChild(loadingRow);
    }

    state.loading = true;
    try {
      const sep = state.apiUrl.includes('?') ? '&' : '?';
      const extraFilter = await buildFilterQueryString();
      const url = `${state.apiUrl}${sep}query=${encodeURIComponent(query)}&page=${state.page}&pageSize=${state.pageSize}${extraFilter}`;
      const response = await fetch(url);
      const result = await response.json();

      if (response.ok && result.status === 'success') {
        const data = Array.isArray(result.data) ? result.data : [];
        if (loadMore && state.query === query) state.rows = [...state.rows, ...data];
        else state.rows = [...data];
        renderRows();
        state.hasMore = typeof result.hasMore === 'boolean' ? result.hasMore : (data.length >= state.pageSize);
        if (state.hasMore) state.page += 1;
      } else if (!loadMore) {
        dom.tbody.innerHTML = `<tr><td colspan="${Math.max(state.columns.length, 1)}" class="text-center py-4 text-red-500">Error: ${escapeHtml(result?.message || '查詢失敗')}</td></tr>`;
      }
    } catch (err) {
      if (!loadMore) {
        dom.tbody.innerHTML = `<tr><td colspan="${Math.max(state.columns.length, 1)}" class="text-center py-4 text-red-500">Fetch Failed: ${escapeHtml(err?.message || 'Unknown error')}</td></tr>`;
      }
    } finally {
      document.getElementById('genericLovLoadingRow')?.remove();
      state.loading = false;
    }
  }

  function renderRows() {
    dom.tbody.innerHTML = '';

    const source = Array.isArray(state.rows) ? state.rows : [];
    const sortKey = state.sortKey;
    const sortDir = state.sortDir === 'desc' ? -1 : 1;
    const rows = !sortKey ? source : [...source].sort((a, b) => {
      const av = a?.[sortKey] ?? '';
      const bv = b?.[sortKey] ?? '';
      if (av === bv) return 0;
      return av < bv ? -sortDir : sortDir;
    });

    if (!rows.length) {
      dom.tbody.innerHTML = `<tr><td colspan="${Math.max(state.columns.length, 1)}" class="text-center py-4 text-slate-500">查無資料</td></tr>`;
      return;
    }

    rows.forEach((item, index) => {
      const tr = document.createElement('tr');
      const rowBaseClass = `cursor-pointer transition-colors border-b border-slate-200 ${(index % 2 === 0) ? 'bg-white' : 'bg-slate-50'}`;
      tr.className = rowBaseClass;
      tr.dataset.rowBaseClass = rowBaseClass;
      tr.addEventListener('click', () => selectRow(tr, item));
      tr.addEventListener('dblclick', async () => {
        selectRow(tr, item);
        await confirmGenericLovSelection();
      });

      state.dataFields.forEach((field, idx) => {
        const td = document.createElement('td');
        td.className = 'px-3 py-1.5 border-r border-slate-200 truncate max-w-[200px]' + (idx === 0 ? ' font-mono text-blue-700' : ' text-slate-700 font-medium');
        const val = item[field];
        td.innerText = val === 0 ? '0' : (val || '');
        tr.appendChild(td);
      });

      dom.tbody.appendChild(tr);
      if (index === 0) selectRow(tr, item);
    });
  }

  function selectRow(rowEl, rowData) {
    dom.tbody.querySelectorAll('tr').forEach((tr) => {
      const baseClass = tr.dataset.rowBaseClass || 'cursor-pointer transition-colors border-b border-slate-200 bg-white';
      tr.className = baseClass;
      tr.querySelectorAll('td').forEach((td) => td.classList.remove('text-white'));
    });

    state.currentRow = rowEl;
    rowEl.classList.remove('bg-white', 'bg-slate-50');
    rowEl.classList.add('bg-blue-100', 'ring-1', 'ring-inset', 'ring-blue-400');
    rowEl.querySelectorAll('td').forEach((td) => {
      td.classList.remove('text-slate-500');
      td.classList.add('text-slate-800');
    });

    state.selectedValue = rowData || {};
    if (typeof state.onSelectCallback === 'function') {
      try { state.onSelectCallback(state.selectedValue); } catch (_) { }
    }
  }

  async function confirmGenericLovSelection() {
    if (!state.selectedValue || Object.keys(state.selectedValue).length === 0) {
      alert('請先選擇一筆資料');
      return;
    }

    suppressAutoSearch(900);
    await applyColumnMatches(state.selectedValue);

    Object.entries(state.targetInputs || {}).forEach(([dataKey, inputId]) => {
      const el = document.getElementById(inputId);
      if (!el) return;
      clearTimeout(el.__lovTypeTimer);

      if (dataKey === 'FORMATTED_DISPLAY' && typeof state.formatDisplay === 'function') {
        el.value = state.formatDisplay(state.selectedValue);
      } else if (state.selectedValue[dataKey] !== undefined) {
        el.value = state.selectedValue[dataKey];
      }

      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
    });

    if (typeof state.onConfirmCallback === 'function') {
      try { state.onConfirmCallback(state.selectedValue); } catch (_) { }
    }

    closeGenericLov();
  }

  async function resolveDynamicValue(methodName, fallbackValue, payload) {
    if (typeof methodName === 'string' && methodName.trim()) {
      const fn = window[methodName.trim()];
      if (typeof fn === 'function') {
        const v = fn(payload);
        return (v && typeof v.then === 'function') ? await v : v;
      }
    }
    return fallbackValue;
  }

  async function buildFilterQueryString() {
    const items = Array.isArray(state.filterItems) ? state.filterItems : [];
    if (!items.length) return '';

    const parts = [];
    for (const item of items) {
      const fieldName = String(item?.FieldName ?? item?.FiledName ?? '').trim();
      if (!fieldName) continue;

      const whereField = String(item?.WhereField ?? '').trim();
      const whereMethod = String(item?.WhereMethod ?? '').trim();
      let value = item?.WhereValue ?? '';

      if (whereField) {
        const el = document.getElementById(whereField);
        if (el && typeof el.value !== 'undefined') value = el.value;
      }

      value = await resolveDynamicValue(whereMethod, value, { item, state });
      if (value === undefined || value === null || `${value}` === '') continue;

      parts.push(`${encodeURIComponent(fieldName)}=${encodeURIComponent(value)}`);
    }

    return parts.length ? `&${parts.join('&')}` : '';
  }

  async function applyColumnMatches(selected) {
    const matches = Array.isArray(state.columnMatches) ? state.columnMatches : [];
    if (!matches.length || !selected) return;

    for (const match of matches) {
      const sourceField = String(match?.SourceFieldName ?? '').trim();
      const sourceMethod = String(match?.SourceMethod ?? '').trim();
      const targetField = String(match?.TargetFieldName ?? '').trim();
      if (!targetField) continue;

      let value = sourceField ? selected?.[sourceField] : undefined;
      value = await resolveDynamicValue(sourceMethod, value, { row: selected, match, state });

      const el = document.getElementById(targetField);
      if (!el) continue;
      el.value = value ?? '';
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
    }
  }

  const registry = {};
  function merge(base, extra) {
    return Object.assign({}, base || {}, extra || {});
  }

  function openByName(name, overrides) {
    const key = String(name || '').trim();
    const base = registry[key];
    if (!base) {
      alert(`LOV 設定不存在: ${key}`);
      return;
    }

    const cfg = merge(base, overrides || {});
    if (base.options || (overrides && overrides.options)) cfg.options = merge(base.options, overrides.options);
    if (base.map || (overrides && overrides.map)) cfg.map = merge(base.map, overrides.map);
    if (base.columnMatches || (overrides && overrides.columnMatches)) {
      cfg.columnMatches = Array.isArray(overrides?.columnMatches) ? overrides.columnMatches : (Array.isArray(base.columnMatches) ? base.columnMatches : []);
    }
    if (base.filterItems || (overrides && overrides.filterItems)) {
      cfg.filterItems = Array.isArray(overrides?.filterItems) ? overrides.filterItems : (Array.isArray(base.filterItems) ? base.filterItems : []);
    }

    return openGenericLov(cfg);
  }

  function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>'"]/g, (ch) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '\'': '&#39;', '"': '&quot;' }[ch]));
  }

  window.gLov = window.gLov || {};
  window.gLov.define = (name, config) => {
    const key = String(name || '').trim();
    if (!key) return;
    registry[key] = merge(config, {});
  };
  window.gLov.openByName = openByName;
  window.gLov.open = (config) => openGenericLov(config);
  window.gLov.openLegacy = (...args) => openGenericLov(...args);
  window.gLov.typeSearchInput = (inputEl, immediate) => {
    if (!inputEl) return;
    const exec = () => {
      if (Date.now() < (state.suppressAutoSearchUntil || 0)) return;
      const value = String(inputEl.value || '').trim();
      if (!immediate) {
        if (!value || value.length < 2) return;
      }
      if (dom.modal && dom.modal.style.display === 'flex' && state.sourceInputId === inputEl.id) {
        dom.input.value = value;
        fetchGenericLovData(false);
        return;
      }
      const btn = inputEl.closest('[data-glov-input]')?.querySelector('[data-lov-open-btn]');
      if (btn) btn.click();
    };

    if (immediate) {
      clearTimeout(inputEl.__lovTypeTimer);
      exec();
      return;
    }

    clearTimeout(inputEl.__lovTypeTimer);
    inputEl.__lovTypeTimer = setTimeout(exec, 260);
  };
  window.gLov._registry = registry;

  window.__openGenericLovFromModal = (...args) => openGenericLov(...args);
  window.openGenericLov = (...args) => openGenericLov(...args);
})();

