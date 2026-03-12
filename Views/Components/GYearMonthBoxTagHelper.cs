using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Restruct.Views.Components
{ [HtmlTargetElement("g-yearmonthbox")] [HtmlTargetElement("g-year-month-box")] public class GYearMonthBoxTagHelper : GDateBoxTagHelper { public override void Process(TagHelperContext context, TagHelperOutput output) { Type = "month"; base.Process(context, output); } }
} 

