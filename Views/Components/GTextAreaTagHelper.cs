using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Restruct.Views.Components
{ [HtmlTargetElement("g-textarea")] [HtmlTargetElement("g-text-area")] public class GTextAreaTagHelper : GTextBoxTagHelper { public override void Process(TagHelperContext context, TagHelperOutput output) { Type = "textarea"; base.Process(context, output); } }
} 

