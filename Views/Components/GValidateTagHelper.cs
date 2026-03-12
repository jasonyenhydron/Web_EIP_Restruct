using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web_EIP_Restruct.Views.Components
{

    public enum DuplicateCheckMode
    {
        ByLocal,
        ByWhere
    }

    public enum ValidateMode
    {
        One,
        All
    }


    public class GValidateColumn
    {
        public string FieldName { get; set; } = "";

        public bool CheckNull { get; set; } = false;

        public string ValidateType { get; set; } = "";

        public string CheckMethod { get; set; } = "";

        public bool RemoteMethod { get; set; } = false;

        public string ValidateMessage { get; set; } = "";

        public string RangeFrom { get; set; } = "";

        public string RangeTo { get; set; } = "";

        public string CompareField { get; set; } = "";

        public string CompareMode { get; set; } = "";

        public string ValidateLabelLink { get; set; } = "";

        public string DefaultValue { get; set; } = "";

        public bool CarryOn { get; set; } = false;

        public string Validate { get; set; } = "";

        public string WarningMsg { get; set; } = "";
    }


    [HtmlTargetElement("g-validate")]
    [RestrictChildren("validate-column")]
    public class GValidateTagHelper : TagHelper
    {
        public string Name { get; set; } = "";

        public string BindingObjectId { get; set; } = "";

        public bool CarryOn { get; set; } = false;

        public bool CheckKeyFieldEmpty { get; set; } = false;

        public bool DefaultActive { get; set; } = false;

        public bool ValidActive { get; set; } = true;

        public bool DuplicateCheck { get; set; } = false;

        public DuplicateCheckMode DuplicateCheckMode { get; set; } = DuplicateCheckMode.ByLocal;

        public ValidateMode ValidateMode { get; set; } = ValidateMode.One;

        public bool LeaveValidation { get; set; } = false;

        public string ValidateColor { get; set; } = "#dc2626";

        public string ValidateChar { get; set; } = "*";

        public override int Order => int.MinValue;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var columns = new List<GValidateColumn>();
            context.Items[typeof(GValidateTagHelper)] = columns;

            await output.GetChildContentAsync();

            output.TagName = null;
            output.Content.SetHtmlContent(BuildScriptBlock(this, columns));
        }

        private static string BuildScriptBlock(GValidateTagHelper th, List<GValidateColumn> columns)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = false,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            var payload = new
            {
                name = OrNull(th.Name),
                bindingObjectId = th.BindingObjectId,
                carryOn = th.CarryOn ? (bool?)true : null,
                checkKeyFieldEmpty = th.CheckKeyFieldEmpty ? (bool?)true : null,
                defaultActive = th.DefaultActive ? (bool?)true : null,
                validActive = th.ValidActive,
                duplicateCheck = th.DuplicateCheck ? (bool?)true : null,
                duplicateCheckMode = th.DuplicateCheck ? th.DuplicateCheckMode.ToString() : null,
                validateMode = th.ValidateMode != ValidateMode.One ? th.ValidateMode.ToString() : null,
                leaveValidation = th.LeaveValidation ? (bool?)true : null,
                validateColor = th.ValidateColor != "#dc2626" ? th.ValidateColor : null,
                validateChar = th.ValidateChar != "*" ? th.ValidateChar : null,
                columns = columns.Select(c => new
                {
                    fieldName = c.FieldName,
                    checkNull = c.CheckNull ? (bool?)true : null,
                    remoteMethod = c.RemoteMethod ? (bool?)true : null,
                    carryOn = c.CarryOn ? (bool?)true : null,
                    validateType = OrNull(c.ValidateType),
                    validate = OrNull(c.Validate),
                    checkMethod = OrNull(c.CheckMethod),
                    validateMessage = OrNull(c.ValidateMessage),
                    warningMsg = OrNull(c.WarningMsg),
                    rangeFrom = OrNull(c.RangeFrom),
                    rangeTo = OrNull(c.RangeTo),
                    compareField = OrNull(c.CompareField),
                    compareMode = OrNull(c.CompareMode),
                    validateLabelLink = OrNull(c.ValidateLabelLink),
                    defaultValue = OrNull(c.DefaultValue),
                })
            };

            var json = JsonSerializer.Serialize(payload, options);
            var encoded = System.Net.WebUtility.HtmlEncode(th.BindingObjectId);

            return $"""<script type="application/json" data-g-validate="{encoded}">{json}</script>""";
        }

        private static string? OrNull(string s) =>
            string.IsNullOrWhiteSpace(s) ? null : s;
    }


    [HtmlTargetElement("validate-column", ParentTag = "g-validate")]
    public class GValidateColumnTagHelper : TagHelper
    {
        public string FieldName { get; set; } = "";
        public bool CheckNull { get; set; } = false;
        public string ValidateType { get; set; } = "";
        [HtmlAttributeName("validate")]
        public string Validate { get; set; } = "";
        public string CheckMethod { get; set; } = "";
        public string ValidateMessage { get; set; } = "";
        public string WarningMsg { get; set; } = "";
        public string RangeFrom { get; set; } = "";
        public string RangeTo { get; set; } = "";
        public string CompareField { get; set; } = "";
        public string CompareMode { get; set; } = "";
        public bool RemoteMethod { get; set; } = false;
        public string ValidateLabelLink { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public bool CarryOn { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (context.Items.TryGetValue(typeof(GValidateTagHelper), out var obj)
                && obj is List<GValidateColumn> columns)
            {
                columns.Add(new GValidateColumn
                {
                    FieldName = FieldName,
                    CheckNull = CheckNull,
                    ValidateType = ValidateType,
                    Validate = Validate,
                    CheckMethod = CheckMethod,
                    ValidateMessage = ValidateMessage,
                    WarningMsg = WarningMsg,
                    RangeFrom = RangeFrom,
                    RangeTo = RangeTo,
                    CompareField = CompareField,
                    CompareMode = CompareMode,
                    RemoteMethod = RemoteMethod,
                    ValidateLabelLink = ValidateLabelLink,
                    DefaultValue = DefaultValue,
                    CarryOn = CarryOn,
                });
            }
        }
    }
}

