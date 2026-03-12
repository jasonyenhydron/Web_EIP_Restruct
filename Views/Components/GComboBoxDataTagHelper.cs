using System.Text;
using System.Text.Encodings.Web;
using System.Data;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Web_EIP_Restruct.Helpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-combobox")]
    public class GComboBoxDataTagHelper : TagHelper
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Placeholder { get; set; } = "請選擇";
        public string Items { get; set; } = ""; // value:text,value:text
        public string Sql { get; set; } = "";   // SELECT value,text FROM ...
        public string ValueField { get; set; } = "";
        public string TextField { get; set; } = "";
        [HtmlAttributeName("x-model")]
        public string AlpineModel { get; set; } = "";
        [HtmlAttributeName("x-bind-disabled")]
        public string XBindDisabled { get; set; } = "";
        public bool Required { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public int ColSpan { get; set; } = 1;
        public string Class { get; set; } = "";
        public string ExtraClass { get; set; } = "";
        public string InputClass { get; set; } = "block w-20 px-2.5 py-1.5 border border-slate-300 rounded-lg text-sm focus:ring-blue-500 focus:border-blue-500";
        public string Onchange { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId = string.IsNullOrWhiteSpace(Id) ? $"gcb_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var requiredMark = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var disAttr = Disabled ? " disabled" : "";
            var reqAttr = Required ? " required" : "";
            var xmodel = string.IsNullOrWhiteSpace(AlpineModel) ? "" : $@" x-model=""{HtmlEncoder.Default.Encode(AlpineModel)}""";
            var bindDisabled = string.IsNullOrWhiteSpace(XBindDisabled) ? "" : $@" :disabled=""{HtmlEncoder.Default.Encode(XBindDisabled)}""";
            var onchange = string.IsNullOrWhiteSpace(Onchange) ? "" : $@" onchange=""{HtmlEncoder.Default.Encode(Onchange)}""";

            var optionHtml = new StringBuilder();
            optionHtml.Append($@"<option value="""">{HtmlEncoder.Default.Encode(Placeholder)}</option>");
            AppendItemsOptions(optionHtml);
            AppendSqlOptions(optionHtml);

            var labelHtml = string.IsNullOrWhiteSpace(Label)
                ? ""
                : $@"<label for=""{inputId}"" class=""text-xs font-bold text-slate-600"">{HtmlEncoder.Default.Encode(Label)}{requiredMark}</label>";

            output.TagName = "div";
            var defaultClass = $"flex flex-col gap-1 {colClass}";
            var finalWrapperClass = TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass);
            output.Attributes.SetAttribute("class", finalWrapperClass);
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <select id=""{inputId}"" name=""{Name}"" class=""{InputClass}""{disAttr}{reqAttr}{xmodel}{bindDisabled}{onchange}>
                    {optionHtml}
                </select>
            ");
        }

        private void AppendItemsOptions(StringBuilder optionHtml)
        {
            if (string.IsNullOrWhiteSpace(Items)) return;

            foreach (var item in Items.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var separator = item.Contains('=') ? '=' : ':';
                var parts = item.Split(separator, 2, StringSplitOptions.TrimEntries);
                var val = parts[0];
                var text = parts.Length > 1 ? parts[1] : val;
                var selected = string.Equals(val, Value, StringComparison.OrdinalIgnoreCase) ? " selected" : "";
                optionHtml.Append($@"<option value=""{HtmlEncoder.Default.Encode(val)}""{selected}>{HtmlEncoder.Default.Encode(text)}</option>");
            }
        }

        private void AppendSqlOptions(StringBuilder optionHtml)
        {
            if (string.IsNullOrWhiteSpace(Sql)) return;

            try
            {
                var dt = DbHelper.GetDataTable(CommandType.Text, Sql);
                using var reader = dt.CreateDataReader();
                while (reader.Read())
                {
                    var rawValue = ResolveColumn(reader, ValueField, 0);
                    var rawText = ResolveColumn(reader, TextField, 1);
                    var val = rawValue?.ToString() ?? "";
                    var text = rawText?.ToString() ?? val;
                    var selected = string.Equals(val, Value, StringComparison.OrdinalIgnoreCase) ? " selected" : "";
                    optionHtml.Append($@"<option value=""{HtmlEncoder.Default.Encode(val)}""{selected}>{HtmlEncoder.Default.Encode(text)}</option>");
                }
            }
            catch
            {
            }
        }

        private static object ResolveColumn(IDataReader reader, string fieldName, int fallbackIndex)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                try { return reader[fieldName]; } catch { }
            }

            if (reader.FieldCount > fallbackIndex) return reader.GetValue(fallbackIndex);
            if (reader.FieldCount > 0) return reader.GetValue(0);
            return "";
        }
    }
}




