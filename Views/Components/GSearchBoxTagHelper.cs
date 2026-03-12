using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-search-box")]
    public class GSearchBoxTagHelper : TagHelper
    {
        public string Id           { get; set; } = "";
        public string Name         { get; set; } = "";
        public string Placeholder  { get; set; } = "請輸入關鍵字...";
        public string ApiUrl       { get; set; } = "";
        public string DisplayField { get; set; } = "";
        public string ValueField   { get; set; } = "";
        public string LabelFields  { get; set; } = "";
        public string TargetId     { get; set; } = "";
        public string Class        { get; set; } = "";
        public bool   Disabled     { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId    = string.IsNullOrEmpty(Id) ? $"gsb_{Guid.NewGuid():N}" : Id;
            var listId    = $"{compId}_drop";
            var fields    = LabelFields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(f => f.Trim()).ToList();
            var displayJs = string.IsNullOrEmpty(DisplayField) ? "item[Object.keys(item)[0]]" : $"item['{DisplayField}']";
            var valueJs   = string.IsNullOrEmpty(ValueField) ? displayJs : $"item['{ValueField}']";
            var labelJs   = fields.Count <= 1
                ? $"(item['{(fields.Count == 1 ? fields[0] : "name")}'] ?? '')"
                : $"(item['{fields[0]}'] ?? '') + ' <small class=\"text-slate-400 ml-2\">' + (item['{fields[1]}'] ?? '') + '</small>'";
            var disAttr   = Disabled ? "disabled" : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"relative {Class}");
            output.Content.SetHtmlContent($@"
                <input type=""text"" id=""{compId}"" name=""{Name}"" placeholder=""{Placeholder}"" autocomplete=""off"" {disAttr}
                       class=""g-input w-full pr-8""
                       oninput=""gSearchBox('{compId}','{listId}','{ApiUrl}','{TargetId}','{displayJs.Replace("'","\\'")}','{valueJs.Replace("'","\\'")}')""  >
                <span class=""absolute inset-y-0 right-2 flex items-center pointer-events-none text-slate-400"">
                    <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/>
                    </svg>
                </span>
                <ul id=""{listId}"" class=""absolute z-[150] w-full mt-1 bg-white rounded-xl shadow-2xl border border-slate-200 hidden overflow-y-auto max-h-52"" role=""listbox""></ul>
                <script>
                (function(){{
                    let _t;
                    const inp = document.getElementById('{compId}');
                    const lst = document.getElementById('{listId}');
                    const tgt = document.getElementById('{TargetId}');
                    inp.addEventListener('input', function(){{
                        clearTimeout(_t);
                        const q = this.value.trim();
                        if(!q){{ lst.classList.add('hidden'); return; }}
                        _t = setTimeout(async ()=>{{
                            try{{
                                const r = await fetch(`{ApiUrl}?query=${{encodeURIComponent(q)}}`);
                                const j = await r.json();
                                const d = j.data ?? j;
                                lst.innerHTML = d.length ? '' : '<li class=""px-4 py-3 text-sm text-slate-400"">無符合資料</li>';
                                d.forEach(item=>{{
                                    const li = document.createElement('li');
                                    li.className='px-4 py-2.5 text-sm cursor-pointer text-slate-700 hover:bg-blue-50 hover:text-blue-700 transition-colors';
                                    li.innerHTML=`{labelJs.Replace("`","\\`")}`;
                                    li.addEventListener('click',()=>{{
                                        inp.value={displayJs};
                                        if(tgt) tgt.value={valueJs};
                                        lst.classList.add('hidden');
                                        inp.dispatchEvent(new Event('change'));
                                    }});
                                    lst.appendChild(li);
                                }});
                                lst.classList.remove('hidden');
                            }}catch(e){{console.error(e);}}
                        }},300);
                    }});
                    document.addEventListener('click',e=>{{if(!inp.contains(e.target)&&!lst.contains(e.target))lst.classList.add('hidden');}});
                }})();
                </script>
            ");
        }
    }
}

