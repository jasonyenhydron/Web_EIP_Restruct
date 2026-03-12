using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-page-title")]
    public class GPageTitleTagHelper : TagHelper
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Icon { get; set; } = "document";
        public string Class { get; set; } = "mb-6";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", Class);

            string iconSvg = GIconSet.RenderForContext(Icon, "w-7 h-7", "page-title");

            string subtitleHtml = !string.IsNullOrEmpty(Subtitle)
                ? $"<p class=\"text-slate-600 mt-1\">{HtmlEncode(Subtitle)}</p>"
                : string.Empty;

            output.Content.SetHtmlContent($@"
<h1 class=""text-2xl font-bold text-slate-800 flex items-center gap-2"">
    {iconSvg}
    {HtmlEncode(Title)}
</h1>
{subtitleHtml}");
        }
        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}

