using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-textbox")]
    public class GTextBoxTagHelper : TagHelper
    {
        public string Label { get; set; } = "";
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
        public string Type { get; set; } = "text"; // text|number|date|email|password|tel|textarea|datetime-local
        public string Placeholder { get; set; } = "";
        public string Value { get; set; } = "";
        public string AlpineModel { get; set; } = "";
        [HtmlAttributeName("x-model")]
        public string XModel { get; set; } = "";
        [HtmlAttributeName("x-bind-readonly")]
        public string XBindReadonly { get; set; } = "";
        [HtmlAttributeName("x-bind-disabled")]
        public string XBindDisabled { get; set; } = "";
        [HtmlAttributeName("x-on-enter")]
        public string XOnEnter { get; set; } = "";
        public bool Required { get; set; } = false;
        public bool Readonly { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public string Help { get; set; } = "";
        public string Min { get; set; } = "";
        public string Max { get; set; } = "";
        public string Step { get; set; } = "";
        public string Maxlength { get; set; } = "";
        public int ColSpan { get; set; } = 1;
        public int Rows { get; set; } = 3;

        public string Class { get; set; } = "";
        public string ExtraClass { get; set; } = "";
        public string WrapperClass { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId = string.IsNullOrEmpty(Id) ? $"gt_{Guid.NewGuid():N}" : Id;
            var requiredMark = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var helpHtml = !string.IsNullOrEmpty(Help)
                ? $@"<p class=""text-xs text-slate-400 mt-1"">{Help}</p>"
                : "";
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var disAttr = Disabled ? " disabled" : "";
            var rdoAttr = Readonly ? " readonly" : "";
            var reqAttr = Required ? " required" : "";
            var model = !string.IsNullOrWhiteSpace(XModel) ? XModel : AlpineModel;
            var xmodel = !string.IsNullOrEmpty(model) ? $@" x-model=""{model}""" : "";
            var bindReadonlyAttr = !string.IsNullOrWhiteSpace(XBindReadonly) ? $@" :readonly=""{XBindReadonly}""" : "";
            var bindDisabledAttr = !string.IsNullOrWhiteSpace(XBindDisabled) ? $@" :disabled=""{XBindDisabled}""" : "";
            var xOnEnterAttr = !string.IsNullOrWhiteSpace(XOnEnter) ? $@" x-on:keydown.enter=""{XOnEnter}""" : "";
            var minAttr = !string.IsNullOrEmpty(Min) ? $@" min=""{Min}""" : "";
            var maxAttr = !string.IsNullOrEmpty(Max) ? $@" max=""{Max}""" : "";
            var stepAttr = !string.IsNullOrEmpty(Step) ? $@" step=""{Step}""" : "";
            var maxlenAttr = !string.IsNullOrEmpty(Maxlength) ? $@" maxlength=""{Maxlength}""" : "";
            var readonlyClass = Readonly ? " bg-slate-100 text-slate-500 cursor-not-allowed" : "";

            var defaultInputClass = $"block w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-blue-500 focus:border-blue-500 {readonlyClass}";
            var finalInputClass = TagHelperClassResolver.Resolve(defaultInputClass, Class, ExtraClass);

            string inputHtml;
            if (string.Equals(Type, "textarea", StringComparison.OrdinalIgnoreCase))
            {
                inputHtml = $@"<textarea id=""{inputId}"" name=""{Name}"" rows=""{Rows}""
                    placeholder=""{Placeholder}""{disAttr}{rdoAttr}{reqAttr}{xmodel}{bindReadonlyAttr}{bindDisabledAttr}
                    class=""{finalInputClass} resize-y""{xOnEnterAttr}>{Value}</textarea>";
            }
            else
            {
                inputHtml = $@"<input type=""{Type}"" id=""{inputId}"" name=""{Name}""
                    placeholder=""{Placeholder}"" value=""{Value}""
                    {disAttr}{rdoAttr}{reqAttr}{xmodel}{bindReadonlyAttr}{bindDisabledAttr}{xOnEnterAttr}{minAttr}{maxAttr}{stepAttr}{maxlenAttr}
                    class=""{finalInputClass}"">";
            }

            var defaultWrapperClass = $"flex flex-col gap-1 {colClass}";
            var finalWrapperClass = string.IsNullOrWhiteSpace(WrapperClass) ? defaultWrapperClass : WrapperClass.Trim();

            output.TagName = "div";
            output.Attributes.SetAttribute("class", finalWrapperClass);
            output.Content.SetHtmlContent($@"
                <label for=""{inputId}"" class=""block text-xs font-semibold text-slate-600"">{Label}{requiredMark}</label>
                {inputHtml}
                {helpHtml}
            ");
        }
    }
}



