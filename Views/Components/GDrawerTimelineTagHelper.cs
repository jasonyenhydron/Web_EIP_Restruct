using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-drawer")]
    public class GDrawerTagHelper : TagHelper
    {
        [HtmlAttributeName("id")]
        public string Id       { get; set; } = "";
        public string Title    { get; set; } = "";
        public string Position { get; set; } = "right";  // right | left | top | bottom
        public string Width    { get; set; } = "380px";
        public string Height   { get; set; } = "300px";  // for top/bottom drawer
        public bool   Overlay  { get; set; } = true;
        public string Class    { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content  = (await output.GetChildContentAsync()).GetContent();
            var drawerId = string.IsNullOrEmpty(Id) ? $"gdrw_{Guid.NewGuid():N}" : Id;
            var overlayId= $"{drawerId}_ov";

            var (posClass, sizeStyle, translateIn, translateOut) = Position?.ToLower() switch
            {
                "left"   => ("top-0 left-0 h-full", $"width:{Width};max-width:calc(100vw - 16px)", "translate-x-0", "-translate-x-full"),
                "top"    => ("top-0 left-0 w-full", $"height:{Height}", "translate-y-0", "-translate-y-full"),
                "bottom" => ("bottom-0 left-0 w-full", $"height:{Height}", "translate-y-0", "translate-y-full"),
                _        => ("top-0 right-0 h-full", $"width:{Width};max-width:calc(100vw - 16px)", "translate-x-0", "translate-x-full")
            };

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "");
            output.Content.SetHtmlContent($@"
                {(Overlay ? $@"<div id=""{overlayId}"" onclick=""gDrawerClose('{drawerId}')""
                     class=""fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[55] hidden transition-opacity duration-300""></div>" : "")}
                <div id=""{drawerId}"" style=""{sizeStyle}""
                     class=""fixed {posClass} bg-white shadow-2xl z-[60] transform {translateOut} transition-transform duration-300 flex flex-col {Class}"">
                    <!-- Header -->
                    <div class=""flex items-center justify-between px-4 py-3 border-b border-slate-200 bg-slate-50 shrink-0"">
                        <h3 class=""text-sm font-bold text-slate-700"">{Title}</h3>
                        <button type=""button"" onclick=""gDrawerClose('{drawerId}')""
                                class=""text-slate-400 hover:text-slate-600 bg-slate-100 p-1.5 rounded-lg transition-colors"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>
                            </svg>
                        </button>
                    </div>
                    <!-- Content -->
                    <div class=""flex-1 overflow-auto p-4"">
                        {content}
                    </div>
                </div>
                <script>
                (function() {{
                    const drawerId = '{drawerId}';
                    const overlayId = '{overlayId}';
                    const pos = '{Position?.ToLower() ?? "right"}';
                    const translateOut = pos === 'left' ? '-translate-x-full' : pos === 'top' ? '-translate-y-full' : pos === 'bottom' ? 'translate-y-full' : 'translate-x-full';
                    if (!window.gDrawerOpen) {{
                        window.gDrawerOpen = function(id) {{
                            const d = document.getElementById(id);
                            const o = document.getElementById(id + '_ov');
                            if (d) {{ d.classList.remove('-translate-x-full','translate-x-full','-translate-y-full','translate-y-full'); }}
                            if (o) {{ o.classList.remove('hidden'); }}
                        }};
                        window.gDrawerClose = function(id) {{
                            const d = document.getElementById(id);
                            const o = document.getElementById(id + '_ov');
                            const p = d ? d.dataset.pos : 'right';
                            if (d) {{
                                if (p === 'left') d.classList.add('-translate-x-full');
                                else if (p === 'top') d.classList.add('-translate-y-full');
                                else if (p === 'bottom') d.classList.add('translate-y-full');
                                else d.classList.add('translate-x-full');
                            }}
                            if (o) {{ setTimeout(() => o.classList.add('hidden'), 300); }}
                        }};
                    }}
                    const d = document.getElementById(drawerId);
                    if (d) d.dataset.pos = pos;
                }})();
                </script>
            ");
        }
    }

    public class GTimelineContext
    {
        public List<(string Date, string Title, string Content, string Icon, string Type)> Items { get; } = new();
    }

    [HtmlTargetElement("g-timeline-item", ParentTag = "g-timeline")]
    public class GTimelineItemTagHelper : TagHelper
    {
        public string Date  { get; set; } = "";
        public string Title { get; set; } = "";
        public string Icon  { get; set; } = "check";
        public string Type  { get; set; } = "primary";  // primary|success|warning|danger|slate

        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput output)
        {
            var tl = ctx.Items[typeof(GTimelineContext)] as GTimelineContext;
            if (tl != null)
                tl.Items.Add((Date, Title, (await output.GetChildContentAsync()).GetContent(), Icon, Type));
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement("g-timeline")]
    [RestrictChildren("g-timeline-item")]
    public class GTimelineTagHelper : TagHelper
    {
        public string Class { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tl = new GTimelineContext();
            context.Items[typeof(GTimelineContext)] = tl;
            await output.GetChildContentAsync();

            var sb = new System.Text.StringBuilder();
            sb.Append(@"<div class=""relative pl-6"">");
            sb.Append(@"<div class=""absolute left-3 top-0 bottom-0 w-0.5 bg-slate-100""></div>");

            foreach (var (date, title, content, icon, type) in tl.Items)
            {
                var dotColor = type switch
                {
                    "success" => "bg-green-500 border-green-200",  // 成功：綠色
                    "warning" => "bg-amber-500 border-amber-200",  // 警告：橙色
                    "danger"  => "bg-red-500 border-red-200",      // 危險：紅色
                    "slate"   => "bg-slate-300 border-slate-200",  // 中性：灰色
                    _         => "bg-blue-600 border-blue-200"     // 預設：跟隨 --dx-brand
                };
                sb.Append($@"
                <div class=""relative mb-6 last:mb-0"">
                    <div class=""absolute -left-3.5 top-1 w-5 h-5 rounded-full border-2 {dotColor} flex items-center justify-center"">
                        <svg class=""w-2.5 h-2.5 text-white"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""3"" d=""M5 13l4 4L19 7""/>
                        </svg>
                    </div>
                    <div class=""ml-4 bg-white rounded-xl border border-slate-200 shadow-sm p-3"">
                        <div class=""flex items-center justify-between mb-1"">
                            <span class=""text-sm font-semibold text-slate-700"">{title}</span>
                            <span class=""text-xs text-slate-400"">{date}</span>
                        </div>
                        {(string.IsNullOrEmpty(content) ? "" : $@"<div class=""text-sm text-slate-500"">{content}</div>")}
                    </div>
                </div>");
            }
            sb.Append("</div>");

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"g-timeline {Class}");
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}

