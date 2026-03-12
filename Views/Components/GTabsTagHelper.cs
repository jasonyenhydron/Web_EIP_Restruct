using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    public class GTabContext
    {
        public List<(string Title, string Icon, string Content)> Tabs { get; } = new();
    }

    [HtmlTargetElement("g-tab", ParentTag = "g-tabs")]
    public class GTabTagHelper : TagHelper
    {
        public string Title { get; set; } = "Tab";
        public string Icon  { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tabCtx  = context.Items[typeof(GTabContext)] as GTabContext;
            var content = (await output.GetChildContentAsync()).GetContent();
            tabCtx?.Tabs.Add((Title, Icon, content));
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement("g-tabs")]
    [RestrictChildren("g-tab")]
    public class GTabsTagHelper : TagHelper
    {
        public int    ActiveTab { get; set; } = 0;
        public string Class     { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tabCtx = new GTabContext();
            context.Items[typeof(GTabContext)] = tabCtx;
            await output.GetChildContentAsync();

            var tabs    = tabCtx.Tabs;
            var headers = new System.Text.StringBuilder();
            for (int i = 0; i < tabs.Count; i++)
            {
                var (title, icon, _) = tabs[i];
                var iconHtml = GIconSet.Render(icon, "w-3.5 h-3.5");
                headers.Append($"""
                    <button type="button"
                        @@click="active={i}"
                        :class="active==={i}
                            ? 'border-blue-600 text-blue-700 font-bold bg-white shadow-sm'
                            : 'border-transparent text-slate-500 hover:text-slate-700 bg-slate-100'"
                        class="flex items-center gap-1.5 px-4 py-2.5 text-sm border-b-2 -mb-px transition-all whitespace-nowrap rounded-t-lg">
                        {iconHtml}{title}
                    </button>
                """);
            }

            var panels = new System.Text.StringBuilder();
            for (int i = 0; i < tabs.Count; i++)
            {
                panels.Append($"""
                    <div x-show="active==={i}" x-cloak class="p-4">
                        {tabs[i].Content}
                    </div>
                """);
            }

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden {Class}");
            output.Attributes.SetAttribute("x-data", $"{{ active: {ActiveTab} }}");
            output.Content.SetHtmlContent($"""
                <div class="flex flex-wrap gap-0.5 border-b border-slate-200 bg-slate-100 px-3 pt-2 overflow-x-auto">
                    {headers}
                </div>
                <div>{panels}</div>
            """);
        }
    }
}



