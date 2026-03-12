using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Restruct.Views.Components
{
    [HtmlTargetElement("g-messager", TagStructure = TagStructure.WithoutEndTag)]
    public class GMessagerTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("id", "gMessagerRoot");
            output.Content.SetHtmlContent($@"
                <!-- g-messager: Alert/Confirm/Prompt/Toast -->
                <div id=""gMsgOverlay"" class=""fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[900] hidden items-center justify-center"">
                    <div id=""gMsgBox"" class=""bg-white rounded-2xl shadow-2xl w-full max-w-sm mx-4 p-6 transform transition-all"">
                        <div class=""flex items-start gap-3 mb-4"">
                            <div id=""gMsgIcon"" class=""shrink-0 w-10 h-10 rounded-full flex items-center justify-center""></div>
                            <div class=""flex-1"">
                                <h3 id=""gMsgTitle"" class=""text-base font-bold text-slate-800 mb-1""></h3>
                                <p id=""gMsgContent"" class=""text-sm text-slate-600""></p>
                                <input id=""gMsgInput"" type=""text"" class=""g-input w-full mt-2 hidden"">
                            </div>
                        </div>
                        <div id=""gMsgBtns"" class=""flex justify-end gap-2""></div>
                    </div>
                </div>
                <!-- Toast -->
                <div id=""gToastContainer"" class=""fixed bottom-4 right-4 z-[950] flex flex-col gap-2 pointer-events-none""></div>

                <script>
                const gMsg = (function() {{
                    const icons = {{
                        success: {{ bg:'bg-green-100', color:'text-green-600', svg:'<path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""/>' }},
                        error  : {{ bg:'bg-slate-100',   color:'text-red-600',   svg:'<path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/>' }},
                        warning: {{ bg:'bg-slate-100', color:'text-amber-600', svg:'<path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z""/>' }},
                        info   : {{ bg:'bg-blue-50',  color:'text-blue-600',  svg:'<path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""/>' }}
                    }};
                    function mkBtn(text, cls, cb) {{
                        const b = document.createElement('button');
                        b.type = 'button'; b.textContent = text;
                        b.className = `px-4 py-2 text-sm font-semibold rounded-lg transition-colors ${{cls}}`;
                        b.onclick = cb; return b;
                    }}
                    function show(title, msg, type, btns, showInput) {{
                        const ic = icons[type] || icons.info;
                        document.getElementById('gMsgTitle').textContent   = title;
                        document.getElementById('gMsgContent').textContent = msg;
                        const iconEl = document.getElementById('gMsgIcon');
                        iconEl.className = `shrink-0 w-10 h-10 rounded-full flex items-center justify-center ${{ic.bg}} ${{ic.color}}`;
                        iconEl.innerHTML = `<svg class=""w-5 h-5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">${{ic.svg}}</svg>`;
                        const inpEl = document.getElementById('gMsgInput');
                        inpEl.classList.toggle('hidden', !showInput);
                        if (showInput) {{ inpEl.value = ''; setTimeout(() => inpEl.focus(), 100); }}
                        const btnsEl = document.getElementById('gMsgBtns');
                        btnsEl.innerHTML = '';
                        btns.forEach(b => btnsEl.appendChild(b));
                        const ov = document.getElementById('gMsgOverlay');
                        ov.classList.remove('hidden');
                        ov.classList.add('flex');
                    }}
                    function hide() {{
                        const ov = document.getElementById('gMsgOverlay');
                        ov.classList.add('hidden');
                        ov.classList.remove('flex');
                    }}
                    return {{
                        alert(title, msg, type='info', onOk) {{
                            show(title, msg, type, [mkBtn('確定','bg-blue-600 text-white', ()=>{{ hide(); onOk&&onOk(); }})]);
                        }},
                        confirm(title, msg, onOk, onCancel, type='warning') {{
                            show(title, msg, type, [
                                mkBtn('取消','bg-slate-100 text-slate-700', ()=>{{ hide(); onCancel&&onCancel(); }}),
                                mkBtn('確定','bg-blue-600 text-white',       ()=>{{ hide(); onOk&&onOk();     }})
                            ]);
                        }},
                        prompt(title, defaultVal='', onOk, type='info') {{
                            const inpEl = document.getElementById('gMsgInput');
                            inpEl.value = defaultVal;
                            show(title, '', type, [
                                mkBtn('取消','bg-slate-100 text-slate-700', ()=>hide()),
                                mkBtn('確定','bg-blue-600 text-white',       ()=>{{ hide(); onOk&&onOk(inpEl.value); }})
                            ], true);
                        }},
                        toast(msg, type='success', duration=3000) {{
                            const tc = icons[type] || icons.info;
                            const bgMap = {{ success:'bg-green-600', error:'bg-red-600', warning:'bg-amber-500', info:'bg-blue-600' }};  // Toast 背景色語義化
                            const t = document.createElement('div');
                            t.className = `pointer-events-auto flex items-center gap-2 px-4 py-3 ${{bgMap[type]||'bg-slate-900/60'}} text-white rounded-xl shadow-lg text-sm font-medium translate-y-4 opacity-0 transition-all duration-300`;
                            t.innerHTML = `<svg class=""w-4 h-4 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""/></svg>${{msg}}`;
                            document.getElementById('gToastContainer').appendChild(t);
                            requestAnimationFrame(()=>{{ t.classList.remove('translate-y-4','opacity-0'); }});
                            setTimeout(()=>{{
                                t.classList.add('opacity-0','translate-y-4');
                                setTimeout(()=>t.remove(), 300);
                            }}, duration);
                        }}
                    }};
                }})();
                </script>
            ");
        }
    }
    [HtmlTargetElement("g-datalist")]
    public class GDataListTagHelper : TagHelper
    {
        public string Id       { get; set; } = "";
        public string ApiUrl   { get; set; } = "";
        public string Template { get; set; } = "";   // "field:label:class,..."
        public string OnClick  { get; set; } = "";
        public string Class    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId = string.IsNullOrEmpty(Id) ? $"gdl_{Guid.NewGuid():N}" : Id;
            var fn     = $"gDataList_{compId}";
            var fields = Template.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => { var p = f.Trim().Split(':'); return (Field: p[0].Trim(), Label: p.Length > 1 ? p[1].Trim() : p[0].Trim(), Cls: p.Length > 2 ? p[2].Trim() : ""); })
                .ToList();

            var rowTemplate = new System.Text.StringBuilder();
            if (fields.Count > 0)
            {
                rowTemplate.Append($@"<div class=""flex items-center gap-2"">");
                foreach (var (field, label, cls) in fields)
                    rowTemplate.Append($@"<span class=""text-sm {cls}"" x-text=""row['{field}']??''""></span>");
                rowTemplate.Append("</div>");
            }
            else
            {
                rowTemplate.Append(@"<span class=""text-sm"" x-text=""JSON.stringify(row)""></span>");
            }

            var rowClick = !string.IsNullOrEmpty(OnClick) ? $"@click=\"({OnClick})(row)\" cursor-pointer" : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("id", compId);
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 overflow-hidden {Class}");
            output.Attributes.SetAttribute("x-data", $"{fn}()");
            output.Attributes.SetAttribute("x-init", "init()");
            output.Content.SetHtmlContent($@"
                <div x-show=""loading"" class=""flex justify-center items-center py-8 text-slate-400"">
                    <svg class=""w-5 h-5 animate-spin mr-2"" fill=""none"" viewBox=""0 0 24 24"">
                        <circle class=""opacity-25"" cx=""12"" cy=""12"" r=""10"" stroke=""currentColor"" stroke-width=""4""/>
                        <path class=""opacity-75"" fill=""currentColor"" d=""M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z""/>
                    </svg>
                    <span class=""text-sm"">載入中...</span>
                </div>
                <div x-show=""!loading && rows.length===0"" class=""py-8 text-center text-sm text-slate-400"">目前無資料</div>
                <ul x-show=""!loading && rows.length>0"" class=""divide-y divide-slate-100"">
                    <template x-for=""(row, idx) in rows"" :key=""idx"">
                        <li class=""px-4 py-3 bg-slate-100 transition-colors {rowClick}"" {(!string.IsNullOrEmpty(OnClick) ? $@"@click=""({OnClick})(row)""" : "")}>
                            {rowTemplate}
                        </li>
                    </template>
                </ul>
                <script>
                function {fn}() {{
                    return {{
                        rows: [], loading: false,
                        async init() {{
                            this.loading = true;
                            try {{
                                const r = await fetch('{ApiUrl}');
                                const j = await r.json();
                                this.rows = j.data ?? j;
                            }} catch(e) {{ console.error(e); }}
                            finally {{ this.loading = false; }}
                        }},
                        refresh() {{ this.init(); }}
                    }};
                }}
                </script>
            ");
        }
    }
    public class GPropertyGridContext
    {
        public List<(string Name, string Value, string Type)> Rows { get; } = new();
    }

    [HtmlTargetElement("g-property", ParentTag = "g-property-grid")]
    public class GPropertyTagHelper : TagHelper
    {
        public string Name  { get; set; } = "";
        public string Value { get; set; } = "";
        public string Type  { get; set; } = "text";  // text|badge|link

        public override void Process(TagHelperContext ctx, TagHelperOutput output)
        {
            var pg = ctx.Items[typeof(GPropertyGridContext)] as GPropertyGridContext;
            pg?.Rows.Add((Name, Value, Type));
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement("g-property-grid")]
    [RestrictChildren("g-property")]
    public class GPropertyGridTagHelper : TagHelper
    {
        public string Title { get; set; } = "";
        public string Class { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var pg = new GPropertyGridContext();
            context.Items[typeof(GPropertyGridContext)] = pg;
            await output.GetChildContentAsync();

            var sb = new System.Text.StringBuilder();
            sb.Append(@"<table class=""min-w-full border-collapse"">");
            sb.Append(@"<colgroup><col class=""w-1/3 bg-slate-100""><col class=""w-2/3""></colgroup>");

            foreach (var (name, value, type) in pg.Rows)
            {
                var valueHtml = type switch
                {
                    "badge" => $@"<span class=""inline-flex px-2 py-0.5 text-xs font-bold bg-blue-100 text-blue-700 rounded-full"">{value}</span>",
                    "link"  => $@"<a href=""{value}"" class=""text-blue-600 hover:underline text-sm"">{value}</a>",
                    _       => $@"<span class=""text-sm text-slate-800"">{value}</span>"
                };
                sb.Append($@"
                <tr class=""border-b border-slate-200 last:border-0"">
                    <td class=""px-3 py-2.5 text-xs font-semibold text-slate-500 bg-slate-100 border-r border-slate-200"">{name}</td>
                    <td class=""px-3 py-2.5"">{valueHtml}</td>
                </tr>");
            }
            sb.Append("</table>");

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 overflow-hidden {Class}");

            var headerHtml = !string.IsNullOrEmpty(Title)
                ? $@"<div class=""px-4 py-2.5 bg-slate-100 border-b border-slate-200 text-sm font-bold text-slate-700"">{Title}</div>"
                : "";

            output.Content.SetHtmlContent($@"{headerHtml}<div class=""overflow-x-auto"">{sb}</div>");
        }
    }
    [HtmlTargetElement("g-treegrid")]
    public class GTreeGridTagHelper : TagHelper
    {
        public string Id       { get; set; } = "";
        public string ApiUrl   { get; set; } = "";
        public string Columns  { get; set; } = "";  // "field:title:width:align,..."
        public string IdField  { get; set; } = "id";
        public string PidField { get; set; } = "pid";
        public int    PageSize { get; set; } = 0;
        public bool   Striped  { get; set; } = true;
        public string OnRowClick { get; set; } = "";
        public string Class    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId = string.IsNullOrEmpty(Id) ? $"gtg_{Guid.NewGuid():N}" : Id;
            var fn     = $"gTreeGrid_{compId}";

            var cols = Columns.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c =>
            {
                var p = c.Trim().Split(':');
                return (Field: p[0].Trim(), Title: p.Length > 1 ? p[1].Trim() : p[0].Trim(),
                        Width: p.Length > 2 ? p[2].Trim() : "", Align: p.Length > 3 ? p[3].Trim() : "left");
            }).ToList();

            var thSb = new System.Text.StringBuilder();
            var tdSb = new System.Text.StringBuilder();
            bool first = true;
            foreach (var (field, title, width, align) in cols)
            {
                var wStyle = string.IsNullOrEmpty(width) ? "" : $"width:{width}px;min-width:{width}px;";
                var thAlignCls = align is "center" or "right" ? $"text-{align}" : "text-left";
                var tdAlignCls = align is "center" or "right" ? $"text-{align}" : "text-left";
                thSb.Append($@"<th style=""{wStyle}"" class=""px-3 py-2.5 {thAlignCls} text-xs font-bold text-slate-500 uppercase bg-slate-100 border-b-2 border-slate-200"">{title}</th>");

                if (first)
                {
                    tdSb.Append($@"<td class=""px-3 py-2 text-sm text-slate-700 border-b border-slate-100"">
                        <div class=""flex items-center"" :style=""'padding-left:' + (row._depth * 16) + 'px'"">
                            <button type=""button"" x-show=""row._hasChildren""
                                    @click=""toggleNode(row)"" class=""mr-1.5 w-4 h-4 flex items-center justify-center"">
                                <svg :class=""row._open ? 'rotate-90' : ''"" class=""w-3.5 h-3.5 text-slate-400 transition-transform"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                    <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 5l7 7-7 7""/>
                                </svg>
                            </button>
                            <span x-show=""!row._hasChildren"" class=""w-5""></span>
                            <span x-text=""row['{field}']??''""></span>
                        </div>
                    </td>");
                    first = false;
                }
                else
                {
                    tdSb.Append($@"<td class=""px-3 py-2 {tdAlignCls} text-sm text-slate-700 border-b border-slate-100"" x-text=""row['{field}']??''""></td>");
                }
            }

            var rowClick = !string.IsNullOrEmpty(OnRowClick)
                ? $"@click=\"({OnRowClick})(row)\""
                : "";
            var rowCursor = !string.IsNullOrEmpty(OnRowClick) ? "cursor-pointer" : "";
            var striped   = Striped ? "bg-white bg-slate-100" : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("id", compId);
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden {Class}");
            output.Attributes.SetAttribute("x-data", $"{fn}()");
            output.Attributes.SetAttribute("x-init", "init()");
            output.Content.SetHtmlContent($@"
                <div x-show=""loading"" class=""flex justify-center py-10 text-slate-400"">
                    <svg class=""animate-spin w-5 h-5"" fill=""none"" viewBox=""0 0 24 24"">
                        <circle class=""opacity-25"" cx=""12"" cy=""12"" r=""10"" stroke=""currentColor"" stroke-width=""4""/>
                        <path class=""opacity-75"" fill=""currentColor"" d=""M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z""/>
                    </svg>
                </div>
                <div class=""overflow-auto"" x-show=""!loading"">
                    <table class=""min-w-full border-collapse"">
                        <thead><tr>{thSb}</tr></thead>
                        <tbody>
                            <template x-for=""(row, idx) in visibleRows"" :key=""row['{IdField}']"">
                                <tr class=""{striped} transition-colors {rowCursor}"" {rowClick}>
                                    {tdSb}
                                </tr>
                            </template>
                        </tbody>
                    </table>
                    <div x-show=""!loading && visibleRows.length===0"" class=""text-center py-8 text-sm text-slate-400"">目前無資料</div>
                </div>
                <script>
                function {fn}() {{
                    return {{
                        allRows   : [],
                        loading   : false,
                        idField   : '{IdField}',
                        pidField  : '{PidField}',
                        get visibleRows() {{
                            return this.allRows.filter(r => r._visible);
                        }},
                        async init() {{
                            this.loading = true;
                            try {{
                                const res  = await fetch('{ApiUrl}');
                                const json = await res.json();
                                const data = json.data ?? json;
                                const map  = {{}};
                                data.forEach(r => {{ r._open=false; r._depth=0; r._hasChildren=false; r._visible=true; map[r[this.idField]]=r; }});
                                data.forEach(r => {{ if(r[this.pidField] && map[r[this.pidField]]) map[r[this.pidField]]._hasChildren=true; }});
                                this.allRows = this.buildTree(data, null, map, 0);
                            }} catch(e) {{ console.error(e); }}
                            finally {{ this.loading = false; }}
                        }},
                        buildTree(data, pid, map, depth) {{
                            const result = [];
                            data.filter(r => r[this.pidField] == pid).forEach(r => {{
                                r._depth = depth;
                                result.push(r);
                                result.push(...this.buildTree(data, r[this.idField], map, depth + 1));
                            }});
                            return result;
                        }},
                        toggleNode(row) {{
                            row._open = !row._open;
                            this.setChildrenVisible(row[this.idField], row._open);
                        }},
                        setChildrenVisible(pid, visible) {{
                            this.allRows.filter(r => r[this.pidField] == pid).forEach(r => {{
                                r._visible = visible;
                                if (!visible) r._open = false;
                                this.setChildrenVisible(r[this.idField], visible && r._open);
                            }});
                        }},
                        refresh() {{ this.init(); }}
                    }};
                }}
                </script>
            ");
        }
    }
}

