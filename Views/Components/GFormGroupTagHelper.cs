using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-form-group")]
    public class GFormGroupTagHelper : TagHelper
    {
        public string Label   { get; set; } = "";
        public bool   Required{ get; set; } = false;
        public string Help    { get; set; } = "";
        public string Error   { get; set; } = "";
        public int    ColSpan { get; set; } = 1;
        public string Class   { get; set; } = "";
        public string ExtraClass { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content  = (await output.GetChildContentAsync()).GetContent();
            var required = Required ? """<span class="text-red-500 ml-0.5 font-bold">*</span>""" : "";
            var helpHtml = !string.IsNullOrEmpty(Help)
                ? $"""<p class="text-xs text-slate-400 mt-1">{Help}</p>"""
                : "";
            var errHtml  = !string.IsNullOrEmpty(Error)
                ? $"""<p class="text-xs text-red-500 mt-1 flex items-center gap-1"><svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>{Error}</p>"""
                : "";
            var colClass = ColSpan switch
            {
                2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1"
            };

            output.TagName = "div";
            var defaultClass = $"flex flex-col gap-1 {colClass}";
            var finalClass = TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass);
            output.Attributes.SetAttribute("class", finalClass);
            output.Content.SetHtmlContent($"""
                <label class="block text-xs font-semibold text-slate-600">{Label}{required}</label>
                {content}
                {helpHtml}{errHtml}
            """);
        }
    }
}


