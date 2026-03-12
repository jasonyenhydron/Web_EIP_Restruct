using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Restruct.Views.Components
{ [HtmlTargetElement("g-html")] public class GHtmlTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Html"; }
} 

