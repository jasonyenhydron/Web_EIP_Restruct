using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-error-message", TagStructure = TagStructure.WithoutEndTag)]
    public class GErrorMessageTagHelper : TagHelper
    {
        public string Title { get; set; } = "System Error";
        public bool AutoCapture { get; set; } = true;
        public bool CaptureFetch { get; set; } = true;
        public bool CaptureWindowError { get; set; } = true;
        public bool CaptureUnhandledRejection { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var html = @"
<div id='gErrorMessageRoot' class='hidden fixed inset-0 z-[120] bg-slate-900/60 backdrop-blur-sm items-center justify-center p-4'>
  <div class='w-full max-w-3xl bg-white border border-slate-200 rounded-xl shadow-2xl overflow-hidden'>
    <div class='px-4 py-3 border-b border-slate-200 bg-slate-100 flex items-center justify-between'>
      <div class='text-sm font-bold text-slate-800'>__TITLE__</div>
      <button type='button' class='p-1.5 rounded bg-slate-100 text-slate-500' onclick='gHideErrorMessage()' title='Close'>
        <svg class='w-4 h-4' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 18L18 6M6 6l12 12'></path></svg>
      </button>
    </div>
    <div class='p-4 space-y-3'>
      <div class='text-sm text-slate-700'>
        <div><span class='font-semibold'>Message: </span><span id='gErrMsgText'></span></div>
        <div><span class='font-semibold'>Source: </span><span id='gErrMsgSource'></span></div>
        <div><span class='font-semibold'>Line: </span><span id='gErrMsgLine'></span></div>
      </div>
      <div>
        <label class='block text-xs text-slate-500 mb-1'>Detail</label>
        <pre id='gErrMsgDetail' class='max-h-72 overflow-auto text-xs bg-slate-900/60 text-slate-100 rounded-lg p-3 border border-slate-700 whitespace-pre-wrap'></pre>
      </div>
      <div class='flex justify-end gap-2'>
        <button type='button' onclick='gCopyErrorMessage()' class='px-3 py-1.5 text-xs rounded border border-slate-300 bg-white bg-slate-100 text-slate-700'>Copy</button>
        <button type='button' onclick='gHideErrorMessage()' class='px-3 py-1.5 text-xs rounded bg-blue-600 text-white'>Close</button>
      </div>
    </div>
  </div>
</div>

<script>
(function() {
  if (window.__gErrorMessageInited) return;
  window.__gErrorMessageInited = true;

  const cfg = {
    autoCapture: __AUTO__,
    captureFetch: __FETCH__,
    captureWindowError: __WERR__,
    captureUnhandledRejection: __PREJ__
  };

  let currentErrorText = '';

  function toText(v) {
    if (v == null) return '';
    if (typeof v === 'string') return v;
    try { return JSON.stringify(v, null, 2); } catch (_) { return String(v); }
  }

  function normalizeError(input) {
    if (typeof input === 'string') {
      return { message: input, source: '', lineNumber: '', detail: input };
    }
    if (!input || typeof input !== 'object') {
      const t = toText(input);
      return { message: t || 'Unknown error', source: '', lineNumber: '', detail: t };
    }
    return {
      message: input.message || input.title || 'System error',
      source: input.source || input.fileName || input.url || '',
      lineNumber: input.lineNumber || input.lineno || input.line || '',
      detail: input.detail || input.stack || toText(input)
    };
  }

  window.gShowErrorMessage = function(input) {
    const e = normalizeError(input);
    const root = document.getElementById('gErrorMessageRoot');
    if (!root) return;

    const msg = document.getElementById('gErrMsgText');
    const src = document.getElementById('gErrMsgSource');
    const line = document.getElementById('gErrMsgLine');
    const detail = document.getElementById('gErrMsgDetail');

    if (msg) msg.textContent = e.message || '';
    if (src) src.textContent = e.source || '-';
    if (line) line.textContent = e.lineNumber || '-';
    if (detail) detail.textContent = e.detail || e.message || '';

    currentErrorText = 'Message: ' + (e.message || '') + '\n'
      + 'Source: ' + (e.source || '-') + '\n'
      + 'Line: ' + (e.lineNumber || '-') + '\n\n'
      + (e.detail || e.message || '');

    root.classList.remove('hidden');
    root.classList.add('flex');
  };

  window.gHideErrorMessage = function() {
    const root = document.getElementById('gErrorMessageRoot');
    if (!root) return;
    root.classList.add('hidden');
    root.classList.remove('flex');
  };

  window.gCopyErrorMessage = async function() {
    if (!currentErrorText) return;
    try {
      await navigator.clipboard.writeText(currentErrorText);
      if (window.gToast) window.gToast('Copied', 'success');
    } catch (_) {
      const ta = document.createElement('textarea');
      ta.value = currentErrorText;
      document.body.appendChild(ta);
      ta.select();
      document.execCommand('copy');
      ta.remove();
      if (window.gToast) window.gToast('Copied', 'success');
    }
  };

  if (!cfg.autoCapture) return;

  if (cfg.captureWindowError) {
    window.addEventListener('error', function(ev) {
      window.gShowErrorMessage({
        message: ev.message || 'Frontend error',
        source: ev.filename || '',
        lineNumber: ev.lineno || '',
        detail: (ev.error && ev.error.stack) || ev.message || ''
      });
    });
  }

  if (cfg.captureUnhandledRejection) {
    window.addEventListener('unhandledrejection', function(ev) {
      const reason = ev.reason || {};
      window.gShowErrorMessage({
        message: reason.message || 'Unhandled Promise error',
        source: '',
        lineNumber: '',
        detail: reason.stack || toText(reason)
      });
    });
  }

  if (cfg.captureFetch && !window.__gFetchWrapped) {
    window.__gFetchWrapped = true;
    const rawFetch = window.fetch.bind(window);
    window.fetch = async function() {
      const args = Array.prototype.slice.call(arguments);
      try {
        const res = await rawFetch.apply(window, args);
        if (!res.ok) {
          let payload = null;
          try {
            const ct = res.headers.get('content-type') || '';
            if (ct.indexOf('application/json') >= 0) payload = await res.clone().json();
            else payload = { message: await res.clone().text() };
          } catch (_) {}

          window.gShowErrorMessage({
            message: (payload && payload.message) || ('HTTP ' + res.status + ' ' + res.statusText),
            source: (args[0] && args[0].toString) ? args[0].toString() : '',
            lineNumber: (payload && payload.lineNumber) || '',
            detail: (payload && (payload.detail || payload.stack)) || toText(payload) || ''
          });
        }
        return res;
      } catch (err) {
        window.gShowErrorMessage({
          message: (err && err.message) || 'Network error',
          source: (args[0] && args[0].toString) ? args[0].toString() : '',
          lineNumber: '',
          detail: (err && err.stack) || toText(err)
        });
        throw err;
      }
    };
  }
})();
</script>";

            html = html.Replace("__TITLE__", System.Net.WebUtility.HtmlEncode(Title ?? string.Empty))
                       .Replace("__AUTO__", AutoCapture ? "true" : "false")
                       .Replace("__FETCH__", CaptureFetch ? "true" : "false")
                       .Replace("__WERR__", CaptureWindowError ? "true" : "false")
                       .Replace("__PREJ__", CaptureUnhandledRejection ? "true" : "false");

            output.Content.SetHtmlContent(html);
        }
    }
}

