using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Restruct.Views.Components
{ // JQRefval -> ASP.NET Core TagHelper // Uses current LOV component behavior. [HtmlTargetElement("g-refval")] public class GRefvalTagHelper : GLovInputTagHelper { [HtmlAttributeName("lov-function")] public string LovFunction { get; set; } = ""; [HtmlAttributeName("show-lov-button")] public bool? ShowLovButton { get; set; } public override void Process(TagHelperContext context, TagHelperOutput output) { if (string.IsNullOrWhiteSpace(LovFn) && !string.IsNullOrWhiteSpace(LovFunction)) { LovFn = LovFunction; } if (ShowLovButton.HasValue) { ShowButton = ShowLovButton.Value; } base.Process(context, output); } }
}


