using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-icon")]
    public class GIconTagHelper : TagHelper
    {
        [HtmlAttributeName("name")]
        public string Name { get; set; } = "";

        public string Class { get; set; } = "w-4 h-4";
        public string Tone { get; set; } = "";
        public string Context { get; set; } = "";
        [HtmlAttributeName("extra-class")]
        public string ExtraClass { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            output.Content.SetHtmlContent(!string.IsNullOrWhiteSpace(Context)
                ? GIconSet.RenderForContext(Name, Class, Context, ExtraClass)
                : GIconSet.Render(Name, Class, Tone, ExtraClass));
        }
    }
}

