using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    public class GAccordionContext
    {
        public List<(string Title, string Icon, bool Active, string Content)> Panels { get; } = new();
    }

    [HtmlTargetElement("g-accordion-panel", ParentTag = "g-accordion")]
    public class GAccordionPanelTagHelper : TagHelper
    {
        public string Title  { get; set; } = "";
        public string Icon   { get; set; } = "";
        public bool   Active { get; set; } = false;

        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput output)
        {
            var acc = ctx.Items[typeof(GAccordionContext)] as GAccordionContext;
            if (acc != null)
                acc.Panels.Add((Title, Icon, Active, (await output.GetChildContentAsync()).GetContent()));
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement("g-accordion")]
    [RestrictChildren("g-accordion-panel")]
    public class GAccordionTagHelper : TagHelper
    {
        public bool   Exclusive { get; set; } = true;
        public string Class     { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var acc = new GAccordionContext();
            context.Items[typeof(GAccordionContext)] = acc;
            await output.GetChildContentAsync();

            var accId = $"gacc_{Guid.NewGuid():N}";
            var sb    = new System.Text.StringBuilder();

            for (int i = 0; i < acc.Panels.Count; i++)
            {
                var (title, icon, active, content) = acc.Panels[i];
                var panelId = $"{accId}_p{i}";
                var isOpen  = active || (i == 0 && !acc.Panels.Any(p => p.Active));
                var iconHtml = GIconSet.Render(icon, "w-4 h-4 shrink-0");

                sb.Append($@"
                <div class=""border border-slate-200 rounded-xl overflow-hidden {(i > 0 ? "mt-1" : "")}"">
                    <button type=""button""
                            onclick=""gAccordionToggle('{accId}','{panelId}',{(Exclusive ? "true" : "false")})""
                            class=""w-full flex items-center gap-2 px-4 py-3 bg-slate-100 transition-colors text-left"">
                        {iconHtml}
                        <span class=""text-sm font-bold text-slate-700 flex-1"">{title}</span>
                        <svg id=""{panelId}-arrow"" class=""w-4 h-4 text-slate-400 transition-transform duration-200{(isOpen ? "" : " rotate-180")}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 15l7-7 7 7""/>
                        </svg>
                    </button>
                    <div id=""{panelId}"" class=""overflow-hidden transition-all duration-300{(isOpen ? "" : " hidden")}"">
                        <div class=""p-4 border-t border-slate-100"">{content}</div>
                    </div>
                </div>");
            }

            sb.Append($@"
            <script>
            function gAccordionToggle(accId, panelId, exclusive) {{
                const panel = document.getElementById(panelId);
                const arrow = document.getElementById(panelId + '-arrow');
                if (exclusive) {{
                    document.querySelectorAll('[id^=""' + accId + '_p""]').forEach(p => {{
                        if (p.id !== panelId) {{ p.classList.add('hidden'); }}
                        const a = document.getElementById(p.id + '-arrow');
                        if (a && p.id !== panelId) a.classList.add('rotate-180');
                    }});
                }}
                panel.classList.toggle('hidden');
                arrow.classList.toggle('rotate-180');
            }}
            </script>");

            output.TagName = "div";
            output.Attributes.SetAttribute("id", accId);
            output.Attributes.SetAttribute("class", $"space-y-0 {Class}");
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}


