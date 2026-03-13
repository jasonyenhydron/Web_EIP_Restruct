'use strict';

window.gDataForm = function (formId, initialFields, initialQueryValues) {
  return {
    formId,
    mode: 'view',
    formData: { ...initialFields },
    queryDefaults: { ...(initialQueryValues || {}) },
    queryValues: { ...(initialQueryValues || {}) },
    queryPanelOpen: true,
    originalData: {},
    queryRows: [],
    currentRowIndex: -1,
    errors: {},
    selectOptions: {},
    hasSelection: false,
    hasRows: false,
    canSelectFirst: false,
    canSelectPrev: false,
    canSelectNext: false,
    canSelectLast: false,
    loading: false,
    alwaysReadOnly: false,

    init() {
      const el = document.getElementById(this.formId);
      if (!el) return;
      this.alwaysReadOnly = el.dataset.alwaysReadonly === '1';
      this._syncNavigationState();

      const parentId = el.dataset.parentId;
      if (parentId) {
        document.addEventListener(`gDataForm:selected:${parentId}`, (e) => {
          this._loadByRelation(el, e.detail);
        });
      }

      const api = el.dataset.api;
      const notInitLoad = el.dataset.notInitLoad === '1';
      if (api && !parentId && !notInitLoad) {
        this._loadData(el);
      }

      const cb = el.dataset.onLoadSuccess;
      if (cb && typeof window[cb] === 'function') {
        this.$nextTick(() => window[cb](this.formData));
      }
    },

    async executeQuery() {
      const el = document.getElementById(this.formId);
      if (!el) return;
      await this._queryData(el);
    },

    resetQuery() {
      this.queryValues = { ...this.queryDefaults };
    },

    refreshData() {
      const el = document.getElementById(this.formId);
      if (!el) return;
      if (Object.keys(this._buildQueryParams(el)).length > 0 || el.dataset.queryApi) {
        this.executeQuery();
        return;
      }
      this._loadData(el);
    },

    handleToolAction(action, customFn, formIdValue) {
      if (action === 'custom' && customFn && typeof window[customFn] === 'function') {
        window[customFn](this.formData);
        return;
      }
      const el = document.getElementById(formIdValue);
      if (!el) return;

      switch (action) {
        case 'add': this.openAdd(formIdValue, el); break;
        case 'edit': this.openEdit(formIdValue, el); break;
        case 'delete': this.openDelete(formIdValue, el); break;
      }
    },

    openAdd(formIdValue, el) {
      this.mode = 'add';
      this.errors = {};
      const defaults = this._getDefaults(el);
      this.formData = { ...defaults };
      this.hasSelection = false;
      this.currentRowIndex = -1;
      this._syncNavigationState();
      this._applyRelationDefaults(el);
      this._openFlowbiteModal(`${formIdValue}_modal`);
    },

    openEdit(formIdValue) {
      if (!this.hasSelection) {
        gDataFormToast.warn('請先選取一筆資料');
        return;
      }
      this.mode = 'edit';
      this.errors = {};
      this.originalData = { ...this.formData };
      this._openFlowbiteModal(`${formIdValue}_modal`);
    },

    openDelete(formIdValue) {
      if (!this.hasSelection) {
        gDataFormToast.warn('請先選取一筆資料');
        return;
      }
      this._openFlowbiteModal(`${formIdValue}_del_modal`);
    },

    async confirmDelete(formIdValue, modalId) {
      const el = document.getElementById(formIdValue);
      if (!el) return;
      const api = el.dataset.api;
      if (!api) return;

      this.loading = true;
      try {
        const pkValue = this._getPrimaryKeyValue(el);
        const url = pkValue ? `${api}/${pkValue}` : api;
        const res = await fetch(url, { method: 'DELETE', headers: this._headers() });
        const json = await res.json().catch(() => ({}));

        if (res.ok && (json.status === 'success' || res.status === 200)) {
          gDataFormToast.success('刪除成功');
          this.formData = this._getDefaults(el);
          this.hasSelection = false;
          this._removeCurrentRecord();
          this.closeModal(modalId);
          this._triggerCallback(el, 'onApplied', null);
          this._notifyChain(el);
        } else {
          gDataFormToast.error(json.message || '刪除失敗');
        }
      } catch (err) {
        gDataFormToast.error(`刪除失敗：${err.message}`);
      } finally {
        this.loading = false;
      }
    },

    async submitForm(formIdValue, modalId) {
      const el = document.getElementById(formIdValue);
      if (!el) return;
      const api = el.dataset.api;
      if (!api) return;

      if (this._triggerCallback(el, 'onBeforeValidate', this.formData) === false) return;

      const validateStyle = el.dataset.validateStyle || 'hint';
      const isValid = this._validate(el);
      if (!isValid) {
        this._setStatus(el, el.dataset.validateFailedMessage || '錯誤：欄位驗證未通過。');
        if (validateStyle === 'dialog') {
          const msgs = Object.values(this.errors).filter(Boolean).join('\n');
          alert(`請修正以下錯誤：\n${msgs}`);
        }
        return;
      }

      if (this._triggerCallback(el, 'onApply', this.formData) === false) return;

      const payload = this._buildSubmitPayload(el);
      const dupCheck = el.dataset.duplicateCheck === '1';
      if (dupCheck && this.mode === 'add') {
        const isDup = await this._checkDuplicate(api, payload);
        if (isDup) {
          const dupMessage = el.dataset.errorToastMessage || '資料重複，請確認後再送出';
          gDataFormToast.error(this._formatMessage(dupMessage, { message: '資料重複，請確認後再送出' }, payload));
          return;
        }
      }

      this.loading = true;
      this._setStatus(el, el.dataset.savingMessage || '資料庫寫入中...');
      try {
        const isAdd = this.mode === 'add';
        const pkVal = this._getPrimaryKeyValue(el);
        const url = isAdd ? api : `${api}/${pkVal}`;
        const method = isAdd ? 'POST' : 'PUT';

        const res = await fetch(url, {
          method,
          headers: this._headers(),
          body: JSON.stringify(payload)
        });
        const json = await res.json().catch(() => ({}));

        if (res.ok && (json.status === 'success' || res.status === 200 || res.status === 201)) {
          const pkField = el.querySelector('[data-is-pk="1"]');
          if (pkField && json.id != null) {
            this.formData[pkField.dataset.fieldName] = json.id;
            payload[pkField.dataset.fieldName] = json.id;
          }

          if (el.dataset.showApplyButton === '1' && json.data) {
            this.formData = { ...this.formData, ...json.data };
          }

          this._setStatus(el, this._formatMessage(
            el.dataset.saveSuccessMessage || 'Success: {message} (流水號: {id})',
            json,
            json.data || payload
          ));
          const successToast = el.dataset.successToastMessage || '';
          gDataFormToast.success(successToast
            ? this._formatMessage(successToast, json, json.data || payload)
            : (isAdd ? '新增成功' : '儲存成功'));

          this._triggerCallback(el, 'onApplied', json.data || payload);

          const continueAdd = el.dataset.continueAdd === '1';
          if (continueAdd && isAdd) {
            this.formData = this._getDefaults(el);
            this.errors = {};
            this.hasSelection = false;
            this.currentRowIndex = -1;
            this._syncNavigationState();
            } else {
              this.closeModal(modalId);
              if (json.data) this.formData = { ...this.formData, ...json.data };
              this.hasSelection = !!this._getPrimaryKeyValue(el);
              this._upsertCurrentRecord(json.data || payload, el);
              this.mode = 'view';
            }

          this._notifyChain(el);

          if (el.dataset.autoPageClose === '1') {
            setTimeout(() => window.close(), 800);
          }
        } else {
          this._setStatus(el, this._formatMessage(
            el.dataset.saveErrorMessage || 'Error: {message}',
            json,
            this.formData
          ));
          const errorToast = el.dataset.errorToastMessage || '';
          gDataFormToast.error(errorToast
            ? this._formatMessage(errorToast, json, this.formData)
            : (json.message || '儲存失敗'));
        }
      } catch (err) {
        this._setStatus(el, this._formatMessage(
          el.dataset.saveErrorMessage || 'Error: {message}',
          { message: '系統例外錯誤。' },
          this.formData
        ));
        const exceptionToast = el.dataset.exceptionToastMessage || el.dataset.errorToastMessage || '';
        gDataFormToast.error(exceptionToast
          ? this._formatMessage(exceptionToast, { message: '系統例外錯誤。' }, this.formData)
          : `儲存失敗：${err.message}`);
      } finally {
        this.loading = false;
      }
    },

    cancelForm(formIdValue, modalId) {
      const el = document.getElementById(formIdValue);
      if (this.mode === 'edit') {
        this.formData = { ...this.originalData };
      }
      this.errors = {};
      this.mode = 'view';
      this._triggerCallback(el, 'onCancel', null);
      this.closeModal(modalId);
    },

    async _loadData(el, extraParams) {
      const api = el.dataset.api;
      if (!api) return;

      const params = extraParams ? '?' + new URLSearchParams(extraParams).toString() : '';

      try {
        const res = await fetch(`${api}${params}`, { headers: this._headers() });
        const json = await res.json().catch(() => ({}));
        if (res.ok && json.data) {
          this.formData = { ...this.formData, ...json.data };
          this.hasSelection = !!this._getPrimaryKeyValue(el);
          this._syncCurrentRowIndex(el);
          this._triggerCallback(el, 'onLoadSuccess', this.formData);
        }
      } catch (_) {
      }
    },

    async _queryData(el) {
      const api = el.dataset.queryApi || el.dataset.api;
      if (!api) return;

      const params = this._buildQueryParams(el);
      const query = new URLSearchParams(params).toString();
      const url = query ? `${api}${api.includes('?') ? '&' : '?'}${query}` : api;

      try {
        const res = await fetch(url, { headers: this._headers() });
        const json = await res.json().catch(() => ({}));
        if (!res.ok) {
          gDataFormToast.error(json.message || '查詢失敗');
          return;
        }

        const payload = json.data;
        const rows = Array.isArray(payload) ? payload : (payload ? [payload] : []);
        const row = rows[0] || null;
        if (!row) {
          gDataFormToast.warn('查無資料');
          this.queryRows = [];
          this.hasSelection = false;
          this.currentRowIndex = -1;
          this._syncNavigationState();
          this._triggerCallback(el, 'onQueryLoaded', null);
          return;
        }

        this.queryRows = rows;
        this.formData = { ...this.formData, ...row };
        this.hasSelection = !!this._getPrimaryKeyValue(el);
        this.currentRowIndex = 0;
        this._syncNavigationState();
        this.mode = 'view';
        this._triggerCallback(el, 'onQueryLoaded', { row, rows, raw: json });
        this._triggerCallback(el, 'onLoadSuccess', this.formData);
      } catch (err) {
        gDataFormToast.error(`查詢失敗：${err.message}`);
      }
    },

    _loadByRelation(el, masterData) {
      const relJson = el.dataset.relationColumns;
      if (!relJson) return;
      try {
        const relations = JSON.parse(relJson);
        const params = {};
        relations.forEach((r) => {
          if (r.MasterField && r.DetailField && masterData[r.MasterField] != null) {
            params[r.DetailField] = masterData[r.MasterField];
          }
        });
        this._loadData(el, params);
      } catch (_) {
      }
    },

    selectRow(data) {
      this.formData = { ...data };
      this.hasSelection = true;
      this.mode = 'view';
      const el = document.getElementById(this.formId);
      this._syncCurrentRowIndex(el, data);
      this._notifyChain(el);
      document.dispatchEvent(new CustomEvent(`gDataForm:selected:${this.formId}`, { detail: data }));
    },

    selectFirstRecord() {
      if (!this.canSelectFirst) return;
      this._selectRecordAt(0);
    },

    selectPrevRecord() {
      if (!this.canSelectPrev) return;
      this._selectRecordAt(this.currentRowIndex - 1);
    },

    selectNextRecord() {
      if (!this.canSelectNext) return;
      this._selectRecordAt(this.currentRowIndex + 1);
    },

    selectLastRecord() {
      if (!this.canSelectLast) return;
      this._selectRecordAt(this.queryRows.length - 1);
    },

    async loadSelectOptions(apiUrl, fieldName) {
      if (!apiUrl || this.selectOptions[fieldName]) return;
      try {
        const res = await fetch(apiUrl, { headers: this._headers() });
        const json = await res.json().catch(() => ({}));
        if (res.ok && Array.isArray(json.data)) {
          this.selectOptions[fieldName] = json.data.map((item) => ({
            value: item.value ?? item.id ?? item.code ?? '',
            label: item.label ?? item.name ?? item.text ?? ''
          }));
        }
      } catch (_) {
      }
    },

    _validate(el) {
      this.errors = {};
      const fields = el.querySelectorAll('[data-validate="1"]');
      let valid = true;

      fields.forEach((f) => {
        const name = f.dataset.fieldName;
        const required = f.dataset.required === '1';
        const validateFn = f.dataset.validateFn;
        const validateMsg = f.dataset.validateMsg;
        const min = f.dataset.min;
        const max = f.dataset.max;
        const compareField = f.dataset.compareField;
        const compareMode = f.dataset.compareMode;
        const val = this.formData[name];

        if (required && (val === null || val === undefined || String(val).trim() === '')) {
          this.errors[name] = validateMsg || `${f.dataset.caption || name} 為必填`;
          valid = false;
          return;
        }

        if (compareField && compareMode && val !== null && val !== undefined && String(val).trim() !== '') {
          const compareVal = this.formData[compareField];
          if (compareVal !== null && compareVal !== undefined && String(compareVal).trim() !== '') {
            const leftDate = Date.parse(val);
            const rightDate = Date.parse(compareVal);
            const useDate = !Number.isNaN(leftDate) && !Number.isNaN(rightDate);
            const leftNum = Number(val);
            const rightNum = Number(compareVal);
            const useNumber = !useDate && !Number.isNaN(leftNum) && !Number.isNaN(rightNum);
            const left = useDate ? leftDate : (useNumber ? leftNum : String(val));
            const right = useDate ? rightDate : (useNumber ? rightNum : String(compareVal));
            const caption = f.dataset.caption || name;
            const otherCaption = compareField;

            if ((compareMode === 'after-field' || compareMode === 'gt-field') && !(left > right)) {
              this.errors[name] = validateMsg || `${caption} 必須大於 ${otherCaption}`;
              valid = false;
              return;
            }
            if ((compareMode === 'before-field' || compareMode === 'lt-field') && !(left < right)) {
              this.errors[name] = validateMsg || `${caption} 必須小於 ${otherCaption}`;
              valid = false;
              return;
            }
            if ((compareMode === 'gte-field' || compareMode === 'on-or-after-field') && !(left >= right)) {
              this.errors[name] = validateMsg || `${caption} 必須大於等於 ${otherCaption}`;
              valid = false;
              return;
            }
            if ((compareMode === 'lte-field' || compareMode === 'on-or-before-field') && !(left <= right)) {
              this.errors[name] = validateMsg || `${caption} 必須小於等於 ${otherCaption}`;
              valid = false;
              return;
            }
          }
        }

        if (val !== null && val !== undefined && String(val).trim() !== '') {
          const numericVal = Number(val);
          if (!Number.isNaN(numericVal)) {
            if (min !== undefined && min !== '' && numericVal < Number(min)) {
              this.errors[name] = validateMsg || `${f.dataset.caption || name} 必須大於等於 ${min}`;
              valid = false;
              return;
            }
            if (max !== undefined && max !== '' && numericVal > Number(max)) {
              this.errors[name] = validateMsg || `${f.dataset.caption || name} 必須小於等於 ${max}`;
              valid = false;
              return;
            }
          }
        }

        if (validateFn && typeof window[validateFn] === 'function') {
          const result = window[validateFn](val, this.formData);
          if (result !== true) {
            this.errors[name] = typeof result === 'string' ? result : (validateMsg || '驗證失敗');
            valid = false;
          }
        }
      });

      return valid;
    },

    async _checkDuplicate(api, payload) {
      try {
        const url = `${api}/check-duplicate`;
        const res = await fetch(url, {
          method: 'POST',
          headers: this._headers(),
          body: JSON.stringify(payload || this.formData)
        });
        const json = await res.json().catch(() => ({}));
        return json.isDuplicate === true;
      } catch (_) {
        return false;
      }
    },

    _getDefaults(el) {
      const defaults = {};
      el.querySelectorAll('[data-field-default]').forEach((f) => {
        defaults[f.dataset.fieldName] = f.dataset.fieldDefault ?? '';
      });
      return defaults;
    },

    _applyRelationDefaults(el) {
      const parentId = el.dataset.parentId;
      if (!parentId) return;
      const parentEl = document.getElementById(parentId);
      if (!parentEl || !parentEl._x_dataStack) return;

      try {
        const parentData = Alpine.$data(parentEl)?.formData;
        const relJson = el.dataset.relationColumns;
        if (!parentData || !relJson) return;
        const relations = JSON.parse(relJson);
        relations.forEach((r) => {
          if (r.MasterField && r.DetailField && parentData[r.MasterField] != null) {
            this.formData[r.DetailField] = parentData[r.MasterField];
          }
        });
      } catch (_) {
      }
    },

    _getPrimaryKeyValue(el) {
      const pkField = el.querySelector('[data-is-pk="1"]');
      if (pkField) return this.formData[pkField.dataset.fieldName];
      const keys = Object.keys(this.formData);
      const idField = keys.find((k) => k.toLowerCase().endsWith('_id') || k.toLowerCase() === 'id');
      return idField ? this.formData[idField] : null;
    },

    _getPrimaryKeyFieldName(el) {
      const pkField = el?.querySelector?.('[data-is-pk="1"]');
      if (pkField?.dataset?.fieldName) return pkField.dataset.fieldName;
      const keys = Object.keys(this.formData || {});
      return keys.find((k) => k.toLowerCase().endsWith('_id') || k.toLowerCase() === 'id') || '';
    },

    _selectRecordAt(index) {
      if (!Array.isArray(this.queryRows) || index < 0 || index >= this.queryRows.length) return;
      const row = this.queryRows[index];
      this.formData = { ...row };
      this.hasSelection = true;
      this.mode = 'view';
      this.currentRowIndex = index;
      this._syncNavigationState();
      const el = document.getElementById(this.formId);
      this._notifyChain(el);
      document.dispatchEvent(new CustomEvent(`gDataForm:selected:${this.formId}`, { detail: row }));
    },

    _syncCurrentRowIndex(el, data) {
      const source = data || this.formData;
      if (!Array.isArray(this.queryRows) || this.queryRows.length === 0) {
        this.currentRowIndex = -1;
        this._syncNavigationState();
        return;
      }

      const pkName = this._getPrimaryKeyFieldName(el);
      if (pkName) {
        const pkValue = source?.[pkName];
        const idx = this.queryRows.findIndex((row) => row?.[pkName] === pkValue);
        this.currentRowIndex = idx;
      } else {
        this.currentRowIndex = this.queryRows.findIndex((row) => row === source);
      }

      this._syncNavigationState();
    },

    _syncNavigationState() {
      const total = Array.isArray(this.queryRows) ? this.queryRows.length : 0;
      this.hasRows = total > 0;
      this.canSelectFirst = total > 0 && this.currentRowIndex !== 0;
      this.canSelectPrev = total > 0 && this.currentRowIndex > 0;
      this.canSelectNext = total > 0 && this.currentRowIndex >= 0 && this.currentRowIndex < total - 1;
      this.canSelectLast = total > 0 && this.currentRowIndex !== total - 1;
    },

    _upsertCurrentRecord(record, el) {
      if (!record) {
        this._syncCurrentRowIndex(el);
        return;
      }

      const pkName = this._getPrimaryKeyFieldName(el);
      if (!Array.isArray(this.queryRows)) this.queryRows = [];

      if (!pkName) {
        if (this.currentRowIndex >= 0 && this.currentRowIndex < this.queryRows.length) {
          this.queryRows[this.currentRowIndex] = { ...this.queryRows[this.currentRowIndex], ...record };
        } else {
          this.queryRows.push({ ...record });
          this.currentRowIndex = this.queryRows.length - 1;
        }
        this._syncNavigationState();
        return;
      }

      const pkValue = record?.[pkName] ?? this.formData?.[pkName];
      const idx = this.queryRows.findIndex((row) => row?.[pkName] === pkValue);
      if (idx >= 0) {
        this.queryRows[idx] = { ...this.queryRows[idx], ...record };
        this.currentRowIndex = idx;
      } else {
        this.queryRows.push({ ...record });
        this.currentRowIndex = this.queryRows.length - 1;
      }
      this._syncNavigationState();
    },

    _removeCurrentRecord() {
      if (!Array.isArray(this.queryRows) || this.currentRowIndex < 0 || this.currentRowIndex >= this.queryRows.length) {
        this.currentRowIndex = -1;
        this._syncNavigationState();
        return;
      }

      this.queryRows.splice(this.currentRowIndex, 1);
      if (this.queryRows.length === 0) {
        this.currentRowIndex = -1;
      } else if (this.currentRowIndex >= this.queryRows.length) {
        this.currentRowIndex = this.queryRows.length - 1;
        this.formData = { ...this.queryRows[this.currentRowIndex] };
        this.hasSelection = true;
      } else {
        this.formData = { ...this.queryRows[this.currentRowIndex] };
        this.hasSelection = true;
      }
      this._syncNavigationState();
    },

    _notifyChain(el) {
      if (!el) return;
      const chainId = el.dataset.chainId;
      if (!chainId) return;
      document.dispatchEvent(new CustomEvent(`gDataForm:selected:${this.formId}`, {
        detail: this.formData
      }));
    },

    _buildQueryParams(el) {
      const params = {};
      el.querySelectorAll('[data-query-field]').forEach((input) => {
        const fieldName = input.dataset.queryField;
        const modelName = input.dataset.queryModel || fieldName;
        const value = this.queryValues[modelName];
        if (value === undefined || value === null || String(value).trim() === '') return;
        params[fieldName] = value;
      });
      return params;
    },

    _buildSubmitPayload(el) {
      const payload = { ...this.formData };
      const meta = this._getColumnMeta(el);
      const emptyStringAsNull = el.dataset.emptyStringAsNull === '1';

      for (const col of meta) {
        const fieldName = col.fieldName;
        if (!fieldName || !(fieldName in payload)) continue;
        payload[fieldName] = this._coerceValue(payload[fieldName], col.valueType, emptyStringAsNull);
      }

      if (emptyStringAsNull) {
        Object.keys(payload).forEach((key) => {
          if (payload[key] === '') payload[key] = null;
        });
      }

      return payload;
    },

    _getColumnMeta(el) {
      if (!el) return [];
      try {
        const raw = el.dataset.columnMeta || '[]';
        const parsed = JSON.parse(raw);
        return Array.isArray(parsed) ? parsed : [];
      } catch (_) {
        return [];
      }
    },

    _coerceValue(value, valueType, emptyStringAsNull) {
      if (value === undefined) return value;
      if (value === null) return null;

      if (typeof value === 'string') {
        const trimmed = value.trim();
        if (trimmed === '') return emptyStringAsNull ? null : value;
        value = trimmed;
      }

      switch (String(valueType || '').toLowerCase()) {
        case 'int':
        case 'integer':
        case 'long': {
          const n = parseInt(value, 10);
          return Number.isNaN(n) ? null : n;
        }
        case 'decimal':
        case 'number':
        case 'float':
        case 'double': {
          const n = parseFloat(value);
          return Number.isNaN(n) ? null : n;
        }
        case 'bool':
        case 'boolean':
          if (typeof value === 'boolean') return value;
          if (value === 'Y' || value === 'true' || value === '1') return true;
          if (value === 'N' || value === 'false' || value === '0') return false;
          return !!value;
        default:
          return value;
      }
    },

    _triggerCallback(el, dataAttr, payload) {
      if (!el) return;
      const attrMap = {
        onLoadSuccess: 'onLoadSuccess',
        onQueryLoaded: 'onQueryLoaded',
        onApply: 'onApply',
        onApplied: 'onApplied',
        onCancel: 'onCancel',
        onBeforeValidate: 'onBeforeValidate'
      };
      const key = attrMap[dataAttr] || dataAttr;
      const cbName = el.dataset[key];
      if (!cbName) return;
      const fn = window[cbName];
      if (typeof fn === 'function') return fn(payload);
    },

    _setStatus(el, message) {
      if (!el) return;
      const targetId = el.dataset.statusTargetId;
      if (!targetId) return;
      const target = document.getElementById(targetId);
      if (!target) return;
      target.value = message || '';
    },

    _formatMessage(template, response, formData) {
      const payload = response || {};
      const data = formData || {};
      return String(template || '')
        .replaceAll('{message}', payload.message ?? '')
        .replaceAll('{id}', payload.id ?? data.emAskForLeaveId ?? '')
        .replaceAll('{status}', payload.status ?? '')
        .replaceAll('{mode}', this.mode ?? '');
    },

    _headers() {
      const token = document.querySelector('meta[name="csrf-token"]')?.content;
      const h = { 'Content-Type': 'application/json', 'Accept': 'application/json' };
      if (token) h['X-CSRF-TOKEN'] = token;
      return h;
    },

    _openFlowbiteModal(modalId) {
      const el = document.getElementById(modalId);
      if (!el) return;
      if (window.FlowbiteInstances) {
        const instance = window.FlowbiteInstances.getInstance('Modal', modalId);
        if (instance) {
          instance.show();
          return;
        }
      }
      el.classList.remove('hidden');
      el.classList.add('flex');
    },

    closeModal(modalId) {
      const el = document.getElementById(modalId);
      if (!el) return;
      if (window.FlowbiteInstances) {
        const instance = window.FlowbiteInstances.getInstance('Modal', modalId);
        if (instance) {
          instance.hide();
          return;
        }
      }
      el.classList.add('hidden');
      el.classList.remove('flex');
    }
  };
};

window.gDataFormToast = (() => {
  function show(message, type) {
    const colors = {
      success: { bg: 'bg-green-50 border-green-300', icon: 'text-green-500', path: 'M5 13l4 4L19 7' },
      error: { bg: 'bg-red-50 border-red-300', icon: 'text-red-500', path: 'M6 18L18 6M6 6l12 12' },
      warn: { bg: 'bg-yellow-50 border-yellow-300', icon: 'text-yellow-500', path: 'M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z' }
    };
    const c = colors[type] || colors.success;

    let container = document.getElementById('_gdf_toast_container');
    if (!container) {
      container = document.createElement('div');
      container.id = '_gdf_toast_container';
      container.className = 'fixed top-4 right-4 z-[200] flex flex-col gap-2';
      document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `flex items-center gap-3 px-4 py-3 rounded-xl border shadow-md text-sm font-medium text-slate-700 ${c.bg} transition-all duration-300 opacity-0 translate-x-4`;
    toast.innerHTML = `
      <svg class="w-5 h-5 flex-shrink-0 ${c.icon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="${c.path}"/>
      </svg>
      <span>${message}</span>
      <button onclick="this.closest('div[data-toast]').remove()" class="ml-2 text-slate-400 hover:text-slate-600">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
      </button>`;
    toast.setAttribute('data-toast', '1');
    container.appendChild(toast);

    requestAnimationFrame(() => {
      toast.classList.remove('opacity-0', 'translate-x-4');
    });

    setTimeout(() => {
      toast.classList.add('opacity-0', 'translate-x-4');
      setTimeout(() => toast.remove(), 300);
    }, 3500);
  }

  return {
    success: (msg) => show(msg, 'success'),
    error: (msg) => show(msg, 'error'),
    warn: (msg) => show(msg, 'warn')
  };
})();

document.addEventListener('DOMContentLoaded', () => {
  if (typeof initFlowbite === 'function') initFlowbite();
});

