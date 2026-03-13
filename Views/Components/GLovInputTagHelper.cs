using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.RegularExpressions;
using Web_EIP_Restruct.Models.Lov;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-lov-input")]
    public class GLovInputTagHelper : TagHelper
    {
        private const string RuntimeInjectedKey = "__g_lov_runtime_injected";

        [Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public string Label { get; set; } = string.Empty;
        public bool Required { get; set; } = false;
        public int ColSpan { get; set; } = 1;
        [HtmlAttributeName("row-span")]
        public int RowSpan { get; set; } = 1;

        public string HiddenId { get; set; } = string.Empty;
        public string HiddenValue { get; set; } = string.Empty;
        public string CodeId { get; set; } = string.Empty;
        public string CodeValue { get; set; } = string.Empty;
        public string CodePlaceholder { get; set; } = "請輸入代碼...";
        public string CodeWidth { get; set; } = "w-1/3";
        public string NameId { get; set; } = string.Empty;
        public string NameValue { get; set; } = string.Empty;
        public string NamePlaceholder { get; set; } = "請選擇資料";

        [HtmlAttributeName("x-model-code")]
        public string XModelCode { get; set; } = string.Empty;
        [HtmlAttributeName("x-model-name")]
        public string XModelName { get; set; } = string.Empty;
        [HtmlAttributeName("x-model-hidden")]
        public string XModelHidden { get; set; } = string.Empty;

        [HtmlAttributeName("id-prefix")]
        public string IdPrefix { get; set; } = string.Empty;

        [HtmlAttributeName("lov-fn")]
        public string LovFn { get; set; } = string.Empty;
        [HtmlAttributeName("config")]
        public LovInputConfig? Config { get; set; }

        [HtmlAttributeName("lov-title")]
        public string LovTitle { get; set; } = string.Empty;
        [HtmlAttributeName("lov-api")]
        public string LovApi { get; set; } = string.Empty;
        [HtmlAttributeName("lov-controller")]
        public string LovController { get; set; } = string.Empty;
        [HtmlAttributeName("lov-action")]
        public string LovAction { get; set; } = string.Empty;
        [HtmlAttributeName("lov-api-params")]
        public string LovApiParams { get; set; } = string.Empty;
        [HtmlAttributeName("lov-columns")]
        public string LovColumns { get; set; } = string.Empty;
        [HtmlAttributeName("lov-fields")]
        public string LovFields { get; set; } = string.Empty;
        [HtmlAttributeName("lov-key-hidden")]
        public string LovKeyHidden { get; set; } = string.Empty;
        [HtmlAttributeName("lov-key-code")]
        public string LovKeyCode { get; set; } = string.Empty;
        [HtmlAttributeName("lov-key-name")]
        public string LovKeyName { get; set; } = string.Empty;
        [HtmlAttributeName("lov-return-value-field")]
        public string LovReturnValueField { get; set; } = string.Empty;
        [HtmlAttributeName("lov-return-display-field")]
        public string LovReturnDisplayField { get; set; } = string.Empty;
        [HtmlAttributeName("lov-display-format")]
        public string LovDisplayFormat { get; set; } = string.Empty;
        [HtmlAttributeName("lov-on-confirm")]
        public string LovOnConfirm { get; set; } = string.Empty;
        [HtmlAttributeName("on-select")]
        public string OnSelect { get; set; } = string.Empty;
        [HtmlAttributeName("column-matches")]
        public string ColumnMatches { get; set; } = string.Empty;
        [HtmlAttributeName("filter-items")]
        public string FilterItems { get; set; } = string.Empty;

        [HtmlAttributeName("lov-buffer-view")]
        public bool? LovBufferView { get; set; }
        [HtmlAttributeName("lov-page-size")]
        public int? LovPageSize { get; set; }
        [HtmlAttributeName("lov-sort-enabled")]
        public bool? LovSortEnabled { get; set; }
        [HtmlAttributeName("lov-request-mode")]
        public string LovRequestMode { get; set; } = "auto";
        [HtmlAttributeName("lov-name")]
        public string LovName { get; set; } = string.Empty;
        [HtmlAttributeName("selectonly")]
        public bool SelectOnly { get; set; } = false;

        public bool ShowButton { get; set; } = true;
        public bool Readonly { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ApplyConfig();
            ApplyLovFieldDefaults();
            ApplyAutoBindingIds(context);
            ApplyAutoValues(context);
            EnsureRuntimeInjected(output);

            var colSpan  = Math.Max(1, ColSpan);
            var colClass = colSpan > 1 ? $"md:col-span-{colSpan}" : string.Empty;
            var rowSpan  = RowSpan >= 2 ? 2 : 1;
            var rowClass = rowSpan > 1 ? $"md:row-span-{rowSpan}" : "md:row-span-1";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {rowClass}".Trim());
            output.Attributes.SetAttribute("data-glov-input", "1");
            if (!string.IsNullOrWhiteSpace(LovName))
                output.Attributes.SetAttribute("data-lov-name", LovName.Trim());

            var labelHtml  = BuildLabelHtml();
            var hiddenHtml = BuildHiddenHtml();
            var codeHtml   = BuildCodeHtml();
            var nameHtml   = BuildNameHtml();
            var btnHtml    = BuildButtonHtml();

            var inputsHtml = $"<div class=\"flex items-center\">{hiddenHtml}{codeHtml}{nameHtml}{btnHtml}</div>";
            output.Content.SetHtmlContent(labelHtml + inputsHtml);
            AppendNamedLovRegistration(output);
        }


        private string BuildLabelHtml()
        {
            var req = Required ? " <span class=\"text-red-500 ml-0.5\">*</span>" : string.Empty;
            return $"<label class=\"text-xs font-semibold text-slate-600\">{HtmlEncode(Label)}{req}</label>";
        }

        private string BuildHiddenHtml()
        {
            if (string.IsNullOrEmpty(HiddenId)) return string.Empty;
            var xm = !string.IsNullOrEmpty(XModelHidden) ? $" x-model=\"{XModelHidden}\"" : string.Empty;
            return $"<input type=\"hidden\" id=\"{HtmlId(HiddenId)}\" name=\"{HtmlEncode(HiddenId)}\" data-lov-slot=\"hidden\"{xm} value=\"{HtmlEncode(HiddenValue)}\">";
        }

        private string BuildCodeHtml()
        {
            if (string.IsNullOrEmpty(CodeId)) return string.Empty;
            var xmAttr            = !string.IsNullOrEmpty(XModelCode) ? $" x-model=\"{XModelCode}\"" : string.Empty;
            return $"<input type=\"hidden\" id=\"{HtmlId(CodeId)}\" name=\"{HtmlEncode(CodeId)}\" data-lov-slot=\"code\""
                 + $" value=\"{HtmlEncode(CodeValue)}\"{xmAttr}>";
        }

        private string BuildNameHtml()
        {
            var hiddenNameHtml = string.Empty;
            if (!string.IsNullOrEmpty(NameId))
            {
                var hiddenXmAttr = !string.IsNullOrEmpty(XModelName) ? $" x-model=\"{XModelName}\"" : string.Empty;
                hiddenNameHtml = $"<input type=\"hidden\" id=\"{HtmlId(NameId)}\" name=\"{HtmlEncode(NameId)}\" data-lov-slot=\"name\""
                               + $" value=\"{HtmlEncode(NameValue)}\"{hiddenXmAttr}>";
            }

            var displayId = GetDisplayInputId();
            var readonlyAttr = Readonly || SelectOnly ? " readonly" : string.Empty;
            var onclick = BuildOnClick();
            var onClickAttr = SelectOnly && !string.IsNullOrWhiteSpace(onclick)
                ? $" onclick=\"{HtmlAttr(onclick)}\""
                : string.Empty;
            var onKeydown = Readonly || SelectOnly
                ? string.Empty
                : " onkeydown=\"if(event.key==='Enter'){event.preventDefault();window.gLov&&window.gLov.typeSearchInput&&window.gLov.typeSearchInput(this,true);}\"";
            var placeholder = !string.IsNullOrWhiteSpace(NamePlaceholder)
                ? NamePlaceholder
                : (!string.IsNullOrWhiteSpace(CodePlaceholder) ? CodePlaceholder : "請選擇資料");
            var displayValue = BuildInitialDisplayValue();

            return hiddenNameHtml
                 + $"<input type=\"text\" id=\"{HtmlId(displayId)}\" data-lov-slot=\"display\""
                 + $" value=\"{HtmlEncode(displayValue)}\""
                 + $" placeholder=\"{HtmlEncode(placeholder)}\""
                 + readonlyAttr + onKeydown + onClickAttr
                 + " class=\"block min-w-0 w-full flex-1 h-[42px] px-3 py-2.5 border border-slate-300 rounded-l-xl border-r-0 text-sm"
                 + " text-slate-700 bg-white placeholder:text-slate-400"
                 + (Readonly || SelectOnly ? " cursor-pointer" : " cursor-text")
                 + " focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors\">";
        }

        private string BuildButtonHtml()
        {
            if (!ShowButton) return string.Empty;

            var openCmd = BuildOnClick();
            var onclick = string.IsNullOrEmpty(openCmd) ? string.Empty : $" onclick=\"{HtmlAttr(openCmd)}\"";

            return $"<button type=\"button\" data-lov-open-btn=\"1\"{onclick}"
                 + " class=\"shrink-0 inline-flex items-center justify-center h-[42px] min-w-[42px] px-3 border border-l-0 border-slate-300 rounded-r-xl"
                 + " bg-white hover:bg-slate-50 text-slate-600 hover:text-blue-600 transition-colors\">"
                 + "<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\">"
                 + "<path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\""
                 + " d=\"M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z\"/>"
                 + "</svg>"
                 + "</button>";
        }


        private void ApplyAutoBindingIds(TagHelperContext context)
        {
            var hasAnyId = !string.IsNullOrWhiteSpace(HiddenId)
                        || !string.IsNullOrWhiteSpace(CodeId)
                        || !string.IsNullOrWhiteSpace(NameId);
            if (hasAnyId) return;

            var prefix = (IdPrefix ?? string.Empty).Trim();
            prefix = !string.IsNullOrWhiteSpace(prefix)
                ? prefix
                : BuildIdPrefixFromLovNameOrApi()
                    .Coalesce(BuildIdPrefixFromLabel(Label))
                    .Coalesce($"lov{(context.UniqueId ?? "lov").Replace("-", "").Replace(":", "")}");

            HiddenId = $"{prefix}Id";
            CodeId   = $"{prefix}Code";
            NameId   = $"{prefix}Name";
        }

        private void ApplyAutoValues(TagHelperContext context)
        {
            var isEmployeeLov =
                (LovApi   ?? string.Empty).Contains("/employees",  StringComparison.OrdinalIgnoreCase) ||
                (LovName  ?? string.Empty).Contains("employee",    StringComparison.OrdinalIgnoreCase);
            if (!isEmployeeLov || ViewContext?.ViewData == null) return;

            var hasExplicitBinding =
                context.AllAttributes.ContainsName("hidden-id") ||
                context.AllAttributes.ContainsName("code-id") ||
                context.AllAttributes.ContainsName("name-id") ||
                context.AllAttributes.ContainsName("hidden-value") ||
                context.AllAttributes.ContainsName("code-value") ||
                context.AllAttributes.ContainsName("name-value") ||
                context.AllAttributes.ContainsName("id-prefix");

            if (!hasExplicitBinding) return;

            if (!context.AllAttributes.ContainsName("hidden-value") && string.IsNullOrWhiteSpace(HiddenValue) &&
                ViewContext.ViewData.TryGetValue("NumericUserId", out var numId) && numId != null)
                HiddenValue = numId.ToString() ?? string.Empty;

            if (!context.AllAttributes.ContainsName("code-value") && string.IsNullOrWhiteSpace(CodeValue) &&
                ViewContext.ViewData.TryGetValue("UserId", out var userId) && userId != null)
                CodeValue = userId.ToString() ?? string.Empty;

            if (!context.AllAttributes.ContainsName("name-value") && string.IsNullOrWhiteSpace(NameValue) &&
                ViewContext.ViewData.TryGetValue("UserName", out var userName) && userName != null)
                NameValue = userName.ToString() ?? string.Empty;
        }

        private void ApplyConfig()
        {
            if (Config == null) return;
            LovTitle         = LovTitle.Coalesce(Config.Title);
            LovApi           = LovApi.Coalesce(Config.Api);
            LovColumns       = LovColumns.Coalesce(Config.Columns);
            LovFields        = LovFields.Coalesce(Config.Fields);
            LovKeyHidden     = LovKeyHidden.Coalesce(Config.KeyHidden);
            LovKeyCode       = LovKeyCode.Coalesce(Config.KeyCode);
            LovKeyName       = LovKeyName.Coalesce(Config.KeyName);
            LovDisplayFormat = LovDisplayFormat.Coalesce(Config.DisplayFormat);
            LovOnConfirm     = LovOnConfirm.Coalesce(Config.OnConfirm);
            if (!LovBufferView.HasValue && Config.BufferView.HasValue) LovBufferView = Config.BufferView.Value;
            if (!LovPageSize.HasValue   && Config.PageSize.HasValue)   LovPageSize   = Config.PageSize.Value;
            if (!LovSortEnabled.HasValue && Config.SortEnabled.HasValue) LovSortEnabled = Config.SortEnabled.Value;
        }

        private void ApplyLovFieldDefaults()
        {
            var fields = SplitCsv(LovFields);
            if (fields.Count == 0) return;

            var firstField = fields.ElementAtOrDefault(0) ?? string.Empty;
            var secondField = fields.ElementAtOrDefault(1) ?? firstField;
            var thirdField = fields.ElementAtOrDefault(2) ?? firstField;

            LovKeyCode = LovKeyCode.Coalesce(firstField);
            LovKeyName = LovKeyName.Coalesce(secondField);
            LovReturnDisplayField = LovReturnDisplayField.Coalesce(LovKeyName).Coalesce(secondField);
            LovReturnValueField = LovReturnValueField.Coalesce(LovKeyHidden).Coalesce(thirdField);
            LovKeyHidden = LovKeyHidden.Coalesce(LovReturnValueField).Coalesce(thirdField);
        }

        private void EnsureRuntimeInjected(TagHelperOutput output)
        {
            var httpContext = ViewContext?.HttpContext;
            if (httpContext == null) return;
            if (httpContext.Items.ContainsKey(RuntimeInjectedKey)) return;
            httpContext.Items[RuntimeInjectedKey] = true;
            output.PostElement.AppendHtml(@"
<div id=""gLovHost""></div>
<script src=""/js/g-lov-modal-runtime.js""></script>");
        }

        private void AppendNamedLovRegistration(TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(LovName)) return;

            var api = BuildLovApiUrl();
            if (string.IsNullOrWhiteSpace(api) || string.IsNullOrWhiteSpace(LovColumns) || string.IsNullOrWhiteSpace(LovFields))
            {
                return;
            }

            var title = string.IsNullOrWhiteSpace(LovTitle) ? Label : LovTitle;
            var cols = SplitCsv(LovColumns);
            var fields = SplitCsv(LovFields);
            var colsJs = "[" + string.Join(",", cols.Select(c => $"'{EscapeJs(c)}'")) + "]";
            var fieldsJs = "[" + string.Join(",", fields.Select(f => $"'{EscapeJs(f)}'")) + "]";

            output.PostElement.AppendHtml($@"
<script>
window.gLov = window.gLov || {{}};
if (typeof window.gLov.define === 'function') {{
  window.gLov.define('{EscapeJs(LovName)}', {{
    title: '{EscapeJs(title)}',
    api: '{EscapeJs(api)}',
    columns: {colsJs},
    fields: {fieldsJs}
  }});
}}
</script>");
        }


        private string BuildOnClick()
        {
            if (!string.IsNullOrWhiteSpace(LovFn)) return LovFn;

            var api         = BuildLovApiUrl();
            var formatFn    = BuildFormatFunction(GetEffectiveDisplayFormat());
            var callback    = string.IsNullOrWhiteSpace(LovOnConfirm) ? "null" : LovOnConfirm;
            var onSelectJs  = string.IsNullOrWhiteSpace(OnSelect) ? "null" : OnSelect;
            var colMatchJs  = ToJsObjectOrArrayOrNull(ColumnMatches);
            var filterJs    = ToJsObjectOrArrayOrNull(FilterItems);
            var pageSize    = Math.Max(1, LovPageSize ?? 50);
            var bufferView  = LovBufferView ?? true;
            var sortEnabled = LovSortEnabled ?? false;
            var reqMode     = NormalizeRequestMode(LovRequestMode);
            var sourceId    = EscapeJs(GetDisplayInputId());

            var options = $"{{ bufferView: {Bool(bufferView)}, pageSize: {pageSize},"
                        + $" sortEnabled: {Bool(sortEnabled)}, requestMode: '{reqMode}',"
                        + $" selectOnly: {Bool(SelectOnly)}, sourceInputId: '{sourceId}' }}";

            var map = BuildMapJs();

            if (!string.IsNullOrWhiteSpace(LovName))
                return $"gLov.openByName('{EscapeJs(LovName)}',"
                     + $"{{ map:{map}, formatDisplay:{formatFn}, onConfirm:{callback},"
                     + $" onSelect:{onSelectJs}, columnMatches:{colMatchJs}, filterItems:{filterJs}, options:{options} }})";

            if (string.IsNullOrWhiteSpace(api) || string.IsNullOrWhiteSpace(LovColumns) || string.IsNullOrWhiteSpace(LovFields))
                return string.Empty;

            var cols      = SplitCsv(LovColumns);
            var fields    = SplitCsv(LovFields);
            var title     = string.IsNullOrWhiteSpace(LovTitle) ? Label : LovTitle;
            var colsJs    = "[" + string.Join(",", cols.Select(c => $"'{EscapeJs(c)}'")) + "]";
            var fieldsJs  = "[" + string.Join(",", fields.Select(f => $"'{EscapeJs(f)}'")) + "]";

            return $"gLov.open({{ title:'{EscapeJs(title)}', api:'{EscapeJs(api)}',"
                 + $" columns:{colsJs}, fields:{fieldsJs}, map:{map},"
                 + $" formatDisplay:{formatFn}, onConfirm:{callback},"
                 + $" onSelect:{onSelectJs}, columnMatches:{colMatchJs}, filterItems:{filterJs}, options:{options} }})";
        }

        private string BuildMapJs()
        {
            var sb    = new StringBuilder("{");
            bool first = true;

            var keyHidden = LovKeyHidden.Coalesce(LovReturnValueField);
            var keyCode   = LovKeyCode.Coalesce(LovReturnValueField);
            var keyName   = LovKeyName.Coalesce(LovReturnDisplayField);
            var displayId = GetDisplayInputId();

            void Append(string key, string targetId)
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(targetId)) return;
                if (!first) sb.Append(',');
                sb.Append($"'{EscapeJs(key)}':'{EscapeJs(targetId)}'");
                first = false;
            }

            Append(keyHidden, HiddenId);
            Append(keyCode,   CodeId);
            Append(keyName,   NameId);
            Append("FORMATTED_DISPLAY", displayId);

            sb.Append('}');
            return sb.ToString();
        }

        private string GetDisplayInputId()
        {
            if (!string.IsNullOrWhiteSpace(NameId)) return $"{NameId}_display";
            if (!string.IsNullOrWhiteSpace(CodeId)) return $"{CodeId}_display";
            if (!string.IsNullOrWhiteSpace(HiddenId)) return $"{HiddenId}_display";
            return "lovDisplay";
        }

        private string BuildInitialDisplayValue()
        {
            var code = (CodeValue ?? string.Empty).Trim();
            var name = (NameValue ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
            {
                return $"{code}-{name}";
            }

            return !string.IsNullOrWhiteSpace(name) ? name : code;
        }

        private string GetEffectiveDisplayFormat()
        {
            if (!string.IsNullOrWhiteSpace(LovDisplayFormat))
            {
                return LovDisplayFormat;
            }

            var keyCode = LovKeyCode.Coalesce(LovReturnValueField);
            var keyName = LovKeyName.Coalesce(LovReturnDisplayField);

            if (!string.IsNullOrWhiteSpace(keyCode) &&
                !string.IsNullOrWhiteSpace(keyName) &&
                !string.Equals(keyCode, keyName, StringComparison.OrdinalIgnoreCase))
            {
                return $"{{{keyCode}}}-{{{keyName}}}";
            }

            if (!string.IsNullOrWhiteSpace(keyName))
            {
                return $"{{{keyName}}}";
            }

            if (!string.IsNullOrWhiteSpace(keyCode))
            {
                return $"{{{keyCode}}}";
            }

            return string.Empty;
        }


        private string BuildIdPrefixFromLovNameOrApi()
        {
            var source = LovName.Trim().Coalesce(LovApi.Trim());
            if (string.IsNullOrWhiteSpace(source)) return string.Empty;

            source = Regex.Replace(source, "[^A-Za-z0-9]+", " ").Trim();
            var parts = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;

            var sb = new StringBuilder(parts[0].ToLowerInvariant());
            for (var i = 1; i < parts.Length; i++)
            {
                var p = parts[i];
                sb.Append(char.ToUpperInvariant(p[0]));
                if (p.Length > 1) sb.Append(p.Substring(1).ToLowerInvariant());
            }
            return sb.ToString();
        }

        private static string BuildIdPrefixFromLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) return string.Empty;
            var chars = label.Where(ch => ch <= 127 && char.IsLetterOrDigit(ch)).ToArray();
            if (chars.Length == 0) return string.Empty;
            var prefix = new string(chars);
            return char.IsLetter(prefix[0]) ? prefix : "lov" + prefix;
        }

        private static string BuildFormatFunction(string format)
        {
            if (string.IsNullOrWhiteSpace(format)) return "null";
            var sb = new StringBuilder("function(d){ return `");
            var i  = 0;
            while (i < format.Length)
            {
                if (format[i] == '{')
                {
                    int end = format.IndexOf('}', i + 1);
                    if (end > i + 1)
                    {
                        var key = format.Substring(i + 1, end - i - 1).Trim();
                        sb.Append("${d.").Append(EscapeTemplateKey(key)).Append(" ?? ''}");
                        i = end + 1;
                        continue;
                    }
                }
                if (format[i] == '`') sb.Append("\\`");
                else sb.Append(format[i]);
                i++;
            }
            sb.Append("`; }");
            return sb.ToString();
        }

        private string BuildLovApiUrl()
        {
            var api = (LovApi ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(api)) return api;

            var controller = (LovController ?? string.Empty).Trim().Trim('/');
            var action     = (LovAction     ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action)) return string.Empty;

            var path = $"/{controller}/{action}";
            var q    = (LovApiParams ?? string.Empty).Trim().TrimStart('?', '&');
            return string.IsNullOrWhiteSpace(q) ? path : $"{path}?{q}";
        }

        private static string NormalizeRequestMode(string mode)
        {
            var m = (mode ?? string.Empty).Trim().ToLowerInvariant();
            return m is "htmx" or "fetch" ? m : "auto";
        }

        private static string ToJsObjectOrArrayOrNull(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "null";
            var t = raw.Trim();
            return (t.StartsWith("[") && t.EndsWith("]")) || (t.StartsWith("{") && t.EndsWith("}")) ? t : "null";
        }

        private static List<string> SplitCsv(string s)
            => s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        private static string Bool(bool v)       => v ? "true" : "false";
        private static string EscapeJs(string s) => (s ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
        private static string EscapeTemplateKey(string s)
            => string.IsNullOrWhiteSpace(s) ? "" : s.Replace("`", "").Replace("{", "").Replace("}", "").Replace(" ", "");
        private static string HtmlId(string s)     => System.Net.WebUtility.HtmlEncode(s);
        private static string HtmlEncode(string s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string s)   => s?.Replace("\"", "&quot;") ?? string.Empty;
    }

    internal static class StringExtensions
    {
        public static string Coalesce(this string? self, string? fallback)
            => string.IsNullOrWhiteSpace(self) ? (fallback ?? string.Empty) : self;
    }
}

