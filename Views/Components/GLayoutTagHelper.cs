using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Web_EIP_Restruct.Views.Components
{
    public class GLayoutContext
    {
        public string? NorthHtml  { get; set; }
        public string? NorthHeight{ get; set; }
        public string? SouthHtml  { get; set; }
        public string? SouthHeight{ get; set; }
        public string? WestHtml   { get; set; }
        public string? WestWidth  { get; set; }
        public bool    WestCollapsible { get; set; }
        public string? EastHtml   { get; set; }
        public string? EastWidth  { get; set; }
        public bool    EastCollapsible { get; set; }
        public string? CenterHtml { get; set; }
    }
    [HtmlTargetElement("g-north", ParentTag = "g-layout")]
    public class GNorthTagHelper : TagHelper
    {
        public string Height { get; set; } = "auto";
        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput out2)
        {
            var lc = ctx.Items[typeof(GLayoutContext)] as GLayoutContext;
            if (lc != null) { lc.NorthHtml = (await out2.GetChildContentAsync()).GetContent(); lc.NorthHeight = Height; }
            out2.SuppressOutput();
        }
    }
    [HtmlTargetElement("g-south", ParentTag = "g-layout")]
    public class GSouthTagHelper : TagHelper
    {
        public string Height { get; set; } = "auto";
        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput out2)
        {
            var lc = ctx.Items[typeof(GLayoutContext)] as GLayoutContext;
            if (lc != null) { lc.SouthHtml = (await out2.GetChildContentAsync()).GetContent(); lc.SouthHeight = Height; }
            out2.SuppressOutput();
        }
    }
    [HtmlTargetElement("g-west", ParentTag = "g-layout")]
    public class GWestTagHelper : TagHelper
    {
        public string Width       { get; set; } = "220px";
        public bool   Collapsible { get; set; } = false;
        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput out2)
        {
            var lc = ctx.Items[typeof(GLayoutContext)] as GLayoutContext;
            if (lc != null)
            {
                lc.WestHtml = (await out2.GetChildContentAsync()).GetContent();
                lc.WestWidth = Width;
                lc.WestCollapsible = Collapsible;
            }
            out2.SuppressOutput();
        }
    }
    [HtmlTargetElement("g-east", ParentTag = "g-layout")]
    public class GEastTagHelper : TagHelper
    {
        public string Width       { get; set; } = "220px";
        public bool   Collapsible { get; set; } = false;
        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput out2)
        {
            var lc = ctx.Items[typeof(GLayoutContext)] as GLayoutContext;
            if (lc != null)
            {
                lc.EastHtml = (await out2.GetChildContentAsync()).GetContent();
                lc.EastWidth = Width;
                lc.EastCollapsible = Collapsible;
            }
            out2.SuppressOutput();
        }
    }
    [HtmlTargetElement("g-center", ParentTag = "g-layout")]
    public class GCenterTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput out2)
        {
            var lc = ctx.Items[typeof(GLayoutContext)] as GLayoutContext;
            if (lc != null) lc.CenterHtml = (await out2.GetChildContentAsync()).GetContent();
            out2.SuppressOutput();
        }
    }
    [HtmlTargetElement("g-layout")]
    [RestrictChildren("g-north", "g-south", "g-west", "g-east", "g-center")]
    public class GLayoutTagHelper : TagHelper
    {
        public string Class { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var lc = new GLayoutContext();
            context.Items[typeof(GLayoutContext)] = lc;
            await output.GetChildContentAsync();

            var westId  = $"gwest_{Guid.NewGuid():N}";
            var eastId  = $"geast_{Guid.NewGuid():N}";
            var northHtml = lc.NorthHtml != null
                ? $@"<div class=""g-layout-north bg-white border-b border-slate-200 shrink-0"" style=""height:{lc.NorthHeight}"">{lc.NorthHtml}</div>"
                : "";
            var southHtml = lc.SouthHtml != null
                ? $@"<div class=""g-layout-south bg-white border-t border-slate-200 shrink-0"" style=""height:{lc.SouthHeight}"">{lc.SouthHtml}</div>"
                : "";
            var westToggle = lc.WestCollapsible
                ? $@"<button type=""button"" onclick=""gLayoutToggle('{westId}')""
                         class=""absolute top-1/2 -translate-y-1/2 -right-3 z-10 w-6 h-10 bg-slate-100 hover:bg-blue-600 hover:text-white text-slate-400 rounded-r-lg flex items-center justify-center transition-colors shadow-sm"">
                         <svg id=""{westId}-icon"" class=""w-3 h-3 transition-transform"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                             <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 19l-7-7 7-7""/>
                         </svg>
                     </button>"
                : "";
            var westHtml = lc.WestHtml != null
                ? $@"<div id=""{westId}"" class=""g-layout-west bg-white border-r border-slate-200 overflow-auto relative transition-all duration-200 shrink-0 hidden md:block"" style=""width:{lc.WestWidth}"">
                       {westToggle}
                       {lc.WestHtml}
                   </div>"
                : "";
            var eastToggle = lc.EastCollapsible
                ? $@"<button type=""button"" onclick=""gLayoutToggle('{eastId}')""
                         class=""absolute top-1/2 -translate-y-1/2 -left-3 z-10 w-6 h-10 bg-slate-100 hover:bg-blue-600 hover:text-white text-slate-400 rounded-l-lg flex items-center justify-center transition-colors shadow-sm"">
                         <svg id=""{eastId}-icon"" class=""w-3 h-3 rotate-180 transition-transform"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                             <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 19l-7-7 7-7""/>
                         </svg>
                     </button>"
                : "";
            var eastHtml = lc.EastHtml != null
                ? $@"<div id=""{eastId}"" class=""g-layout-east bg-white border-l border-slate-200 overflow-auto relative transition-all duration-200 shrink-0 hidden md:block"" style=""width:{lc.EastWidth}"">
                       {eastToggle}
                       {lc.EastHtml}
                   </div>"
                : "";
            var centerHtml = $@"<div class=""g-layout-center flex-1 overflow-auto min-w-0"">{lc.CenterHtml}</div>";
            var middleRow  = $@"<div class=""flex flex-1 min-h-0 overflow-hidden"">{westHtml}{centerHtml}{eastHtml}</div>";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col overflow-hidden {Class}");
            output.Content.SetHtmlContent($"{northHtml}{middleRow}{southHtml}");
        }
    }
}

