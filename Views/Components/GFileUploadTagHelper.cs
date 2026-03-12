using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Restruct.Views.Components
{ // JQFileUpload -> ASP.NET Core TagHelper [HtmlTargetElement("g-fileupload")] [HtmlTargetElement("g-file-upload")] public class GFileUploadTagHelper : GFileUploaderTagHelper { [HtmlAttributeName("on-success")] public string OnSuccess { get; set; } = ""; [HtmlAttributeName("on-error")] public string OnError { get; set; } = ""; [HtmlAttributeName("mode")] public string LegacyMode { get; set; } = ""; public override void Process(TagHelperContext context, TagHelperOutput output) { if (string.IsNullOrWhiteSpace(OnUploaded) && !string.IsNullOrWhiteSpace(OnSuccess)) { OnUploaded = OnSuccess; } if (string.IsNullOrWhiteSpace(OnUploadError) && !string.IsNullOrWhiteSpace(OnError)) { OnUploadError = OnError; } if (string.IsNullOrWhiteSpace(UploadMode) && !string.IsNullOrWhiteSpace(LegacyMode)) { UploadMode = LegacyMode; } base.Process(context, output); } }
}


