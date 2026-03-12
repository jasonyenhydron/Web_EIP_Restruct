using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-style")]
    public class GStyleTagHelper : TagHelper
    {
        public string Profile { get; set; } = "none"; // none | layout | popup | mis-programs | datagrid | lovinput | grid | form | lov | dashboard | report

        public string Extras { get; set; } = "";

        public string LocalVersion { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var styles = new List<string>();
            styles.AddRange(GetRequiredStyles(Profile));
            styles.AddRange(SplitStyles(Extras));

            var uniqueStyles = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in styles)
            {
                var normalized = NormalizeUrl(s);
                if (string.IsNullOrWhiteSpace(normalized)) continue;
                if (seen.Add(normalized)) uniqueStyles.Add(normalized);
            }

            var sb = new StringBuilder();
            foreach (var href in uniqueStyles)
            {
                var finalHref = AppendVersion(href, LocalVersion);
                sb.AppendLine($@"<link rel=""stylesheet"" href=""{finalHref}"">");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }

        private static IEnumerable<string> GetRequiredStyles(string profile)
        {
            switch ((profile ?? "").Trim().ToLowerInvariant())
            {
                case "dashboard":
                case "report":
                case "layout":
                case "popup":
                case "grid":
                case "form":
                case "lov":
                case "datagrid":
                case "lovinput":
                    return new[]
                    {
                        "https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap",
                        "~/css/main.css"
                    };
                case "mis-programs":
                    return new[] { "~/css/mis_programs.css" };
                default:
                    return Array.Empty<string>();
            }
        }

        private static IEnumerable<string> SplitStyles(string raw)
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

