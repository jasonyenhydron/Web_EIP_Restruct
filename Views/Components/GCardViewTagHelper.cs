using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-cardview")]
    public class GCardViewTagHelper : TagHelper
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Api { get; set; } = "";

        [HtmlAttributeName("source-var")]
        public string DataSourceVar { get; set; } = "";

        public string KeyField { get; set; } = "ID";
        public string CoverField { get; set; } = "";
        public string CoverAltField { get; set; } = "";
        public string TitleField { get; set; } = "";
        public string SubtitleField { get; set; } = "";
        public string DescriptionField { get; set; } = "";
        public string MetaFields { get; set; } = "";   // ex: "Department,Title,Email"
        public string SearchFields { get; set; } = ""; // ex: "First_Name,Last_Name,Email"
        public string SortFields { get; set; } = "";   // ex: "First_Name:姓名,Department:部門"
        public string DefaultSortField { get; set; } = "";
        public string DefaultSortDir { get; set; } = "asc";
        public int PageSize { get; set; } = 8;
        public string CardsPerRow { get; set; } = "auto"; // auto | number
        public int CardMinWidth { get; set; } = 280;
        public bool ShowSearch { get; set; } = true;
        public bool ShowSorting { get; set; } = true;
        public bool ShowPager { get; set; } = true;
        public string EmptyText { get; set; } = "目前無資料";
        public string PrimaryActionText { get; set; } = "";
        public string PrimaryActionIcon { get; set; } = "arrow-right";
        public string SecondaryActionText { get; set; } = "";
        public string SecondaryActionIcon { get; set; } = "message";
        public string OnCardClick { get; set; } = "";
        public string OnPrimaryAction { get; set; } = "";
        public string OnSecondaryAction { get; set; } = "";
        public string Class { get; set; } = "";
        public string ExtraClass { get; set; } = "";
        public string WrapperClass { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId = string.IsNullOrWhiteSpace(Id) ? $"gcv_{Guid.NewGuid():N}" : Id.Trim();
            var defaultWrapperClass = "bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden";
            var finalWrapperClass = string.IsNullOrWhiteSpace(WrapperClass)
                ? TagHelperClassResolver.Resolve(defaultWrapperClass, Class, ExtraClass)
                : WrapperClass.Trim();

            var headerTitle = string.IsNullOrWhiteSpace(Title)
                ? "Card View"
                : WebUtility.HtmlEncode(Title);

            var normalizedCardsPerRow = NormalizeCardsPerRow(CardsPerRow);
            var normalizedSortDir = NormalizeSortDir(DefaultSortDir);
            var safeEmptyText = EmptyText ?? "目前無資料";

            output.TagName = "section";
            output.Attributes.SetAttribute("id", compId);
            output.Attributes.SetAttribute("class", finalWrapperClass);
            output.Attributes.SetAttribute("data-g-cardview", "1");
            output.Attributes.SetAttribute("data-api", Api ?? "");
            output.Attributes.SetAttribute("data-source-var", DataSourceVar ?? "");
            output.Attributes.SetAttribute("data-key-field", KeyField ?? "ID");
            output.Attributes.SetAttribute("data-cover-field", CoverField ?? "");
            output.Attributes.SetAttribute("data-cover-alt-field", CoverAltField ?? "");
            output.Attributes.SetAttribute("data-title-field", TitleField ?? "");
            output.Attributes.SetAttribute("data-subtitle-field", SubtitleField ?? "");
            output.Attributes.SetAttribute("data-description-field", DescriptionField ?? "");
            output.Attributes.SetAttribute("data-meta-fields", MetaFields ?? "");
            output.Attributes.SetAttribute("data-search-fields", SearchFields ?? "");
            output.Attributes.SetAttribute("data-sort-fields", SortFields ?? "");
            output.Attributes.SetAttribute("data-default-sort-field", DefaultSortField ?? "");
            output.Attributes.SetAttribute("data-default-sort-dir", normalizedSortDir);
            output.Attributes.SetAttribute("data-page-size", Math.Max(1, PageSize).ToString());
            output.Attributes.SetAttribute("data-cards-per-row", normalizedCardsPerRow);
            output.Attributes.SetAttribute("data-card-min-width", Math.Max(180, CardMinWidth).ToString());
            output.Attributes.SetAttribute("data-show-search", ShowSearch.ToString().ToLowerInvariant());
            output.Attributes.SetAttribute("data-show-sorting", ShowSorting.ToString().ToLowerInvariant());
            output.Attributes.SetAttribute("data-show-pager", ShowPager.ToString().ToLowerInvariant());
            output.Attributes.SetAttribute("data-empty-text", safeEmptyText);
            output.Attributes.SetAttribute("data-primary-action-text", PrimaryActionText ?? "");
            output.Attributes.SetAttribute("data-primary-action-icon", PrimaryActionIcon ?? "arrow-right");
            output.Attributes.SetAttribute("data-secondary-action-text", SecondaryActionText ?? "");
            output.Attributes.SetAttribute("data-secondary-action-icon", SecondaryActionIcon ?? "message");
            output.Attributes.SetAttribute("data-on-card-click", OnCardClick ?? "");
            output.Attributes.SetAttribute("data-on-primary-action", OnPrimaryAction ?? "");
            output.Attributes.SetAttribute("data-on-secondary-action", OnSecondaryAction ?? "");

            output.Content.SetHtmlContent($@"
                <div class=""px-4 py-3 border-b border-slate-200 bg-slate-100 flex items-center justify-between gap-3 flex-wrap"">
                    <h3 class=""text-sm font-bold text-slate-700 flex items-center gap-2"">
                        <svg class=""w-4 h-4 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M3 7h18M3 12h18M3 17h18""></path></svg>
                        <span>{headerTitle}</span>
                    </h3>
                    <div class=""flex items-center gap-2 flex-wrap"">
                        <div class=""relative {(ShowSearch ? "" : "hidden")}"" data-role=""search-wrap"">
                            <input type=""text"" data-role=""search"" placeholder=""搜尋...""
                                   class=""w-56 pl-8 pr-2 py-1.5 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400"">
                            <svg class=""w-4 h-4 text-slate-400 absolute left-2.5 top-1/2 -translate-y-1/2"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""></path>
                            </svg>
                        </div>
                        <div class=""flex items-center gap-1 {(ShowSorting ? "" : "hidden")}"" data-role=""sort-wrap"">
                            <select data-role=""sort-field"" class=""pl-2 pr-6 py-1.5 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400""></select>
                            <button type=""button"" data-role=""sort-dir"" class=""w-8 h-8 rounded-lg border border-slate-300 bg-white bg-slate-100 text-slate-600"" title=""切換排序方向"">↑</button>
                        </div>
                    </div>
                </div>
                <div class=""p-4"">
                    <div data-role=""loading"" class=""hidden text-sm text-slate-500 py-6 text-center"">載入中...</div>
                    <div data-role=""empty"" class=""hidden text-sm text-slate-400 py-8 text-center"">{WebUtility.HtmlEncode(safeEmptyText)}</div>
                    <div data-role=""cards"" class=""grid gap-4""></div>
                </div>
                <div data-role=""pager-wrap"" class=""px-4 py-2 border-t border-slate-200 bg-slate-100 flex items-center justify-between gap-2 text-xs {(ShowPager ? "" : "hidden")}"">
                    <div class=""text-slate-500"">共 <span data-role=""total-count"" class=""font-semibold text-slate-700"">0</span> 筆</div>
                    <div class=""flex items-center gap-1"">
                        <button type=""button"" data-role=""prev"" class=""px-2 py-1 rounded border border-slate-300 bg-white bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed"">上一頁</button>
                        <span data-role=""page-info"" class=""px-2 text-slate-600"">1 / 1</span>
                        <button type=""button"" data-role=""next"" class=""px-2 py-1 rounded border border-slate-300 bg-white bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed"">下一頁</button>
                    </div>
                </div>
            ");
        }

        private static string NormalizeCardsPerRow(string cardsPerRow)
        {
            var value = (cardsPerRow ?? "").Trim().ToLowerInvariant();
            if (value == "auto") return "auto";
            return int.TryParse(value, out var n) && n > 0 ? n.ToString() : "auto";
        }

        private static string NormalizeSortDir(string sortDir)
        {
            var value = (sortDir ?? "").Trim().ToLowerInvariant();
            return value == "desc" ? "desc" : "asc";
        }
    }
}


