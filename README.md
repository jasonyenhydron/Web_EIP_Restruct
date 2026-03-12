# Web_EIP_Restruct

`Web_EIP_Restruct` 是一個以 ASP.NET Core MVC 為基礎的 ERP / EIP 重構專案，目標是把既有 `Web_EIP_Csharp` 功能逐步收斂到 `SKILL.md` 定義的架構：

- `Area + ProgramId` 路由
- Razor TagHelper 元件化頁面
- `/api/internal/...` 內部 API
- Oracle 為主的資料存取

目前專案已可編譯，且部分模組已從舊式 controller 路由收斂到新的 Area 與 Internal API。

## 技術棧

- .NET 10 (`net10.0`)
- ASP.NET Core MVC
- Razor Views / TagHelpers
- Oracle Managed Data Access Core
- Session-based 驗證
- 前端元件：
  - `g-datagrid`
  - `g-dataform`
  - `g-lov-input`
  - `g-error-message`
  - `g-style`
  - `g-js`

## 專案結構

```text
Areas/
  MIS/
  HRM/
  CMM/
  PRM/
Controllers/
Helpers/
Models/
Services/
Views/
wwwroot/
SKILL.md
Program.cs
```

## 目前主要模組

### 1. 登入與主控台

- 登入：`/Account/Login`
- 主控台：`/Dashboard`
- Dashboard API：`/api/internal/dashboard/summary`

### 2. MIS

- `IDMGD01`：`/MIS/IDMGD01`
- API：
  - `GET /api/internal/grid/IDMGD01/select`
  - `GET /api/internal/form/IDMGD01/{programNo}`
  - `POST /api/internal/form/IDMGD01`
  - `POST /api/internal/grid/IDMGD01/delete`
  - `GET /api/internal/lov/EMPLOYEE`

### 3. HRM

- `HRMGD47`：`/HRM/HRMGD47`
- API：
  - `GET /api/internal/form/HRMGD47`
  - `GET /api/internal/grid/HRMGD47`
  - `POST /api/internal/form/HRMGD47`
  - `PUT /api/internal/form/HRMGD47/{id}`
  - `DELETE /api/internal/form/HRMGD47/{id}`

### 4. CMM

- `CMMGD16`：`/CMM/CMMGD16`
- API：
  - `GET /api/internal/grid/CMMGD16/select`
  - `POST /api/internal/grid/CMMGD16/insert`
  - `POST /api/internal/grid/CMMGD16/update`
  - `POST /api/internal/grid/CMMGD16/delete`

### 5. PRM

- `PRMPQ11`：`/PRM/PRMPQ11`
- API：
  - `GET /api/internal/grid/PRMPQ11/select`
  - `GET /api/internal/grid/PRMPQ11/detail`

### 6. 其他功能

- 檔案上傳：`POST /api/files/upload`
- LOV：
  - `GET /api/lov/query`
  - `GET /api/lov/hrm/employees`
  - `GET /api/lov/hrm/leave-types`
  - `GET /api/lov/hrm/booking-departments`
- Oracle 操作：
  - `/api/oracle/proc/execute`
  - `/api/oracle/func/execute`
  - `/api/oracle/job/*`

## 啟動方式

### 1. 還原與建置

```powershell
dotnet restore
dotnet build
```

### 2. 執行

```powershell
dotnet run
```

預設開發網址請參考 `Properties/launchSettings.json`：

- `http://localhost:5218`
- `https://localhost:7120`

## 設定

主要設定檔：

- `appsettings.json`
- `appsettings.Development.json`

目前 `Database:Provider` 預設為 Oracle，並使用以下連線字串：

- `ConnectionStrings:oracleConn`
- `ConnectionStrings:oracleConn_MIS`
- `ConnectionStrings:oracleConn_TEST`

`DbHelper` 會依 Session 內的 `tns` 自動切換對應 Oracle 連線。

## 驗證與 Session

登入成功後，Session 會保存：

- `username`
- `password`
- `tns`
- `user_name`

多數 Area 頁面與 Internal API 都套用了 `RequireLogon`。

## 架構現況

目前專案屬於「重構進行中」狀態。

### 已完成

- 建立 `Area + ProgramId` 入口
- 建立 Internal API 別名
- 建立共用 TagHelper 元件
- 修正多個因 legacy route 與 area route 並存造成的 endpoint 衝突
- `IDMGD01` datagrid / form 已改為查真 DB，不再使用 demo grid 資料

### 尚未完全收斂

- 部分舊 controller 仍保留 legacy SQL 邏輯
- Dashboard 仍使用 demo repository
- 並非所有頁面都已完全改寫成 `Areas/{Area}/Views/{ProgramId}/...`
- 部分頁面仍保留 inline script 與舊命名方式

## 與 `SKILL.md` 的關係

本專案開發以 `SKILL.md` 為準，目標方向包括：

- View 儘量透過 TagHelper 組裝
- 路由往 `/{area}/{programId}` 收斂
- CRUD 逐步改為 `/api/internal/grid|form|lov/...`
- 資料存取集中，不在 View 直接碰資料庫

目前是「部分對齊」，不是最終完成版。

## 開發注意事項

- 修改功能前請先閱讀 `SKILL.md`
- 避免新增新的 legacy route
- 新功能優先放在 `Areas/{Area}` 結構下
- 新查詢與儲存 API 優先用 `/api/internal/...`
- 若畫面有載到舊內容，先確認是否為舊站台程序未重啟

## 已知限制

- Oracle 套件 `Oracle.ManagedDataAccess.Core` 必須可正常還原
- 若目前執行中的站台未重啟，瀏覽器可能仍看到舊版本行為
- 某些模組仍可能混用新舊資料流

## 後續建議

1. 將 Dashboard 從 `DemoProgramRepository` 改成真 DB
2. 持續移除 legacy controller 中直接寫 SQL 的邏輯
3. 將 `Views/MisPrograms` 逐步遷移到 `Areas/{Area}/Views/{ProgramId}`
4. 統一 `LOV / Grid / Form` 的 Internal API 命名與回傳格式

