using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    public class GMenuContext
    {
        public List<string> Items { get; } = new();
    }

    [HtmlTargetElement("g-menu-item", ParentTag = "g-menu")]
    public class GMenuItemTagHelper : TagHelper
    {
        public string Text    { get; set; } = "";
        public string Icon    { get; set; } = "";
        public string Onclick { get; set; } = "";
        public string Type    { get; set; } = "";    // danger | warning
        public string Href    { get; set; } = "";
        public bool   Disabled{ get; set; } = false;

        public override void Process(TagHelperContext ctx, TagHelperOutput output)
        {
            var mc = ctx.Items[typeof(GMenuContext)] as GMenuContext;
            if (mc == null) { output.SuppressOutput(); return; }

            var textColor = Type switch
            {
                "danger"  => "text-red-600 hover:bg-red-50 hover:text-red-700",
                "warning" => "text-amber-600 hover:bg-amber-50",
                _         => "text-slate-700 hover:bg-blue-50 hover:text-blue-700"  // hover 跟隨 --dx-brand-soft
            };
            var disAttr     = Disabled ? "pointer-events-none opacity-40" : "";
            var iconHtml    = !string.IsNullOrEmpty(Icon) ? GIconSet.Render(Icon, "w-4 h-4 shrink-0") : @"<span class=""w-4 h-4""></span>";
            var clickAttr   = !string.IsNullOrEmpty(Onclick) ? $@"onclick=""{Onclick}""" : "";
            var hrefAttr    = !string.IsNullOrEmpty(Href) ? $@"href=""{Href}""" : @"href=""#""";
            var tag         = !string.IsNullOrEmpty(Href) ? "a" : "button";
            var typeAttr    = tag == "button" ? @"type=""button""" : "";

            mc.Items.Add($@"
                <{tag} {typeAttr} {hrefAttr} {clickAttr}
                     class=""w-full flex items-center gap-2.5 px-3 py-2 text-sm {textColor} {disAttr} transition-colors rounded-lg"">
                    {iconHtml}
                    <span>{Text}</span>
                </{tag}>");
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement("g-menu-divider", ParentTag = "g-menu")]
    public class GMenuDividerTagHelper : TagHelper
    {
        public override void Process(TagHelperContext ctx, TagHelperOutput output)
        {
            var mc = ctx.Items[typeof(GMenuContext)] as GMenuContext;
            mc?.Items.Add(@"<hr class=""my-1 border-slate-200""/>");
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement("g-menu")]
    [RestrictChildren("g-menu-item", "g-menu-divider")]
    public class GMenuTagHelper : TagHelper
    {
        [HtmlAttributeName("id")]
        public string Id        { get; set; } = "";
        public string TriggerId { get; set; } = "";  // trigger element id
        public string Position  { get; set; } = "bottom-left";  // bottom-left|bottom-right|top-left|top-right
        public string Class     { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var mc    = new GMenuContext();
            context.Items[typeof(GMenuContext)] = mc;
            await output.GetChildContentAsync();

            var menuId = string.IsNullOrEmpty(Id) ? $"gmenu_{Guid.NewGuid():N}" : Id;
            var (posT, posL) = Position switch
            {
                "top-left"     => ("bottom-full mb-1", "left-0"),
                "top-right"    => ("bottom-full mb-1", "right-0"),
                "bottom-right" => ("top-full mt-1", "right-0"),
                _              => ("top-full mt-1", "left-0")
            };

            var items = string.Join("", mc.Items);
            var triggerJs = !string.IsNullOrEmpty(TriggerId)
                ? $@"document.getElementById('{TriggerId}')?.addEventListener('click',e=>{{e.stopPropagation();gMenuToggle('{menuId}');}});"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("id", menuId);
            output.Attributes.SetAttribute("class", $"relative inline-block {Class}");
            output.Content.SetHtmlContent($@"
                <div id=""{menuId}_list""
                     class=""absolute {posT} {posL} z-[100] bg-white rounded-xl border border-slate-200 shadow-xl py-1.5 px-1 hidden min-w-[160px] animate-in fade-in slide-in-from-top-1"">
                    {items}
                </div>
                <script>
                (function() {{
                    if (!window.gMenuToggle) {{
                        window.gMenuToggle = function(id) {{
                            const m = document.getElementById(id + '_list');
                            if (!m) return;
                            m.classList.toggle('hidden');
                        }};
                        document.addEventListener('click', () => {{
                            document.querySelectorAll('[id$=""_list""].absolute').forEach(m => m.classList.add('hidden'));
                        }});
                    }}
                    {triggerJs}
                }})();
                </script>
            ");
        }
    }

    [HtmlTargetElement("g-switch-button")]
    public class GSwitchButtonTagHelper : TagHelper
    {
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public string LabelOn     { get; set; } = "開";
        public string LabelOff    { get; set; } = "關";
        public bool   Checked     { get; set; } = false;
        public string AlpineModel { get; set; } = "";
        public bool   Disabled    { get; set; } = false;
        public string Id          { get; set; } = "";
        public string Class       { get; set; } = "";
        public string Onchange    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gsw_{Guid.NewGuid():N}" : Id;
            var chkAttr  = Checked ? "checked" : "";
            var disAttr  = Disabled ? "disabled" : "";
            var xmodel   = !string.IsNullOrEmpty(AlpineModel) ? $@" x-model=""{AlpineModel}""" : "";
            var onChange  = !string.IsNullOrEmpty(Onchange) ? $@"onchange=""{Onchange}""" : "";

            output.TagName = "label";
            output.Attributes.SetAttribute("class", $"inline-flex items-center gap-2 cursor-pointer select-none {(Disabled ? "opacity-50 cursor-not-allowed" : "")} {Class}");

            output.Content.SetHtmlContent($@"
                <input type=""checkbox"" id=""{inputId}"" name=""{Name}"" {chkAttr} {disAttr} {xmodel} {onChange}
                       class=""sr-only peer"">
                <div class=""relative w-11 h-6 bg-slate-200 peer-checked:bg-blue-600 rounded-full transition-colors duration-200 peer-focus:ring-2 peer-focus:ring-blue-300
                              after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-5 after:w-5
                              after:transition-transform after:duration-200 peer-checked:after:translate-x-5"">
                </div>
                {(string.IsNullOrEmpty(Label) ? "" : $@"<span class=""text-sm text-slate-700""><span class=""peer-checked:hidden"">{LabelOff}</span><span class=""hidden peer-checked:inline"">{LabelOn}</span></span>")}
            ");
        }
    }
}

