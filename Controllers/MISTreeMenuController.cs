
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models.Lov;
using Web_EIP_Restruct.Models.MISTreeMenu;
using Web_EIP_Restruct.Models.ViewModels;

namespace Web_EIP_Restruct.Controllers
{
    public class MISTreeMenuController : Controller
    {
        private static string BuildDbConnectionString(string tns) =>
            DbHelper.BuildConnectionString(tns);

        private static LovInputConfig BuildEmployeeLovConfig()
        {
            var employeeLovSql = Uri.EscapeDataString(@"
SELECT employee_id, employee_no, employee_name
FROM (
    SELECT employee_id, employee_no, employee_name,
           ROW_NUMBER() OVER (ORDER BY employee_no ASC) AS rn
    FROM hrm_employee_v
    WHERE (UPPER(employee_no) LIKE :q OR UPPER(employee_name) LIKE :q)
)
WHERE rn > :offset AND rn <= :endRow");

            return new LovInputConfig
            {
                Title = "員工資料 (Employee)",
                Api = $"/api/lov/query?sql={employeeLovSql}",
                Columns = "員工編號,員工姓名,ID",
                Fields = "employee_no,employee_name,employee_id",
                KeyHidden = "employee_id",
                KeyCode = "employee_id",
                DisplayFormat = "{employee_id} {employee_name}",
                SortEnabled = true,
                BufferView = true,
                PageSize = 50
            };
        }

        [HttpGet("mis/programs")]
        public async Task<IActionResult> Index(string program_no, string employee_id, string display_code = "Y")
        {
            var vm = new MisProgramsViewModel
            {
                EmployeeLov = BuildEmployeeLovConfig()
            };

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return RedirectToAction("Login", "Account");

            try
            {
                var programs = await QueryProgramsAsync(tns, program_no, employee_id, display_code);
                var categories = BuildCategories(programs);

                ViewBag.Categories = categories;
                ViewBag.Programs = programs;
                ViewBag.ProgramNoFilter = program_no ?? string.Empty;
                ViewBag.EmployeeIdFilter = employee_id ?? string.Empty;
                ViewBag.DisplayCodeFilter = display_code ?? "Y";
                ViewBag.UserName = HttpContext.Session.GetString("user_name") ?? username;

                return View("~/Views/MisPrograms/MISTreeMenu.cshtml", vm);
            }
            catch (Exception e)
            {
                ViewBag.Error = $"系統錯誤: {e.Message}";
                ViewBag.Programs = new List<IdmProgramVModel>();
                return View("~/Views/MisPrograms/MISTreeMenu.cshtml", vm);
            }
        }

        [HttpGet("api/mis/programs/search")]
        public async Task<IActionResult> SearchPrograms([FromQuery] string program_no, [FromQuery] string employee_id, [FromQuery] string display_code = "Y")
        {
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                var programs = await QueryProgramsAsync(tns, program_no, employee_id, display_code);
                var categories = BuildCategories(programs);

                var result = categories
                    .Select(x => new
                    {
                        name = x.Key,
                        count = x.Value.Count,
                        programs = x.Value.Select(p => new
                        {
                            programNo = p.ProgramNo,
                            programName = p.ProgramName,
                            purpose = p.Purpose,
                            employeeId = p.EmployeeId,
                            programType = p.ProgramType,
                            planStart = p.PlanStartDevelopDate,
                            planFinish = p.PlanFinishDevelopDate,
                            realStart = p.RealStartDevelopDate,
                            realFinish = p.RealFinishDevelopDate,
                            planHours = p.PlanWorkHours,
                            realHours = p.RealWorkHours,
                            displayCode = p.DisplayCode
                        })
                    });

                return Json(new
                {
                    status = "success",
                    count = programs.Count,
                    filters = new
                    {
                        program_no = program_no ?? string.Empty,
                        employee_id = employee_id ?? string.Empty,
                        display_code = display_code ?? "Y"
                    },
                    categories = result
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = "error", message = e.Message });
            }
        }

        [HttpGet("api/mis/programs/suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 1 || q.Length > 50)
                return Json(new List<object>());

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                const string sql = @"
                    SELECT PROGRAM_NO, PROGRAM_NAME
                    FROM (
                        SELECT PROGRAM_NO, PROGRAM_NAME
                        FROM idm_program_v
                        WHERE (UPPER(PROGRAM_NO) LIKE :q OR UPPER(PROGRAM_NAME) LIKE :q)
                          AND LANGUAGE_ID = 1
                        ORDER BY PROGRAM_NO ASC
                    )
                    WHERE ROWNUM <= 10";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("q", $"%{q.ToUpper()}%")
                    });

                var list = dt.Rows.Cast<DataRow>()
                    .Select(row => new
                    {
                        program_no = row["PROGRAM_NO"]?.ToString() ?? string.Empty,
                        program_name = row["PROGRAM_NAME"]?.ToString() ?? string.Empty
                    })
                    .ToList();

                return Json(list);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = "error", message = e.Message });
            }
        }

        [HttpGet("api/mis/programs/{program_no}")]
        public async Task<IActionResult> GetProgramDetail(string program_no)
        {
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                const string sql = @"
                    SELECT LANGUAGE_ID, PROGRAM_ID, PROGRAM_NO, PROGRAM_NAME,
                           EMPLOYEE_ID, PURPOSE,
                           PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                           REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                           PLAN_WORK_HOURS, REAL_WORK_HOURS,
                           DISPLAY_CODE, PROGRAM_TYPE
                    FROM idm_program_v
                    WHERE program_no = :program_no
                      AND LANGUAGE_ID = 1";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_no", program_no.ToUpperInvariant())
                    });

                if (dt.Rows.Count == 0)
                    return NotFound(new { status = "error", message = "Program not found" });

                var result = MapIdmProgramV(dt.Rows[0]);
                return Json(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = "error", message = e.Message });
            }
        }

        private static IdmProgramVModel MapIdmProgramV(DataRow row)
        {
            return new IdmProgramVModel
            {
                LanguageId = ToNullableInt(row, "LANGUAGE_ID"),
                ProgramId = ToNullableLong(row, "PROGRAM_ID"),
                ProgramNo = ToStringOrEmpty(row, "PROGRAM_NO"),
                ProgramName = ToStringOrEmpty(row, "PROGRAM_NAME"),
                EmployeeId = ToStringOrEmpty(row, "EMPLOYEE_ID"),
                Purpose = ToStringOrEmpty(row, "PURPOSE"),
                PlanStartDevelopDate = ToStringOrEmpty(row, "PLAN_START_DEVELOP_DATE"),
                PlanFinishDevelopDate = ToStringOrEmpty(row, "PLAN_FINISH_DEVELOP_DATE"),
                RealStartDevelopDate = ToStringOrEmpty(row, "REAL_START_DEVELOP_DATE"),
                RealFinishDevelopDate = ToStringOrEmpty(row, "REAL_FINISH_DEVELOP_DATE"),
                PlanWorkHours = ToNullableDecimal(row, "PLAN_WORK_HOURS"),
                RealWorkHours = ToNullableDecimal(row, "REAL_WORK_HOURS"),
                DisplayCode = ToStringOrEmpty(row, "DISPLAY_CODE"),
                ProgramType = ToStringOrEmpty(row, "PROGRAM_TYPE")
            };
        }

        private static async Task<List<IdmProgramVModel>> QueryProgramsAsync(string tns, string? programNo, string? employeeId, string? displayCode)
        {
            const string sql = @"
                SELECT LANGUAGE_ID, PROGRAM_ID, PROGRAM_NO, PROGRAM_NAME,
                       EMPLOYEE_ID, PURPOSE,
                       PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                       REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                       PLAN_WORK_HOURS, REAL_WORK_HOURS,
                       DISPLAY_CODE, PROGRAM_TYPE
                FROM idm_program_v
                WHERE program_no LIKE :program_no || '%'
                  AND (employee_id = :employee_id OR :employee_id IS NULL)
                  AND (display_code = :display_code OR :display_code IS NULL)
                  AND LANGUAGE_ID = 1
                ORDER BY program_no";

            var dt = await DbHelper.GetDataTableAsync(
                BuildDbConnectionString(tns),
                CommandType.Text,
                sql,
                new DbParameter[]
                {
                    DbHelper.CreateParameter("program_no", string.IsNullOrEmpty(programNo) ? string.Empty : programNo),
                    DbHelper.CreateParameter("employee_id", string.IsNullOrEmpty(employeeId) ? (object)DBNull.Value : employeeId),
                    DbHelper.CreateParameter("display_code", string.IsNullOrEmpty(displayCode) ? (object)DBNull.Value : displayCode)
                });

            return dt.Rows.Cast<DataRow>().Select(MapIdmProgramV).ToList();
        }

        private static Dictionary<string, List<IdmProgramVModel>> BuildCategories(IEnumerable<IdmProgramVModel> programs)
        {
            var categories = new Dictionary<string, List<IdmProgramVModel>>();
            var systemNames = new Dictionary<string, string>
            {
                { "HRM", "HRM 系統" }, { "FIN", "FIN 系統" }, { "INV", "INV 系統" }, { "PUR", "PUR 系統" },
                { "SAL", "SAL 系統" }, { "MFG", "MFG 系統" }, { "SDM", "SDM 系統" }, { "IDM", "IDM 系統" }
            };

            foreach (var program in programs)
            {
                var progNo = program.ProgramNo ?? string.Empty;
                if (progNo.Length >= 3)
                {
                    var prefix = progNo.Substring(0, 3);
                    var systemName = systemNames.TryGetValue(prefix, out var mapped) ? mapped : $"{prefix} 系統";
                    if (!categories.ContainsKey(systemName))
                        categories[systemName] = new List<IdmProgramVModel>();
                    categories[systemName].Add(program);
                }
                else
                {
                    if (!categories.ContainsKey("未分類"))
                        categories["未分類"] = new List<IdmProgramVModel>();
                    categories["未分類"].Add(program);
                }
            }

            return categories;
        }

        private static string ToStringOrEmpty(DataRow row, string column) =>
            row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? row[column]?.ToString() ?? string.Empty
                : string.Empty;

        private static int? ToNullableInt(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return null;
            return int.TryParse(row[column]?.ToString(), out var v) ? v : null;
        }

        private static long? ToNullableLong(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return null;
            return long.TryParse(row[column]?.ToString(), out var v) ? v : null;
        }

        private static decimal? ToNullableDecimal(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return null;
            return decimal.TryParse(row[column]?.ToString(), out var v) ? v : null;
        }
    }
}

