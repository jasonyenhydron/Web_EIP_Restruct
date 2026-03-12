using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;

namespace Web_EIP_Restruct.Views.Components
{
    public abstract class GLegacyPlaceholderTagHelperBase : TagHelper
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Api { get; set; } = "";
        public string Columns { get; set; } = "";

        [HtmlAttributeName("query-columns")]
        public string QueryColumns { get; set; } = "";

        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Icon { get; set; } = "cube";
        public string Url { get; set; } = "";

        [HtmlAttributeName("on-click")]
        public string OnClick { get; set; } = "";

        [HtmlAttributeName("x-data")]
        public string XData { get; set; } = "";

        [HtmlAttributeName("x-init")]
        public string XInit { get; set; } = "";

        public bool Disabled { get; set; } = false;
        public string Title { get; set; } = "";
        public string Class { get; set; } = "";

        protected abstract string DefaultTitle { get; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var child = (await output.GetChildContentAsync()).GetContent();
            var title = string.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title;
            var cls = string.IsNullOrWhiteSpace(Class)
                ? "rounded-xl border border-dashed border-amber-300 bg-slate-100/50 p-3 text-xs text-amber-700"
                : Class;
            var compId = string.IsNullOrWhiteSpace(Id) ? $"legacy_{Guid.NewGuid():N}" : Id;
            var xData = string.IsNullOrWhiteSpace(XData) ? "{ open: true }" : XData;
            var disabledClass = Disabled ? " opacity-60 pointer-events-none" : "";

            var textHtml = string.IsNullOrWhiteSpace(Text)
                ? ""
                : $"<div><span class=\"font-semibold\">text:</span> {WebUtility.HtmlEncode(Text)}</div>";
            var valueHtml = string.IsNullOrWhiteSpace(Value)
                ? ""
                : $"<div><span class=\"font-semibold\">value:</span> {WebUtility.HtmlEncode(Value)}</div>";
            var apiHtml = string.IsNullOrWhiteSpace(Api)
                ? ""
                : $"<div><span class=\"font-semibold\">api:</span> <code>{WebUtility.HtmlEncode(Api)}</code></div>";
            var columnsHtml = string.IsNullOrWhiteSpace(Columns)
                ? ""
                : $"<div><span class=\"font-semibold\">columns:</span> <code>{WebUtility.HtmlEncode(Columns)}</code></div>";
            var queryColumnsHtml = string.IsNullOrWhiteSpace(QueryColumns)
                ? ""
                : $"<div><span class=\"font-semibold\">query-columns:</span> <code>{WebUtility.HtmlEncode(QueryColumns)}</code></div>";

            var actionHtml = (!string.IsNullOrWhiteSpace(Url) || !string.IsNullOrWhiteSpace(OnClick))
                ? $@"<div class=""mt-2 flex items-center gap-2"">
                    <button type=""button"" {(string.IsNullOrWhiteSpace(OnClick) ? "" : $@"onclick=""{WebUtility.HtmlEncode(OnClick)}""")}
                            class=""px-2 py-1 rounded border border-amber-300 bg-white transition-colors"">
                        {(string.IsNullOrWhiteSpace(Text) ? "執行" : WebUtility.HtmlEncode(Text))}
                    </button>
                    {(string.IsNullOrWhiteSpace(Url)
                        ? ""
                        : $@"<a href=""{WebUtility.HtmlEncode(Url)}"" class=""underline text-blue-700 hover:text-blue-900"" target=""_blank"" rel=""noreferrer"">{WebUtility.HtmlEncode(Url)}</a>")}
                </div>"
                : "";

            output.TagName = "section";
            output.Attributes.SetAttribute("id", compId);
            output.Attributes.SetAttribute("data-legacy", context.TagName);
            output.Attributes.SetAttribute("x-data", xData);
            if (!string.IsNullOrWhiteSpace(XInit))
            {
                output.Attributes.SetAttribute("x-init", XInit);
            }
            output.Attributes.SetAttribute("class", $"{cls}{disabledClass}");

            output.Content.SetHtmlContent($@"
<div class=""flex items-center justify-between gap-2"">
    <div class=""font-semibold flex items-center gap-1""><span>{WebUtility.HtmlEncode(Icon)}</span><span>{WebUtility.HtmlEncode(title)} (legacy placeholder)</span></div>
    <button type=""button"" @click=""open=!open"" class=""px-2 py-0.5 rounded border border-amber-300 bg-white transition-colors""><span x-text=""open ? '收合' : '展開'""></span></button>
</div>
<div x-show=""open"" class=""mt-2 space-y-1"">
    {textHtml}
    {valueHtml}
    {apiHtml}
    {columnsHtml}
    {queryColumnsHtml}
    {actionHtml}
    {child}
    {(string.IsNullOrWhiteSpace(Name) ? "" : $@"<input type=""hidden"" name=""{WebUtility.HtmlEncode(Name)}"" value=""{WebUtility.HtmlEncode(Value)}"">")}
</div>");
        }
    }
}

