using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-file-uploader")]
    public class GFileUploaderTagHelper : TagHelper
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "files";
        public string Label { get; set; } = "";
        public string UploadUrl { get; set; } = "/api/files/upload";
        public string UploadMode { get; set; } = "instantly"; // instantly | useButtons | useForm
        public string Accept { get; set; } = "";
        public string AllowedFileExtensions { get; set; } = "";
        public bool Multiple { get; set; } = true;
        public bool Required { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public long MaxFileSize { get; set; } = 0; // bytes, 0 means unlimited
        public long MinFileSize { get; set; } = 0; // bytes
        public string SelectButtonText { get; set; } = "Select file";
        public string UploadButtonText { get; set; } = "Upload";
        public string CancelButtonText { get; set; } = "Clear";
        public string DropZoneText { get; set; } = "or Drop file here";
        public string Hint { get; set; } = "";
        public string Folder { get; set; } = "";
        public string ResultInputId { get; set; } = "";
        public string ResultValueField { get; set; } = "relativePath";
        public bool ToastOnUploaded { get; set; } = false;
        public string SuccessToastText { get; set; } = "附件上傳成功";
        public string OpenUrlTemplate { get; set; } = "/Files/Open?path={path}";
        public string ColumnName { get; set; } = "";
        public string TableId { get; set; } = "";
        public string ExtraData { get; set; } = ""; // JSON string
        public string OnValueChanged { get; set; } = "";
        public string OnUploaded { get; set; } = "";
        public string OnUploadError { get; set; } = "";
        public bool AutoInit { get; set; } = true;
        public bool ShowFileList { get; set; } = true;
        public int ColSpan { get; set; } = 1;
        public string Class { get; set; } = "";
        public string ExtraClass { get; set; } = "";
        public string WrapperClass { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId = string.IsNullOrWhiteSpace(Id) ? $"gfu_{Guid.NewGuid():N}" : Id.Trim();
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var defaultWrapperClass = $"flex flex-col gap-1 {colClass}";
            var finalWrapperClass = string.IsNullOrWhiteSpace(WrapperClass) ? defaultWrapperClass : WrapperClass.Trim();

            var baseClass = "g-file-uploader rounded-xl border border-slate-300 bg-white p-3";
            var finalUploaderClass = TagHelperClassResolver.Resolve(baseClass, Class, ExtraClass);

            var reqMark = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var labelHtml = string.IsNullOrWhiteSpace(Label)
                ? ""
                : $@"<label for=""{compId}_input"" class=""block text-xs font-semibold text-slate-600 mb-1"">{WebUtility.HtmlEncode(Label)}{reqMark}</label>";
            var hintHtml = string.IsNullOrWhiteSpace(Hint)
                ? ""
                : $@"<p class=""mt-1 text-xs text-slate-500"">{WebUtility.HtmlEncode(Hint)}</p>";

            var disabledAttr = Disabled ? " disabled" : "";
            var requiredAttr = Required ? " required" : "";
            var multipleAttr = Multiple ? " multiple" : "";
            var acceptAttr = string.IsNullOrWhiteSpace(Accept) ? "" : $@" accept=""{WebUtility.HtmlEncode(Accept)}""";
            var mode = NormalizeMode(UploadMode);
            var showButtonArea = mode == "useButtons";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", finalWrapperClass);
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <div id=""{compId}""
                     class=""{finalUploaderClass}""
                     data-g-file-uploader=""1""
                     data-gfu-auto=""{AutoInit.ToString().ToLowerInvariant()}""
                     data-upload-url=""{WebUtility.HtmlEncode(UploadUrl)}""
                     data-upload-mode=""{mode}""
                     data-name=""{WebUtility.HtmlEncode(Name)}""
                     data-accept=""{WebUtility.HtmlEncode(Accept)}""
                     data-allowed-extensions=""{WebUtility.HtmlEncode(AllowedFileExtensions)}""
                     data-multiple=""{Multiple.ToString().ToLowerInvariant()}""
                     data-max-file-size=""{MaxFileSize}""
                     data-min-file-size=""{MinFileSize}""
                     data-folder=""{WebUtility.HtmlEncode(Folder)}""
                     data-result-input-id=""{WebUtility.HtmlEncode(ResultInputId)}""
                     data-result-value-field=""{WebUtility.HtmlEncode(ResultValueField)}""
                     data-toast-on-uploaded=""{ToastOnUploaded.ToString().ToLowerInvariant()}""
                     data-success-toast-text=""{WebUtility.HtmlEncode(SuccessToastText)}""
                     data-open-url-template=""{WebUtility.HtmlEncode(OpenUrlTemplate)}""
                     data-column-name=""{WebUtility.HtmlEncode(ColumnName)}""
                     data-table-id=""{WebUtility.HtmlEncode(TableId)}""
                     data-extra-data=""{WebUtility.HtmlEncode(ExtraData)}""
                     data-on-value-changed=""{WebUtility.HtmlEncode(OnValueChanged)}""
                     data-on-uploaded=""{WebUtility.HtmlEncode(OnUploaded)}""
                     data-on-upload-error=""{WebUtility.HtmlEncode(OnUploadError)}""
                     data-show-file-list=""{ShowFileList.ToString().ToLowerInvariant()}""
                     aria-disabled=""{Disabled.ToString().ToLowerInvariant()}"">
                    <input id=""{compId}_input"" type=""file"" class=""sr-only"" name=""{WebUtility.HtmlEncode(Name)}""{disabledAttr}{requiredAttr}{multipleAttr}{acceptAttr}>
                    <div class=""flex flex-wrap items-center gap-2"">
                        <button type=""button"" class=""px-3 py-2 rounded-lg bg-blue-600 text-white text-sm font-semibold bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"" data-role=""pick""{disabledAttr}>
                            {WebUtility.HtmlEncode(SelectButtonText)}
                        </button>
                        <span class=""text-sm text-slate-500"" data-role=""dropzone-text"">{WebUtility.HtmlEncode(DropZoneText)}</span>
                    </div>
                    <div class=""mt-2 min-h-10 rounded-lg border border-dashed border-slate-300 bg-slate-100 px-2 py-1.5 text-sm text-slate-600"" data-role=""dropzone"">
                        <div data-role=""list"" class=""space-y-1""></div>
                        <div data-role=""empty"" class=""text-slate-400"">No file selected</div>
                    </div>
                    <div class=""mt-2 flex items-center gap-2 {(!showButtonArea ? "hidden" : "")}"" data-role=""actions"">
                        <button type=""button"" class=""px-3 py-1.5 rounded-lg bg-blue-600 text-white text-sm font-semibold bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"" data-role=""upload""{disabledAttr}>
                            {WebUtility.HtmlEncode(UploadButtonText)}
                        </button>
                        <button type=""button"" class=""px-3 py-1.5 rounded-lg bg-slate-100 text-slate-700 text-sm font-semibold bg-slate-100 disabled:opacity-50 disabled:cursor-not-allowed"" data-role=""clear""{disabledAttr}>
                            {WebUtility.HtmlEncode(CancelButtonText)}
                        </button>
                    </div>
                    <div class=""mt-2 hidden text-xs"" data-role=""message""></div>
                </div>
                {hintHtml}
            ");
        }

        private static string NormalizeMode(string mode)
        {
            var normalized = (mode ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "usebuttons" => "useButtons",
                "useform" => "useForm",
                _ => "instantly"
            };
        }
    }
}

