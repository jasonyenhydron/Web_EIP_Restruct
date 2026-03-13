---
name: web-erp-csharp-development
description: >
  用於 ASP.NET Core MVC + Dapper + Razor TagHelper + HTMX 的 Web ERP / EIP
  專案開發、維護、重構與除錯。觸發時機：開發新 ERP 功能頁面、維護既有 CRUD 元件、
  擴充 TagHelper、設計 LOV / DataGrid / DataForm、處理多資料庫 SQL、重構頁面架構。
version: 2.1.0
owner: internal
stack:
  runtime: ASP.NET Core MVC (net10.0)
  orm: Dapper (micro-ORM / data access layer)
  databases:
    - Oracle
    - MariaDB
    - SQL Server
    - SQLite
  frontend:
    - Razor Views
    - Razor TagHelpers
    - HTMX
    - Alpine.js
    - Tailwind CSS
    - Flowbite
---

# web-erp-csharp-development

## 架構定位（重要，請先閱讀）

本專案為 **Hybrid ERP Web Framework**，定位如下：

- **Server-rendered MVC 為主**：Razor View + TagHelper + Area 路由
- **Progressive Enhancement 為輔**：HTMX 局部更新 + Alpine.js 前端狀態
- **內部 API Layer**：提供 Grid / Form / LOV / Dialog 元件呼叫
- **Dapper**：主要資料存取工具（micro-ORM / object mapper，非全功能 ORM）
- **互動模式**：Datagrid + DataForm + LOV 為核心

> ⚠️ 本專案**純 SPA**，也**使用技術由taghelper渲染前端**。
> 正確描述為：**Hybrid MVC + HTMX ERP Framework**
> 若需前端重互動，優先以 HTMX + Partial View + Alpine.js 實現，
> 不引入大型前端框架。

---

## 三層互動架構

### Layer 1 — Server-rendered MVC
- Razor View
- TagHelper 元件
- Area 分區模組
- Layout / Partial / ViewComponent

### Layer 2 — Progressive Enhancement
- HTMX：負責局部 DOM 更新
- Alpine.js：負責前端狀態管理
- Tailwind CSS / Flowbite：UI 樣式

### Layer 3 — Internal API
- 供 Grid / LOV / Form / Dialog 呼叫
- 回傳 JSON 或 Partial HTML
- 與外部整合 API 分離，不強求完全 RESTful

---

## Project Rules（必須遵守）

### Rule 1：UI 元件優先級

1. 優先使用既有 TagHelpers，例如 `<g-data-grid>`、`<g-data-form>`、`<g-lov-input>`，含有標準事件與屬性
2. 無法實現時，擴充共用 TagHelper
3. 最後才允許頁面專屬 HTML / JS
4. CRUD 行為優先導向 Form 元件，不直接寫在 Grid 頁面 ，避免重複造輪80％工作由tagHelper 處理 CRUD

**禁止：**
- 在頁面中重複貼相同 Grid / Form / LOV 行為邏輯
- 在單一 `.cshtml` 中塞大量 inline script 處理 CRUD

---

### Rule 2：頁面與路由規範

1. 所有 ERP 功能頁面優先使用 `Area`
2. 路由以 `/{area}/{programId}` 或 `/{area}/{controller}/{action}` 規劃
3. 不以 Controller 名稱作為功能識別主軸，應以 Program / Module 概念命名
4. 頁面命名與 ERP 程式代號對齊，例如：
   - `IDMGD01`（身份識別管理）
   - `HRMGD47`（人資管理）

---

### Rule 3：資料存取規範

1. 資料庫存取**優先透過** `DbHelper` / `IDbExecutor`
2. **禁止**在 Controller / View 直接 `new SqlConnection()` / `new OracleConnection()`
3. 所有 SQL **必須參數化**，禁止字串拼接（防 SQL Injection）
4. Oracle SQL 特別注意：
   - bind parameter 語法
   - column / table alias
   - 避免 `ORA-00933`
   - 分頁語法（`OFFSET...FETCH` 或 `ROWNUM`）
5. 跨資料庫語法差異**封裝於 Provider Layer**，不可散落在 page / controller

---

### Rule 4：Dapper 使用規範

1. Dapper 僅負責 data access，不承擔 domain tracking / change detection
2. CRUD 請明確區分 **Query Service** 與 **Command Service**  insert/update/delete 分開，避免混淆
3. 多步驟異動必須使用**交易控制** transaction scope，確保資料一致性
4. 跨 repository / 多 SQL 異動需透過 transaction scope abstraction
5. 不要把所有查詢做成 generic repository；複雜查詢請明確撰寫 SQL

---

### Rule 5：GDataGrid 規範

GDataGrid 是清單頁的主元件，負責：
- 顯示資料（斑馬紋）
- 分頁 / 排序 / 查詢
- CRUD 入口事件
- 列選取狀態
- Toolbar button 綁定

**標準事件（必須全部綁定）：**

toolbar event 觸發 CRUD 行為，統一導向 Form 元件，避免在 Grid 頁面直接寫 CRUD 邏輯
| 事件 | 說明 |
|------|------|
| `@first` | 最前筆 |
| `@prev` | 前一筆 |
| `@next` | 下一筆 |
| `@last` | 最後筆 |
| `@add` | 新增 |
| `@view` | 檢視 |
| `@edit` | 編輯 |
| `@delete` | 刪除 |
| `@query` | 查詢 |
| `@print` | 列印（pdf, xlsx, csv） |

**規則：**
1. Grid **不直接承載商業邏輯**
2. Grid 的 edit / insert / view 一律導向 `GDataForm`
3. 共用行為優先擴充 `GDataGridTagHelper`
4. 避免各頁面重複實作相同 grid 行為
5. toolbar 在grid上方，可設定各別button visable
6. querypanel在grid上方，可設定visable，，可展開收合，按query按鈕觸發 `@query` 事件將資料顯示在grid上
7. 可設定querycolumns，指定哪些欄位參與查詢條件，並自動生成查詢語法傳至api
8. querycolumns的type可設定lovinput,textinput,textarea,dropdownlist,checkboxlist,radiobuttonlist,datepicker,timepicker,datetimepicker,numberinput等，lovinput可指定lovcode，其他type可指定dataformat（例如日期的yyyy/MM/dd），優先使用內建的元件類型，特殊需求再擴充 TagHelper
---

### Rule 6：GDataForm 規範

GDataForm 是單筆資料維護主元件，負責：
- 顯示單筆資料
- 新增 / 檢視 / 編輯 / 刪除
- 上下筆 / 首末筆導航
- 欄位驗證訊息整合
- 儲存與取消流程

**標準事件（必須全部綁定）：**

| 事件 | 說明 |
|------|------|
| `@first` | 最前筆 |
| `@prev` | 前一筆 |
| `@next` | 下一筆 |
| `@last` | 最後筆 |
| `@add` | 新增 |
| `@view` | 檢視 |
| `@edit` | 編輯 |
| `@delete` | 刪除 |
| `@query` | 查詢 |
| `@print` | 列印（pdf, xlsx, csv） |

**規則：**
1. 單筆表單互動邏輯集中在 `GDataFormTagHelper`
2. 頁面只調整屬性參數與欄位配置
3. 不在 Grid 頁面內直接寫 Form CRUD 邏輯
5. toolbar 在dataform上方，可設定各別button visable
6. querypanel在dataform上方，可設定visable，可展開收合，按query按鈕觸發 `@query` 事件將資料顯示在form上
7. 可設定querycolumns，指定哪些欄位參與查詢條件，並自動生成查詢語法傳至api
8. querycolumns的type可設定lovinput,textinput,textarea,dropdownlist,checkboxlist,radiobuttonlist,datepicker,timepicker,datetimepicker,numberinput等，lovinput可指定lovcode，其他type可指定dataformat（例如日期的yyyy/MM/dd），優先使用內建的元件類型，特殊需求再擴充 TagHelper
---

### Rule 7：LOV 規範

優先使用 `<g-lov-input>` 宣告式屬性：

| 屬性 | 說明 |
|------|------|
| `lov-api` | 資料來源 API 路徑 |
| `lov-columns` | 顯示欄位定義 |
| `lov-fields` | 回填欄位對應 |
| `lov-key-hidden` | 儲存用 hidden key |
| `lov-key-code` | 顯示用 code 欄位 |
| `lov-key-name` | 顯示用 name 欄位 |

**規則：**
1. 缺資料來源時，補 `LovController` 或 Query API
2. 預設 buffer view：每次 50 筆
3. LOV 必須支援：
   - code / name 顯示
   - hidden key 儲存
   - 鍵盤操作（Enter / Esc / 方向鍵）
   - 清除值
   - 回填多欄位
4. LOV SQL 必須參數化
5. LOV 不直接寫死在頁面 JS

---

### Rule 8：例外與錯誤處理規範

1. API 例外格式以 `Program.cs` global exception handler 為準
2. 前端錯誤顯示統一使用 `<g-error-message />`
3. 標準錯誤回傳格式：

```json
{
  "success": false,
  "message": "資料驗證失敗",
  "code": "VALIDATION_ERROR",
  "detail": "...",
  "fileName": "...",
  "lineNumber": 123,
  "traceId": "..."
}
```

4. Production 環境**禁止**回傳敏感 stack trace
5. 前端僅顯示必要訊息；詳細資訊寫入 server log

---

### Rule 9：共用資源載入規範

1. 優先使用 `<g-style profile="...">`
2. 優先使用 `<g-js profile="...">`
3. **禁止**在多頁面重複貼相同 `<script>` / `<link>`
4. 標準 profile 命名：

| Profile | 用途 |
|---------|------|
| `grid` | Datagrid 相關資源 |
| `form` | DataForm 相關資源 |
| `lov` | LOV 相關資源 |
| `report` | 報表相關資源 |
| `dashboard` | Dashboard 相關資源 |

---

### Rule 10：安全規範

1. **禁止**在 skill、程式碼、設定檔、Git 中寫死帳號密碼
2. DB connection string 來源：
   - `environment variables`
   - `user secrets`（開發環境）
   - `secret manager`（正式環境）
   - `deployment secret store`
3. Schema 掃描功能應透過 `SchemaReaderService`，帳密從環境變數讀取
4. 所有 API 必須預留權限驗證與審計欄位
5. 所有刪除功能應評估 soft delete 或 audit log

---

## Key Files

### 核心設定
| 類型 | 路徑 |
|------|------|
| 程式進入點 | `Program.cs` |
| 主 Layout | `Views/Shared/_Layout.cshtml` |
| LOV Modal | `Views/Shared/_LovModal.cshtml` |
| 程式視窗 Layout | `Views/Shared/_popupLayout.cshtml` |
### DB Helpers
| DB | 路徑 |
|----|------|
| Oracle | `Helpers/OracleDbHelper.cs` |
| MariaDB | `Helpers/MariaDbHelper.cs` |
| SQL Server | `Helpers/SqlServerDbHelper.cs` |
| SQLite | `Helpers/SqliteDbHelper.cs` |

### Core TagHelper 元件
| 元件 | 路徑 | 功能 |
|------|------|------|
| GDataGridTagHelper | `Views/Components/GDataGridTagHelper.cs` | 清單 / CRUD 入口 |
| GDataFormTagHelper | `Views/Components/GDataFormTagHelper.cs` | 單筆表單 CRUD |
| GLovInputTagHelper | `Views/Components/GLovInputTagHelper.cs` | LOV 選擇欄位 |
| GErrorMessageTagHelper | `Views/Components/GErrorMessageTagHelper.cs` | 統一錯誤顯示 |
| GStyleTagHelper | `Views/Components/GStyleTagHelper.cs` | 共用 CSS 載入 |
| GJsTagHelper | `Views/Components/GJsTagHelper.cs` | 共用 JS 載入 |

### 主要頁面範例
| 程式代號 | 路徑 |
|----------|------|
| 主選單 | `Views/MisPrograms/MisProgramsMain.cshtml` |
| IDMGD01 | `Views/MisPrograms/IDMGD01.cshtml` |
| HRMGD47 | `Views/MisPrograms/HRMGD47.cshtml` |

---

## 開發模式說明

### List Page 開發模式
1. 使用 `GDataGridTagHelper`
2. 查詢由 Query Service + API 提供
3. CRUD 入口由 toolbar event 觸發
4. View / Edit / Add 導向 `GDataForm`

```cshtml
<g-datagrid
  program-id="IDMGD01"
  api="/api/internal/grid/IDMGD01"
  @add="onAdd"
  @view="onView"
  @edit="onEdit"
  @delete="onDelete"
  @query="onQuery"
  @first="onFirst"
  @prev="onPrev"
  @next="onNext"
  @last="onLast">
</g-datagrid>
```

### Form Page 開發模式
1. 使用 `GDataFormTagHelper`
2. 由 Query API 載入資料
3. 由 Command API 提交資料
4. 驗證錯誤顯示至 `<g-error-message />`

```cshtml
<g-dataform
  program-id="IDMGD01"
  get-api="/api/internal/form/IDMGD01"
  post-api="/api/internal/form/IDMGD01"
  @add="onAdd"
  @view="onView"
  @edit="onEdit"
  @delete="onDelete"
  @first="onFirst"
  @prev="onPrev"
  @next="onNext"
  @last="onLast">
</g-dataform>
<g-error-message />
```

### LOV Field 開發模式
```cshtml
<g-lov-input
  lov-api="/api/internal/lov/EMPLOYEE"
  lov-columns="EMP_NO,EMP_NAME,DEPT_NAME"
  lov-key-hidden="empId"
  lov-key-code="empNo"
  lov-key-name="empName"
  lov-fields="deptId=DEPT_ID,deptName=DEPT_NAME">
</g-lov-input>
```

---

## API 設計規範

### API 分層

| 層別 | 路由格式 | 說明 |
|------|----------|------|
| 內部元件 API | `/api/internal/grid/{programId}` | 供 Grid 呼叫 |
| 內部元件 API | `/api/internal/form/{programId}/{id}` | 供 Form 呼叫 |
| 內部元件 API | `/api/internal/lov/{lovCode}` | 供 LOV 呼叫 |
| 外部整合 API | `/api/external/...` | 嚴格 RESTful |

### 規則
1. 內部 API 偏向元件驅動，不強求完全 REST 純度
2. 外部整合 API 嚴格採 RESTful resource naming
3. Response contract 必須一致
4. Controller **禁止**直接回傳未包裝例外

---

## Schema Discovery 規範

若需依據資料庫 schema 進行開發：

1. 使用 `SchemaReaderService` 讀取 metadata
2. 讀取內容至少包含：
   - `table_name`
   - `column_name`
   - `data_type`
   - `nullable`
   - `is_pk`
   - `is_fk`
   - `index_info`
3. Schema metadata 輸出為 JSON 或文件，不直接寫死在 skill
4. 連線資訊從環境變數讀取，例如：
   ```
   ERP_DB_PROVIDER=oracle
   ERP_DB_CONNECTION_STRING=...
   ERP_DB_TNS=TEST
   ```
5. 禁止在 skill 或文件中記錄正式環境帳密

---

## ERP Program Wizard（自動產生器）

類似 Oracle Forms Wizard，本框架提供 **Program Wizard** 功能，
讀取資料庫 Schema 後，透過步驟式設定，自動產生完整 ERP 程式檔案組合。

---

### Wizard 觸發方式

```
/Wizard/ERP/Index
```

或透過開發工具選單：**工具 → ERP 程式產生器（Program Wizard）**

---

### Wizard 流程（6 個步驟）

```
Step 1：連線設定
    ↓
Step 2：選擇主表 / 關聯表
    ↓
Step 3：欄位設定（Grid / Form / LOV / 隱藏）
    ↓
Step 4：程式設定（ProgramId / Area / Title / 權限）
    ↓
Step 5：預覽產生清單
    ↓
Step 6：確認產生 → 輸出檔案
```

---

### Step 1：連線設定

| 設定項目 | 說明 |
|----------|------|
| DB Provider | Oracle / MariaDB / SQL Server / SQLite |
| Connection String | 從環境變數選擇或暫時輸入（不儲存明文） |
| Schema / Owner | Oracle 指定 schema owner |
| 連線測試 | 按鈕驗證連線是否成功 |

> 連線資訊**僅在 Wizard Session 中暫存**，不寫入任何檔案或 log。

---

### Step 2：選擇主表 / 關聯表

Wizard 讀取 Schema 後，顯示可選表格：

| 設定項目 | 說明 |
|----------|------|
| 主表（Master Table） | 主要 CRUD 操作的資料表 |
| 明細表（Detail Table） | 可選，啟用 Master-Detail 模式 |
| JOIN 條件 | 自動偵測 FK，可手動調整 |
| 頁面模式 | Single Table / Master-Detail / Multi-Tab |

**自動偵測邏輯：**
1. 讀取 `ALL_CONSTRAINTS` / `INFORMATION_SCHEMA` 取得 PK / FK 資訊
2. 推薦關聯表清單
3. 顯示 ER 關係預覽（簡易圖示）

---

### Step 3：欄位設定

對每個欄位可設定：

| 屬性 | 可選值 | 說明 |
|------|--------|------|
| `顯示用途` | Grid / Form / Both / Hidden | 欄位出現在哪個元件 |
| `欄位標籤` | 自訂中文 / 英文 Label | 預設取 column comment |
| `元件類型` | Text / Number / Date / Select / LOV / Checkbox / Textarea | 自動由 data type 推薦 |
| `必填` | Yes / No | Form 驗證 |
| `唯讀` | Yes / No | Form 唯讀欄位 |
| `LOV 來源` | 選擇已有 LOV Code 或新建 | 僅 LOV 類型 |
| `預設值` | 固定值 / 系統變數（今日/使用者） | 新增時預設填入 |
| `Grid 寬度` | px 或 auto | Grid 欄位寬 |
| `Grid 排序` | 可拖拉調整順序 | - |
| `Form 排版` | 全寬 / 半寬 / 1/3 寬 | Form 欄位佔比 |

**欄位類型自動推薦規則：**

| DB 資料型別 | 推薦元件 |
|-------------|----------|
| `VARCHAR2` / `NVARCHAR` | Text |
| `NUMBER` / `INT` / `DECIMAL` | Number |
| `DATE` / `DATETIME` / `TIMESTAMP` | Date Picker |
| `CHAR(1)` | Checkbox 或 Select |
| 有 FK 指向其他表 | LOV |
| `CLOB` / `TEXT` | Textarea |

---

### Step 4：程式設定

| 設定項目 | 說明 | 範例 |
|----------|------|------|
| Program ID | ERP 程式代號 | `HRMGD01` |
| Area | 所屬模組 Area | `HR` / `FIN` / `MIS` |
| 程式名稱 | 中文顯示名稱 | `員工基本資料維護` |
| Controller 名稱 | 自動建議，可修改 | `HrEmployeeController` |
| 權限代碼 | 對應權限系統 | `HR.EMP.MAINTAIN` |
| 啟用查詢列 | Yes / No | Grid 上方查詢區 |
| 啟用匯出 | Yes / No | Excel / CSV 匯出 |
| 啟用審計欄位 | Yes / No | 自動加入 CREATED_BY 等欄位 |
| 分頁筆數 | 預設 50 | Grid 每頁顯示筆數 |

---

### Step 5：預覽產生清單

確認前，Wizard 顯示將要產生的檔案清單：

| 檔案 | 路徑 | 說明 |
|------|------|------|
| View（List） | `Areas/{Area}/Views/{ProgramId}/{ProgramId}.cshtml` | Datagrid 清單頁 |
| View（Form） | `Areas/{Area}/Views/{ProgramId}/{ProgramId}Form.cshtml` | DataForm 表單頁 |
| Controller | `Areas/{Area}/Controllers/{Controller}.cs` | MVC Controller |
| Query Service | `Services/{ProgramId}QueryService.cs` | 查詢邏輯 |
| Command Service | `Services/{ProgramId}CommandService.cs` | 新增/修改/刪除邏輯 |
| API Controller | `Areas/{Area}/Controllers/{ProgramId}ApiController.cs` | 內部 API |
| LOV Controller | `Controllers/Lov/{LovCode}LovController.cs` | （若有 LOV 欄位） |
| DTO Model | `Models/{ProgramId}Dto.cs` | 資料傳輸物件 |
| SQL Script | `SqlScripts/{ProgramId}_query.sql` | 初始查詢 SQL（參考用） |
| Metadata JSON | `WizardOutput/{ProgramId}.meta.json` | 程式 metadata 存檔 |

> 每個檔案旁顯示「預覽」按鈕，可查看即將產生的程式碼內容。

---

### Step 6：確認產生

1. 點擊「**產生程式**」按鈕
2. Wizard 執行 Code Generator
3. 顯示產生進度與結果清單
4. 若檔案已存在，提示：**覆蓋 / 略過 / 備份後覆蓋**
5. 完成後提示：`dotnet build` 驗證

---

### Wizard 產生檔案規範

#### 產生的 View（List Page）範本

```cshtml
@* 由 ERP Program Wizard 產生 — {ProgramId} — {DateTime} *@
@{
    ViewData["Title"] = "{程式名稱}";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<g-style profile="grid" />
<g-js profile="grid" />

<g-datagrid
    program-id="{ProgramId}"
    title="{程式名稱}"
    api="/api/internal/grid/{ProgramId}"
    columns="{自動產生欄位定義}"
    @add="onAdd"
    @view="onView"
    @edit="onEdit"
    @delete="onDelete"
    @query="onQuery"
    @first="onFirst"
    @prev="onPrev"
    @next="onNext"
    @last="onLast">
</g-datagrid>

<g-error-message />
```

#### 產生的 Controller 範本

```csharp
// 由 ERP Program Wizard 產生 — {ProgramId} — {DateTime}
[Area("{Area}")]
[Route("{Area}/{ProgramId}")]
public class {Controller}Controller : Controller
{
    private readonly {ProgramId}QueryService _queryService;
    private readonly {ProgramId}CommandService _commandService;

    public {Controller}Controller(
        {ProgramId}QueryService queryService,
        {ProgramId}CommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    public IActionResult Index() => View();
}
```

#### 產生的 API Controller 範本

```csharp
// 由 ERP Program Wizard 產生 — {ProgramId} — {DateTime}
[ApiController]
[Area("{Area}")]
[Route("api/internal/[controller]")]
public class {ProgramId}ApiController : ControllerBase
{
    // GET  api/internal/{ProgramId}/grid
    // GET  api/internal/{ProgramId}/form/{id}
    // POST api/internal/{ProgramId}/form
    // PUT  api/internal/{ProgramId}/form/{id}
    // DELETE api/internal/{ProgramId}/form/{id}
}
```

#### 產生的 DTO 範本

```csharp
// 由 ERP Program Wizard 產生 — {ProgramId} — {DateTime}
public class {ProgramId}Dto
{
    // 自動由 Wizard 選擇的欄位產生
    // 審計欄位若啟用，自動附加：
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### Wizard Metadata 格式（.meta.json）

Wizard 產生後保存 metadata JSON，供後續重新編輯 / 重新產生使用：

```json
{
  "programId": "HRMGD01",
  "area": "HR",
  "title": "員工基本資料維護",
  "generatedAt": "2025-01-01T00:00:00Z",
  "generatedBy": "wizard-v2.1",
  "dbProvider": "oracle",
  "masterTable": "HR_EMPLOYEE",
  "detailTable": null,
  "pageMode": "single",
  "columns": [
    {
      "columnName": "EMP_NO",
      "label": "員工編號",
      "dataType": "VARCHAR2",
      "componentType": "Text",
      "usage": "Both",
      "isPk": true,
      "required": true,
      "readonly": false,
      "gridWidth": 120,
      "formWidth": "half"
    }
  ],
  "settings": {
    "enableQueryBar": true,
    "enableExport": true,
    "enableAuditFields": true,
    "pageSize": 50,
    "permissionCode": "HR.EMP.MAINTAIN"
  }
}
```

---

### Wizard 核心服務架構

```
Controllers/
  WizardController.cs          ← Wizard UI 流程控制

Services/Wizard/
  SchemaReaderService.cs       ← 讀取 DB schema / PK / FK / column comment
  WizardSessionService.cs      ← 管理 Wizard 步驟狀態（server session）
  CodeGeneratorService.cs      ← 根據 metadata 產生程式碼字串
  FileWriterService.cs         ← 將產生的程式碼寫入目標路徑
  MetadataSerializerService.cs ← 儲存 / 讀取 .meta.json

Templates/Wizard/
  ListPage.cshtml.template      ← View 清單頁範本
  FormPage.cshtml.template      ← View 表單頁範本
  Controller.cs.template        ← Controller 範本
  ApiController.cs.template     ← API Controller 範本
  QueryService.cs.template      ← Query Service 範本
  CommandService.cs.template    ← Command Service 範本
  Dto.cs.template               ← DTO 範本

Views/Wizard/
  Index.cshtml                  ← Step 1 進入點
  Step1_Connection.cshtml       ← 連線設定
  Step2_TableSelect.cshtml      ← 選擇主表/關聯表
  Step3_ColumnConfig.cshtml     ← 欄位設定
  Step4_ProgramConfig.cshtml    ← 程式設定
  Step5_Preview.cshtml          ← 預覽產生清單
  Step6_Result.cshtml           ← 產生結果
```

---

### Wizard SchemaReader SQL（各資料庫）

**Oracle：**
```sql
-- 取得所有 table
SELECT TABLE_NAME FROM ALL_TABLES WHERE OWNER = :schema ORDER BY TABLE_NAME;

-- 取得 columns（含 comment）
SELECT
    c.COLUMN_NAME, c.DATA_TYPE, c.DATA_LENGTH, c.NULLABLE, c.DATA_DEFAULT,
    cm.COMMENTS AS COLUMN_COMMENT
FROM ALL_TAB_COLUMNS c
LEFT JOIN ALL_COL_COMMENTS cm
    ON cm.OWNER = c.OWNER AND cm.TABLE_NAME = c.TABLE_NAME
       AND cm.COLUMN_NAME = c.COLUMN_NAME
WHERE c.OWNER = :schema AND c.TABLE_NAME = :table
ORDER BY c.COLUMN_ID;

-- 取得 PK
SELECT col.COLUMN_NAME
FROM ALL_CONSTRAINTS con
JOIN ALL_CONS_COLUMNS col ON con.CONSTRAINT_NAME = col.CONSTRAINT_NAME
WHERE con.OWNER = :schema AND con.TABLE_NAME = :table
  AND con.CONSTRAINT_TYPE = 'P';

-- 取得 FK
SELECT
    col.COLUMN_NAME,
    r_col.TABLE_NAME AS REF_TABLE,
    r_col.COLUMN_NAME AS REF_COLUMN
FROM ALL_CONSTRAINTS con
JOIN ALL_CONS_COLUMNS col ON con.CONSTRAINT_NAME = col.CONSTRAINT_NAME
JOIN ALL_CONS_COLUMNS r_col ON con.R_CONSTRAINT_NAME = r_col.CONSTRAINT_NAME
WHERE con.OWNER = :schema AND con.TABLE_NAME = :table
  AND con.CONSTRAINT_TYPE = 'R';
```

**MariaDB / MySQL：**
```sql
-- columns
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT,
       COLUMN_COMMENT, COLUMN_KEY, EXTRA
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = :schema AND TABLE_NAME = :table
ORDER BY ORDINAL_POSITION;

-- FK
SELECT COLUMN_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = :schema AND TABLE_NAME = :table
  AND REFERENCED_TABLE_NAME IS NOT NULL;
```

---

### Wizard 規則

1. Wizard 產生的檔案**不得覆蓋已手動修改的邏輯**，提示使用者確認
2. 所有產生的檔案頂部加上 `// 由 ERP Program Wizard 產生` 標記
3. Wizard 不直接執行 `dotnet build`，產生完成後提示使用者手動驗證
4. `.meta.json` 可重新載入 Wizard，修改設定後重新產生
5. Wizard 不是萬能的，複雜業務邏輯需人工補充到 Service 層
6. Wizard Session 逾時（30分鐘）後自動清除，不保留連線資訊
7. FileWriter 寫入前檢查目標路徑是否在專案目錄內（防路徑穿越）

---

### Wizard Validation Checklist

- [ ] 連線測試成功後才允許進入 Step 2
- [ ] 至少選擇一個主表才允許進入 Step 3
- [ ] 至少有一個 PK 欄位才允許進入 Step 4
- [ ] ProgramId 格式符合命名規範（英數大寫，6~8 字元）
- [ ] 檔案寫入前確認目標路徑合法
- [ ] 產生完成後記錄至 Wizard 操作日誌
- [ ] `dotnet build` 由使用者手動執行驗證

---

## 建議補充的框架能力（Roadmap）

### 必補（影響穩定性）

| 能力 | 說明 |
|------|------|
| **權限模型** | Area / Program / Action / Field 層級 permission |
| **審計欄位** | CreatedBy / CreatedAt / UpdatedBy / UpdatedAt |
| **操作日誌** | 查詢 / 新增 / 修改 / 刪除 / 匯出 |
| **統一 Result Contract** | API / HTMX / Partial response 格式一致 |
| **多資料庫 Provider Factory** | Oracle / MariaDB / SQL Server / SQLite 差異封裝 |

### 建議補（提升開發效率）

| 能力 | 說明 |
|------|------|
| **Program Metadata** | ProgramId / TableName / GridColumns / FormFields / LovConfig |
| **欄位型別映射** | varchar→text / date→datepicker / fk→lov |
| **Schema Metadata Cache** | 減少每次掃描 schema 的開銷 |
| **Code Generator / Scaffold** | 由 table metadata 產生 Grid/Form/API 初稿（✅ 已納入 Program Wizard）|

### 參考設計（長期）

| 參考來源 | 可借鑑重點 |
|----------|-----------|
| **Infolight EEP** | metadata/schema 驅動、wizard 快速產生頁面與服務、欄位層級權限 |
| **DevExpress** | Grid sorting/filtering/column chooser/export、Reporting pipeline、Dashboard widgets |

> 注意：不建議直接引入 DevExpress 商業套件（與現有 Tailwind/Flowbite/HTMX 技術棧衝突）。
> 建議學習其設計理念，以現有 TagHelper 體系實現。

---

## Validation Checklist（每次改動必做）

- [ ] `dotnet build` 無錯誤
- [ ] 開頁面確認 Console 無 Alpine.js / HTMX / JS error
- [ ] CRUD 或查詢流程至少走一次
- [ ] LOV 欄位資料正確顯示與回填
- [ ] 例外拋出時 `<g-error-message>` 正確顯示
- [ ] API 回傳格式符合標準 Result Contract
- [ ] SQL 確認參數化，無字串拼接
- [ ] Grid / Form / LOV 標準事件全部綁定
- [ ] Area 路由正確（不使用 Controller 名稱直接路由）
- [ ] 共用資源無重複載入（`<g-style>` / `<g-js>`）

---

## Anti-Patterns（禁止事項）

以下為本專案明確禁止的開發模式：

| 禁止行為 | 說明 |
|----------|------|
| Controller 直接寫大量 SQL | SQL 應封裝於 DbHelper / Service |
| View 中塞 CRUD 商業邏輯 | 應透過 TagHelper + API |
| 每頁重複實作 Datagrid JS | 應擴充 GDataGridTagHelper |
| 直接 new connection object | 應透過 DbHelper |
| DB 帳密硬寫進程式碼或 skill | 應使用 environment variables / secret store |
| SQL 字串拼接 | 必須參數化 |
| 將 MVC 誤稱為完全前後端分離 SPA | 本專案為 Hybrid MVC + HTMX 架構 |
| 將所有功能塞進單一萬能 TagHelper | 應按功能切分元件 |
| 在多頁重複貼相同 `<script>` / `<link>` | 應使用 `<g-style>` / `<g-js>` |

---

## Notes

- 歷史檔案若有亂碼，先以「**可編譯、可執行、功能正確**」為優先，再逐步清理。
- 若需求與現有元件行為衝突，以**實際程式碼與可維護性**優先，不盲從舊行為。
- 若規格不清楚，優先比對：
  1. 現有 TagHelper 行為
  2. 相似 ERP 程式頁面
  3. API 回傳契約
  4. DB schema 與索引設計
- 不清楚的細節**可以詢問**，或要求提供選項 / 直接看程式碼。
- 實作細節以**程式碼為準**。
