using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-suggest-input")]
    public class GSuggestInputTagHelper : TagHelper
    {
        private const string RuntimeInjectedKey = "__g_suggest_runtime_injected";

        [Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContext]
        [HtmlAttributeNotBound]
        public Microsoft.AspNetCore.Mvc.Rendering.ViewContext? ViewContext { get; set; }

        public string Label { get; set; } = "";
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
        public string Placeholder { get; set; } = "";
        public string Value { get; set; } = "";
        public bool Required { get; set; } = false;
        public bool Readonly { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public int ColSpan { get; set; } = 1;
        [HtmlAttributeName("row-span")]
        public int RowSpan { get; set; } = 1;
        public string WrapperClass { get; set; } = "";
        public string Class { get; set; } = "";
        public string ExtraClass { get; set; } = "";

        [HtmlAttributeName("suggest-api")]
        public string SuggestApi { get; set; } = "";
        [HtmlAttributeName("suggest-query-param")]
        public string SuggestQueryParam { get; set; } = "q";
        [HtmlAttributeName("suggest-value-field")]
        public string SuggestValueField { get; set; } = "";
        [HtmlAttributeName("suggest-primary-field")]
        public string SuggestPrimaryField { get; set; } = "";
        [HtmlAttributeName("suggest-secondary-field")]
        public string SuggestSecondaryField { get; set; } = "";
        [HtmlAttributeName("min-length")]
        public int MinLength { get; set; } = 1;
        [HtmlAttributeName("debounce-ms")]
        public int DebounceMs { get; set; } = 280;
        [HtmlAttributeName("on-select")]
        public string OnSelect { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            EnsureRuntimeInjected(output);

            var inputId = string.IsNullOrWhiteSpace(Id) ? $"gs_{Guid.NewGuid():N}" : Id.Trim();
            var listId = $"{inputId}_suggestions";
            var requiredMark = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var colClass = ColSpan > 1 ? $"md:col-span-{ColSpan}" : "md:col-span-1";
            var rowClass = RowSpan >= 2 ? "md:row-span-2" : "md:row-span-1";
            var defaultWrapperClass = $"flex flex-col gap-1 {colClass} {rowClass}".Trim();
            var finalWrapperClass = string.IsNullOrWhiteSpace(WrapperClass) ? defaultWrapperClass : WrapperClass.Trim();
            var defaultInputClass = "mis-filter-input";
            var finalInputClass = TagHelperClassResolver.Resolve(defaultInputClass, Class, ExtraClass);

            var reqAttr = Required ? " required" : "";
            var readonlyAttr = Readonly ? " readonly" : "";
            var disabledAttr = Disabled ? " disabled" : "";
            var autoCompleteAttr = " autocomplete=\"off\"";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", finalWrapperClass);
            output.Attributes.SetAttribute("data-gsuggest", "1");
            output.Attributes.SetAttribute("data-api", SuggestApi ?? string.Empty);
            output.Attributes.SetAttribute("data-query-param", SuggestQueryParam ?? "q");
            output.Attributes.SetAttribute("data-value-field", SuggestValueField ?? string.Empty);
            output.Attributes.SetAttribute("data-primary-field", SuggestPrimaryField ?? string.Empty);
            output.Attributes.SetAttribute("data-secondary-field", SuggestSecondaryField ?? string.Empty);
            output.Attributes.SetAttribute("data-min-length", Math.Max(0, MinLength).ToString());
            output.Attributes.SetAttribute("data-debounce-ms", Math.Max(0, DebounceMs).ToString());
            if (!string.IsNullOrWhiteSpace(OnSelect))
            {
                output.Attributes.SetAttribute("data-on-select", OnSelect.Trim());
            }

            output.Content.SetHtmlContent($@"
                <label for=""{inputId}"" class=""mis-filter-label"">{Label}{requiredMark}</label>
                <input type=""text"" id=""{inputId}"" name=""{Name}"" value=""{System.Net.WebUtility.HtmlEncode(Value)}"" placeholder=""{System.Net.WebUtility.HtmlEncode(Placeholder)}"" class=""{finalInputClass}""{reqAttr}{readonlyAttr}{disabledAttr}{autoCompleteAttr} />
                <div id=""{listId}"" class=""g-suggest-list hidden"" role=""listbox"" aria-label=""{System.Net.WebUtility.HtmlEncode(Label)} 撱箄降皜""></div>
            ");
        }

        private void EnsureRuntimeInjected(TagHelperOutput output)
        {
            var httpContext = ViewContext?.HttpContext;
            if (httpContext == null) return;
            if (httpContext.Items.ContainsKey(RuntimeInjectedKey)) return;
            httpContext.Items[RuntimeInjectedKey] = true;
            output.PostElement.AppendHtml(@"
<script src=""/js/g-suggest-input.js""></script>");
        }
    }
}

