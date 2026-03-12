using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-progress-bar")]
    public class GProgressBarTagHelper : TagHelper
    {
        public int    Value     { get; set; } = 0;
        public string Label     { get; set; } = "";
        public bool   ShowLabel { get; set; } = true;
        public string Type      { get; set; } = "primary";  // primary|success|warning|danger
        public bool   Striped   { get; set; } = false;
        public bool   Animated  { get; set; } = false;
        public string Class     { get; set; } = "";
        public string Id        { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var val      = Math.Clamp(Value, 0, 100);
            var compId   = string.IsNullOrEmpty(Id) ? $"gpb_{Guid.NewGuid():N}" : Id;
            var barColor = Type switch
            {
                "success" => "bg-green-500",    // 成功：綠色
                "warning" => "bg-amber-500",    // 警告：橙色
                "danger"  => "bg-red-500",      // 危險：紅色
                _         => "bg-blue-600"      // 預設(primary)：跟隨 --dx-brand
            };
            var stripedCls  = Striped ? @"bg-slate-100:1rem_1rem] bg-slate-100(45deg,rgba(255,255,255,.15)_25%,transparent_25%,transparent_50%,rgba(255,255,255,.15)_50%,rgba(255,255,255,.15)_75%,transparent_75%,transparent)]" : "";
            var animatedCls = Animated ? "animate-[progress_1s_linear_infinite]" : "";
            var label       = !string.IsNullOrEmpty(Label) ? Label : $"{val}%";

            output.TagName = "div";
            output.Attributes.SetAttribute("id", compId);
            output.Attributes.SetAttribute("class", $"w-full {Class}");

            output.Content.SetHtmlContent($@"
                <div class=""w-full bg-slate-100 rounded-full overflow-hidden h-4 relative"">
                    <div class=""{barColor} {stripedCls} {animatedCls} h-full rounded-full transition-all duration-500 ease-out flex items-center justify-center""
                         style=""width:{val}%"">
                        {(ShowLabel && val > 10 ? $@"<span class=""text-white text-xs font-semibold px-1"">{label}</span>" : "")}
                    </div>
                </div>
                {(ShowLabel ? $@"<div class=""text-xs text-slate-500 mt-1 text-right"">{label}</div>" : "")}
            ");
        }
    }
    [HtmlTargetElement("g-tooltip")]
    public class GTooltipTagHelper : TagHelper
    {
        public string Text     { get; set; } = "";
        public string Position { get; set; } = "top";
        public string Class    { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
            var (tooltipCls, arrowCls) = Position?.ToLower() switch
            {
                "bottom" => ("top-full left-1/2 -translate-x-1/2 mt-2", "bottom-full left-1/2 -translate-x-1/2 border-l-transparent border-r-transparent border-t-transparent border-b-slate-700 -mb-2"),
                "left"   => ("right-full top-1/2 -translate-y-1/2 mr-2", "left-full top-1/2 -translate-y-1/2 border-t-transparent border-b-transparent border-r-transparent border-l-slate-700 -ml-2"),
                "right"  => ("left-full top-1/2 -translate-y-1/2 ml-2", "right-full top-1/2 -translate-y-1/2 border-t-transparent border-b-transparent border-l-transparent border-r-slate-700 -mr-2"),
                _        => ("bottom-full left-1/2 -translate-x-1/2 mb-2", "top-full left-1/2 -translate-x-1/2 border-l-transparent border-r-transparent border-b-transparent border-t-slate-700 -mt-2")
            };

            output.TagName = "span";
            output.Attributes.SetAttribute("class", $"relative inline-block group {Class}");
            output.Content.SetHtmlContent($@"
                {content}
                <span class=""absolute {tooltipCls} z-[200] px-2 py-1 text-xs text-white bg-slate-900/60 rounded-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none shadow-lg"">
                    {System.Net.WebUtility.HtmlEncode(Text)}
                </span>
            ");
        }
    }
}

