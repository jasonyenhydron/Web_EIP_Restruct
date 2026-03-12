using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-tree-leaf", ParentTag = "g-tree-node")]
    public class GTreeLeafTagHelper : TagHelper
    {
        public string Label   { get; set; } = "";
        public string Icon    { get; set; } = "file";   // file | circle
        public string Onclick { get; set; } = "";
        public string Class   { get; set; } = "";
        public string Title   { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var onclickAttr = !string.IsNullOrEmpty(Onclick)
                ? $"""onclick="{Onclick}" """
                : "";
            var titleAttr   = !string.IsNullOrEmpty(Title) ? $"""title="{Title}" """ : "";
            var cursor      = !string.IsNullOrEmpty(Onclick) ? "cursor-pointer" : "";
            var iconHtml    = GIconSet.RenderForContext(Icon, "w-3.5 h-3.5 shrink-0", "tree-leaf");

            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                $"flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm text-slate-700 hover:bg-blue-50 hover:text-blue-700 transition-colors {cursor} {Class}");
            if (!string.IsNullOrEmpty(Onclick)) output.Attributes.SetAttribute("onclick", Onclick);
            if (!string.IsNullOrEmpty(Title))   output.Attributes.SetAttribute("title", Title);
            output.Content.SetHtmlContent($"{iconHtml}<span class='truncate'>{Label}</span>");
        }
    }
    [HtmlTargetElement("g-tree-node", ParentTag = "g-tree")]
    public class GTreeNodeTagHelper : TagHelper
    {
        public string Label    { get; set; } = "";
        public string Icon     { get; set; } = "folder"; // folder | setting | code | user
        public bool   Expanded { get; set; } = false;
        public string Badge    { get; set; } = "";
        public string Onclick  { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content  = (await output.GetChildContentAsync()).GetContent();
            var nodeId   = $"gtn_{Guid.NewGuid():N}";
            var initOpen = Expanded;
            var iconHtml = GIconSet.RenderForContext(Icon, "w-4 h-4 shrink-0", "tree-node");
            var badge    = !string.IsNullOrEmpty(Badge)
                ? $"""<span class="ml-auto text-xs bg-slate-100 text-slate-600 px-1.5 py-0.5 rounded-full font-semibold">{Badge}</span>"""
                : "";
            var hdrClick = !string.IsNullOrEmpty(Onclick)
                ? $"""ondblclick="{Onclick}" """
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "tree-node");
            output.Content.SetHtmlContent($"""
                <div class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-semibold text-slate-700 bg-slate-100 cursor-pointer transition-colors select-none group"
                     onclick="gTreeToggle('{nodeId}')" {hdrClick}>
                    <svg id="{nodeId}-arrow"
                         class="w-3.5 h-3.5 text-slate-400 shrink-0 transition-transform duration-150{(initOpen ? " rotate-90" : "")}"
                         fill="currentColor" viewBox="0 0 16 16">
                        <path d="M6 4l4 4-4 4V4z"/>
                    </svg>
                    {iconHtml}
                    <span class="flex-1 truncate">{Label}</span>
                    {badge}
                </div>
                <div id="{nodeId}" class="pl-4 overflow-hidden transition-all duration-200{(initOpen ? "" : " hidden")}">
                    {content}
                </div>
            """);
        }
    }
    [HtmlTargetElement("g-tree")]
    [RestrictChildren("g-tree-node")]
    public class GTreeTagHelper : TagHelper
    {
        public string Id    { get; set; } = "";
        public string Class { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
            var idAttr  = !string.IsNullOrEmpty(Id) ? $"""id="{Id}" """ : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"g-tree overflow-auto {Class}");
            if (!string.IsNullOrEmpty(Id)) output.Attributes.SetAttribute("id", Id);
            output.Content.SetHtmlContent($"""<div class="py-2 space-y-0.5">{content}</div>""");
        }
    }
}

