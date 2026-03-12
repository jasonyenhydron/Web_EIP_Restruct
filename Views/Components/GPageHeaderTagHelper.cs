using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-page-header")]
    public class GPageHeaderTagHelper : TagHelper
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = "企業資源管理系統 ERP / 管理平台";
        public string Icon { get; set; } = "home";
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                "bg-white rounded-2xl shadow-sm border border-slate-200/60 p-4 " +
                "flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4");
            string iconSvg = GIconSet.Render(Icon, "w-6 h-6");

            string leftHtml = $@"
<div class=""flex items-center gap-3"">
    <div class=""bg-blue-600 from-blue-500 to-indigo-600 p-2.5 rounded-xl shadow-lg shadow-blue-500/30 text-white"">
        {iconSvg}
    </div>
    <div>
        <h1 class=""text-xl font-extrabold text-slate-800 tracking-tight"">{HtmlEncode(Title)}</h1>
        <p class=""text-xs text-slate-500 mt-0.5"">{HtmlEncode(Subtitle)}</p>
    </div>
</div>";
            string userIcon = GIconSet.RenderForContext("user", "w-3.5 h-3.5", "page-meta");

            string calIcon = GIconSet.RenderForContext("calendar", "w-3.5 h-3.5", "page-meta");

            string rightHtml = $@"
<div class=""flex flex-wrap items-center gap-3 text-xs font-medium text-slate-500 bg-slate-100 px-4 py-2 rounded-lg border border-slate-200"">
    <span class=""flex items-center gap-1"">
        {userIcon}
        {HtmlEncode(UserId)} {HtmlEncode(UserName)}
    </span>
    <span class=""text-slate-300"">|</span>
    <span class=""flex items-center gap-1"">
        {calIcon}
        {HtmlEncode(Date)}
    </span>
</div>";

            output.Content.SetHtmlContent(leftHtml + rightHtml);
        }
        private static string HtmlEncode(string? s) =>
            System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}


