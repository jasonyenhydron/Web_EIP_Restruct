using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-panel")]
    public class GPanelTagHelper : TagHelper
    {
        public string Title        { get; set; } = "";
        public string Icon         { get; set; } = "";
        public bool   Collapsible  { get; set; } = false;
        public bool   Collapsed    { get; set; } = false;
        public string Class        { get; set; } = "";
        public string ExtraClass   { get; set; } = "";
        public bool   AllowOverflow { get; set; } = false;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content  = (await output.GetChildContentAsync()).GetContent();
            var panelId  = $"gp_{Guid.NewGuid():N}";
            var iconSvg  = GIconSet.Render(Icon, "w-4 h-4 shrink-0");
            var colBtn   = Collapsible
                ? $@"<button type=""button"" onclick=""gPanelToggle('{panelId}')"" title=""展開/收合""
                         class=""ml-auto text-slate-500 p-1 rounded"">
                         <svg id=""{panelId}-arrow"" class=""w-4 h-4 transition-transform duration-200{(Collapsed ? " rotate-180" : "")}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                             <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 15l7-7 7 7""/>
                         </svg>
                     </button>"
                : "";

            var hiddenClass  = Collapsed ? " hidden" : "";
            var overflowCls  = AllowOverflow ? "overflow-visible" : "overflow-hidden";

            output.TagName = "div";
            var defaultClass = $"bg-white rounded-xl border border-slate-200 shadow-sm {overflowCls}";
            var finalClass = TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass);
            output.Attributes.SetAttribute("class", finalClass);
            output.Content.SetHtmlContent($@"
                <div class=""flex items-center gap-2 px-4 py-3 bg-slate-100 border-b border-slate-200"">
                    {iconSvg}
                    <span class=""text-sm font-bold text-slate-700 flex-1"">{Title}</span>
                    {colBtn}
                </div>
                <div id=""{panelId}"" class=""p-4{hiddenClass}"">
                    {content}
                </div>
            ");
        }

    }
}



