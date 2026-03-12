using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-dialog")]
    public class GDialogTagHelper : TagHelper
    {
        public string Id             { get; set; } = "";
        public string Title          { get; set; } = "";
        public string Width          { get; set; } = "md";  // sm|md|lg|xl|full
        public bool   CloseBtn       { get; set; } = true;
        public bool   BackdropClose  { get; set; } = true;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
            var maxW    = Width switch
            {
                "sm"   => "max-w-sm",
                "md"   => "max-w-lg",
                "lg"   => "max-w-3xl",
                "xl"   => "max-w-5xl",
                "full" => "max-w-full mx-4",
                _      => "max-w-lg"
            };
            var closeBtnHtml = CloseBtn
                ? $"""<button type="button" onclick="gDialogClose('{Id}')" class="text-white/85 hover:text-white bg-white/10 hover:bg-white/20 border border-white/25 p-1.5 rounded-lg transition-all" title="關閉"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>"""
                : "";
            var backdropAttr = BackdropClose
                ? $"""onclick="if(event.target===this)gDialogClose('{Id}')" """
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("id", Id);
            output.Attributes.SetAttribute("role", "dialog");
            output.Attributes.SetAttribute("aria-modal", "true");
            output.Attributes.SetAttribute("class", "fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[200] items-center justify-center p-4");
            output.Attributes.SetAttribute("style", "display:none;");
            if (!string.IsNullOrEmpty(backdropAttr))
                output.Attributes.SetAttribute("onclick", $"if(event.target===this)gDialogClose('{Id}')");

            output.Content.SetHtmlContent($"""
                <div class="bg-white rounded-2xl shadow-2xl w-full {maxW} mx-4 sm:mx-auto flex flex-col border border-slate-200 transform transition-all duration-200 scale-95 opacity-0" id="{Id}-content">
                    <div class="flex items-center justify-between px-5 py-4 bg-blue-600 from-blue-600 to-blue-700 rounded-t-2xl shrink-0">
                        <h3 class="text-base font-bold text-white truncate pr-2">{Title}</h3>
                        {closeBtnHtml}
                    </div>
                    <div class="p-4 sm:p-5 overflow-y-auto max-h-[80vh]">
                        {content}
                    </div>
                </div>
            """);
        }
    }
}


