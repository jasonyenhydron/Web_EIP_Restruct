using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-pagination")]
    public class GPaginationTagHelper : TagHelper
    {
        public string AlpineTotal    { get; set; } = "totalPages";
        public string AlpineCurrent  { get; set; } = "currentPage";
        public string AlpinePrev     { get; set; } = "prevPage()";
        public string AlpineNext     { get; set; } = "nextPage()";
        public string AlpineJump     { get; set; } = "jumpPage($event.target.value)";
        public string AlpineCount    { get; set; } = "";
        public string AlpinePageSize { get; set; } = "pageSize";
        public string PageSizeOptions{ get; set; } = "10,20,50";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var opts = PageSizeOptions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Select(s => $"""<option :selected="{AlpinePageSize}=={s}" value="{s}">{s} 筆/頁</option>""");
            var countHtml = !string.IsNullOrEmpty(AlpineCount)
                ? $"""<span class="text-slate-400 text-xs">共<span x-text="{AlpineCount}" class="font-bold text-slate-700"></span> 筆</span>"""
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                "flex flex-wrap items-center justify-between gap-3 px-4 py-2 bg-slate-100 border-t border-slate-200 text-sm select-none");
            output.Content.SetHtmlContent($"""
                <div class="flex items-center gap-3 flex-wrap">
                    {countHtml}
                    <select x-model="{AlpinePageSize}" @@change="currentPage=1"
                            class="pl-2 pr-6 py-1 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400 cursor-pointer">
                        {string.Join("", opts)}
                    </select>
                </div>
                <div class="flex items-center gap-1">
                    <button type="button" @@click="{AlpinePrev}" :disabled="{AlpineCurrent}<=1"
                            :class="{AlpineCurrent}<=1?'opacity-40 cursor-not-allowed':'bg-slate-100'"
                            class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>
                    </button>
                    <div class="flex items-center gap-1.5 px-2">
                        <input type="number" min="1" :max="{AlpineTotal}" :value="{AlpineCurrent}"
                               @@change="{AlpineJump}"
                               class="w-14 text-center text-xs border border-slate-300 rounded-lg py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-400">
                        <span class="text-slate-400 text-xs">/</span>
                        <span x-text="{AlpineTotal}" class="text-xs font-bold text-slate-700 min-w-[1.25rem] text-center"></span>
                        <span class="text-slate-400 text-xs">頁</span>
                    </div>
                    <button type="button" @@click="{AlpineNext}" :disabled="{AlpineCurrent}>={AlpineTotal}"
                            :class="{AlpineCurrent}>={AlpineTotal}?'opacity-40 cursor-not-allowed':'bg-slate-100'"
                            class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/></svg>
                    </button>
                </div>
            """);
        }
    }
}




