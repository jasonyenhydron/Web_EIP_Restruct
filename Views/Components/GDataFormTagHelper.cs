using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Json;
using Web_EIP_Restruct.Models.DataForm;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-dataform")]
    [RestrictChildren("form-column", "query-column")]
    public class GDataFormTagHelper : TagHelper
    {
        private const string RuntimeInjectedKey = "__g_dataform_runtime_injected";

        [Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public string Id { get; set; } = "";
        public string Title { get; set; } = "";

        [HtmlAttributeName("api")]
        public string Api { get; set; } = "";

        [HtmlAttributeName("member-id")]
        public string DataMember { get; set; } = "";

        public int HorizontalColumnsCount { get; set; } = 2;
        public int HorizontalGap { get; set; } = 4;
        public int VerticalGap { get; set; } = 4;

        [HtmlAttributeName("caption-alignment")]
        public string CaptionAlignment { get; set; } = "left";

        [HtmlAttributeName("extra-class")]
        public string ExtraClass { get; set; } = "";

        [HtmlAttributeName("always-read-only")]
        public bool AlwaysReadOnly { get; set; } = false;

        [HtmlAttributeName("continue-add")]
        public bool ContinueAdd { get; set; } = false;

        [HtmlAttributeName("is-auto-page-close")]
        public bool IsAutoPageClose { get; set; } = false;

        [HtmlAttributeName("duplicate-check")]
        public bool DuplicateCheck { get; set; } = false;

        [HtmlAttributeName("validate-style")]
        public string ValidateStyle { get; set; } = "Hint";

        [HtmlAttributeName("show-apply-button")]
        public bool ShowApplyButton { get; set; } = false;

        [HtmlAttributeName("not-init-load")]
        public bool NotInitLoad { get; set; } = false;

        [HtmlAttributeName("chain-dataform-id")]
        public string ChainDataFormID { get; set; } = "";

        [HtmlAttributeName("parent-object-id")]
        public string ParentObjectID { get; set; } = "";

        [HtmlAttributeName("relation-columns")]
        public string RelationColumns { get; set; } = "";

        [HtmlAttributeName("tool-items")]
        public string ToolItems { get; set; } = "";

        [HtmlAttributeName("query-api")]
        public string QueryApi { get; set; } = "";

        [HtmlAttributeName("query-title")]
        public string QueryTitle { get; set; } = "查詢條件";

        [HtmlAttributeName("on-load-success")]
        public string OnLoadSuccess { get; set; } = "";

        [HtmlAttributeName("on-apply")]
        public string OnApply { get; set; } = "";

        [HtmlAttributeName("on-applied")]
        public string OnApplied { get; set; } = "";

        [HtmlAttributeName("on-cancel")]
        public string OnCancel { get; set; } = "";

        [HtmlAttributeName("on-before-validate")]
        public string OnBeforeValidate { get; set; } = "";

        [HtmlAttributeName("on-query-loaded")]
        public string OnQueryLoaded { get; set; } = "";

        [HtmlAttributeName("status-target-id")]
        public string StatusTargetId { get; set; } = "";

        [HtmlAttributeName("saving-message")]
        public string SavingMessage { get; set; } = "資料庫寫入中...";

        [HtmlAttributeName("validate-failed-message")]
        public string ValidateFailedMessage { get; set; } = "錯誤：欄位驗證未通過。";

        [HtmlAttributeName("save-success-message")]
        public string SaveSuccessMessage { get; set; } = "Success: {message} (流水號: {id})";

        [HtmlAttributeName("save-error-message")]
        public string SaveErrorMessage { get; set; } = "Error: {message}";

        [HtmlAttributeName("success-toast-message")]
        public string SuccessToastMessage { get; set; } = "";

        [HtmlAttributeName("error-toast-message")]
        public string ErrorToastMessage { get; set; } = "";

        [HtmlAttributeName("exception-toast-message")]
        public string ExceptionToastMessage { get; set; } = "";

        [HtmlAttributeName("empty-string-as-null")]
        public bool EmptyStringAsNull { get; set; } = false;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(DataMember) &&
                context.AllAttributes.TryGetAttribute("data-member", out var memberAttr))
            {
                DataMember = memberAttr.Value?.ToString() ?? string.Empty;
            }

            var colCtx = new FormColumnContext();
            context.Items[typeof(FormColumnContext)] = colCtx;
            await output.GetChildContentAsync();

            var formId = string.IsNullOrWhiteSpace(Id)
                ? $"gdf_{Guid.NewGuid():N}"
                : Id.Trim();

            var gridCols = Math.Max(1, HorizontalColumnsCount);

            EnsureRuntimeInjected(output);

            output.TagName = "div";
            output.Attributes.SetAttribute("id", formId);
            output.Attributes.SetAttribute("data-g-dataform", "1");
            output.Attributes.SetAttribute("data-api", Api);
            output.Attributes.SetAttribute("data-query-api", string.IsNullOrWhiteSpace(QueryApi) ? Api : QueryApi);
            output.Attributes.SetAttribute("data-member", DataMember);
            output.Attributes.SetAttribute("data-validate-style", ValidateStyle.ToLowerInvariant());
            output.Attributes.SetAttribute("data-duplicate-check", DuplicateCheck ? "1" : "0");
            output.Attributes.SetAttribute("data-continue-add", ContinueAdd ? "1" : "0");
            output.Attributes.SetAttribute("data-auto-page-close", IsAutoPageClose ? "1" : "0");
            output.Attributes.SetAttribute("data-show-apply-button", ShowApplyButton ? "1" : "0");
            output.Attributes.SetAttribute("data-not-init-load", NotInitLoad ? "1" : "0");
            output.Attributes.SetAttribute("data-always-readonly", AlwaysReadOnly ? "1" : "0");
            output.Attributes.SetAttribute("data-parent-id", ParentObjectID);
            output.Attributes.SetAttribute("data-chain-id", ChainDataFormID);
            output.Attributes.SetAttribute("data-on-load-success", OnLoadSuccess);
            output.Attributes.SetAttribute("data-on-apply", OnApply);
            output.Attributes.SetAttribute("data-on-applied", OnApplied);
            output.Attributes.SetAttribute("data-on-cancel", OnCancel);
            output.Attributes.SetAttribute("data-on-before-validate", OnBeforeValidate);
            output.Attributes.SetAttribute("data-on-query-loaded", OnQueryLoaded);
            output.Attributes.SetAttribute("data-status-target-id", StatusTargetId);
            output.Attributes.SetAttribute("data-saving-message", SavingMessage);
            output.Attributes.SetAttribute("data-validate-failed-message", ValidateFailedMessage);
            output.Attributes.SetAttribute("data-save-success-message", SaveSuccessMessage);
            output.Attributes.SetAttribute("data-save-error-message", SaveErrorMessage);
            output.Attributes.SetAttribute("data-success-toast-message", SuccessToastMessage);
            output.Attributes.SetAttribute("data-error-toast-message", ErrorToastMessage);
            output.Attributes.SetAttribute("data-exception-toast-message", ExceptionToastMessage);
            output.Attributes.SetAttribute("data-empty-string-as-null", EmptyStringAsNull ? "1" : "0");
            output.Attributes.SetAttribute("data-column-meta", BuildColumnMetaJson(colCtx.Columns));
            if (!string.IsNullOrWhiteSpace(RelationColumns))
                output.Attributes.SetAttribute("data-relation-columns", RelationColumns);

            var baseClass = $"bg-white rounded-xl border border-slate-200 shadow-sm {ExtraClass}".Trim();
            output.Attributes.SetAttribute("class", baseClass);

            var alpineData = BuildAlpineData(formId, colCtx.Columns, colCtx.QueryColumns);
            output.Attributes.SetAttribute("x-data", alpineData);
            output.Attributes.SetAttribute("x-init", "init()");

            output.Content.SetHtmlContent(BuildHtml(formId, colCtx.Columns, colCtx.QueryColumns, gridCols));
        }

        private string BuildAlpineData(string formId, List<FormColumn> columns, List<QueryColumn> queryColumns)
        {
            var fields = string.Join(",", columns
                .Where(c => !c.Hidden)
                .Select(c => $"'{JsEsc(c.FieldName)}':''"));
            var queryDefaults = string.Join(",", queryColumns.Select(c =>
                $"'{JsEsc(c.FieldName)}':'{JsEsc(c.DefaultValue ?? string.Empty)}'"));

            return $"gDataForm('{formId}',{{{fields}}},{{{queryDefaults}}})";
        }

        private string BuildHtml(string formId, List<FormColumn> columns, List<QueryColumn> queryColumns, int gridCols)
        {
            var sb = new StringBuilder();
            sb.Append(BuildQueryPanel(formId, queryColumns));
            sb.Append(BuildToolbar(formId));

            sb.Append($"<div x-show=\"mode==='view'\" class=\"p-{VerticalGap}\">");
            sb.Append(BuildViewGrid(columns, gridCols));
            sb.Append("</div>");

            sb.Append(BuildModal(formId, columns, gridCols));
            sb.Append(BuildDeleteModal(formId));

            return sb.ToString();
        }

        private string BuildQueryPanel(string formId, List<QueryColumn> queryColumns)
        {
            if (queryColumns.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.Append("<div class=\"border-b border-slate-200 shrink-0\">");
            sb.Append("<div class=\"flex items-center justify-between px-4 py-3 border-b border-slate-200 bg-slate-50\">");
            sb.Append($"<div class=\"inline-flex items-center gap-2 text-blue-600 font-semibold\"><svg class=\"w-4 h-4 text-blue-600\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M7 7h10l-4 5v5l-2-1v-4L7 7z\"/></svg><span>{HtmlEnc(QueryTitle)}</span></div>");
            sb.Append("<button type=\"button\" @click=\"queryPanelOpen = !queryPanelOpen\" class=\"inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-slate-300 text-sm font-semibold text-slate-700 bg-white hover:bg-slate-50 transition-colors\">");
            sb.Append("<svg class=\"w-3.5 h-3.5 transition-transform\" :class=\"queryPanelOpen ? '' : 'rotate-180'\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M19 9l-7 7-7-7\"/></svg>");
            sb.Append("<span x-text=\"queryPanelOpen ? '收合' : '展開'\"></span></button></div>");
            sb.Append("<div x-show=\"queryPanelOpen\" class=\"px-4 py-3\">");
            sb.Append("<div class=\"grid grid-cols-1 md:grid-cols-12 gap-3\">");

            foreach (var col in queryColumns)
            {
                var field = HtmlEnc(col.FieldName);
                var caption = HtmlEnc(col.Caption);
                var span = Math.Clamp(col.Span <= 0 ? 3 : col.Span, 1, 12);
                sb.Append($"<div class=\"md:col-span-{span} flex flex-col gap-1\">");
                sb.Append($"<label class=\"text-xs font-semibold text-slate-600\">{caption}</label>");
                sb.Append(BuildQueryField(formId, col));
                sb.Append("</div>");
            }

            sb.Append("</div>");
            sb.Append("<div class=\"mt-4 pt-4 border-t border-slate-100 flex flex-wrap gap-2\">");
            sb.Append("<button type=\"button\" @click=\"executeQuery()\" class=\"inline-flex items-center gap-1.5 px-3 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700\">");
            sb.Append("<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z\"/></svg><span>查詢</span></button>");
            sb.Append("<button type=\"button\" @click=\"resetQuery()\" class=\"inline-flex items-center gap-1.5 px-3 py-2 text-sm font-semibold text-slate-700 bg-slate-100 rounded-lg hover:bg-slate-200\">");
            sb.Append("<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15\"/></svg><span>清除條件</span></button>");
            sb.Append("</div></div></div>");
            return sb.ToString();
        }

        private string BuildQueryField(string formId, QueryColumn col)
        {
            var field = JsEsc(col.FieldName);
            var placeholder = string.IsNullOrWhiteSpace(col.Placeholder) ? "請輸入" : HtmlEnc(col.Placeholder);
            var baseInput = "w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none";
            var queryAttr = $" data-query-field=\"{HtmlEnc(string.IsNullOrWhiteSpace(col.QueryField) ? col.FieldName : col.QueryField)}\" data-query-model=\"{HtmlEnc(col.FieldName)}\"";

            switch ((col.Editor ?? "text").Trim().ToLowerInvariant())
            {
                case "readonly":
                    return $"<input type=\"text\" x-model=\"queryValues['{field}']\" readonly class=\"{baseInput} bg-slate-100 text-slate-500 cursor-not-allowed\" placeholder=\"{placeholder}\"{queryAttr}>";
                case "date":
                case "datebox":
                    return $"<input type=\"date\" x-model=\"queryValues['{field}']\" class=\"{baseInput}\"{queryAttr}>";
                case "checkbox":
                    return $"<label class=\"inline-flex items-center gap-2 py-2\"><input type=\"checkbox\" :checked=\"queryValues['{field}']==='Y' || queryValues['{field}']===true || queryValues['{field}']===1\" @change=\"queryValues['{field}']=$event.target.checked ? 'Y' : 'N'\" class=\"w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500\"{queryAttr}><span class=\"text-sm text-slate-600\">是/否</span></label>";
                case "select":
                case "gcombobox":
                case "infocombobox":
                    return BuildQuerySelectField(field, col, baseInput, queryAttr);
                case "lov":
                case "lovinput":
                    return BuildQueryLovField(formId, col, baseInput, queryAttr);
                default:
                    return $"<input type=\"text\" x-model=\"queryValues['{field}']\" class=\"{baseInput}\" placeholder=\"{placeholder}\"{queryAttr}>";
            }
        }

        private string BuildQuerySelectField(string field, QueryColumn col, string baseInput, string queryAttr)
        {
            var sb = new StringBuilder();
            sb.Append($"<select x-model=\"queryValues['{field}']\" class=\"{baseInput}\"{queryAttr}>");
            sb.Append("<option value=\"\">請選擇</option>");
            if (!string.IsNullOrWhiteSpace(col.Options))
            {
                foreach (var opt in col.Options.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = opt.Split(new[] { '=', ':' }, 2);
                    var value = HtmlEnc(parts[0].Trim());
                    var text = parts.Length > 1 ? HtmlEnc(parts[1].Trim()) : value;
                    sb.Append($"<option value=\"{value}\">{text}</option>");
                }
            }
            sb.Append("</select>");
            return sb.ToString();
        }

        private string BuildQueryLovField(string formId, QueryColumn col, string baseInput, string queryAttr)
        {
            var field = JsEsc(col.FieldName);
            var title = JsEsc(string.IsNullOrWhiteSpace(col.LovTitle) ? col.Caption : col.LovTitle);
            var cols = JsEsc(col.LovColumns);
            var fields = JsEsc(col.LovFields);
            var api = JsEsc(col.LovApi);
            var keyVal = JsEsc(string.IsNullOrWhiteSpace(col.LovKeyValue) ? col.FieldName : col.LovKeyValue);
            var keyDisp = JsEsc(string.IsNullOrWhiteSpace(col.LovKeyDisplay) ? col.FieldName : col.LovKeyDisplay);
            var displayField = string.IsNullOrWhiteSpace(col.LovKeyDisplay) ? col.FieldName : col.LovKeyDisplay;
            var displayFieldJs = JsEsc(displayField);
            var effectiveFormat = GetEffectiveLovDisplayFormat(col.LovKeyValue, col.LovKeyDisplay, col.LovDisplayFormat, col.FieldName);
            var fmt = JsEsc(effectiveFormat);
            var inputId = $"{formId}_query_{col.FieldName}";
            var displayMap = $"'FORMATTED_DISPLAY':'{HtmlAttr(inputId)}'";
            var onConfirm = $"function(selected){{const root=document.getElementById('{HtmlAttr(formId)}');if(!root||!root._x_dataStack)return;const data=Alpine.$data(root);if(data){{data.queryValues['{field}']=selected['{keyVal}']??'';data.queryValues['{displayFieldJs}']={(string.IsNullOrWhiteSpace(fmt) ? $"(selected['{keyDisp}']??'')" : BuildLovDisplayExpression(fmt, "selected"))};}}}}";
            var openJs = $"gLov.open({{title:'{title}',api:'{api}',columns:['{cols}'.split(',')].flat(),fields:['{fields}'.split(',')].flat(),map:{{'{keyVal}':'{HtmlAttr(inputId)}_hidden',{displayMap}}},formatDisplay:{(string.IsNullOrWhiteSpace(fmt) ? "null" : $"function(d){{return {BuildLovDisplayExpression(fmt, "d")};}}")},onConfirm:{onConfirm}}})";

            return "<div class=\"flex items-center\">"
                 + $"<input type=\"hidden\" id=\"{inputId}_hidden\" x-model=\"queryValues['{field}']\"{queryAttr}>"
                 + $"<input type=\"text\" id=\"{inputId}\" x-model=\"queryValues['{displayFieldJs}']\" readonly placeholder=\"{HtmlEnc(col.Placeholder.Coalesce("請選擇資料"))}\" class=\"block min-w-0 flex-1 px-3 py-2.5 border border-slate-300 rounded-l-xl border-r-0 text-sm text-slate-700 bg-white placeholder:text-slate-400 cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors\">"
                 + $"<button type=\"button\" onclick=\"{HtmlAttr(openJs)}\" class=\"shrink-0 inline-flex items-center justify-center px-3 border border-slate-300 rounded-r-xl bg-white hover:bg-slate-50 text-slate-600 hover:text-blue-600 transition-colors\">"
                 + "<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z\"/></svg></button></div>";
        }

        private string BuildToolbar(string formId)
        {
            if (AlwaysReadOnly) return string.Empty;

            var defaultTools = string.IsNullOrWhiteSpace(ToolItems)
                ? "[{\"action\":\"add\",\"label\":\"新增\"},{\"action\":\"edit\",\"label\":\"修改\",\"requireSelection\":true},{\"action\":\"delete\",\"label\":\"刪除\",\"requireSelection\":true}]"
                : ToolItems;

            var sb = new StringBuilder();
            sb.Append("<div class=\"flex items-center justify-between px-4 py-2 border-b border-slate-200 bg-slate-50 rounded-t-xl\">");

            if (!string.IsNullOrWhiteSpace(Title))
                sb.Append($"<span class=\"text-sm font-bold text-slate-700\">{HtmlEnc(Title)}</span>");
            else
                sb.Append("<span></span>");

            sb.Append("<div class=\"flex gap-2\" x-data=\"{}\">");
            sb.Append($"<template x-for=\"tool in {HtmlAttr(defaultTools)}\" :key=\"tool.action\">");
            sb.Append("<button type=\"button\"");
            sb.Append(" :disabled=\"tool.requireSelection && !hasSelection\"");
            sb.Append($" @click=\"handleToolAction(tool.action, tool.onClick, '{formId}')\"");
            sb.Append(" :class=\"{'opacity-40 cursor-not-allowed': tool.requireSelection && !hasSelection}\"");
            sb.Append(" class=\"inline-flex items-center gap-1 px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-300 bg-white text-slate-700 hover:bg-blue-50 hover:text-blue-700 hover:border-blue-400 transition-colors shadow-sm\">");
            sb.Append("<span x-text=\"tool.label\"></span>");
            sb.Append("</button>");
            sb.Append("</template>");
            sb.Append("</div>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private string BuildViewGrid(List<FormColumn> columns, int gridCols)
        {
            var sb = new StringBuilder();
            sb.Append($"<div class=\"grid gap-{HorizontalGap}\" style=\"grid-template-columns:repeat({gridCols},minmax(0,1fr));\">");

            foreach (var col in columns.Where(c => !c.Hidden && c.ColumnType != FormColumnType.Hidden))
            {
                var colSpan = Math.Max(1, col.ColSpan);
                var spanStyle = colSpan > 1 ? $" style=\"grid-column:span {colSpan}/span {colSpan};\"" : "";
                var alignClass = col.CaptionAlignment switch
                {
                    "center" => "text-center",
                    "right" => "text-right",
                    _ => "text-left"
                };

                sb.Append($"<div class=\"flex flex-col gap-1\"{spanStyle}>");
                sb.Append($"<label class=\"text-xs font-semibold text-slate-500 {alignClass}\">{HtmlEnc(col.Caption)}</label>");
                var displayField = col.ColumnType == FormColumnType.Lov && !string.IsNullOrWhiteSpace(col.LovKeyDisplay)
                    ? JsEsc(col.LovKeyDisplay)
                    : JsEsc(col.FieldName);
                sb.Append($"<div class=\"min-h-9 px-3 py-2 rounded-lg border border-slate-200 bg-slate-50 text-sm text-slate-700 break-all\"");
                sb.Append($" x-text=\"(formData && formData['{displayField}'] != null) ? formData['{displayField}'] : ''\"></div>");
                sb.Append("</div>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        private string BuildModal(string formId, List<FormColumn> columns, int gridCols)
        {
            var modalId = $"{formId}_modal";
            var sb = new StringBuilder();

            sb.Append($"<div id=\"{modalId}\" tabindex=\"-1\" aria-hidden=\"true\"");
            sb.Append(" class=\"hidden overflow-y-auto overflow-x-hidden fixed top-0 right-0 left-0 z-50 justify-center items-center w-full md:inset-0 h-[calc(100%-1rem)] max-h-full\">");
            sb.Append("<div class=\"relative p-4 w-full max-w-2xl max-h-full\">");
            sb.Append("<div class=\"relative bg-white rounded-xl shadow-xl border border-slate-200\">");

            sb.Append("<div class=\"flex items-center justify-between p-4 border-b border-slate-200 rounded-t-xl bg-blue-600\">");
            sb.Append($"<h3 class=\"text-sm font-bold text-white\" x-text=\"mode==='add' ? '新增 - {HtmlEnc(Title)}' : '修改 - {HtmlEnc(Title)}'\"></h3>");
            sb.Append($"<button type=\"button\" @click=\"closeModal('{modalId}')\"");
            sb.Append(" class=\"text-white hover:bg-blue-700 rounded-lg p-1 transition-colors\">");
            sb.Append("<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M6 18L18 6M6 6l12 12\"/></svg>");
            sb.Append("</button>");
            sb.Append("</div>");

            sb.Append("<div class=\"p-4\">");
            sb.Append($"<div class=\"grid gap-{HorizontalGap}\" style=\"grid-template-columns:repeat({gridCols},minmax(0,1fr));\">");

            foreach (var col in columns.Where(c => c.ColumnType != FormColumnType.Hidden))
            {
                if (col.Hidden) continue;
                var colSpan = Math.Max(1, col.ColSpan);
                var spanStyle = colSpan > 1 ? $" style=\"grid-column:span {colSpan}/span {colSpan};\"" : "";
                var req = col.Required ? " <span class=\"text-red-500\">*</span>" : "";
                var alignClass = (col.CaptionAlignment ?? CaptionAlignment) switch
                {
                    "center" => "text-center",
                    "right" => "text-right",
                    _ => "text-left"
                };

                sb.Append($"<div class=\"flex flex-col gap-1\"{spanStyle}>");
                sb.Append($"<label class=\"text-xs font-semibold text-slate-600 {alignClass}\">{HtmlEnc(col.Caption)}{req}</label>");
                sb.Append(BuildFormField(formId, col));

                if (ValidateStyle.Equals("Hint", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append($"<p class=\"text-xs text-red-500 hidden\" id=\"{formId}_err_{col.FieldName}\"");
                    sb.Append($" x-show=\"errors['{JsEsc(col.FieldName)}']\"");
                    sb.Append($" x-text=\"errors['{JsEsc(col.FieldName)}']\"></p>");
                }

                sb.Append("</div>");
            }

            foreach (var col in columns.Where(c => c.Hidden || c.ColumnType == FormColumnType.Hidden))
            {
                var defaultAttr = col.DefaultValue == null ? "" : $" data-field-default=\"{HtmlEnc(col.DefaultValue)}\"";
                sb.Append($"<input type=\"hidden\" name=\"{HtmlEnc(col.FieldName)}\" data-field-name=\"{HtmlEnc(col.FieldName)}\"{defaultAttr} :value=\"formData['{JsEsc(col.FieldName)}']\"/>");
            }

            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<div class=\"flex items-center justify-end gap-3 p-4 border-t border-slate-200 bg-slate-50 rounded-b-xl\">");
            sb.Append($"<button type=\"button\" @click=\"submitForm('{formId}','{modalId}')\"");
            sb.Append(" :disabled=\"loading\"");
            sb.Append(" :class=\"loading ? 'opacity-60 cursor-not-allowed' : ''\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors shadow-sm\">");
            sb.Append("<span x-text=\"loading ? '處理中...' : (mode==='add' ? '新增' : '儲存')\"></span></button>");
            sb.Append($"<button type=\"button\" @click=\"cancelForm('{formId}','{modalId}')\"");
            sb.Append(" :disabled=\"loading\"");
            sb.Append(" :class=\"loading ? 'opacity-60 cursor-not-allowed' : ''\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-100 transition-colors shadow-sm\">取消</button>");
            sb.Append("</div>");

            sb.Append("</div></div></div>");
            return sb.ToString();
        }

        private string BuildDeleteModal(string formId)
        {
            var delModalId = $"{formId}_del_modal";
            var sb = new StringBuilder();

            sb.Append($"<div id=\"{delModalId}\" tabindex=\"-1\" aria-hidden=\"true\"");
            sb.Append(" class=\"hidden overflow-y-auto overflow-x-hidden fixed top-0 right-0 left-0 z-50 justify-center items-center w-full md:inset-0 h-[calc(100%-1rem)] max-h-full\">");
            sb.Append("<div class=\"relative p-4 w-full max-w-md max-h-full\">");
            sb.Append("<div class=\"relative bg-white rounded-xl shadow-xl border border-slate-200 p-6 text-center\">");
            sb.Append("<svg class=\"mx-auto mb-4 text-red-400 w-12 h-12\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"1.5\" d=\"M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z\"/></svg>");
            sb.Append("<h3 class=\"mb-2 text-base font-bold text-slate-800\">確認刪除</h3>");
            sb.Append("<p class=\"mb-5 text-sm text-slate-500\">確定要刪除此筆資料嗎？此操作無法復原。</p>");
            sb.Append("<div class=\"flex justify-center gap-3\">");
            sb.Append($"<button type=\"button\" @click=\"confirmDelete('{formId}','{delModalId}')\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors\">確認刪除</button>");
            sb.Append($"<button type=\"button\" @click=\"closeModal('{delModalId}')\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-100 transition-colors\">取消</button>");
            sb.Append("</div>");
            sb.Append("</div></div></div>");

            return sb.ToString();
        }

        private string BuildFormField(string formId, FormColumn col)
        {
            var f = JsEsc(col.FieldName);
            var isRo = col.AlwaysReadOnly || col.IsPrimaryKey;
            var baseInput = "px-3 py-2 border rounded-lg text-sm w-full focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors";
            var roAttr = isRo ? " :readonly=\"mode==='edit' || alwaysReadOnly\"" : "";
            var disAttr = isRo ? " :disabled=\"mode==='edit' || alwaysReadOnly\"" : "";
            var classBinding = isRo
                ? " :class=\"(mode==='edit' || alwaysReadOnly) ? 'bg-slate-100 text-slate-500 cursor-not-allowed border-slate-200' : 'bg-white text-slate-800 border-slate-300'\""
                : " :class=\"'bg-white text-slate-800 border-slate-300'\"";
            var onChange = !string.IsNullOrWhiteSpace(col.OnChange) ? $" @change=\"{HtmlAttr(col.OnChange)}($event,formData)\"" : "";
            var vModel = $" x-model=\"formData['{f}']\"";
            var validateAttrs = BuildValidateAttrs(col);

            return col.ColumnType switch
            {
                FormColumnType.Readonly => BuildReadonlyField(f),
                FormColumnType.Textarea => BuildTextareaField(f, col, baseInput, onChange, roAttr, classBinding, validateAttrs),
                FormColumnType.Select => BuildSelectField(f, col, baseInput, onChange, disAttr, classBinding, validateAttrs),
                FormColumnType.Checkbox => BuildCheckboxField(f, col, onChange, validateAttrs),
                FormColumnType.Radio => BuildRadioField(f, col, onChange, validateAttrs),
                FormColumnType.Date => $"<input type=\"date\"{vModel}{roAttr}{onChange} class=\"{baseInput}\"{classBinding}{validateAttrs}/>",
                FormColumnType.DateTimeLocal => $"<input type=\"datetime-local\"{vModel}{roAttr}{onChange} class=\"{baseInput}\"{classBinding}{validateAttrs}/>",
                FormColumnType.Number => BuildNumberField(f, col, baseInput, onChange, roAttr, classBinding, validateAttrs),
                FormColumnType.Lov => BuildLovField(formId, col),
                FormColumnType.Hidden => $"<input type=\"hidden\"{vModel}/>",
                _ => $"<input type=\"text\"{vModel}{roAttr}{onChange}"
                     + (col.MaxLength.HasValue ? $" maxlength=\"{col.MaxLength}\"" : "")
                     + (string.IsNullOrEmpty(col.Placeholder) ? "" : $" placeholder=\"{HtmlEnc(col.Placeholder)}\"")
                     + $" class=\"{baseInput}\"{classBinding}{validateAttrs}/>"
            };
        }

        private static string BuildReadonlyField(string f)
            => $"<div class=\"min-h-9 px-3 py-2 rounded-lg border border-slate-200 bg-slate-100 text-sm text-slate-600 break-all\""
             + $" x-text=\"formData['{f}'] ?? ''\"></div>";

        private static string BuildTextareaField(string f, FormColumn col, string baseInput, string onChange, string roAttr, string classBinding, string validateAttrs)
            => $"<textarea x-model=\"formData['{f}']\"{roAttr}{onChange}"
             + (col.MaxLength.HasValue ? $" maxlength=\"{col.MaxLength}\"" : "")
             + $" rows=\"3\" class=\"{baseInput} resize-y\"{classBinding}{validateAttrs}></textarea>";

        private static string BuildSelectField(string f, FormColumn col, string baseInput, string onChange, string disAttr, string classBinding, string validateAttrs)
        {
            if (!string.IsNullOrWhiteSpace(col.OptionsApi))
            {
                return $"<select x-model=\"formData['{f}']\"{disAttr}{onChange} class=\"{baseInput}\"{classBinding}{validateAttrs}"
                     + $" x-init=\"loadSelectOptions('{JsEsc(col.OptionsApi)}','{f}')\">"
                     + "<option value=\"\">-- 請選擇 --</option>"
                     + $"<template x-for=\"opt in (selectOptions['{f}'] || [])\" :key=\"opt.value\">"
                     + "<option :value=\"opt.value\" x-text=\"opt.label\"></option>"
                     + "</template></select>";
            }

            var sb = new StringBuilder();
            sb.Append($"<select x-model=\"formData['{f}']\"{disAttr}{onChange} class=\"{baseInput}\"{classBinding}{validateAttrs}>");
            sb.Append("<option value=\"\">-- 請選擇 --</option>");

            if (!string.IsNullOrWhiteSpace(col.Options))
            {
                foreach (var opt in col.Options.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = opt.Split(':', 2);
                    var val = HtmlEnc(parts[0].Trim());
                    var lbl = parts.Length > 1 ? HtmlEnc(parts[1].Trim()) : val;
                    sb.Append($"<option value=\"{val}\">{lbl}</option>");
                }
            }

            sb.Append("</select>");
            return sb.ToString();
        }

        private static string BuildCheckboxField(string f, FormColumn col, string onChange, string validateAttrs)
            => "<div class=\"flex items-center min-h-9\">"
             + $"<input type=\"checkbox\" x-model=\"formData['{f}']\"{onChange}"
             + $" class=\"w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-400\"{validateAttrs}/>"
             + "</div>";

        private static string BuildRadioField(string f, FormColumn col, string onChange, string validateAttrs)
        {
            var sb = new StringBuilder("<div class=\"flex items-center gap-4 min-h-9\">");
            if (!string.IsNullOrWhiteSpace(col.Options))
            {
                foreach (var opt in col.Options.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = opt.Split(':', 2);
                    var val = HtmlEnc(parts[0].Trim());
                    var lbl = parts.Length > 1 ? HtmlEnc(parts[1].Trim()) : val;
                    sb.Append("<label class=\"flex items-center gap-1 text-sm text-slate-700 cursor-pointer\">");
                    sb.Append($"<input type=\"radio\" x-model=\"formData['{f}']\" value=\"{val}\"{onChange}");
                    sb.Append($" class=\"w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-400\"{validateAttrs}/>{lbl}</label>");
                }
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string BuildNumberField(string f, FormColumn col, string baseInput, string onChange, string roAttr, string classBinding, string validateAttrs)
        {
            var minAttr = col.Min.HasValue ? $" min=\"{col.Min}\"" : "";
            var maxAttr = col.Max.HasValue ? $" max=\"{col.Max}\"" : "";
            return $"<input type=\"number\" x-model=\"formData['{f}']\"{roAttr}{onChange}{minAttr}{maxAttr}"
                 + $" class=\"{baseInput}\"{classBinding}{validateAttrs}/>";
        }

        private string BuildLovField(string formId, FormColumn col)
        {
            var f = JsEsc(col.FieldName);
            var displayF = JsEsc(string.IsNullOrWhiteSpace(col.LovKeyDisplay) ? col.FieldName : col.LovKeyDisplay);
            var title = JsEsc(string.IsNullOrWhiteSpace(col.LovTitle) ? col.Caption : col.LovTitle);
            var cols = JsEsc(col.LovColumns);
            var fields = JsEsc(col.LovFields);
            var api = JsEsc(col.LovApi);
            var keyVal = JsEsc(col.LovKeyValue);
            var keyDisp = JsEsc(col.LovKeyDisplay);
            var effectiveFormat = GetEffectiveLovDisplayFormat(col.LovKeyValue, col.LovKeyDisplay, col.LovDisplayFormat, col.FieldName);
            var fmt = JsEsc(effectiveFormat);
            var cb = string.IsNullOrWhiteSpace(col.LovOnConfirm) ? "null" : col.LovOnConfirm;
            var inputId = $"{formId}_lov_{col.FieldName}";
            var validateAttrs = BuildValidateAttrs(col);
            var displayMap = $"'FORMATTED_DISPLAY':'{HtmlAttr(inputId)}'";

            var openJs = $"gLov.open({{title:'{title}',api:'{api}',columns:['{cols}'.split(',')].flat(),"
                       + $"fields:['{fields}'.split(',')].flat(),"
                       + $"map:{{'{keyVal}':'{HtmlAttr(inputId)}_hidden',{displayMap}}},"
                       + $"formatDisplay:{(string.IsNullOrWhiteSpace(fmt) ? "null" : $"function(d){{return {BuildLovDisplayExpression(fmt, "d")};}}")}"
                       + $",onConfirm:{cb}}})";

            return "<div class=\"flex items-center\">"
                 + $"<input type=\"hidden\" id=\"{inputId}_hidden\" x-model=\"formData['{f}']\"{validateAttrs}/>"
                 + $"<input type=\"text\" id=\"{inputId}\" x-model=\"formData['{displayF}']\" readonly"
                 + $" placeholder=\"{HtmlEnc(col.Placeholder.Coalesce("請選擇資料"))}\""
                 + " class=\"block min-w-0 flex-1 px-3 py-2.5 border border-slate-300 rounded-l-xl border-r-0 text-sm text-slate-700 bg-white placeholder:text-slate-400 cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors\"/>"
                 + $"<button type=\"button\" onclick=\"{HtmlAttr(openJs)}\""
                 + " class=\"shrink-0 inline-flex items-center justify-center px-3 border border-slate-300 rounded-r-xl bg-white hover:bg-slate-50 text-slate-600 hover:text-blue-600 transition-colors\">"
                 + "<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z\"/></svg>"
                 + "</button>"
                 + "</div>";
        }

        private static string GetEffectiveLovDisplayFormat(string? keyValue, string? keyDisplay, string? displayFormat, string fallbackField)
        {
            if (!string.IsNullOrWhiteSpace(displayFormat))
            {
                return displayFormat;
            }

            var valueField = string.IsNullOrWhiteSpace(keyValue) ? fallbackField : keyValue.Trim();
            var displayField = string.IsNullOrWhiteSpace(keyDisplay) ? fallbackField : keyDisplay.Trim();

            if (!string.IsNullOrWhiteSpace(valueField) &&
                !string.IsNullOrWhiteSpace(displayField) &&
                !string.Equals(valueField, displayField, StringComparison.OrdinalIgnoreCase))
            {
                return $"{{{valueField}}}-{{{displayField}}}";
            }

            return !string.IsNullOrWhiteSpace(displayField)
                ? $"{{{displayField}}}"
                : (!string.IsNullOrWhiteSpace(valueField) ? $"{{{valueField}}}" : string.Empty);
        }

        private static string BuildValidateAttrs(FormColumn col)
        {
            var required = col.Required ? "1" : "0";
            return $" data-validate=\"1\" data-field-name=\"{HtmlEnc(col.FieldName)}\" data-caption=\"{HtmlEnc(col.Caption)}\""
                 + $" data-required=\"{required}\" data-validate-fn=\"{HtmlEnc(col.ValidateFn)}\" data-validate-msg=\"{HtmlEnc(col.ValidateMessage)}\""
                 + (col.Min.HasValue ? $" data-min=\"{col.Min.Value}\"" : "")
                 + (col.Max.HasValue ? $" data-max=\"{col.Max.Value}\"" : "")
                 + (string.IsNullOrWhiteSpace(col.CompareField) ? "" : $" data-compare-field=\"{HtmlEnc(col.CompareField)}\"")
                 + (string.IsNullOrWhiteSpace(col.CompareMode) ? "" : $" data-compare-mode=\"{HtmlEnc(col.CompareMode)}\"")
                 + (col.IsPrimaryKey ? " data-is-pk=\"1\"" : "")
                 + (col.DefaultValue == null ? "" : $" data-field-default=\"{HtmlEnc(col.DefaultValue)}\"");
        }

        private static string BuildColumnMetaJson(List<FormColumn> columns)
        {
            var meta = columns.Select(c => new
            {
                fieldName = c.FieldName,
                columnType = c.ColumnType.ToString(),
                valueType = string.IsNullOrWhiteSpace(c.ValueType) ? InferValueType(c) : c.ValueType
            });

            return JsonSerializer.Serialize(meta);
        }

        private static string InferValueType(FormColumn col)
        {
            return col.ColumnType switch
            {
                FormColumnType.Number => "decimal",
                _ => string.Empty
            };
        }

        private void EnsureRuntimeInjected(TagHelperOutput output)
        {
            var ctx = ViewContext?.HttpContext;
            if (ctx == null) return;
            if (ctx.Items.ContainsKey(RuntimeInjectedKey)) return;
            ctx.Items[RuntimeInjectedKey] = true;
            output.PostElement.AppendHtml("<script src=\"/js/g-dataform.js\"></script>");
        }

        private static string BuildLovDisplayExpression(string format, string dataVar)
        {
            if (string.IsNullOrWhiteSpace(format)) return "''";
            var parts = new List<string>();
            var matches = System.Text.RegularExpressions.Regex.Matches(format, "\\{([^}]+)\\}");
            var lastIndex = 0;

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    var literal = format.Substring(lastIndex, match.Index - lastIndex);
                    parts.Add(JsonSerializer.Serialize(literal));
                }

                var key = JsEsc(match.Groups[1].Value.Trim());
                parts.Add($"(({dataVar} && {dataVar}['{key}'] != null) ? String({dataVar}['{key}']) : '')");
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < format.Length)
            {
                parts.Add(JsonSerializer.Serialize(format[lastIndex..]));
            }

            return parts.Count == 0 ? "''" : string.Join(" + ", parts);
        }

        private static string JsEsc(string s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
        private static string HtmlEnc(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
        private static string HtmlAttr(string s) => (s ?? "").Replace("\"", "&quot;");
    }

}

