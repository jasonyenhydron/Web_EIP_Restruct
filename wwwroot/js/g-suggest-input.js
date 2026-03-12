(function () {
  if (window.__gSuggestRuntimeReady) return;
  window.__gSuggestRuntimeReady = true;

  function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>'"]/g, function (ch) {
      return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '\'': '&#39;', '"': '&quot;' })[ch];
    });
  }

  function initSuggest(root) {
    const input = root.querySelector('input');
    const list = root.querySelector('.g-suggest-list');
    if (!input || !list) return;

    const api = root.dataset.api || '';
    const queryParam = root.dataset.queryParam || 'q';
    const valueField = root.dataset.valueField || 'value';
    const primaryField = root.dataset.primaryField || valueField;
    const secondaryField = root.dataset.secondaryField || '';
    const minLength = Math.max(0, parseInt(root.dataset.minLength || '1', 10) || 1);
    const debounceMs = Math.max(0, parseInt(root.dataset.debounceMs || '280', 10) || 280);
    const onSelectName = (root.dataset.onSelect || '').trim();

    let timer = null;
    let currentFocus = -1;
    let items = [];
    let suppressUntil = 0;

    function positionList() {
      const rect = input.getBoundingClientRect();
      list.style.top = `${rect.bottom + 4}px`;
      list.style.left = `${rect.left}px`;
      list.style.width = `${rect.width}px`;
    }

    function hideList() {
      list.classList.remove('show');
      currentFocus = -1;
      window.setTimeout(() => {
        if (!list.classList.contains('show')) {
          list.classList.add('hidden');
        }
      }, 180);
    }

    function showList() {
      positionList();
      list.classList.remove('hidden');
      requestAnimationFrame(() => list.classList.add('show'));
    }

    function removeActive() {
      list.querySelectorAll('.g-suggest-item').forEach((el) => el.classList.remove('active'));
    }

    function addActive() {
      const nodes = list.querySelectorAll('.g-suggest-item');
      if (!nodes.length) return;
      if (currentFocus >= nodes.length) currentFocus = 0;
      if (currentFocus < 0) currentFocus = nodes.length - 1;
      removeActive();
      nodes[currentFocus].classList.add('active');
      nodes[currentFocus].scrollIntoView({ block: 'nearest' });
    }

    function selectItem(item) {
      suppressUntil = Date.now() + 300;
      input.value = item?.[valueField] ?? '';
      input.dispatchEvent(new Event('input', { bubbles: true }));
      input.dispatchEvent(new Event('change', { bubbles: true }));
      hideList();

      if (onSelectName && typeof window[onSelectName] === 'function') {
        try {
          window[onSelectName](item, input, root);
        } catch (err) {
          console.error('[g-suggest-input] onSelect failed:', err);
        }
      }
    }

    function render(data) {
      list.innerHTML = '';
      currentFocus = -1;
      items = Array.isArray(data) ? data : [];

      if (!items.length) {
        hideList();
        return;
      }

      items.forEach((item) => {
        const primary = item?.[primaryField] ?? '';
        const secondary = secondaryField ? (item?.[secondaryField] ?? '') : '';
        const row = document.createElement('div');
        row.className = 'g-suggest-item';
        row.setAttribute('role', 'option');
        row.innerHTML = `
          <span class="g-suggest-primary">${escapeHtml(primary)}</span>
          ${secondary ? `<span class="g-suggest-secondary">${escapeHtml(secondary)}</span>` : ''}
        `;
        row.addEventListener('mousedown', (e) => {
          e.preventDefault();
          selectItem(item);
        });
        list.appendChild(row);
      });

      showList();
    }

    async function fetchSuggestions(query) {
      if (!api) return;
      try {
        const separator = api.includes('?') ? '&' : '?';
        const response = await fetch(`${api}${separator}${encodeURIComponent(queryParam)}=${encodeURIComponent(query)}`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const data = await response.json();
        render(data);
      } catch (err) {
        console.error('[g-suggest-input] fetch failed:', err);
        hideList();
      }
    }

    input.addEventListener('input', function () {
      if (Date.now() < suppressUntil) {
        return;
      }

      const value = input.value.trim();
      clearTimeout(timer);

      if (value.length < minLength) {
        hideList();
        return;
      }

      timer = window.setTimeout(() => fetchSuggestions(value), debounceMs);
    });

    input.addEventListener('keydown', function (e) {
      const nodes = list.querySelectorAll('.g-suggest-item');

      if (e.key === 'ArrowDown') {
        e.preventDefault();
        currentFocus += 1;
        addActive();
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        currentFocus -= 1;
        addActive();
      } else if (e.key === 'Enter') {
        if (currentFocus > -1 && items[currentFocus]) {
          e.preventDefault();
          selectItem(items[currentFocus]);
        }
      } else if (e.key === 'Escape') {
        hideList();
      } else if (!nodes.length) {
        currentFocus = -1;
      }
    });

    document.addEventListener('click', function (e) {
      if (!root.contains(e.target) && !list.contains(e.target)) {
        hideList();
      }
    });

    window.addEventListener('scroll', function () {
      if (list.classList.contains('show')) positionList();
    }, true);
    window.addEventListener('resize', function () {
      if (list.classList.contains('show')) positionList();
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-gsuggest]').forEach(initSuggest);
  });
})();

