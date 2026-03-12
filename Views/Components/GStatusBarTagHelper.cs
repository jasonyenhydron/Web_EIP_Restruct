using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-status-bar")]
    public class GStatusBarTagHelper : TagHelper
    {
        public string MsgId { get; set; } = "statusBarMsg";

        public string RecordInfo { get; set; } = string.Empty;

        public string DefaultMsg { get; set; } = "Ready.";

        public string AlpineRecordInfo { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                "bg-slate-900/60 rounded-lg p-3 text-white shadow-inner " +
                "flex items-center justify-between font-mono text-sm");

            string infoHtml;
            if (!string.IsNullOrEmpty(AlpineRecordInfo))
            {
                infoHtml = $"<div class=\"text-xs text-slate-500 ml-4 shrink-0\" x-text=\"{HtmlAttr(AlpineRecordInfo)}\"></div>";
            }
            else if (!string.IsNullOrEmpty(RecordInfo))
            {
                infoHtml = $"<div class=\"text-xs text-slate-500 ml-4 shrink-0\">記錄: {HtmlEncode(RecordInfo)}</div>";
            }
            else
            {
                infoHtml = "<div class=\"text-xs text-slate-400 ml-4 shrink-0\">記錄: -</div>";
            }

            string html = $@"
<div class=""flex items-center gap-3 w-full"">
    <span class=""text-emerald-400 font-bold shrink-0"">狀態 &gt;</span>
    <input type=""text""
           id=""{HtmlEncode(MsgId)}""
           class=""bg-slate-900/60 border border-slate-700 rounded px-3 py-1 flex-1
                  text-slate-300 focus:outline-none focus:border-indigo-500 shadow-inner""
           value=""{HtmlEncode(DefaultMsg)}"" readonly>
</div>
{infoHtml}";

            output.Content.SetHtmlContent(html);
        }

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string? s)   => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}







