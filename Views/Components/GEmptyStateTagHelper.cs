using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-empty-state")]
    public class GEmptyStateTagHelper : TagHelper
    {
        public string Title { get; set; } = "查無資料";
        public string Subtitle { get; set; } = "請調整查詢條件後再試一次";
        public string Icon { get; set; } = "document";
        [HtmlAttributeName("x-show")]
        public string? XShow { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "text-center py-12 text-slate-500");

            if (!string.IsNullOrEmpty(XShow))
                output.Attributes.SetAttribute("x-show", XShow);

            string iconSvg = GIconSet.RenderForContext(Icon, "w-12 h-12 mx-auto mb-3", "empty-state");

            string subtitleHtml = !string.IsNullOrEmpty(Subtitle)
                ? $"<p class=\"text-sm mt-1\">{HtmlEncode(Subtitle)}</p>"
                : string.Empty;

            output.Content.SetHtmlContent($@"
{iconSvg}
<p class=""text-lg font-medium"">{HtmlEncode(Title)}</p>
{subtitleHtml}");
        }
        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}

