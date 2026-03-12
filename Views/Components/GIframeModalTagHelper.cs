using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-iframe-modal")]
    public class GIframeModalTagHelper : TagHelper
    {
        [HtmlAttributeName("id")]
        public string Id { get; set; } = "executionModal";

        public string ModalContentId { get; set; } = "executionModalContent";
        public string IframeId { get; set; } = "executionIframe";
        public string TitleId { get; set; } = "executionModalTitle";
        public string MaximizeBtnId { get; set; } = "execMaximizeBtn";
        public string MaximizeIconId { get; set; } = "execMaximizeIcon";
        public string RestoreIconId { get; set; } = "execRestoreIcon";
        public string Title { get; set; } = "程式執行";
        public string Gradient { get; set; } = "indigo";
        public string CloseFn { get; set; } = "closeExecutionModal()";
        public string MaximizeFn { get; set; } = "toggleExecutionMaximize()";
        public string Height { get; set; } = "95vh";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("id", Id);
            output.Attributes.SetAttribute(
                "class",
                "fixed inset-0 bg-slate-900/60 backdrop-blur-sm hidden z-[60] items-center justify-center p-4 transition-all duration-300");

            var gradientClass = Gradient?.ToLower() switch
            {
                "blue" => "from-blue-600 to-blue-700",
                "green" => "from-green-600 to-teal-700",
                "slate" => "from-slate-700 to-slate-800",
                _ => "from-indigo-600 to-blue-700"
            };

            const string titleIconSvg = @"<svg class=""w-5 h-5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4""/></svg>";

            const string maximizeSvg = @"<svg class=""w-5 h-5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4""/></svg>";

            var restoreSvg = $@"<svg id=""{RestoreIconId}"" class=""w-[85%] h-[85%] hidden absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M9 9L4 4m0 0v4m0-4h4m12 12l-5-5m5 5v-4m0 4h-4M9 15l-5 5m0 0v-4m0 4h4m11-11l-5 5m5-5v4m0-4h-4""/></svg>";

            const string closeSvg = @"<svg class=""w-6 h-6"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/></svg>";

            var html = $@"
<div class=""bg-white rounded-2xl shadow-2xl w-full max-w-7xl flex flex-col overflow-hidden border border-slate-200 transform scale-95 transition-transform duration-300""
     id=""{HtmlEncode(ModalContentId)}"" style=""height:{HtmlEncode(Height)}"">

    <div class=""bg-blue-600 {gradientClass} text-white px-4 py-3 flex items-center justify-between shadow-lg shrink-0"">
        <h2 class=""text-lg font-bold flex items-center gap-3"">
            {titleIconSvg}
            <span id=""{HtmlEncode(TitleId)}"">{HtmlEncode(Title)}</span>
        </h2>
        <div class=""flex items-center gap-1"">
            <button type=""button""
                    id=""{HtmlEncode(MaximizeBtnId)}""
                    onclick=""{HtmlAttr(MaximizeFn)}""
                    class=""text-white/70 hover:text-white bg-white w-8 h-8 flex items-center justify-center rounded-lg transition-all relative"">
                <span id=""{HtmlEncode(MaximizeIconId)}"">{maximizeSvg}</span>
                {restoreSvg}
            </button>
            <button type=""button""
                    onclick=""{HtmlAttr(CloseFn)}""
                    class=""text-white/70 hover:text-white bg-white p-1.5 rounded-lg transition-all"">
                {closeSvg}
            </button>
        </div>
    </div>

    <div class=""flex-1 bg-slate-100 relative min-h-0"">
        <iframe id=""{HtmlEncode(IframeId)}"" src=""""
                class=""absolute inset-0 w-full h-full border-0 rounded-b-2xl"" scrolling=""auto"">
        </iframe>
    </div>

</div>";

            output.Content.SetHtmlContent(html);
        }

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string? s) => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}


