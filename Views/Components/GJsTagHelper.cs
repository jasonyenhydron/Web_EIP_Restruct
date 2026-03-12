using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-js")]
    public class GJsTagHelper : TagHelper
    {
        public string Profile { get; set; } = "none"; // none | popup | main | mis-programs | datagrid | lovinput | grid | form | lov | dashboard | report

        public string Extras { get; set; } = "";

        public bool IncludeAlpine { get; set; } = false;
        public string AlpineVersion { get; set; } = "3.x.x";

        public bool IncludeHtmx { get; set; } = false;
        public string HtmxVersion { get; set; } = "1.9.12";

        public bool IncludeTailwind { get; set; } = false;

        public bool IncludeTheme { get; set; } = false;
        public string ThemeScriptPath { get; set; } = "~/js/theme.js";

        public string LocalVersion { get; set; } = "";

        public bool Defer { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var scripts = new List<string>();
            scripts.AddRange(GetRequiredScripts(Profile));
            scripts.AddRange(SplitScripts(Extras));

            var uniqueScripts = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in scripts)
            {
                var normalized = NormalizeUrl(s);
                if (string.IsNullOrWhiteSpace(normalized)) continue;
                if (seen.Add(normalized)) uniqueScripts.Add(normalized);
            }

            var sb = new StringBuilder();

            if (IncludeAlpine)
            {
                var alpine = $"https://cdn.jsdelivr.net/npm/alpinejs@{AlpineVersion}/dist/cdn.min.js";
                sb.AppendLine($@"<script src=""{alpine}"" defer></script>");
            }

            if (IncludeHtmx)
            {
                var htmx = $"https://unpkg.com/htmx.org@{HtmxVersion}";
                sb.AppendLine($@"<script src=""{htmx}""></script>");
            }

            if (IncludeTailwind)
            {
                sb.AppendLine(@"<script src=""https://cdn.tailwindcss.com""></script>");
            }

            if (IncludeTheme)
            {
                var themeSrc = NormalizeUrl(ThemeScriptPath);
                if (!string.IsNullOrWhiteSpace(themeSrc))
                {
                    sb.AppendLine($@"<script src=""{themeSrc}"" defer></script>");
                }
            }

            foreach (var script in uniqueScripts)
            {
                var src = AppendVersion(script, LocalVersion);
                var deferAttr = Defer ? " defer" : "";
                sb.AppendLine($@"<script src=""{src}""{deferAttr}></script>");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }

        private static IEnumerable<string> GetRequiredScripts(string profile)
        {
            switch ((profile ?? "").Trim().ToLowerInvariant())
            {
                case "dashboard":
                case "report":
                    return new[]
                    {
                        "~/js/g-components.js",
                        "~/js/sidebar.js"
                    };
                case "popup":
                    return new[]
                    {
                        "~/js/g-components.js"
                    };
                case "main":
                    return new[]
                    {
                        "~/js/g-components.js",
                        "~/js/sidebar.js"
                    };
                case "mis-programs":
                    return new[]
                    {
                        "~/js/g-components.js",
                        "~/js/sidebar.js",
                        "~/js/mis_programs.js"
                    };
                case "grid":
                case "form":
                case "lov":
                case "datagrid":
                case "lovinput":
                    return new[]
                    {
                        "~/js/g-components.js"
                    };
                default:
                    return Array.Empty<string>();
            }
        }

        private static IEnumerable<string> SplitScripts(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
            return raw
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            var u = url.Trim();
            if (u.StartsWith("~/", StringComparison.Ordinal)) return "/" + u.Substring(2);
            return u;
        }

        private static string AppendVersion(string url, string version)
        {
            if (string.IsNullOrWhiteSpace(version)) return url;
            return url.Contains('?') ? $"{url}&v={version}" : $"{url}?v={version}";
        }
    }
}

