using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using Oracle.ManagedDataAccess.Client;
using Web_EIP_Restruct.Helpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-combobox-legacy")]
    public class GComboBoxTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public string Value       { get; set; } = "";
        public string Placeholder { get; set; } = "請選擇..";
        public string Options     { get; set; } = "";  // "value:text,value:text,..."
        public string ApiUrl      { get; set; } = "";   // dynamic options API
        public string ValueField  { get; set; } = "id";
        public string TextField   { get; set; } = "name";
        public string AlpineModel { get; set; } = "";
        public bool   Required    { get; set; } = false;
        public bool   Disabled    { get; set; } = false;
        public int    ColSpan     { get; set; } = 1;
        public string Class       { get; set; } = "";
        public string Onchange    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gcb_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var disAttr  = Disabled ? " disabled" : "";
            var reqAttr  = Required ? " required" : "";
            var xmodel   = !string.IsNullOrEmpty(AlpineModel) ? $@" x-model=""{AlpineModel}""" : "";
            var onChange = !string.IsNullOrEmpty(Onchange) ? $@" onchange=""{Onchange}""" : "";
            var required = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var optSb = new System.Text.StringBuilder();
            optSb.Append($@"<option value="""">{Placeholder}</option>");
            if (!string.IsNullOrWhiteSpace(Options))
            {
                foreach (var item in Options.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = item.Trim().Split(':', 2);
                    var v = parts[0].Trim();
                    var t = parts.Length > 1 ? parts[1].Trim() : v;
                    var sel = v == Value ? " selected" : "";
                    optSb.Append($@"<option value=""{v}""{sel}>{t}</option>");
                }
            }
            var dynScript = !string.IsNullOrEmpty(ApiUrl) ? $@"
                <script>
                (function() {{
                    const sel = document.getElementById('{inputId}');
                    if (!sel) return;
                    fetch('{ApiUrl}').then(r=>r.json()).then(j=>{{
                        const d = j.data ?? j;
                        d.forEach(item => {{
                            const opt = document.createElement('option');
                            opt.value = item['{ValueField}'];
                            opt.textContent = item['{TextField}'];
                            if (opt.value === '{Value}') opt.selected = true;
                            sel.appendChild(opt);
                        }});
                    }}).catch(console.error);
                }})();
                </script>" : "";

            var labelHtml = !string.IsNullOrEmpty(Label)
                ? $@"<label for=""{inputId}"" class=""block text-xs font-semibold text-slate-600 mb-1"">{Label}{required}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {Class}");
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <select id=""{inputId}"" name=""{Name}"" {disAttr} {reqAttr} {xmodel} {onChange}
                        class=""g-input w-full cursor-pointer"">
                    {optSb}
                </select>
                {dynScript}
            ");
        }
    }
    [HtmlTargetElement("g-datebox")]
    public class GDateBoxTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public string Value       { get; set; } = "";
        public string Type        { get; set; } = "date";   // date|datetime-local|month|week
        public string Min         { get; set; } = "";
        public string Max         { get; set; } = "";
        public string AlpineModel { get; set; } = "";
        public bool   Required    { get; set; } = false;
        public bool   Disabled    { get; set; } = false;
        public bool   Readonly    { get; set; } = false;
        public int    ColSpan     { get; set; } = 1;
        public string Class       { get; set; } = "";
        public string Onchange    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gdb_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var attrs    = new System.Text.StringBuilder();
            if (Disabled)  attrs.Append(" disabled");
            if (Readonly)  attrs.Append(" readonly");
            if (Required)  attrs.Append(" required");
            if (!string.IsNullOrEmpty(Min))         attrs.Append($@" min=""{Min}""");
            if (!string.IsNullOrEmpty(Max))         attrs.Append($@" max=""{Max}""");
            if (!string.IsNullOrEmpty(AlpineModel)) attrs.Append($@" x-model=""{AlpineModel}""");
            if (!string.IsNullOrEmpty(Onchange))    attrs.Append($@" onchange=""{Onchange}""");

            var required  = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var extraCls  = Readonly ? " bg-slate-100 text-slate-500 cursor-not-allowed" : "";
            var labelHtml = !string.IsNullOrEmpty(Label)
                ? $@"<label for=""{inputId}"" class=""block text-xs font-semibold text-slate-600 mb-1"">{Label}{required}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {Class}");
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <input type=""{Type}"" id=""{inputId}"" name=""{Name}"" value=""{Value}"" {attrs}
                       class=""g-input w-full{extraCls}"">
            ");
        }
    }
    [HtmlTargetElement("g-numberbox")]
    public class GNumberBoxTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public string Value       { get; set; } = "";
        public string Min         { get; set; } = "";
        public string Max         { get; set; } = "";
        public string Step        { get; set; } = "1";
        public int    Precision   { get; set; } = 0;  // decimal precision
        public string Prefix      { get; set; } = "";  // prefix text, e.g. $
        public string Suffix      { get; set; } = "";  // suffix text, e.g. %
        public string AlpineModel { get; set; } = "";
        public bool   Required    { get; set; } = false;
        public bool   Disabled    { get; set; } = false;
        public int    ColSpan     { get; set; } = 1;
        public string Class       { get; set; } = "";
        public string Onchange    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gnb_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var step     = Precision > 0 ? $"0.{new string('0', Precision - 1)}1" : Step;
            var attrs    = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(Min))         attrs.Append($@" min=""{Min}""");
            if (!string.IsNullOrEmpty(Max))         attrs.Append($@" max=""{Max}""");
            attrs.Append($@" step=""{step}""");
            if (Disabled)  attrs.Append(" disabled");
            if (Required)  attrs.Append(" required");
            if (!string.IsNullOrEmpty(AlpineModel)) attrs.Append($@" x-model.number=""{AlpineModel}""");
            if (!string.IsNullOrEmpty(Onchange))    attrs.Append($@" onchange=""{Onchange}""");

            var required  = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var labelHtml = !string.IsNullOrEmpty(Label)
                ? $@"<label for=""{inputId}"" class=""block text-xs font-semibold text-slate-600 mb-1"">{Label}{required}</label>"
                : "";
            var hasFix    = !string.IsNullOrEmpty(Prefix) || !string.IsNullOrEmpty(Suffix);

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {Class}");

            if (hasFix)
            {
                output.Content.SetHtmlContent($@"
                    {labelHtml}
                    <div class=""flex items-center border border-slate-300 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-blue-400 bg-white"">
                        {(string.IsNullOrEmpty(Prefix) ? "" : $@"<span class=""px-2 text-sm text-slate-500 bg-slate-100 border-r border-slate-300"">{Prefix}</span>")}
                        <input type=""number"" id=""{inputId}"" name=""{Name}"" value=""{Value}"" {attrs}
                               class=""flex-1 px-2 py-2 text-sm border-0 focus:outline-none bg-slate-100"">
                        {(string.IsNullOrEmpty(Suffix) ? "" : $@"<span class=""px-2 text-sm text-slate-500 bg-slate-100 border-l border-slate-300"">{Suffix}</span>")}
                    </div>
                ");
            }
            else
            {
                output.Content.SetHtmlContent($@"
                    {labelHtml}
                    <input type=""number"" id=""{inputId}"" name=""{Name}"" value=""{Value}"" {attrs}
                           class=""g-input w-full"">
                ");
            }
        }
    }
    [HtmlTargetElement("g-number-spinner")]
    public class GNumberSpinnerTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public int    Value       { get; set; } = 0;
        public int    Min         { get; set; } = 0;
        public int    Max         { get; set; } = 999;
        public int    Step        { get; set; } = 1;
        public string AlpineModel { get; set; } = "";
        public bool   Disabled    { get; set; } = false;
        public int    ColSpan     { get; set; } = 1;
        public string Class       { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gns_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var xmodel   = !string.IsNullOrEmpty(AlpineModel) ? $@" x-model.number=""{AlpineModel}""" : "";
            var disAttr  = Disabled ? " disabled" : "";
            var labelHtml= !string.IsNullOrEmpty(Label)
                ? $@"<label for=""{inputId}"" class=""block text-xs font-semibold text-slate-600 mb-1"">{Label}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {Class}");
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <div class=""flex items-center border border-slate-300 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-blue-400 bg-white"">
                    <button type=""button"" {disAttr}
                            onclick=""const i=document.getElementById('{inputId}');const v=parseInt(i.value||0);if(v>{Min}-1){{i.value=Math.max({Min},v-{Step});i.dispatchEvent(new Event('change'));}}""
                            class=""px-2.5 py-2 text-slate-600 bg-slate-100 transition-colors border-r border-slate-300 font-bold text-lg leading-none disabled:opacity-40"">-</button>
                    <input type=""number"" id=""{inputId}"" name=""{Name}"" value=""{Value}""
                           min=""{Min}"" max=""{Max}"" step=""{Step}"" {xmodel} {disAttr}
                           class=""flex-1 text-center text-sm border-0 focus:outline-none bg-slate-100 py-2 [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none"">
                    <button type=""button"" {disAttr}
                            onclick=""const i=document.getElementById('{inputId}');const v=parseInt(i.value||0);if(v<{Max}+1){{i.value=Math.min({Max},v+{Step});i.dispatchEvent(new Event('change'));}}""
                            class=""px-2.5 py-2 text-slate-600 bg-slate-100 transition-colors border-l border-slate-300 font-bold text-lg leading-none disabled:opacity-40"">+</button>
                </div>
            ");
        }
    }
}





