using Microsoft.AspNetCore.Razor.TagHelpers;
using Web_EIP_Restruct.Models.DataForm;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("form-column", ParentTag = "g-dataform")]
    public class FormColumnTagHelper : TagHelper
    {
        [HtmlAttributeName("field-name")]
        public string FieldName { get; set; } = string.Empty;

        [HtmlAttributeName("caption")]
        public string Caption { get; set; } = string.Empty;

        [HtmlAttributeName("caption-alignment")]
        public string CaptionAlignment { get; set; } = "left";

        [HtmlAttributeName("column-type")]
        public string ColumnType { get; set; } = "g-textbox";

        [HtmlAttributeName("col-span")]
        public int ColSpan { get; set; } = 1;

        [HtmlAttributeName("new-line")]
        public bool NewLine { get; set; } = false;

        [HtmlAttributeName("always-read-only")]
        public bool AlwaysReadOnly { get; set; } = false;

        [HtmlAttributeName("required")]
        public bool Required { get; set; } = false;

        [HtmlAttributeName("is-primary-key")]
        public bool IsPrimaryKey { get; set; } = false;

        [HtmlAttributeName("hidden")]
        public bool Hidden { get; set; } = false;

        [HtmlAttributeName("default-value")]
        public string? DefaultValue { get; set; }

        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; } = string.Empty;

        [HtmlAttributeName("max-length")]
        public int? MaxLength { get; set; }

        [HtmlAttributeName("min")]
        public decimal? Min { get; set; }

        [HtmlAttributeName("max")]
        public decimal? Max { get; set; }

        [HtmlAttributeName("options")]
        public string Options { get; set; } = string.Empty;

        [HtmlAttributeName("options-api")]
        public string OptionsApi { get; set; } = string.Empty;

        [HtmlAttributeName("lov-title")]
        public string LovTitle { get; set; } = string.Empty;

        [HtmlAttributeName("lov-api")]
        public string LovApi { get; set; } = string.Empty;

        [HtmlAttributeName("lov-columns")]
        public string LovColumns { get; set; } = string.Empty;

        [HtmlAttributeName("lov-fields")]
        public string LovFields { get; set; } = string.Empty;

        [HtmlAttributeName("lov-key-value")]
        public string LovKeyValue { get; set; } = string.Empty;

        [HtmlAttributeName("lov-key-display")]
        public string LovKeyDisplay { get; set; } = string.Empty;

        [HtmlAttributeName("lov-display-format")]
        public string LovDisplayFormat { get; set; } = string.Empty;

        [HtmlAttributeName("lov-on-confirm")]
        public string LovOnConfirm { get; set; } = string.Empty;

        [HtmlAttributeName("validate-fn")]
        public string ValidateFn { get; set; } = string.Empty;

        [HtmlAttributeName("validate-message")]
        public string ValidateMessage { get; set; } = string.Empty;

        [HtmlAttributeName("compare-field")]
        public string CompareField { get; set; } = string.Empty;

        [HtmlAttributeName("compare-mode")]
        public string CompareMode { get; set; } = string.Empty;

        [HtmlAttributeName("value-type")]
        public string ValueType { get; set; } = string.Empty;

        [HtmlAttributeName("on-change")]
        public string OnChange { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (!context.Items.TryGetValue(typeof(FormColumnContext), out var ctx)) return;
            if (ctx is not FormColumnContext colCtx) return;
            if (string.IsNullOrWhiteSpace(FieldName)) return;

            colCtx.Columns.Add(new FormColumn
            {
                FieldName = FieldName,
                Caption = string.IsNullOrWhiteSpace(Caption) ? FieldName : Caption,
                CaptionAlignment = CaptionAlignment,
                ColumnType = ResolveColumnType(ColumnType),
                ColSpan = ColSpan,
                NewLine = NewLine,
                AlwaysReadOnly = AlwaysReadOnly,
                Required = Required,
                IsPrimaryKey = IsPrimaryKey,
                Hidden = Hidden,
                DefaultValue = DefaultValue,
                Placeholder = Placeholder,
                MaxLength = MaxLength,
                Min = Min,
                Max = Max,
                Options = Options,
                OptionsApi = OptionsApi,
                LovTitle = LovTitle,
                LovApi = LovApi,
                LovColumns = LovColumns,
                LovFields = LovFields,
                LovKeyValue = LovKeyValue,
                LovKeyDisplay = LovKeyDisplay,
                LovDisplayFormat = LovDisplayFormat,
                LovOnConfirm = LovOnConfirm,
                ValidateFn = ValidateFn,
                ValidateMessage = ValidateMessage,
                CompareField = CompareField,
                CompareMode = CompareMode,
                ValueType = ValueType,
                OnChange = OnChange
            });
        }

        private static FormColumnType ResolveColumnType(string raw)
        {
            var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
            return value switch
            {
                "" => FormColumnType.Text,
                "text" => FormColumnType.Text,
                "textbox" => FormColumnType.Text,
                "gtextbox" => FormColumnType.Text,
                "g-textbox" => FormColumnType.Text,

                "number" => FormColumnType.Number,
                "numberbox" => FormColumnType.Number,
                "gnumberbox" => FormColumnType.Number,
                "g-numberbox" => FormColumnType.Number,

                "date" => FormColumnType.Date,
                "datebox" => FormColumnType.Date,
                "gdatebox" => FormColumnType.Date,
                "g-datebox" => FormColumnType.Date,

                "datetimelocal" => FormColumnType.DateTimeLocal,
                "datetime-local" => FormColumnType.DateTimeLocal,
                "datetimebox" => FormColumnType.DateTimeLocal,
                "gdatetimebox" => FormColumnType.DateTimeLocal,
                "g-datetimebox" => FormColumnType.DateTimeLocal,

                "select" => FormColumnType.Select,
                "combobox" => FormColumnType.Select,
                "gcombobox" => FormColumnType.Select,
                "g-combobox" => FormColumnType.Select,

                "checkbox" => FormColumnType.Checkbox,
                "gcheckbox" => FormColumnType.Checkbox,
                "g-checkbox" => FormColumnType.Checkbox,

                "radio" => FormColumnType.Radio,
                "radiogroup" => FormColumnType.Radio,
                "gradiogroup" => FormColumnType.Radio,
                "g-radiogroup" => FormColumnType.Radio,

                "lov" => FormColumnType.Lov,
                "lovinput" => FormColumnType.Lov,
                "glovinput" => FormColumnType.Lov,
                "g-lov-input" => FormColumnType.Lov,

                "readonly" => FormColumnType.Readonly,
                "greadonly" => FormColumnType.Readonly,
                "g-readonly" => FormColumnType.Readonly,

                "textarea" => FormColumnType.Textarea,
                "gtextarea" => FormColumnType.Textarea,
                "g-textarea" => FormColumnType.Textarea,

                "hidden" => FormColumnType.Hidden,
                "ghidden" => FormColumnType.Hidden,
                "g-hidden" => FormColumnType.Hidden,

                _ when Enum.TryParse<FormColumnType>(raw, true, out var parsed) => parsed,
                _ => FormColumnType.Text
            };
        }
    }
}

