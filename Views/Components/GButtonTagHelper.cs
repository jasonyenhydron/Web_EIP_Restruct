using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-button")]
    public class GButtonTagHelper : TagHelper
    {
        public string Text { get; set; } = "";
        public string Type { get; set; } = "primary";
        public string Icon { get; set; } = "";
        public string Class { get; set; } = "";
        public string ExtraClass { get; set; } = "";
        public string Onclick { get; set; } = "";
        public string Href { get; set; } = "";
        public string Target { get; set; } = "";
        public string Id { get; set; } = "";
        public string Size { get; set; } = "md";
        public string Title { get; set; } = "";
        public bool Submit { get; set; } = false;
        public bool Disabled { get; set; } = false;

        private const string BaseClass = "inline-flex items-center justify-center rounded-lg px-4 py-2 text-sm font-medium transition-colors";

        private static readonly Dictionary<string, string> TypeClassMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["primary"] = "bg-blue-600 text-white hover:bg-blue-700",
            ["secondary"] = "bg-slate-100 text-slate-700 hover:bg-slate-200",
            ["danger"] = "bg-red-600 text-white hover:bg-red-700",
            ["warning"] = "bg-amber-400 text-white hover:bg-amber-500",
            ["success"] = "bg-green-600 text-white hover:bg-green-700",
            ["info"] = "bg-sky-500 text-white hover:bg-sky-600",
            ["ghost"] = "bg-transparent text-blue-700 hover:text-blue-800 px-2 py-1",
        };

        private static readonly Dictionary<string, string> SizeClassMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["sm"] = "g-btn-sm",
            ["lg"] = "g-btn-lg",
        };

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            bool isLink = !string.IsNullOrEmpty(Href);
            output.TagName = isLink ? "a" : "button";

            if (isLink)
            {
                output.Attributes.SetAttribute("href", Href);
                if (!string.IsNullOrEmpty(Target))
                {
                    output.Attributes.SetAttribute("target", Target);
                }
            }
            else
            {
                output.Attributes.SetAttribute("type", Submit ? "submit" : "button");
                if (Disabled)
                {
                    output.Attributes.SetAttribute("disabled", "disabled");
                }
            }

            if (!string.IsNullOrEmpty(Id))
            {
                output.Attributes.SetAttribute("id", Id);
            }
            if (!string.IsNullOrEmpty(Onclick))
            {
                output.Attributes.SetAttribute("onclick", Onclick);
            }
            if (!string.IsNullOrEmpty(Title))
            {
                output.Attributes.SetAttribute("title", Title);
            }

            var typeClass = TypeClassMap.GetValueOrDefault(Type, TypeClassMap["primary"]);
            var sizeClass = SizeClassMap.GetValueOrDefault(Size, "");
            var disabledClass = Disabled ? "opacity-50 cursor-not-allowed pointer-events-none" : "";

            var defaultClass = string.Join(" ",
                new[] { BaseClass, typeClass, sizeClass, disabledClass }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            var finalClass = TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass);
            output.Attributes.SetAttribute("class", finalClass);

            var iconHtml = string.IsNullOrEmpty(Icon) ? "" : GetIconSvg(Icon);
            var gap = (!string.IsNullOrEmpty(iconHtml) && !string.IsNullOrEmpty(Text)) ? "<span class=\"mr-1\"></span>" : "";
            output.Content.SetHtmlContent($"{iconHtml}{gap}{Text}");
        }

        public static string GetIconSvg(string icon, string cls = "w-4 h-4 shrink-0")
        {
            if (string.IsNullOrEmpty(icon)) return "";

            static string Stroke(string cls, string path) =>
                $"""<svg class="{cls}" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="{path}"/></svg>""";

            static string StrokeMulti(string cls, string[] paths)
            {
                var joined = string.Join("", paths.Select(p =>
                    $"""<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="{p}"/>"""));
                return $"""<svg class="{cls}" fill="none" stroke="currentColor" viewBox="0 0 24 24">{joined}</svg>""";
            }
            return icon switch
            {
                "save" => Stroke(cls, "M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4"),
                "trash" => Stroke(cls, "M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"),
                "edit" => Stroke(cls, "M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"),
                "add" => Stroke(cls, "M12 9v3m0 0v3m0-6h3m-3 0H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z"),
                "search" => Stroke(cls, "M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"),
                "plus" => Stroke(cls, "M12 4v16m8-8H4"),
                "close" => Stroke(cls, "M6 18L18 6M6 6l12 12"),
                "check" => Stroke(cls, "M5 13l4 4L19 7"),
                "refresh" => Stroke(cls, "M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"),
                "upload" => Stroke(cls, "M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"),
                "download" => Stroke(cls, "M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"),
                "excute" => Stroke(cls, "M12 9v3m0 0v3m0-6h3m-3 0H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z"),
                "copy" => Stroke(cls, "M8 11h3v10h2V11h3l-4-4-4 4zM4 19v2h16v-2H4z"),
                "delete" => Stroke(cls, "M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"),
                "settings" => Stroke(cls, "M12 14c1.66 0 3-1.34 3-3s-1.34-3-3-3-3 1.34-3 3 1.34 3 3 3zm1-6h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"),
                "print" => StrokeMulti(cls, new[]
                {
                    "M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"
                }),
                "eye" => StrokeMulti(cls, new[]
                {
                    "M15 12a3 3 0 11-6 0 3 3 0 016 0z",
                    "M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                }),
                "list" => Stroke(cls, "M4 6h16M4 10h16M4 14h16M4 18h16"),
                "play" => $"""<svg class="{cls}" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>""",
                "filter" => Stroke(cls, "M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"),
                "back" => Stroke(cls, "M10 19l-7-7m0 0l7-7m-7 7h18"),
                "forward" => Stroke(cls, "M14 5l7 7m0 0l-7 7m7-7H3"),
                "export" => Stroke(cls, "M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"),
                _ => ""
            };
        }
    }
}

