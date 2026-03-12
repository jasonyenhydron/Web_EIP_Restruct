using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Restruct.Views.Components
{ [HtmlTargetElement("g-label")] public class GLabelTagHelper : TagHelper { public string Text { get; set; } = string.Empty; public string Class { get; set; } = "text-sm text-slate-700"; public override void Process(TagHelperContext context, TagHelperOutput output) { output.TagName = "span"; output.Attributes.SetAttribute("class", Class); output.Content.SetContent(Text); } }
} 

