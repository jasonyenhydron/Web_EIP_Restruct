using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Web_EIP_Restruct.Models.DataForm;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-datagrid")]
    public class GDataGridTagHelper : TagHelper
    {

        private sealed class QueryColumnDefinition
        {
            public string FieldName      { get; set; } = "";
            public string Caption        { get; set; } = "";
            public string Condition      { get; set; } = "=";
            public string AndOr          { get; set; } = "AND";
            public string DataType       { get; set; } = "string";
            public string DefaultMethod  { get; set; } = "";
            public string DefaultValue   { get; set; } = "";
            public string Editor         { get; set; } = "text";
            public string EditorOptions  { get; set; } = "";
            public string Format         { get; set; } = "";
            public bool   IsNvarChar     { get; set; } = false;
            public bool   NewLine        { get; set; } = false;
            public bool   RemoteMethod   { get; set; } = false;
            public int    RowSpan        { get; set; } = 1;
            public int    Span           { get; set; } = 3;
            public string TableName      { get; set; } = "";
            public int    Width          { get; set; } = 0;
            public bool   Api            { get; set; } = false;
            public string LovTitle       { get; set; } = "";
            public string LovApi         { get; set; } = "";
            public string LovColumns     { get; set; } = "";
            public string LovFields      { get; set; } = "";
            public string LovKeyValue    { get; set; } = "";
            public string LovKeyDisplay  { get; set; } = "";
            public string LovDisplayFormat { get; set; } = "";
        }

        private sealed class FormColumnDefinition
        {
            public string   FieldName        { get; set; } = "";
            public string   Caption          { get; set; } = "";
            public string   ColumnType       { get; set; } = "text";
            public int      ColSpan          { get; set; } = 1;
            public bool     AlwaysReadOnly   { get; set; } = false;
            public bool     Required         { get; set; } = false;
            public bool     IsPrimaryKey     { get; set; } = false;
            public bool     Hidden           { get; set; } = false;
            public string   DefaultValue     { get; set; } = "";
            public string   Placeholder      { get; set; } = "";
            public int?     MaxLength        { get; set; }
            public decimal? Min              { get; set; }
            public decimal? Max              { get; set; }
            public string   Options          { get; set; } = "";
            public string   OptionsApi       { get; set; } = "";
            public string   LovTitle         { get; set; } = "";
            public string   LovApi           { get; set; } = "";
            public string   LovColumns       { get; set; } = "";
            public string   LovFields        { get; set; } = "";
            public string   LovKeyValue      { get; set; } = "";
            public string   LovKeyDisplay    { get; set; } = "";
            public string   LovDisplayFormat { get; set; } = "";
        }

        private record ColumnDef(
            string Field,
            string Title,
            string Width,
            string Align,
            string EditorType,
            string EditorOptions,
            string FilterType);


        private const string CssInput      = "w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none";
        private const string CssInputSm    = "w-full px-2 py-1 text-xs border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none";
        private const string CssThBase     = "px-3 py-2.5 text-xs font-bold text-slate-500 uppercase tracking-wider bg-slate-100 border-b-2 border-slate-200";
        private const string CssTdBase     = "px-3 py-2 text-sm text-slate-700 border-b border-slate-100 whitespace-nowrap";
        private const string CssBtnPrimary = "inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors";
        private const string CssBtnSecondary = "inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors";
        private const string CssBtnIcon    = "p-1.5 rounded-md transition-colors";
        private const string CssBtnToolbarCompact = "inline-flex items-center justify-center w-8 h-8 text-slate-600 bg-white border border-slate-300 rounded-md transition-colors";


        public string Id             { get; set; } = "";

        [HtmlAttributeName("api")]
        public string Api            { get; set; } = "";
        public string ApiUrl         { get; set; } = "";
        public string RemoteName     { get; set; } = "";

        [HtmlAttributeName("member-id")]
        public string DataMember     { get; set; } = "";

        public string Columns        { get; set; } = "";
        public int    PageSize       { get; set; } = 20;
        public bool   Striped        { get; set; } = true;
        public bool   ShowRowNum     { get; set; } = false;
        public bool   RowNumbers     { get; set; } = false;
        public string OnRowClick     { get; set; } = "";
        public string OnRowDblClick  { get; set; } = "";
        public string ToolbarHtml    { get; set; } = "";
        public string Class          { get; set; } = "";
        public string ExtraClass     { get; set; } = "";

        [HtmlAttributeName("clean-style")]
        public bool CleanStyle       { get; set; } = false;
        public string EditMode       { get; set; } = "";
        public string IdField        { get; set; } = "ROWID";
        public string IDField        { get; set; } = "";
        public bool   ShowFilter     { get; set; } = false;
        public bool   QueryAutoColumn    { get; set; } = false;
        public bool   TitleFilterEnabled { get; set; } = true;
        public bool   AllowAdd       { get; set; } = true;
        public bool   AllowDelete    { get; set; } = true;
        public bool   AllowUpdate    { get; set; } = true;
        public bool   AlwaysClose    { get; set; } = true;
        public bool   NotInitGrid    { get; set; } = true;
        public bool   AutoApply      { get; set; } = true;
        public string Title          { get; set; } = "";
        public string HelpLink       { get; set; } = "";

        public bool   ColumnsHibeable  { get; set; } = false;
        public bool   ColumnsHideable  { get; set; } = false;

        public bool   Pagination     { get; set; } = true;
        public string PageList       { get; set; } = "10,20,50,100";
        public string QueryMode      { get; set; } = "Panel";
        public string QueryTitle     { get; set; } = "查詢條件";
        public int    QueryLeft      { get; set; } = 0;
        public int    QueryTop       { get; set; } = 0;
        public string QueryColumns   { get; set; } = "";

        [HtmlAttributeName("form-columns")]
        public string FormColumns    { get; set; } = "";

        public bool   MultiSelect    { get; set; } = false;
        public string EditDialogID   { get; set; } = "";
        public bool   TitleSortEnabled   { get; set; } = false;
        public string TitleSortField     { get; set; } = "";
        public string SortableColumns    { get; set; } = "";
        public bool   MultiSortEnabled   { get; set; } = false;
        public bool   BufferView         { get; set; } = false;
        public bool   CheckOnSelect      { get; set; } = true;
        public string CloudReportName    { get; set; } = "";
        public string ReportFileName     { get; set; } = "";
        public bool   DuplicateCheck     { get; set; } = false;
        public bool   EditOnEnter        { get; set; } = false;
        public string MultiSelectGridID  { get; set; } = "";
        public string ParentObjectID     { get; set; } = "";
        public string RelationColumns    { get; set; } = "";
        public bool   RecordLock         { get; set; } = false;
        public string RecordLockMode     { get; set; } = "";

        public string TotalCpation   { get; set; } = "";
        public string TotalCaption   { get; set; } = "";

        public bool   UpdateCommandVisible { get; set; } = true;
        public bool   DeleteCommandVisible { get; set; } = true;
        public bool   ViewCommandVisible   { get; set; } = true;
        public bool   ToolbarViewCommandVisible   { get; set; } = false;
        public bool   ToolbarEditCommandVisible   { get; set; } = false;
        public bool   ToolbarDeleteCommandVisible { get; set; } = false;
        public bool   ToolbarNavCommandVisible    { get; set; } = false;
        public bool   ToolbarCompact             { get; set; } = false;

        public string OnLoadSuccess { get; set; } = "";
        public string OnSelect      { get; set; } = "";
        public string OnInsert      { get; set; } = "";
        public string OnInserted    { get; set; } = "";
        public string OnUpdate      { get; set; } = "";
        public string OnUpdated     { get; set; } = "";
        public string OnDelete      { get; set; } = "";
        public string OnDeleted     { get; set; } = "";
        public string OnDeleting    { get; set; } = "";
        public string OnFilter      { get; set; } = "";
        public string OnView        { get; set; } = "";


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var cfg = new GridConfig(this);

            var cols        = ParseColumns(Columns);
            var queryColumns = ParseQueryColumns(QueryColumns);
            var formColumns  = ParseFormColumns(FormColumns);
            var sortableFieldSet = ParseSortableFields(SortableColumns, cols);

            var theadHtml   = BuildTheadHtml(cfg, cols, sortableFieldSet);
            var tbodyTdHtml = BuildTbodyTdHtml(cfg, cols);
            var actionTdHtml = BuildActionColumnHtml(cfg);

            var queryPanelHtml       = BuildQueryPanelHtml(queryColumns);
            var paginationHtml       = BuildPaginationHtml(cfg);
            var titleHeaderHtml      = BuildTitleHeaderHtml(cfg, cols, sortableFieldSet);
            var generatedFormDialog  = BuildGeneratedFormDialog(cfg.CompId, formColumns);
            var alpineScript         = BuildAlpineScript(cfg, cols, queryColumns, formColumns, sortableFieldSet);

            output.TagName = "div";
            output.Attributes.SetAttribute("id", cfg.CompId);

            var cleanClass  = CleanStyle ? "flex-1 border-0 rounded-none w-full shadow-none" : "";
            var defaultClass = $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden flex flex-col relative min-h-0 w-full {cleanClass}".Trim();
            output.Attributes.SetAttribute("class", TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass));
            output.Attributes.SetAttribute("x-data", $"gDataGrid_{cfg.CompId}()");
            output.Attributes.SetAttribute("x-init", "init()");

            output.Content.SetHtmlContent(
                BuildRootHtml(cfg, theadHtml, tbodyTdHtml, actionTdHtml,
                              queryPanelHtml, paginationHtml, titleHeaderHtml,
                              generatedFormDialog, alpineScript));
        }


        private sealed class GridConfig
        {
            public string CompId             { get; }
            public string FnName             { get; }
            public string ActualApiUrl       { get; }
            public string ActualIdField      { get; }
            public string ActualTotalCaption { get; }
            public bool   ActualShowRowNum   { get; }
            public bool   ActualShowFilter   { get; }
            public bool   ActualTitleFilter  { get; }
            public bool   ActualNotInitGrid  { get; }
            public string ZebraClassExpr     { get; }
            public string RowCursorClass     { get; }
            public string DefaultTitleSortField { get; }

            public GDataGridTagHelper H { get; }

            public GridConfig(GDataGridTagHelper h)
            {
                H = h;
                CompId  = string.IsNullOrEmpty(h.Id) ? $"grid_{Guid.NewGuid():N}" : h.Id;
                FnName  = $"gDataGrid_{CompId}";

                ActualApiUrl = !string.IsNullOrEmpty(h.ApiUrl)   ? h.ApiUrl
                             : !string.IsNullOrEmpty(h.Api)      ? h.Api
                             : !string.IsNullOrEmpty(h.RemoteName)
                               ? $"/{h.RemoteName}/select{(string.IsNullOrEmpty(h.DataMember) ? "" : $"?DataMember={h.DataMember}")}".Replace("//", "/")
                             : "";

                ActualIdField      = !string.IsNullOrEmpty(h.IDField) ? h.IDField : h.IdField;
                ActualTotalCaption = !string.IsNullOrEmpty(h.TotalCaption) ? h.TotalCaption : h.TotalCpation;
                ActualShowRowNum   = h.RowNumbers || h.ShowRowNum;
                ActualShowFilter   = h.QueryAutoColumn || h.ShowFilter;
                ActualTitleFilter  = ActualShowFilter && h.TitleFilterEnabled;
                ActualNotInitGrid  = h.AlwaysClose || h.NotInitGrid;
                ZebraClassExpr     = h.Striped ? "(rowIdx % 2 === 0 ? 'bg-white' : 'bg-slate-50')" : "''";
                RowCursorClass     = (!string.IsNullOrEmpty(h.OnRowClick) || !string.IsNullOrEmpty(h.OnRowDblClick))
                                     ? "cursor-pointer" : "";

                var cols = GDataGridTagHelper.ParseColumns(h.Columns);
                var sortable = GDataGridTagHelper.ParseSortableFields(h.SortableColumns, cols);
                var sortableList = cols.Where(c => sortable.Contains(c.Field)).Select(c => c.Field).Distinct().ToList();
                DefaultTitleSortField = !string.IsNullOrEmpty(h.TitleSortField)
                    ? h.TitleSortField
                    : (sortableList.FirstOrDefault() ?? cols.FirstOrDefault()?.Field ?? "");
            }

            public bool HasEditActions =>
                H.ViewCommandVisible ||
                (H.AllowDelete && H.DeleteCommandVisible) ||
                (H.AllowUpdate && (H.UpdateCommandVisible || string.Equals(H.EditMode, "row", StringComparison.OrdinalIgnoreCase)));
        }


        private static string BuildRootHtml(
            GridConfig cfg,
            string theadHtml, string tbodyTdHtml, string actionTdHtml,
            string queryPanelHtml, string paginationHtml, string titleHeaderHtml,
            string generatedFormDialog, string alpineScript)
        {
            var h = cfg.H;
            var toolbarCommands = BuildToolbarCommandsHtml(cfg);

            var totalBadge = !h.Pagination
                ? $@"<span class=""text-xs text-slate-400"">
                        {(string.IsNullOrEmpty(cfg.ActualTotalCaption) ? "共 " : cfg.ActualTotalCaption)}
                        <span class=""font-bold text-slate-600"" x-text=""rows.length""></span> 筆
                     </span>"
                : "";

            return $@"
{queryPanelHtml}

<!-- 工具列 -->
<div class=""g-grid-toolbar flex items-center justify-between gap-3 px-4 py-2.5 border-b border-slate-200 bg-slate-100 shrink-0 flex-wrap"">
    <div class=""flex items-center gap-2"">
        {h.ToolbarHtml}
        {toolbarCommands}
    </div>
    {totalBadge}
</div>

{titleHeaderHtml}

<!-- Header Filter 彈窗 -->
{BuildHeaderFilterPopupHtml(cfg)}

<!-- 資料表格 -->
<div class=""g-grid-body overflow-x-auto overflow-y-auto flex-1 min-h-0""
     style=""min-height:120px; max-height: var(--g-grid-body-max-height, 52vh); scrollbar-gutter: stable both-edges;"">

    <!-- Loading 狀態：保留既有資料，只顯示輕量提示 -->
    <div x-show=""loading"" x-cloak
         class=""pointer-events-none absolute inset-x-0 top-0 z-20"">
        <div class=""h-0.5 w-full overflow-hidden bg-transparent"">
            <div class=""h-full w-full animate-pulse bg-blue-600/80 rounded-full""></div>
        </div>
        <div class=""absolute top-2 right-3 inline-flex items-center gap-2 rounded-full border border-blue-200 bg-white/90 px-3 py-1 text-xs font-semibold text-blue-700 shadow-sm"">
            <svg class=""h-3.5 w-3.5 animate-spin"" fill=""none"" viewBox=""0 0 24 24"">
                <circle class=""opacity-25"" cx=""12"" cy=""12"" r=""10"" stroke=""currentColor"" stroke-width=""4""/>
                <path class=""opacity-75"" fill=""currentColor"" d=""M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z""/>
            </svg>
            <span>查詢中</span>
        </div>
    </div>

    <!-- 空資料狀態 -->
    <div x-show=""!loading && rows.length === 0""
         class=""flex flex-col items-center justify-center py-16 text-slate-400 gap-2"">
        <svg class=""w-12 h-12 text-slate-200"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""1.5""
                  d=""M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z""/>
        </svg>
        <p class=""text-sm font-medium"">目前無資料</p>
        <p class=""text-xs text-slate-300"">請嘗試調整查詢條件</p>
    </div>

    <!-- 表格本體 -->
    <table x-show=""rows.length > 0"" class=""min-w-full border-collapse"">
        <thead class=""sticky top-0 z-10"">
            {theadHtml}
        </thead>
        <tbody>
            <template x-for=""(row, rowIdx) in pagedRows"" :key=""rowIdx"">
                <tr class=""group transition-colors {cfg.RowCursorClass}""
                    :class=""[{cfg.ZebraClassExpr}, 'hover:bg-blue-50', selectedRow === row ? 'bg-blue-50 outline outline-1 outline-blue-400' : '']""
                    @dblclick=""handleRowDblClick(row)""
                    @click=""handleRowClick(row)"">
                    {tbodyTdHtml}
                    {actionTdHtml}
                </tr>
            </template>
        </tbody>
    </table>
</div>

{paginationHtml}
{generatedFormDialog}
{alpineScript}";
        }

        private static string BuildToolbarCommandsHtml(GridConfig cfg)
        {
            var h = cfg.H;
            var sb = new StringBuilder();
            var compact = h.ToolbarCompact;

            string BuildButton(string title, string click, string disabledExpr, string activeClass, string iconSvg, string text)
            {
                if (compact)
                {
                    return $@"<button type=""button"" @click=""{click}"" title=""{title}"" :disabled=""{disabledExpr}""
                          :class=""{disabledExpr} ? 'opacity-40 cursor-not-allowed' : '{activeClass}'""
                          class=""{CssBtnToolbarCompact}"">
                        {iconSvg}
                    </button>";
                }

                return $@"<button type=""button"" @click=""{click}"" title=""{title}"" :disabled=""{disabledExpr}""
                          :class=""{disabledExpr} ? 'opacity-40 cursor-not-allowed' : '{activeClass}'""
                          class=""{CssBtnSecondary}"">
                        {iconSvg}
                        {text}
                    </button>";
            }

            void AppendSeparator()
            {
                if (sb.Length == 0) return;
                sb.Append(@"<span class=""mx-1 h-6 w-px bg-slate-200""></span>");
            }

            if (h.ToolbarNavCommandVisible)
            {
                sb.Append(BuildButton("首筆", "selectFirstRow()", "!canSelectFirst", "hover:bg-slate-50",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M11 19l-7-7 7-7m9 14l-7-7 7-7""/></svg>", "首筆"));
                sb.Append(BuildButton("上筆", "selectPrevRow()", "!canSelectPrev", "hover:bg-slate-50",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 19l-7-7 7-7""/></svg>", "上筆"));
                sb.Append(BuildButton("下筆", "selectNextRow()", "!canSelectNext", "hover:bg-slate-50",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 5l7 7-7 7""/></svg>", "下筆"));
                sb.Append(BuildButton("末筆", "selectLastRow()", "!canSelectLast", "hover:bg-slate-50",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M13 5l7 7-7 7M4 5l7 7-7 7""/></svg>", "末筆"));
            }

            if (h.AllowAdd && !string.IsNullOrEmpty(h.EditMode))
            {
                AppendSeparator();
                sb.Append(BuildButton("新增", "addRow()", "false", "hover:bg-slate-50",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 4v16m8-8H4""/></svg>", "新增"));
            }

            if (h.ToolbarViewCommandVisible)
            {
                sb.Append(BuildButton("檢視", "onViewSelected()", "!hasSelection", "hover:bg-slate-50",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 12a3 3 0 11-6 0 3 3 0 016 0""/>
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z""/>
                        </svg>", "檢視"));
            }

            if (h.ToolbarEditCommandVisible)
            {
                sb.Append(BuildButton("編輯", "editSelectedRow()", "!hasSelection", "hover:bg-slate-50",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z""/>
                        </svg>", "編輯"));
            }

            if (h.ToolbarDeleteCommandVisible)
            {
                sb.Append(BuildButton("刪除", "deleteSelectedRow()", "!hasSelection", "hover:bg-rose-50 hover:text-rose-600",
                    @"<svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16""/>
                        </svg>", "刪除"));
            }

            AppendSeparator();
            sb.Append(BuildButton("重整", "fetchData()", "false", "hover:bg-slate-50",
                @"<svg class=""w-4 h-4"" :class=""loading ? 'animate-spin' : ''"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                    <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15""/>
                </svg>", "重整"));

            return sb.ToString();
        }

        private static string BuildTheadHtml(GridConfig cfg, List<ColumnDef> cols, HashSet<string> sortableFieldSet)
        {
            var thHtml     = new StringBuilder();
            var filterHtml = new StringBuilder();

            if (cfg.ActualShowRowNum)
            {
                thHtml.Append($@"<th class=""{CssThBase} text-center w-10 shrink-0"">#</th>");
                if (cfg.ActualShowFilter)
                    filterHtml.Append($@"<th class=""{CssThBase}""></th>");
            }

            if (cfg.H.MultiSelect)
            {
                thHtml.Append($@"<th class=""{CssThBase} text-center w-10 shrink-0"">
                    <input type=""checkbox"" @change=""toggleAll($event.target.checked)""
                           class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 cursor-pointer"">
                </th>");
                if (cfg.ActualShowFilter)
                    filterHtml.Append($@"<th class=""{CssThBase}""></th>");
            }

            foreach (var col in cols)
            {
                thHtml.Append(BuildColumnHeader(cfg, col, sortableFieldSet));
                if (cfg.ActualShowFilter)
                    filterHtml.Append(BuildColumnFilter(cfg, col));
            }

            if (cfg.HasEditActions)
            {
                thHtml.Append($@"<th class=""{CssThBase} text-center w-24 shrink-0 sticky right-0"">操作</th>");
                if (cfg.ActualShowFilter)
                {
                    filterHtml.Append($@"<th class=""{CssThBase} sticky right-0 text-center"">
                        <button type=""button"" @click=""applyFilter()"" title=""套用欄位篩選""
                                class=""inline-flex items-center justify-center w-7 h-7 rounded-lg border border-blue-200 text-blue-600 bg-white transition-colors hover:bg-blue-50"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <use href=""#icon-filter""></use>
                            </svg>
                        </button>
                    </th>");
                }
            }

            var sb = new StringBuilder();
            sb.Append($"<tr>{thHtml}</tr>");
            if (cfg.ActualShowFilter)
                sb.Append($@"<tr class=""bg-slate-100"">{filterHtml}</tr>");

            return sb.ToString();
        }

        private static string BuildColumnHeader(GridConfig cfg, ColumnDef col, HashSet<string> sortableFieldSet)
        {
            var wStyle    = string.IsNullOrEmpty(col.Width) ? "" : $"width:{col.Width}px;min-width:{col.Width}px;";
            var thAlign   = col.Align is "center" or "right" ? $"text-{col.Align}" : "text-left";
            var isSortable = sortableFieldSet.Contains(col.Field);

            var filterIconBtn = cfg.ActualTitleFilter
                ? $@"<button type=""button"" @click.stop=""openHeaderFilter('{col.Field}', $event)"" title=""欄位篩選""
                           class=""ml-1 inline-flex items-center justify-center w-5 h-5 rounded bg-slate-200 text-blue-600 hover:bg-blue-100 transition-colors"">
                        <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <use href=""#icon-filter""></use>
                        </svg>
                    </button>"
                : "";

            var sortIconBtns = $@"
                <span x-show=""isSortableField('{col.Field}')"" class=""inline-flex items-center rounded border border-slate-200 bg-white overflow-hidden"">
                    <button type=""button"" @click.stop=""setSort('{col.Field}', 'asc')"" title=""升冪""
                            :class=""isSorted('{col.Field}', 'asc') ? 'text-blue-600 bg-blue-50' : 'text-slate-400 hover:text-slate-600'""
                            class=""w-4 h-4 inline-flex items-center justify-center transition-colors"">
                        <svg class=""w-3 h-3"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 15l7-7 7 7""/>
                        </svg>
                    </button>
                    <button type=""button"" @click.stop=""setSort('{col.Field}', 'desc')"" title=""降冪""
                            :class=""isSorted('{col.Field}', 'desc') ? 'text-blue-600 bg-blue-50' : 'text-slate-400 hover:text-slate-600'""
                            class=""w-4 h-4 inline-flex items-center justify-center border-l border-slate-200 transition-colors"">
                        <svg class=""w-3 h-3"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 9l-7 7-7-7""/>
                        </svg>
                    </button>
                </span>";

            var sortableClass = isSortable ? "cursor-pointer" : "cursor-default";

            return $@"<th @dblclick=""toggleSort('{col.Field}')"" style=""{wStyle}""
                    class=""{CssThBase} {thAlign} {sortableClass} transition-colors select-none whitespace-nowrap group"">
                <span class=""inline-flex items-center gap-1"">
                    {col.Title}
                    <span x-show=""multiSortEnabled && getSortOrder('{col.Field}') > 0""
                          x-text=""`#${{getSortOrder('{col.Field}')}}`""
                          class=""text-[10px] text-slate-600 font-semibold""></span>
                    {sortIconBtns}
                    {filterIconBtn}
                </span>
            </th>";
        }

        private static string BuildColumnFilter(GridConfig cfg, ColumnDef col)
        {
            var sb = new StringBuilder();
            sb.Append($@"<th class=""px-2 py-1.5 bg-slate-100 border-b border-slate-200"">");

            switch (col.FilterType)
            {
                case "text":
                    sb.Append($@"<input type=""text"" x-model=""filters['{col.Field}']""
                               @keydown.enter=""applyFilter()""
                               class=""{CssInputSm}"">");
                    break;

                case "select" when !string.IsNullOrEmpty(col.EditorOptions):
                    sb.Append($@"<select x-model=""filters['{col.Field}']"" @change=""applyFilter()""
                               class=""{CssInputSm}"">
                               <option value=""""></option>");
                    foreach (var opt in col.EditorOptions.Split(';'))
                    {
                        var kv = opt.Split('=');
                        if (kv.Length == 2)
                            sb.Append($@"<option value=""{kv[0]}"">{kv[1]}</option>");
                    }
                    sb.Append("</select>");
                    break;

                case "lov":
                    sb.Append($@"<div class=""relative flex items-center"">
                        <input type=""text"" x-model=""filters['{col.Field}']""
                               @keydown.enter=""applyFilter()""
                               class=""w-full pl-2 pr-7 py-1 text-xs border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">
                        <button type=""button"" @click=""$dispatch('open-lov', '{col.Field}')""
                                class=""absolute right-1.5 text-slate-400 hover:text-blue-600 transition-colors"">
                            <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                                      d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/>
                            </svg>
                        </button>
                    </div>");
                    break;
            }

            sb.Append("</th>");
            return sb.ToString();
        }

        private static string BuildTbodyTdHtml(GridConfig cfg, List<ColumnDef> cols)
        {
            var sb = new StringBuilder();

            if (cfg.ActualShowRowNum)
                sb.Append($@"<td class=""{CssTdBase} text-center text-xs text-slate-400""
                              x-text=""(currentPage-1)*pageSize+rowIdx+1""></td>");

            if (cfg.H.MultiSelect)
                sb.Append($@"<td class=""{CssTdBase} text-center"" @click.stop>
                    <input type=""checkbox"" :value=""row['{cfg.ActualIdField}']"" x-model=""selectedIds""
                           @change=""onSelectRow(row, $event.target.checked)""
                           class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 cursor-pointer"">
                </td>");

            foreach (var col in cols)
            {
                var tdAlign = col.Align is "center" or "right" ? $"text-{col.Align}" : "text-left";
                sb.Append($@"<td class=""{CssTdBase} {tdAlign}"">");
                sb.Append(BuildCellContent(cfg, col));
                sb.Append("</td>");
            }

            return sb.ToString();
        }

        private static string BuildCellContent(GridConfig cfg, ColumnDef col)
        {
            if (cfg.H.EditMode != "row" || col.EditorType is not ("text" or "select"))
                return $@"<span x-text=""row['{col.Field}'] ?? ''""></span>";

            var sb = new StringBuilder();
            sb.Append($@"<span x-show=""editingId !== row['{cfg.ActualIdField}']""
                              x-text=""row['{col.Field}'] ?? ''""></span>");

            if (col.EditorType == "text")
            {
                sb.Append($@"<input x-cloak x-show=""editingId === row['{cfg.ActualIdField}']""
                                  type=""text"" x-model=""editRowData['{col.Field}']""
                                  class=""{CssInputSm}"">");
            }
            else if (col.EditorType == "select" && !string.IsNullOrEmpty(col.EditorOptions))
            {
                sb.Append($@"<select x-cloak x-show=""editingId === row['{cfg.ActualIdField}']""
                                   x-model=""editRowData['{col.Field}']""
                                   class=""{CssInputSm}"">");
                foreach (var opt in col.EditorOptions.Split(';'))
                {
                    var kv = opt.Split('=');
                    if (kv.Length == 2)
                        sb.Append($@"<option value=""{kv[0]}"">{kv[1]}</option>");
                }
                sb.Append("</select>");
            }

            return sb.ToString();
        }

        private static string BuildActionColumnHtml(GridConfig cfg)
        {
            if (!cfg.HasEditActions) return "";

            var h = cfg.H;

            var btnView = h.ViewCommandVisible
                ? $@"<button type=""button"" @click.stop=""onView(row)""
                          class=""{CssBtnIcon} text-slate-400 hover:text-blue-500"" title=""檢視"">
                        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                                  d=""M15 12a3 3 0 11-6 0 3 3 0 016 0""/>
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                                  d=""M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z""/>
                        </svg>
                    </button>"
                : "";

            var btnEdit = (h.AllowUpdate && h.UpdateCommandVisible)
                ? $@"<button type=""button""
                          @click.stop=""'{h.EditMode}' === 'row' ? startEdit(row) : editRow(row)""
                          class=""{CssBtnIcon} text-slate-400 hover:text-amber-500"" title=""編輯"">
                        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                                  d=""M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z""/>
                        </svg>
                    </button>"
                : "";

            var btnDelete = (h.AllowDelete && h.DeleteCommandVisible)
                ? $@"<button type=""button"" @click.stop=""deleteRow(row)""
                          class=""{CssBtnIcon} text-slate-400 hover:text-red-500"" title=""刪除"">
                        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                                  d=""M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16""/>
                        </svg>
                    </button>"
                : "";

            string innerContent;
            if (h.EditMode == "row")
            {
                innerContent = $@"
                    <template x-if=""editingId !== row['{cfg.ActualIdField}']"">
                        <div class=""flex gap-1"">{btnView}{btnEdit}{btnDelete}</div>
                    </template>
                    <template x-if=""editingId === row['{cfg.ActualIdField}']"">
                        <div class=""flex gap-1"">
                            <button type=""button"" @click.stop=""confirmSave()""
                                    class=""{CssBtnIcon} text-slate-400 hover:text-emerald-500"" title=""儲存"">
                                <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                    <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""/>
                                </svg>
                            </button>
                            <button type=""button"" @click.stop=""cancelEdit()""
                                    class=""{CssBtnIcon} text-slate-400 hover:text-slate-600"" title=""取消"">
                                <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                    <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>
                                </svg>
                            </button>
                        </div>
                    </template>";
            }
            else
            {
                innerContent = $@"<div class=""flex gap-1"">{btnView}{btnEdit}{btnDelete}</div>";
            }

            return $@"<td class=""{CssTdBase} text-center sticky right-0 transition-colors"">
                <div class=""flex items-center justify-center gap-1"">
                    {innerContent}
                </div>
            </td>";
        }

        private static string BuildHeaderFilterPopupHtml(GridConfig cfg)
        {
            return $@"
<div x-show=""headerFilter.open""
     x-transition
     @click.away=""cancelHeaderFilter()""
     class=""absolute z-40 bg-white border border-slate-200 rounded-xl shadow-xl w-64""
     :style=""`left:${{headerFilter.x}}px; top:${{headerFilter.y}}px;`""
     style=""display:none;"">

    <!-- 標題列 -->
    <div class=""px-3 py-2 border-b border-slate-100 flex items-center justify-between"">
        <span class=""text-sm font-semibold text-slate-700"" x-text=""headerFilter.title || '篩選'""></span>
        <button type=""button"" @click=""cancelHeaderFilter()""
                class=""{CssBtnIcon} text-slate-400 hover:text-slate-600"">
            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>
            </svg>
        </button>
    </div>

    <!-- 日期範圍模式 -->
    <template x-if=""headerFilter.mode === 'datetime'"">
        <div class=""px-3 py-2 border-b border-slate-100 space-y-2"">
            <div>
                <label class=""block text-xs text-slate-500 mb-1"">從</label>
                <input type=""datetime-local"" x-model=""headerFilter.dateFrom"" class=""{CssInputSm}"">
            </div>
            <div>
                <label class=""block text-xs text-slate-500 mb-1"">到</label>
                <input type=""datetime-local"" x-model=""headerFilter.dateTo"" class=""{CssInputSm}"">
            </div>
        </div>
    </template>

    <!-- 清單模式 -->
    <template x-if=""headerFilter.mode !== 'datetime'"">
        <div class=""px-3 pt-2 pb-1.5 border-b border-slate-100"">
            <div class=""relative mb-2"">
                <input type=""text"" x-model=""headerFilter.search"" placeholder=""搜尋…""
                       class=""w-full pl-8 pr-2 py-1.5 text-sm border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">
                <svg class=""w-4 h-4 text-slate-400 absolute left-2.5 top-1/2 -translate-y-1/2"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                    <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/>
                </svg>
            </div>
            <label class=""inline-flex items-center gap-2 text-sm text-slate-700 cursor-pointer"">
                <input type=""checkbox""
                       :checked=""isHeaderFilterAllSelected()""
                       @change=""toggleHeaderFilterAll($event.target.checked)""
                       class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500"">
                <span>全選</span>
            </label>
        </div>
    </template>

    <!-- 選項列表 -->
    <div x-show=""headerFilter.mode !== 'datetime'"" class=""max-h-56 overflow-auto px-3 py-2 space-y-1"">
        <template x-for=""opt in getHeaderFilterVisibleOptions()"" :key=""opt"">
            <label class=""flex items-center gap-2 text-sm text-slate-700 cursor-pointer hover:text-blue-600"">
                <input type=""checkbox"" :value=""opt"" x-model=""headerFilter.selected""
                       class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500"">
                <span x-text=""opt""></span>
            </label>
        </template>
    </div>

    <!-- 按鈕列 -->
    <div class=""px-3 py-2 border-t border-slate-100 flex justify-between items-center gap-2"">
        <button type=""button"" @click=""headerFilter.selected = [...headerFilter.options]; confirmHeaderFilter()""
                class=""text-xs text-slate-400 hover:text-slate-600 transition-colors"" title=""清除此欄篩選"">
            全部顯示
        </button>
        <div class=""flex gap-2"">
            <button type=""button"" @click=""cancelHeaderFilter()""
                    class=""{CssBtnSecondary}"">取消</button>
            <button type=""button"" @click=""confirmHeaderFilter()""
                    class=""{CssBtnPrimary}"">套用</button>
        </div>
    </div>
</div>";
        }

        private static string BuildTitleHeaderHtml(GridConfig cfg, List<ColumnDef> cols, HashSet<string> sortableFieldSet)
        {
            if (string.IsNullOrEmpty(cfg.H.Title)) return "";

            var helpIcon = !string.IsNullOrEmpty(cfg.H.HelpLink)
                ? $@"<a href=""{cfg.H.HelpLink}"" target=""_blank""
                      class=""text-slate-400 hover:text-blue-500 transition-colors"" title=""說明"">
                        <svg class=""w-5 h-5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                                  d=""M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""/>
                        </svg>
                   </a>"
                : "";

            var titleSortButtons = cfg.H.TitleSortEnabled
                ? $@"<div class=""inline-flex items-center rounded-lg border border-slate-200 bg-white overflow-hidden"">
                        <button type=""button"" @click=""setTitleSort('asc')""
                                :class=""(sortKey === titleSortField && sortDir === 'asc') ? 'text-blue-600 bg-blue-50' : 'text-slate-500'""
                                class=""w-8 h-8 inline-flex items-center justify-center transition-colors"" title=""升冪"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 15l7-7 7 7""/>
                            </svg>
                        </button>
                        <button type=""button"" @click=""setTitleSort('desc')""
                                :class=""(sortKey === titleSortField && sortDir === 'desc') ? 'text-blue-600 bg-blue-50' : 'text-slate-500'""
                                class=""w-8 h-8 inline-flex items-center justify-center border-l border-slate-200 transition-colors"" title=""降冪"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 9l-7 7-7-7""/>
                            </svg>
                        </button>
                    </div>"
                : "";

            return $@"<div class=""px-4 py-3 bg-slate-100 border-b border-slate-200 flex justify-between items-center"">
                <h3 class=""font-bold text-slate-700 text-sm flex items-center gap-2"">
                    <svg class=""w-4 h-4 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 6h16M4 10h16M4 14h16M4 18h16""/>
                    </svg>
                    {cfg.H.Title}
                </h3>
                <div class=""flex items-center gap-2"">
                    {titleSortButtons}
                    {helpIcon}
                </div>
            </div>";
        }

        private static string BuildPaginationHtml(GridConfig cfg)
        {
            if (!cfg.H.Pagination) return "";

            var pageOpts = new StringBuilder();
            foreach (var p in cfg.H.PageList.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var pt = p.Trim();
                pageOpts.Append($@"<option value=""{pt}"">{pt} 筆/頁</option>");
            }

            var caption = string.IsNullOrEmpty(cfg.ActualTotalCaption) ? "共 " : cfg.ActualTotalCaption;

            return $@"
<div class=""flex flex-wrap items-center justify-between gap-3 px-4 py-2 bg-slate-100 border-t border-slate-200 text-sm shrink-0 select-none"">
    <div class=""flex items-center gap-2"">
        <select x-model=""pageSize"" @change=""currentPage = 1""
                class=""pl-2 pr-6 py-1 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400 cursor-pointer"">
            {pageOpts}
        </select>
    </div>
    <div class=""flex items-center gap-1"">
        <span class=""text-xs text-slate-500 whitespace-nowrap pr-1"">
            {caption}<span class=""font-bold text-slate-700"" x-text=""rows.length""></span> 筆
        </span>
        <button type=""button"" @click=""prevPage()"" :disabled=""currentPage <= 1""
                :class=""currentPage <= 1 ? 'opacity-40 cursor-not-allowed' : 'hover:bg-slate-200'""
                class=""w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors"">
            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 19l-7-7 7-7""/>
            </svg>
        </button>
        <div class=""flex items-center gap-1.5 px-2"">
            <input type=""number"" min=""1"" :max=""totalPages"" :value=""currentPage""
                   @change=""jumpPage($event.target.value)""
                   class=""w-14 text-center text-xs border border-slate-300 rounded-lg py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-400"">
            <span class=""text-slate-400 text-xs"">/</span>
            <span x-text=""totalPages"" class=""text-xs font-bold text-slate-700""></span>
            <span class=""text-slate-400 text-xs"">頁</span>
        </div>
        <button type=""button"" @click=""nextPage()"" :disabled=""currentPage >= totalPages""
                :class=""currentPage >= totalPages ? 'opacity-40 cursor-not-allowed' : 'hover:bg-slate-200'""
                class=""w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors"">
            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 5l7 7-7 7""/>
            </svg>
        </button>
    </div>
</div>";
        }


        private static string BuildAlpineScript(
            GridConfig cfg,
            List<ColumnDef> cols,
            List<QueryColumnDefinition> queryColumns,
            List<FormColumnDefinition> formColumns,
            HashSet<string> sortableFieldSet)
        {
            var h = cfg.H;
            var sortableList = cols.Where(c => sortableFieldSet.Contains(c.Field)).Select(c => c.Field).Distinct().ToList();
            var sortableJs   = string.Join(",", sortableList.Select(f => $"'{f}'"));

            var queryColumnsJson = Serialize(queryColumns);
            var formColumnsJson  = Serialize(formColumns);

            string CallbackJs(string fn, string args = "") =>
                string.IsNullOrEmpty(fn) ? "" : $"window['{fn}'] && window['{fn}']({args});";

            return $@"
<script>
if (!window.gDataGridRuntime) {{
    window.gDataGridRuntime = {{
        showToast(message, type = 'success') {{
            if (typeof gToast === 'function') {{
                gToast(message, type);
                return;
            }}
            const logger = type === 'error' ? console.error : console.log;
            logger('[GDataGridRuntime]', message);
        }},
        toDateInput(v) {{
            if (!v) return '';
            const s = String(v);
            return s.length >= 10 ? s.slice(0, 10) : '';
        }},
        cloneData(data, fallback = null) {{
            if (data === null || data === undefined) return fallback;
            try {{
                return JSON.parse(JSON.stringify(data));
            }} catch {{
                return data;
            }}
        }},
        async executeGridQuery(gridId, fallbackParams = {{}}) {{
            const grid = document.getElementById(gridId)?.gDataGrid;
            if (!grid) return;
            if (typeof grid.executeQuery === 'function') {{
                await grid.executeQuery();
                return;
            }}
            if (typeof grid.fetchData === 'function') {{
                await grid.fetchData(fallbackParams || {{}});
            }}
        }},
        registerAppCallbacks(app, options = {{}}) {{
            if (!app) return;

            const appKey = String(options.appKey || '').trim();
            if (appKey) {{
                window[appKey] = app;
            }}

            const callbacks = options.callbacks || {{}};
            Object.entries(callbacks).forEach(([name, handler]) => {{
                const callbackName = String(name || '').trim();
                const methodName = typeof handler === 'string'
                    ? handler.trim()
                    : '';

                if (!callbackName || !methodName) return;
                window[callbackName] = (...args) => {{
                    const fn = app?.[methodName];
                    if (typeof fn === 'function') {{
                        return fn.apply(app, args);
                    }}
                }};
            }});
        }},
        ensureHtmxBridge() {{
            let form = document.getElementById('gDataGridHtmxForm');
            let sink = document.getElementById('gDataGridHtmxSink');
            if (!form) {{
                form = document.createElement('form');
                form.id = 'gDataGridHtmxForm';
                form.className = 'hidden';
                form.setAttribute('hx-ext', 'json-enc');
                form.setAttribute('hx-swap', 'none');
                document.body.appendChild(form);
            }}
            if (!sink) {{
                sink = document.createElement('div');
                sink.id = 'gDataGridHtmxSink';
                sink.className = 'hidden';
                sink.setAttribute('aria-hidden', 'true');
                document.body.appendChild(sink);
            }}
            form.setAttribute('hx-target', '#gDataGridHtmxSink');
            return {{ form, sink }};
        }},
        async requestJson(method, url, payload) {{
            if (!window.htmx) {{
                const res = await fetch(url, {{
                    method: method || 'POST',
                    headers: {{ 'Content-Type': 'application/json' }},
                    body: payload ? JSON.stringify(payload) : undefined
                }});
                let json = null;
                try {{ json = await res.json(); }} catch {{ json = null; }}
                return {{ ok: res.ok, status: res.status, json }};
            }}

            const bridge = this.ensureHtmxBridge();
            const form = bridge.form;

            ['hx-get', 'hx-post', 'hx-put', 'hx-delete', 'hx-patch'].forEach((a) => form.removeAttribute(a));
            form.setAttribute(`hx-${{String(method || 'POST').toLowerCase()}}`, url);
            form.innerHTML = '';

            const data = payload || {{}};
            Object.entries(data).forEach(([k, v]) => {{
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = k;
                input.value = v == null ? '' : String(v);
                form.appendChild(input);
            }});

            htmx.process(form);

            return await new Promise((resolve, reject) => {{
                const onAfter = (evt) => {{
                    if (evt.detail.elt !== form) return;
                    cleanup();
                    const xhr = evt.detail.xhr;
                    let json = null;
                    try {{ json = JSON.parse(xhr.responseText || '{{}}'); }} catch {{ json = null; }}
                    resolve({{ ok: xhr.status >= 200 && xhr.status < 300, status: xhr.status, json }});
                }};

                const onError = (evt) => {{
                    if (evt.detail.elt !== form) return;
                    cleanup();
                    reject(new Error(evt.detail?.error || 'HTMX request failed'));
                }};

                const cleanup = () => {{
                    document.body.removeEventListener('htmx:afterRequest', onAfter);
                    document.body.removeEventListener('htmx:sendError', onError);
                    document.body.removeEventListener('htmx:responseError', onError);
                }};

                document.body.addEventListener('htmx:afterRequest', onAfter);
                document.body.addEventListener('htmx:sendError', onError);
                document.body.addEventListener('htmx:responseError', onError);
                htmx.trigger(form, 'submit');
            }});
        }},
        openDialogEditor(vm, options = {{}}) {{
            if (!vm || !options.recordKey) return null;

            const row = options.row || {{}};
            const nextMode = options.mode || 'edit';
            const nextPosition = options.positionNo ?? row?.POSITION_NO ?? 1;
            const normalize = typeof options.normalize === 'function'
                ? options.normalize
                : (value) => this.cloneData(value, {{}});

            if (options.modeKey) vm[options.modeKey] = nextMode;
            if (options.positionKey) vm[options.positionKey] = nextPosition;
            vm[options.recordKey] = normalize(row);

            if (options.dialogId && typeof gDialogOpen === 'function') {{
                gDialogOpen(options.dialogId);
            }}

            return vm[options.recordKey];
        }},
        async submitAction(vm, options = {{}}) {{
            const validate = typeof options.validate === 'function' ? options.validate : null;
            if (validate) {{
                const validation = await validate();
                if (validation === false) return false;
                if (typeof validation === 'string' && validation) {{
                    this.showToast(validation, options.validationType || 'warning');
                    return false;
                }}
            }}

            const payload = typeof options.buildPayload === 'function'
                ? await options.buildPayload()
                : (options.payload || {{}});

            try {{
                const res = await this.requestJson(options.method || 'POST', options.url || '', payload);
                const result = res?.json || {{}};
                if (!res?.ok || result.status !== 'success') {{
                    const msg = result.message || `HTTP ${{res?.status ?? 0}}`;
                    this.showToast(`${{options.errorPrefix || '處理失敗: '}}${{msg}}`, 'error');
                    return false;
                }}

                if (options.closeDialogId && typeof gDialogClose === 'function') {{
                    gDialogClose(options.closeDialogId);
                }}

                if (options.successMessage) {{
                    const msg = typeof options.successMessage === 'function'
                        ? options.successMessage(payload, result, vm)
                        : options.successMessage;
                    if (msg) this.showToast(msg, 'success');
                }}

                if (typeof options.onSuccess === 'function') {{
                    await options.onSuccess(result, payload, vm);
                }}

                return true;
            }} catch (e) {{
                const msg = e?.message || String(e);
                this.showToast(`${{options.errorPrefix || '處理失敗: '}}${{msg}}`, 'error');
                return false;
            }}
        }},
        async confirmAndSubmit(vm, options = {{}}) {{
            const confirmMessage = typeof options.confirmMessage === 'function'
                ? options.confirmMessage()
                : options.confirmMessage;
            const confirmTitle = options.confirmTitle || '刪除確認';
            if (typeof gConfirm === 'function') {{
                const ok = await gConfirm(confirmMessage || '確定執行？', confirmTitle);
                if (!ok) return false;
            }}
            return await this.submitAction(vm, options);
        }}
    }};
}}

function gDataGrid_{cfg.CompId}() {{
    return {{
        allRows       : [],
        rows          : [],
        loading       : false,
        currentPage   : 1,
        pageSize      : {h.PageSize},
        selectedRow   : null,
        selectedIds   : [],
        currentParams : '',

        sortKey       : '',
        sortDir       : 'asc',
        sortRules     : [],
        sortableFields: [{sortableJs}],
        titleSortEnabled : {BoolJs(h.TitleSortEnabled)},
        titleSortField   : '{cfg.DefaultTitleSortField}',
        multiSortEnabled : {BoolJs(h.MultiSortEnabled)},

        queryColumns  : {queryColumnsJson},
        formColumns   : {formColumnsJson},
        queryValues        : {{}},
        queryDisplayValues : {{}},
        queryPanelOpen: true,
        formMode      : 'view',
        formRecord    : {{}},

        editingId     : null,
        editRowData   : {{}},
        filters       : {{}},
        columnFilters : {{}},
        dateTimeFilters: {{}},
        headerFilter  : {{
            open: false, field: '', title: '', mode: 'list',
            options: [], selected: [], search: '',
            dateFrom: '', dateTo: '', x: 0, y: 0
        }},

        onRowClickExpr : {JsonSerializer.Serialize(h.OnRowClick ?? "")},
        onRowDblExpr   : {JsonSerializer.Serialize(h.OnRowDblClick ?? "")},
        onSelectExpr   : {JsonSerializer.Serialize(h.OnSelect ?? "")},

        get sortedRows() {{
            const compare = (av, bv, dir) => {{
                av = av ?? ''; bv = bv ?? '';
                if (av === bv) return 0;
                return av < bv ? -dir : dir;
            }};
            if (this.multiSortEnabled && this.sortRules.length > 0) {{
                return [...this.rows].sort((a, b) => {{
                    for (const rule of this.sortRules) {{
                        const dir = rule.dir === 'asc' ? 1 : -1;
                        const r = compare(a?.[rule.key], b?.[rule.key], dir);
                        if (r !== 0) return r;
                    }}
                    return 0;
                }});
            }}
            if (!this.sortKey) return this.rows;
            const dir = this.sortDir === 'asc' ? 1 : -1;
            return [...this.rows].sort((a, b) => compare(a?.[this.sortKey], b?.[this.sortKey], dir));
        }},
        get totalPages() {{ return Math.max(1, Math.ceil(this.sortedRows.length / this.pageSize)); }},
        get pagedRows() {{
            const start = (this.currentPage - 1) * this.pageSize;
            return this.sortedRows.slice(start, start + Number(this.pageSize));
        }},

        init() {{
            this.$el.gDataGrid = this;
            this.initQueryColumns();
            if (this.titleSortEnabled && this.titleSortField && !this.sortKey)
                this.setSort(this.titleSortField, 'asc');
            if ({BoolJs(!cfg.ActualNotInitGrid)})
                this.fetchData();
        }},
        initQueryColumns() {{
            for (const col of (this.queryColumns || [])) {{
                const key = col?.fieldName;
                if (key) {{
                    this.queryValues[key] = col.defaultValue ?? '';
                    this.queryDisplayValues[key] = col.defaultValue ?? '';
                }}
            }}
        }},
        toDateInput(v) {{
            return window.gDataGridRuntime.toDateInput(v);
        }},
        async requestJson(method, url, payload) {{
            return await window.gDataGridRuntime.requestJson(method, url, payload);
        }},

        buildQueryParams() {{
            const params = {{}};
            for (const col of (this.queryColumns || [])) {{
                const key = col?.fieldName;
                if (!key) continue;
                const raw = this.queryValues[key];
                if (raw === null || raw === undefined || `${{raw}}`.trim() === '') continue;
                params[key]                  = raw;
                params[`${{key}}_condition`] = col.condition || '=';
                params[`${{key}}_andOr`]     = col.andOr || 'AND';
                params[`${{key}}_dataType`]  = col.dataType || 'string';
            }}
            return params;
        }},
        executeQuery() {{
            const queryParams = this.buildQueryParams();
            this.$dispatch('query', {{ filters: queryParams }});
            {CallbackJs(h.OnFilter, "queryParams")}
            this.fetchData(queryParams);
        }},
        formatQueryLovDisplay(col, selected, valueField) {{
            if (!selected || !col) return '';
            const rawFormat = String(col.lovDisplayFormat || '').trim();
            if (rawFormat) {{
                return rawFormat.replace(/\{{([^}}]+)\}}/g, (_, token) => {{
                    const key = String(token || '').trim();
                    const value = selected?.[key];
                    return value == null ? '' : String(value);
                }});
            }}

            const displayField = String(col.lovKeyDisplay || '').trim();
            const value = selected?.[valueField];
            const display = displayField ? selected?.[displayField] : '';
            if (display != null && String(display).trim() !== '') return String(display);
            return value == null ? '' : String(value);
        }},
        openQueryLov(fieldName, currentValue = '', ctx = null) {{
            const col = (this.queryColumns || []).find(x => x.fieldName === fieldName);
            if (!col) return false;

            const columns = String(col.lovColumns || '').split(',').map(s => s.trim()).filter(Boolean);
            const fields = String(col.lovFields || '').split(',').map(s => s.trim()).filter(Boolean);
            const api = String(col.lovApi || '').trim();
            const valueField = String(col.lovKeyValue || fields[0] || '').trim();

            if (!api || !columns.length || !fields.length || !valueField || typeof openGenericLov !== 'function') {{
                return false;
            }}

            openGenericLov(
                col.lovTitle || col.caption || '資料查詢',
                api,
                columns,
                fields,
                null,
                null,
                (selected) => {{
                    if (!selected) return;
                    const nextValue = selected?.[valueField] ?? '';
                    this.queryValues[fieldName] = nextValue == null ? '' : String(nextValue);
                    this.queryDisplayValues[fieldName] = this.formatQueryLovDisplay(col, selected, valueField);
                }},
                {{
                    initialQuery: String(currentValue || '').trim(),
                    sourceInputId: ctx?.sourceInputId || ''
                }}
            );

            return true;
        }},
        toggleQueryPanel() {{
            this.queryPanelOpen = !this.queryPanelOpen;
        }},
        invokeQueryDefaultMethod(fieldName, sourceInputId = '', currentValueOverride = null) {{
            const col = (this.queryColumns || []).find(x => x.fieldName === fieldName);
            try {{
                const sourceInput = sourceInputId ? document.getElementById(sourceInputId) : null;
                const currentValue = currentValueOverride !== null && currentValueOverride !== undefined
                    ? currentValueOverride
                    : (sourceInput && typeof sourceInput.value !== 'undefined'
                        ? sourceInput.value
                        : this.queryValues[fieldName]);

                if (currentValue !== undefined) {{
                    this.queryValues[fieldName] = currentValue;
                }}

                const ctx = {{
                    fieldName,
                    currentValue,
                    queryValues: this.queryValues,
                    sourceInputId
                }};

                if (col && this.openQueryLov(fieldName, currentValue, ctx)) return;

                if (!col?.defaultMethod) return;
                if (col.remoteMethod || col.api) return;
                const fnText = String(col.defaultMethod).trim();
                if (!fnText) return;

                if (fnText.includes('(')) {{
                    (new Function(fnText))();
                }} else if (typeof window[fnText] === 'function') {{
                    const ret = window[fnText](fieldName, currentValue, this.queryValues, ctx);
                    if (ret !== undefined && ret !== null) {{
                        if (typeof ret === 'object' && 'value' in ret) {{
                            this.queryValues[fieldName] = ret.value ?? '';
                            this.queryDisplayValues[fieldName] = ret.display ?? String(ret.value ?? '');
                        }} else {{
                            this.queryValues[fieldName] = ret;
                            this.queryDisplayValues[fieldName] = String(ret);
                        }}
                    }}
                }}
            }} catch (e) {{
                console.error('[GDataGrid] invokeQueryDefaultMethod error:', e);
            }}
        }},
        onQueryInputEnter(fieldName, sourceInputId, displayValue) {{
            const col = (this.queryColumns || []).find(x => x.fieldName === fieldName);
            if ((col?.lovApi && col?.lovFields) || (col?.defaultMethod && !col?.remoteMethod && !col?.api)) {{
                this.invokeQueryDefaultMethod(fieldName, sourceInputId, displayValue);
                return;
            }}
            this.executeQuery();
        }},

        async fetchData(queryParams = null) {{
            if (queryParams !== null) {{
                this.currentParams = typeof queryParams === 'object'
                    ? new URLSearchParams(queryParams).toString()
                    : queryParams;
            }}
            this.loading = true;
            this.cancelEdit();
            try {{
                const urlObj = new URL('{cfg.ActualApiUrl}', window.location.origin);
                if (this.currentParams) {{
                    new URLSearchParams(this.currentParams).forEach((v, k) => urlObj.searchParams.append(k, v));
                }}
                const res = await fetch(urlObj.toString());
                if (res.status === 401) {{ window.location.href = '/Account/Login'; return; }}
                let json = null;
                try {{ json = await res.json(); }} catch {{ json = {{ data: [] }}; }}
                if (!res.ok) {{
                    console.error('[GDataGrid] fetch HTTP error:', res.status, json);
                    return;
                }}
                const dataRows = Array.isArray(json?.data) ? json.data : (Array.isArray(json) ? json : []);
                this.allRows = dataRows;
                this.rows = [...dataRows];
                this.applyColumnFilters(false);
                this.currentPage = 1;
                this.selectedRow = null;
                {CallbackJs(h.OnLoadSuccess, "this.rows")}
            }} catch (e) {{
                console.error('[GDataGrid] fetch error:', e);
            }} finally {{
                this.loading = false;
            }}
        }},

        applyFilter() {{
            const clean = Object.fromEntries(
                Object.entries(this.filters).filter(([, v]) => v !== '' && v !== null && v !== undefined)
            );
            const merged = {{ ...this.buildQueryParams(), ...clean }};
            this.$dispatch('query', {{ filters: merged }});
            {CallbackJs(h.OnFilter, "merged")}
            this.fetchData(new URLSearchParams(merged).toString());
        }},
        openHeaderFilter(field, event) {{
            if (!{BoolJs(cfg.ActualTitleFilter)}) return;
            const values = [...new Set((this.allRows || [])
                .map(r => r?.[field])
                .filter(v => v !== null && v !== undefined && `${{v}}`.trim() !== '')
                .map(v => `${{v}}`)
            )].sort((a, b) => a.localeCompare(b, 'zh-Hant'));

            const existing = this.columnFilters[field];
            const titleNode = event?.currentTarget?.closest('th')?.querySelector('span');
            this.headerFilter = {{
                ...this.headerFilter,
                open: true,
                field,
                title: titleNode?.textContent?.trim() || field,
                options: values,
                selected: existing ? [...existing] : [...values],
                search: '',
                mode: this.isDateTimeField(values) ? 'datetime' : 'list',
            }};
            const existingDate = this.dateTimeFilters[field] || {{}};
            this.headerFilter.dateFrom = this.toDateTimeLocal(existingDate.from);
            this.headerFilter.dateTo   = this.toDateTimeLocal(existingDate.to);

            const rect     = event?.currentTarget?.getBoundingClientRect();
            const hostRect = this.$el.getBoundingClientRect();
            const toolbarB = this.$el.querySelector('.g-grid-toolbar')?.getBoundingClientRect()?.bottom ?? rect?.bottom ?? hostRect.top;
            this.headerFilter.x = Math.max(8, (rect?.left ?? hostRect.left) - hostRect.left);
            this.headerFilter.y = Math.max(8, toolbarB - hostRect.top + 6);
        }},
        isDateTimeField(values) {{
            if (!values?.length) return false;
            const sample = values.slice(0, Math.min(values.length, 6));
            return sample.filter(v => !Number.isNaN(Date.parse(v))).length >= Math.ceil(sample.length * 0.8)
                && sample.some(v => /[T\s]\d{{2}}:\d{{2}}/.test(v));
        }},
        toDateTimeLocal(v) {{
            if (!v) return '';
            const d = new Date(v);
            if (Number.isNaN(d.getTime())) return '';
            const p = n => String(n).padStart(2, '0');
            return `${{d.getFullYear()}}-${{p(d.getMonth()+1)}}-${{p(d.getDate())}}T${{p(d.getHours())}}:${{p(d.getMinutes())}}`;
        }},
        isHeaderFilterAllSelected() {{
            const opts = this.getHeaderFilterVisibleOptions();
            const selected = this.headerFilter.selected || [];
            return opts.length > 0 && opts.every(o => selected.includes(o));
        }},
        toggleHeaderFilterAll(checked) {{
            const opts = this.getHeaderFilterVisibleOptions();
            this.headerFilter.selected = checked
                ? [...new Set([...(this.headerFilter.selected || []), ...opts])]
                : (this.headerFilter.selected || []).filter(v => !opts.includes(v));
        }},
        getHeaderFilterVisibleOptions() {{
            const q = (this.headerFilter.search || '').trim().toLowerCase();
            return q ? this.headerFilter.options.filter(v => `${{v}}`.toLowerCase().includes(q)) : this.headerFilter.options;
        }},
        confirmHeaderFilter() {{
            const field = this.headerFilter.field;
            if (!field) return;
            if (this.headerFilter.mode === 'datetime') {{
                const from = this.headerFilter.dateFrom, to = this.headerFilter.dateTo;
                if (!from && !to) delete this.dateTimeFilters[field];
                else this.dateTimeFilters[field] = {{
                    from: from ? new Date(from).toISOString() : '',
                    to:   to   ? new Date(to).toISOString()   : ''
                }};
            }} else {{
                const selected = [...this.headerFilter.selected];
                if (!selected.length || selected.length === this.headerFilter.options.length)
                    delete this.columnFilters[field];
                else
                    this.columnFilters[field] = selected;
                delete this.dateTimeFilters[field];
            }}
            this.applyColumnFilters(true);
            this.headerFilter.open = false;
        }},
        cancelHeaderFilter() {{ this.headerFilter.open = false; }},
        applyColumnFilters(emitEvent) {{
            const fields     = Object.keys(this.columnFilters);
            const dateFields = Object.keys(this.dateTimeFilters);
            if (!fields.length && !dateFields.length) {{
                this.rows = [...this.allRows];
            }} else {{
                this.rows = (this.allRows || []).filter(row => {{
                    const passList = fields.every(f => this.columnFilters[f].includes(`${{row?.[f] ?? ''}}`));
                    if (!passList) return false;
                    return dateFields.every(f => {{
                        const rule = this.dateTimeFilters[f] || {{}};
                        const val  = new Date(row?.[f]).getTime();
                        if (Number.isNaN(val)) return false;
                        const from = rule.from ? new Date(rule.from).getTime() : null;
                        const to   = rule.to   ? new Date(rule.to).getTime()   : null;
                        if (from !== null && val < from) return false;
                        if (to   !== null && val > to)   return false;
                        return true;
                    }});
                }});
            }}
            this.currentPage = 1;
            this.selectedRow = null;
            if (emitEvent) {{
                const summaryList = Object.fromEntries(Object.entries(this.columnFilters).map(([k, v]) => [k, v.join(',')]));
                const summaryDate = Object.fromEntries(
                    Object.entries(this.dateTimeFilters).flatMap(([k, v]) => [
                        [`${{k}}_from`, v.from || ''], [`${{k}}_to`, v.to || '']
                    ])
                );
                const summary = {{ ...summaryList, ...summaryDate }};
                this.$dispatch('query', {{ filters: summary }});
                {CallbackJs(h.OnFilter, "summary")}
            }}
        }},

        onSelectRow(row, isChecked) {{
            {(string.IsNullOrEmpty(h.OnSelect) ? "" : $"if (isChecked) {CallbackJs(h.OnSelect, "row")}")}
        }},
        toggleAll(isChecked) {{
            this.selectedIds = isChecked ? this.rows.map(r => r['{cfg.ActualIdField}']) : [];
        }},

        addRow() {{
            if (Array.isArray(this.formColumns) && this.formColumns.length > 0)
                this.openGeneratedForm('add');
            {CallbackJs(h.OnInsert)}
            this.$dispatch('add');
        }},
        get hasSelection() {{
            return !!this.getSelectedRow();
        }},
        get hasRows() {{
            return this.sortedRows.length > 0;
        }},
        get selectedRowIndex() {{
            return this.findRowIndex(this.selectedRow);
        }},
        get canSelectFirst() {{
            return this.hasRows && this.selectedRowIndex !== 0;
        }},
        get canSelectLast() {{
            return this.hasRows && this.selectedRowIndex !== this.sortedRows.length - 1;
        }},
        get canSelectPrev() {{
            return this.selectedRowIndex > 0;
        }},
        get canSelectNext() {{
            const idx = this.selectedRowIndex;
            if (!this.hasRows) return false;
            if (idx < 0) return true;
            return idx < this.sortedRows.length - 1;
        }},
        findRowIndex(row) {{
            if (!row) return -1;
            const selectedId = row?.['{cfg.ActualIdField}'];
            return this.sortedRows.findIndex(item => item === row || item?.['{cfg.ActualIdField}'] === selectedId);
        }},
        setSelectedRow(row, invokeCallbacks = true) {{
            if (!row) {{
                this.selectedRow = null;
                return null;
            }}

            const idx = this.findRowIndex(row);
            const targetRow = idx >= 0 ? this.sortedRows[idx] : row;
            if (idx >= 0) {{
                this.currentPage = Math.floor(idx / Number(this.pageSize)) + 1;
            }}

            this.selectedRow = targetRow;
            if (invokeCallbacks) {{
                this.invokeExpression(this.onRowClickExpr, targetRow);
                this.invokeExpression(this.onSelectExpr, targetRow);
            }}

            return targetRow;
        }},
        selectRowByIndex(index, invokeCallbacks = true) {{
            const targetIndex = Number(index);
            if (Number.isNaN(targetIndex) || targetIndex < 0 || targetIndex >= this.sortedRows.length) return null;
            const targetRow = this.sortedRows[targetIndex];
            return this.setSelectedRow(targetRow, invokeCallbacks);
        }},
        selectFirstRow() {{
            return this.selectRowByIndex(0);
        }},
        selectPrevRow() {{
            const idx = this.selectedRowIndex;
            if (idx > 0) return this.selectRowByIndex(idx - 1);
            return this.selectedRow;
        }},
        selectNextRow() {{
            const idx = this.selectedRowIndex;
            if (idx >= 0 && idx < this.sortedRows.length - 1) return this.selectRowByIndex(idx + 1);
            if (idx < 0 && this.sortedRows.length > 0) return this.selectFirstRow();
            return this.selectedRow;
        }},
        selectLastRow() {{
            return this.selectRowByIndex(this.sortedRows.length - 1);
        }},
        ensureSelectedRow(actionName) {{
            const row = this.getSelectedRow();
            if (row) return row;
            window.gDataGridRuntime.showToast(`請先選取要${{actionName}}的資料`, 'warning');
            return null;
        }},
        onViewSelected() {{
            const row = this.ensureSelectedRow('檢視');
            if (!row) return;
            this.onView(row);
        }},
        editSelectedRow() {{
            const row = this.ensureSelectedRow('編輯');
            if (!row) return;
            if ('{h.EditMode}' === 'row') {{
                this.startEdit(row);
                return;
            }}
            this.editRow(row);
        }},
        async deleteSelectedRow() {{
            const row = this.ensureSelectedRow('刪除');
            if (!row) return false;
            return await this.deleteRow(row);
        }},
        editRow(row) {{
            if (Array.isArray(this.formColumns) && this.formColumns.length > 0)
                this.openGeneratedForm('edit', row);
            {CallbackJs(h.OnUpdate, "row")}
            this.$dispatch('edit', {{ row }});
        }},
        onView(row) {{
            if (Array.isArray(this.formColumns) && this.formColumns.length > 0)
                this.openGeneratedForm('view', row);
            {CallbackJs(h.OnView, "row")}
            this.$dispatch('view', {{ row }});
        }},
        openGeneratedForm(mode, row = null) {{
            this.formMode = mode || 'view';
            const next = row ? JSON.parse(JSON.stringify(row)) : {{}};
            for (const col of (this.formColumns || [])) {{
                const key = col?.fieldName;
                if (key && (next[key] === undefined || next[key] === null))
                    next[key] = col.defaultValue ?? '';
                if (key && String(col?.columnType || '').toLowerCase() === 'lovinput') {{
                    const displayKey = `${{key}}__DISPLAY`;
                    if (next[displayKey] === undefined || next[displayKey] === null || next[displayKey] === '') {{
                        const rawFormat = String(col?.lovDisplayFormat || '').trim();
                        if (rawFormat) {{
                            next[displayKey] = rawFormat.replace(/\{{([^}}]+)\}}/g, (_, token) => {{
                                const k = String(token || '').trim();
                                const v = next[k];
                                return v == null ? '' : String(v);
                            }});
                        }} else {{
                            const displayField = col?.lovKeyDisplay || '';
                            next[displayKey] = displayField && next[displayField]
                                ? String(next[displayField])
                                : String(next[key] ?? '');
                        }}
                    }}
                }}
            }}
            this.formRecord = next;
            gDialogOpen('{cfg.CompId}_formdlg');
        }},
        formatLovDisplay(col, selected, valueField) {{
            if (!selected || !col) return '';
            const rawFormat = String(col.lovDisplayFormat || '').trim();
            if (rawFormat) {{
                return rawFormat.replace(/\{{([^}}]+)\}}/g, (_, token) => {{
                    const key = String(token || '').trim();
                    const value = selected?.[key];
                    return value == null ? '' : String(value);
                }});
            }}

            const displayField = String(col.lovKeyDisplay || '').trim();
            const value = selected?.[valueField];
            const display = displayField ? selected?.[displayField] : '';
            if (display != null && String(display).trim() !== '') return String(display);
            return value == null ? '' : String(value);
        }},
        openFormLov(fieldName) {{
            const col = (this.formColumns || []).find(x => x.fieldName === fieldName);
            if (!col) return;

            const columns = String(col.lovColumns || '')
                .split(',')
                .map(s => s.trim())
                .filter(Boolean);
            const fields = String(col.lovFields || '')
                .split(',')
                .map(s => s.trim())
                .filter(Boolean);
            const api = String(col.lovApi || '').trim();
            const valueField = String(col.lovKeyValue || fields[0] || '').trim();

            if (!api || !columns.length || !fields.length || !valueField || typeof openGenericLov !== 'function') {{
                console.warn('[GDataGrid] openFormLov config invalid:', fieldName, col);
                return;
            }}

            openGenericLov(
                col.lovTitle || col.caption || '資料查詢',
                api,
                columns,
                fields,
                null,
                null,
                (selected) => {{
                    if (!selected) return;
                    const nextValue = selected?.[valueField] ?? '';
                    this.formRecord[fieldName] = nextValue == null ? '' : String(nextValue);
                    this.formRecord[`${{fieldName}}__DISPLAY`] = this.formatLovDisplay(col, selected, valueField);
                }}
            );
        }},
        closeGeneratedForm() {{ gDialogClose('{cfg.CompId}_formdlg'); }},
        async saveGeneratedForm() {{
            const payload = JSON.parse(JSON.stringify(this.formRecord || {{}}));
            for (const key of Object.keys(payload)) {{
                if (key.endsWith('__DISPLAY')) delete payload[key];
            }}
            const url = this.formMode === 'add'
                ? '{cfg.ActualApiUrl}'.replace('/select', '/insert')
                : '{cfg.ActualApiUrl}'.replace('/select', '/update');
            try {{
                const res = await this.requestJson('POST', url, payload);
                const result = res?.json || null;
                if (!res?.ok || result?.status === 'error') {{
                    const msg = result?.message || `HTTP ${{res?.status ?? 0}}`;
                    console.error('[GDataGrid] saveGeneratedForm failed:', msg);
                    if (typeof gToast === 'function') gToast('儲存失敗：' + msg, 'error');
                    return;
                }}
                this.closeGeneratedForm();
                if (typeof gToast === 'function') gToast(this.formMode === 'add' ? '新增成功' : '更新成功', 'success');
                await this.fetchData();
            }} catch (e) {{
                console.error('[GDataGrid] saveGeneratedForm error:', e);
                if (typeof gToast === 'function') gToast('儲存失敗：' + e.message, 'error');
            }}
        }},
        async confirmDelete(row, options = {{}}) {{
            const submitUrl = String(options.url || '').trim();
            const payload = options.payload ?? row;
            const reloadAction = typeof options.onSuccess === 'function'
                ? options.onSuccess
                : async () => {{
                    if (typeof this.executeQuery === 'function') {{
                        await this.executeQuery();
                    }} else {{
                        await this.fetchData();
                    }}
                }};

            if (!submitUrl) {{
                console.warn('[GDataGrid] confirmDelete url is required.');
                return false;
            }}

            return await window.gDataGridRuntime.confirmAndSubmit(this, {{
                confirmMessage: options.confirmMessage || (() => `確定刪除資料？`),
                confirmTitle: options.confirmTitle || '刪除確認',
                url: submitUrl,
                payload,
                successMessage: options.successMessage || '刪除成功',
                errorPrefix: options.errorPrefix || '刪除失敗: ',
                onSuccess: reloadAction
            }});
        }},
        async deleteRow(row) {{
            {(string.IsNullOrEmpty(h.OnDeleting) ? "" : $"if (window['{h.OnDeleting}'] && window['{h.OnDeleting}'](row) === false) return;")}
            {(h.AutoApply && !string.IsNullOrEmpty(h.RemoteName)
                ? $@"const tableParam_{cfg.CompId} = '{h.DataMember}' ? '?DataMember={h.DataMember}' : '';
                     try {{
                         const res = await this.requestJson('POST', `/{h.RemoteName}/delete${{tableParam_{cfg.CompId}}}`.replace('//', '/'), row);
                         if (res?.ok) this.fetchData();
                     }} catch (e) {{ console.error('[GDataGrid] delete auto-apply failed:', e); }}"
                : "")}
            {CallbackJs(h.OnDelete, "row")}
            this.$dispatch('delete', {{ row }});
        }},
        startEdit(row) {{
            this.editingId  = row['{cfg.ActualIdField}'];
            this.editRowData = JSON.parse(JSON.stringify(row));
        }},
        cancelEdit() {{ this.editingId = null; this.editRowData = {{}}; }},
        async confirmSave() {{
            if (!this.editingId) return;
            {(h.AutoApply && !string.IsNullOrEmpty(h.RemoteName)
                ? $@"const tp_{cfg.CompId} = '{h.DataMember}' ? '?DataMember={h.DataMember}' : '';
                     try {{
                         const res = await this.requestJson('POST', `/{h.RemoteName}/update${{tp_{cfg.CompId}}}`.replace('//', '/'), this.editRowData);
                         if (res?.ok) this.fetchData();
                     }} catch (e) {{ console.error('[GDataGrid] update auto-apply failed:', e); }}"
                : "")}
            this.$dispatch('save', {{
                row: this.editRowData,
                callback: (success) => {{ if (success) this.cancelEdit(); }}
            }});
        }},

        isSortableField(key) {{ return !this.sortableFields?.length || this.sortableFields.includes(key); }},
        getSortRule(key)     {{ return (this.sortRules || []).find(r => r.key === key); }},
        getSortOrder(key)    {{
            const idx = (this.sortRules || []).findIndex(r => r.key === key);
            return idx < 0 ? 0 : idx + 1;
        }},
        isSorted(key, dir) {{
            if (this.multiSortEnabled) {{
                const rule = this.getSortRule(key);
                return !!rule && rule.dir === dir;
            }}
            return this.sortKey === key && this.sortDir === dir;
        }},
        setSort(key, dir) {{
            if (!this.isSortableField(key)) return;
            if (this.multiSortEnabled) {{
                const rules = [...(this.sortRules || [])];
                const idx   = rules.findIndex(r => r.key === key);
                const next  = {{ key, dir: dir === 'desc' ? 'desc' : 'asc' }};
                idx >= 0 ? rules.splice(idx, 1, next) : rules.push(next);
                this.sortRules = rules;
            }}
            this.sortKey = key;
            this.sortDir = dir === 'desc' ? 'desc' : 'asc';
            if (this.titleSortEnabled) this.titleSortField = key;
        }},
        toggleSort(key) {{
            if (!this.isSortableField(key)) return;
            const nextDir = (this.multiSortEnabled ? this.getSortRule(key)?.dir : this.sortDir) === 'asc' ? 'desc' : 'asc';
            this.setSort(key, nextDir);
        }},
        setTitleSort(dir) {{
            if (!this.titleSortEnabled) return;
            const key = this.titleSortField || this.sortKey;
            if (key) this.setSort(key, dir);
        }},

        prevPage() {{ if (this.currentPage > 1) this.currentPage--; }},
        nextPage() {{ if (this.currentPage < this.totalPages) this.currentPage++; }},
        jumpPage(v) {{
            const p = parseInt(v);
            if (!isNaN(p)) this.currentPage = Math.min(Math.max(1, p), this.totalPages);
        }},

        getSelectedRow() {{ return this.selectedRow; }},
        refresh()        {{ this.fetchData(); }},

        invokeExpression(expr, row) {{
            const e = (expr || '').trim();
            if (!e) return;
            try {{
                typeof window[e] === 'function'
                    ? window[e](row)
                    : (new Function('row', `return (${{e}})(row);`))(row);
            }} catch (err) {{
                console.error('[GDataGrid] invokeExpression error:', err, e);
            }}
        }},
        handleRowClick(row) {{
            this.selectedRow = row;
            this.invokeExpression(this.onRowClickExpr, row);
            this.invokeExpression(this.onSelectExpr, row);
        }},
        handleRowDblClick(row) {{
            this.selectedRow = row;
            this.invokeExpression(this.onRowDblExpr, row);
            this.invokeExpression(this.onSelectExpr, row);
        }}
    }};
}}
</script>";
        }


        private static string BuildQueryPanelHtml(List<QueryColumnDefinition> queryColumns)
        {
            if (queryColumns.Count == 0) return "";

            var sb = new StringBuilder();
            sb.AppendLine(@"<div class=""g-grid-query border-b border-slate-200 shrink-0"">");
            sb.AppendLine(@"  <div class=""g-grid-query-head flex items-center justify-between px-4 py-3 border-b border-slate-200"">");
            sb.AppendLine(@"    <div class=""inline-flex items-center gap-2 text-blue-600 font-semibold"">");
            sb.AppendLine(@"      <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M7 7h10l-4 5v5l-2-1v-4L7 7z""/></svg>");
            sb.AppendLine(@"      <span>查詢條件</span>");
            sb.AppendLine(@"    </div>");
            sb.AppendLine(@"    <button type=""button"" @click=""toggleQueryPanel()""");
            sb.AppendLine($@"            class=""{CssBtnSecondary}"">");
            sb.AppendLine(@"      <svg class=""w-3.5 h-3.5 transition-transform"" :class=""queryPanelOpen ? '' : 'rotate-180'"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 9l-7 7-7-7""/></svg>");
            sb.AppendLine(@"      <span x-text=""queryPanelOpen ? '收合' : '展開'""></span>");
            sb.AppendLine(@"    </button>");
            sb.AppendLine(@"  </div>");
            sb.AppendLine(@"  <div x-show=""queryPanelOpen"" class=""g-grid-query-body px-4 py-3"">");
            sb.AppendLine(@"    <div class=""grid grid-cols-1 md:grid-cols-12 gap-3"">");

            foreach (var col in queryColumns)
            {
                var field   = Enc(col.FieldName);
                var caption = Enc(col.Caption);
                var span    = Math.Clamp(col.Span <= 0 ? 3 : col.Span, 1, 12);
                var model   = $"queryValues['{field}']";

                sb.AppendLine($@"    <div class=""md:col-span-{span} flex flex-col gap-1"">");
                sb.AppendLine($@"      <label class=""text-xs font-semibold text-slate-600"">{caption}</label>");
                sb.AppendLine(BuildQueryFieldEditor(col, model, field));
                sb.AppendLine(@"    </div>");
            }

            sb.AppendLine(@"    </div>");
            sb.AppendLine($@"    <div class=""flex items-center gap-2 mt-3"">
      <button type=""button"" @click=""executeQuery()""
              class=""{CssBtnPrimary}"">
        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
          <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/>
        </svg>
        <span>查詢</span>
      </button>
      <button type=""button"" @click=""initQueryColumns(); executeQuery()""
              class=""{CssBtnSecondary}"">
        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
          <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>
        </svg>
        <span>清除</span>
      </button>
    </div>");
            sb.AppendLine(@"  </div>");
            sb.AppendLine(@"</div>");
            return sb.ToString();
        }

        private static string BuildQueryFieldEditor(QueryColumnDefinition col, string model, string field)
        {
            return col.Editor switch
            {
                "numberbox" =>
                    $@"<input type=""number"" x-model=""{model}"" class=""{CssInput}"">",

                "datebox" =>
                    $@"<input type=""date"" x-model=""{model}"" class=""{CssInput}"">",

                "password" =>
                    $@"<input type=""password"" x-model=""{model}"" class=""{CssInput}"">",

                "checkbox" =>
                    $@"<label class=""inline-flex items-center gap-2 py-2"">
                        <input type=""checkbox""
                               :checked=""{model}==='Y' || {model}===true || {model}===1""
                               @change=""{model} = $event.target.checked ? 'Y' : 'N'""
                               class=""w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"">
                        <span class=""text-sm text-slate-600"">是/否</span>
                    </label>",

                "gcombobox" =>
                    $@"<select x-model=""{model}"" class=""{CssInput}"">
                        <option value="""">請選擇</option>
                        {string.Join("", ParseEditorOptions(col.EditorOptions)
                            .Select(o => $@"<option value=""{Enc(o.Value)}"">{Enc(o.Text)}</option>"))}
                    </select>",

                "gcombogrid" or "lovinput" or "gcombogrid" =>
                    $@"<div class=""flex items-center w-full"">
                        <input type=""text"" id=""{field}_query_input""
                               x-model=""queryDisplayValues['{field}']""
                               @input=""queryValues['{field}'] = queryDisplayValues['{field}']""
                               class=""block min-w-0 w-full flex-1 h-[42px] px-3 py-2.5 border border-slate-300 rounded-l-xl border-r-0 text-sm text-slate-700 bg-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors""
                               @keydown.enter.prevent=""onQueryInputEnter('{field}', '{field}_query_input', queryDisplayValues['{field}'])"">
                        <button type=""button"" @click=""invokeQueryDefaultMethod('{field}', '{field}_query_input', queryDisplayValues['{field}'])""
                                class=""shrink-0 inline-flex items-center justify-center h-[42px] min-w-[42px] px-3 border border-l-0 border-slate-300 rounded-r-xl bg-white hover:bg-slate-50 text-slate-600 hover:text-blue-600 transition-colors"" title=""選取"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/>
                            </svg>
                        </button>
                    </div>",

                _ =>
                    $@"<input type=""text"" x-model=""{model}"" class=""{CssInput}"">",
            };
        }


        private static string BuildGeneratedFormDialog(string compId, List<FormColumnDefinition> formColumns)
        {
            if (formColumns.Count == 0) return "";

            var dialogId = $"{compId}_formdlg";
            var fields   = string.Join("", formColumns.Where(c => !c.Hidden).Select(BuildGeneratedFormField));

            return $@"
<div id=""{dialogId}"" role=""dialog"" aria-modal=""true""
     class=""fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[200] items-center justify-center p-4""
     style=""display:none;""
     @keydown.escape.window=""gDialogClose('{dialogId}')""
     onclick=""if(event.target===this)gDialogClose('{dialogId}')"">
    <div class=""relative w-full max-w-3xl rounded-2xl bg-white shadow-2xl overflow-hidden"">

        <!-- 標題列：依模式顯示不同底色 -->
        <div class=""flex items-center justify-between px-5 py-4 text-white""
             :class=""formMode==='add' ? 'bg-emerald-700' : formMode==='edit' ? 'bg-blue-700' : 'bg-slate-700'"">
            <div class=""flex items-center gap-2.5"">
                <!-- 模式圖示 -->
                <template x-if=""formMode==='add'"">
                    <svg class=""w-5 h-5 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 4v16m8-8H4""/>
                    </svg>
                </template>
                <template x-if=""formMode==='edit'"">
                    <svg class=""w-5 h-5 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z""/>
                    </svg>
                </template>
                <template x-if=""formMode==='view'"">
                    <svg class=""w-5 h-5 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                              d=""M15 12a3 3 0 11-6 0 3 3 0 016 0""/>
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                              d=""M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z""/>
                    </svg>
                </template>
                <h3 class=""text-base font-bold""
                    x-text=""formMode==='add' ? '新增資料' : (formMode==='edit' ? '修改資料' : '檢視資料')""></h3>
            </div>
            <button type=""button"" onclick=""gDialogClose('{dialogId}')""
                    class=""text-white/70 hover:text-white transition-colors rounded-lg p-1 hover:bg-white/10"">
                <svg class=""w-5 h-5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                    <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>
                </svg>
            </button>
        </div>

        <!-- 欄位 -->
        <div class=""p-5 overflow-y-auto max-h-[70vh]"">
            <div class=""grid grid-cols-1 md:grid-cols-2 gap-x-5 gap-y-4"">
                {fields}
            </div>
        </div>

        <!-- 按鈕列 -->
        <div class=""flex items-center justify-between gap-2 px-5 py-3.5 border-t border-slate-200 bg-slate-50"">
            <span class=""text-xs text-slate-400"" x-show=""formMode !== 'view'"">
                <span class=""text-red-400"">*</span> 為必填欄位
            </span>
            <div class=""flex gap-2 ml-auto"">
                <button type=""button"" onclick=""gDialogClose('{dialogId}')""
                        class=""{CssBtnSecondary}"">
                    <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>
                    </svg>
                    關閉
                </button>
                <button type=""button"" x-show=""formMode !== 'view'"" @click=""saveGeneratedForm()""
                        class=""{CssBtnPrimary}"">
                    <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""/>
                    </svg>
                    儲存
                </button>
            </div>
        </div>
    </div>
</div>";
        }

        private static string BuildGeneratedFormField(FormColumnDefinition col)
        {
            var f           = Js(col.FieldName);
            var roExpr      = col.AlwaysReadOnly || col.IsPrimaryKey ? "formMode !== 'add'" : "formMode === 'view'";
            var wrapperSpan = col.ColSpan > 1 ? $"md:col-span-{col.ColSpan} " : "";
            var req         = col.Required ? @"<span class=""text-red-500 ml-0.5"">*</span>" : "";
            var placeholder = string.IsNullOrWhiteSpace(col.Placeholder) ? "" : $@" placeholder=""{Enc(col.Placeholder)}""";
            var maxLength   = col.MaxLength.HasValue ? $@" maxlength=""{col.MaxLength.Value}""" : "";
            var min         = col.Min.HasValue ? $@" min=""{col.Min.Value}""" : "";
            var max         = col.Max.HasValue ? $@" max=""{col.Max.Value}""" : "";

            var roClass = $@" :class=""{roExpr} ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white'""";

            var sb = new StringBuilder();
            sb.Append($@"<div class=""{wrapperSpan}flex flex-col gap-1"">
    <label class=""text-xs font-semibold text-slate-600"">{Enc(col.Caption)}{req}</label>");

            switch (NormalizeFormColumnType(col.ColumnType))
            {
                case FormColumnType.Select:
                    sb.Append($@"<select x-model=""formRecord['{f}']"" :disabled=""{roExpr}""{roClass} class=""{CssInput}"">
                        <option value="""">請選擇</option>");
                    foreach (var opt in (col.Options ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = opt.Split(':', 2);
                        sb.Append($@"<option value=""{Enc(parts[0].Trim())}"">{Enc(parts.Length > 1 ? parts[1].Trim() : parts[0].Trim())}</option>");
                    }
                    sb.Append("</select>");
                    break;

                case FormColumnType.Date:
                    sb.Append($@"<input type=""date"" x-model=""formRecord['{f}']"" :readonly=""{roExpr}""{roClass} class=""{CssInput}""{placeholder}>");
                    break;

                case FormColumnType.Number:
                    sb.Append($@"<input type=""number"" x-model=""formRecord['{f}']"" :readonly=""{roExpr}""{roClass} class=""{CssInput}""{placeholder}{min}{max}>");
                    break;

                case FormColumnType.Lov:
                    sb.Append($@"<div class=""flex items-center w-full"">
                        <input type=""hidden"" x-model=""formRecord['{f}']"">
                        <input type=""text"" x-model=""formRecord['{f}__DISPLAY']"" :readonly=""true""
                               {roClass} class=""block min-w-0 w-full flex-1 h-[42px] px-3 py-2.5 border border-slate-300 rounded-l-xl border-r-0 text-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors""{placeholder}{maxLength}>
                        <button type=""button""
                                x-show=""!({roExpr})""
                                @click=""openFormLov('{f}')"" 
                                class=""shrink-0 inline-flex items-center justify-center h-[42px] min-w-[42px] px-3 border border-l-0 border-slate-300 rounded-r-xl bg-white hover:bg-slate-50 text-slate-600 hover:text-blue-600 transition-colors"" title=""選取"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                                      d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/>
                            </svg>
                        </button>
                    </div>");
                    break;

                default:
                    sb.Append($@"<input type=""text"" x-model=""formRecord['{f}']"" :readonly=""{roExpr}""{roClass} class=""{CssInput}""{placeholder}{maxLength}>");
                    break;
            }

            sb.Append("</div>");
            return sb.ToString();
        }


        private static List<ColumnDef> ParseColumns(string cols) =>
            string.IsNullOrWhiteSpace(cols)
                ? new()
                : cols.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(c =>
                      {
                          var p = c.Trim().Split(':');
                          return new ColumnDef(
                              Field       : p.ElementAtOrDefault(0)?.Trim() ?? "",
                              Title       : p.ElementAtOrDefault(1)?.Trim() ?? p[0].Trim(),
                              Width       : p.ElementAtOrDefault(2)?.Trim() ?? "",
                              Align       : p.ElementAtOrDefault(3)?.Trim() ?? "left",
                              EditorType  : p.ElementAtOrDefault(4)?.Trim() ?? "readonly",
                              EditorOptions: p.ElementAtOrDefault(5)?.Trim() ?? "",
                              FilterType  : p.ElementAtOrDefault(6)?.Trim() ?? "");
                      })
                      .Where(c => !string.IsNullOrEmpty(c.Field))
                      .ToList();

        private static HashSet<string> ParseSortableFields(string sortableColumns, List<ColumnDef> cols)
        {
            if (string.IsNullOrWhiteSpace(sortableColumns))
                return cols.Select(c => c.Field).Where(f => !string.IsNullOrWhiteSpace(f))
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return sortableColumns
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static List<QueryColumnDefinition> ParseQueryColumns(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new();
            try
            {
                var list = JsonSerializer.Deserialize<List<QueryColumnDefinition>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                foreach (var q in list)
                {
                    q.FieldName = q.FieldName.Trim();
                    q.Caption   = string.IsNullOrWhiteSpace(q.Caption) ? q.FieldName : q.Caption.Trim();
                    q.Condition = NormalizeQueryCondition(q.Condition);
                    q.AndOr     = NormalizeAndOr(q.AndOr);
                    q.DataType  = NormalizeDataType(q.DataType);
                    q.Editor    = NormalizeEditor(q.Editor);
                    q.Span      = Math.Clamp(q.Span <= 0 ? 3 : q.Span, 1, 12);
                    q.RowSpan   = q.RowSpan <= 0 ? 1 : q.RowSpan;
                    q.LovTitle  = q.LovTitle?.Trim() ?? "";
                    q.LovApi    = q.LovApi?.Trim() ?? "";
                    q.LovColumns = q.LovColumns?.Trim() ?? "";
                    q.LovFields  = q.LovFields?.Trim() ?? "";
                    q.LovKeyValue = q.LovKeyValue?.Trim() ?? "";
                    q.LovKeyDisplay = q.LovKeyDisplay?.Trim() ?? "";
                    q.LovDisplayFormat = q.LovDisplayFormat?.Trim() ?? "";
                }
                return list.Where(q => !string.IsNullOrWhiteSpace(q.FieldName)).ToList();
            }
            catch { return new(); }
        }

        private static List<FormColumnDefinition> ParseFormColumns(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new();
            try
            {
                return JsonSerializer.Deserialize<List<FormColumnDefinition>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?.Where(x => !string.IsNullOrWhiteSpace(x.FieldName)).ToList() ?? new();
            }
            catch { return new(); }
        }

        private static List<(string Value, string Text)> ParseEditorOptions(string options)
        {
            if (string.IsNullOrWhiteSpace(options)) return new();
            return options.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part =>
                {
                    var idx = part.IndexOf('=');
                    if (idx <= 0) return (part, part);
                    var v = part[..idx].Trim();
                    var t = part[(idx + 1)..].Trim();
                    return (v, string.IsNullOrWhiteSpace(t) ? v : t);
                }).ToList();
        }


        private static string NormalizeEditor(string? editor) =>
            (editor ?? "").Trim().ToLowerInvariant() switch
            {
                "checkbox"                                        => "checkbox",
                "numberbox"                                       => "numberbox",
                "datebox"                                         => "datebox",
                "infocombobox" or "gcombobox"                     => "gcombobox",
                "infocombogrid" or "gcombogrid"                   => "gcombogrid",
                "inforefval" or "glovinput" or "lovinput"         => "lovinput",
                "password"                                        => "password",
                "gtext" or "text"                                 => "text",
                _                                                 => "text"
            };

        private static string NormalizeQueryCondition(string? condition) =>
            (condition ?? "").Trim() switch
            {
                "=" => "=", "!=" or "<>" => "!=",
                ">" => ">", ">=" => ">=",
                "<" => "<", "<=" => "<=",
                "%" => "%", "%%" => "%%",
                _   => "="
            };

        private static string NormalizeAndOr(string? andOr) =>
            string.Equals((andOr ?? "").Trim(), "OR", StringComparison.OrdinalIgnoreCase) ? "OR" : "AND";

        private static string NormalizeDataType(string? dataType) =>
            (dataType ?? "").Trim().ToLowerInvariant() switch
            {
                "number"   => "number",
                "datetime" => "datetime",
                "guid"     => "guid",
                _          => "string"
            };

        private static FormColumnType NormalizeFormColumnType(string? raw) =>
            (raw ?? "").Trim().ToLowerInvariant() switch
            {
                "select" or "combobox" or "gcombobox" or "g-combobox"           => FormColumnType.Select,
                "date" or "datebox" or "gdatebox" or "g-datebox"                => FormColumnType.Date,
                "number" or "numberbox" or "gnumberbox" or "g-numberbox"        => FormColumnType.Number,
                "lov" or "lovinput" or "glovinput" or "g-lov-input"             => FormColumnType.Lov,
                _                                                                => FormColumnType.Text
            };


        private static string Enc(string? v) => System.Net.WebUtility.HtmlEncode(v ?? "");
        private static string Js(string? v)  => (v ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
        private static string BoolJs(bool v) => v ? "true" : "false";
        private static string Serialize(object obj) => JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}

