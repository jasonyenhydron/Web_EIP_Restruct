using Microsoft.AspNetCore.Razor.TagHelpers;
using Web_EIP_Restruct.Models.DataForm;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("query-column", ParentTag = "g-dataform")]
    public class QueryColumnTagHelper : TagHelper
    {
        [HtmlAttributeName("field-name")]
        public string FieldName { get; set; } = string.Empty;

        [HtmlAttributeName("query-field")]
        public string QueryField { get; set; } = string.Empty;

        [HtmlAttributeName("caption")]
        public string Caption { get; set; } = string.Empty;

        [HtmlAttributeName("editor")]
        public string Editor { get; set; } = "g-textbox";

        [HtmlAttributeName("span")]
        public int Span { get; set; } = 3;

        [HtmlAttributeName("default-value")]
        public string? DefaultValue { get; set; }

        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; } = string.Empty;

        [HtmlAttributeName("options")]
        public string Options { get; set; } = string.Empty;

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

        [HtmlAttributeName("readonly")]
        public bool Readonly { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (!context.Items.TryGetValue(typeof(FormColumnContext), out var ctx)) return;
            if (ctx is not FormColumnContext formCtx) return;
            if (string.IsNullOrWhiteSpace(FieldName)) return;

            formCtx.QueryColumns.Add(new QueryColumn
            {
                FieldName = FieldName,
                QueryField = string.IsNullOrWhiteSpace(QueryField) ? FieldName : QueryField,
                Caption = string.IsNullOrWhiteSpace(Caption) ? FieldName : Caption,
                Editor = NormalizeEditor(Editor),
                Span = Span,
                DefaultValue = DefaultValue,
                Placeholder = Placeholder,
                Options = Options,
                LovTitle = LovTitle,
                LovApi = LovApi,
                LovColumns = LovColumns,
                LovFields = LovFields,
                LovKeyValue = LovKeyValue,
                LovKeyDisplay = LovKeyDisplay,
                LovDisplayFormat = LovDisplayFormat,
                Readonly = Readonly
            });
        }

        private static string NormalizeEditor(string raw)
        {
            var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
            return value switch
            {
                "" => "text",
                "g-textbox" => "text",
                "gtextbox" => "text",
                "textbox" => "text",
                "text" => "text",

                "g-datebox" => "date",
                "gdatebox" => "date",
                "datebox" => "date",
                "date" => "date",

                "g-checkbox" => "checkbox",
                "gcheckbox" => "checkbox",
                "checkbox" => "checkbox",

                "g-combobox" => "gcombobox",
                "gcombobox" => "gcombobox",
                "combobox" => "gcombobox",
                "infocombobox" => "infocombobox",

                "g-lov-input" => "lovinput",
                "glovinput" => "lovinput",
                "lovinput" => "lovinput",
                "lov" => "lovinput",

                "g-readonly" => "readonly",
                "greadonly" => "readonly",
                "readonly" => "readonly",

                _ => value
            };
        }
    }
}

