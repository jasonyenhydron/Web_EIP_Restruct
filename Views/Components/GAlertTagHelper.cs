using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-alert")]
    public class GAlertTagHelper : TagHelper
    {
        public string Type { get; set; } = "error";

        public string Message { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

        public bool Dismissible { get; set; } = false;

        public string Class { get; set; } = "mb-4";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                output.SuppressOutput();
                return;
            }

            output.TagName = "div";

            (string baseCss, string iconSvg, string defaultPrefix) = Type?.ToLower() switch
            {
                "success" => (
                    "bg-green-50 border border-green-200 text-green-800",
                    @"<svg class=""w-5 h-5 text-green-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z""/></svg>",
                    "成功:"
                ),
                "warning" => (
                    "bg-slate-100 border border-amber-200 text-amber-800",
                    @"<svg class=""w-5 h-5 text-amber-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z""/></svg>",
                    "警告:"
                ),
                "info" => (
                    "bg-blue-50 border border-blue-200 text-blue-800",
                    @"<svg class=""w-5 h-5 text-blue-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""/></svg>",
                    "資訊:"
                ),
                _ => (
                    "bg-slate-100 border border-red-200 text-red-700",
                    @"<svg class=""w-5 h-5 text-red-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z""/></svg>",
                    "錯誤:"
                )
            };

            output.Attributes.SetAttribute("class", $"{baseCss} px-4 py-3 rounded-lg flex items-start gap-3 {Class}".Trim());

            string displayPrefix = !string.IsNullOrEmpty(Prefix)
                ? Prefix
                : defaultPrefix;
            string prefixHtml = $"<strong>{HtmlEncode(displayPrefix)}</strong>";

            string closeBtn = Dismissible
                ? @"<button type=""button""
                           onclick=""this.closest('[class*=rounded-lg]').remove()""
                           class=""ml-auto shrink-0 text-current opacity-60 hover:opacity-100 transition-opacity"">
                        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>
                        </svg>
                    </button>"
                : string.Empty;

            output.Content.SetHtmlContent($@"
{iconSvg}
<span class=""flex-1"">{prefixHtml} {HtmlEncode(Message)}</span>
{closeBtn}");
        }

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}

