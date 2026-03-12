using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models.Lov;
using Web_EIP_Restruct.Models.ViewModels;

namespace Web_EIP_Restruct.Controllers
{
    public class IdmController : Controller
    {
        private static readonly Dictionary<string, string> IdmQueryFieldMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["programNo"] = "PROGRAM_NO",
            ["employeeId"] = "EMPLOYEE_ID",
            ["displayCode"] = "DISPLAY_CODE",
            ["programType"] = "PROGRAM_TYPE",
            ["purpose"] = "PURPOSE",
            ["programId"] = "PROGRAM_ID",
            ["vendorId"] = "VENDOR_ID",
            ["personId"] = "PERSON_ID",
            ["entryId"] = "ENTRY_ID",
            ["trId"] = "TR_ID",
            ["planStartDevelopDate"] = "PLAN_START_DEVELOP_DATE",
            ["planFinishDevelopDate"] = "PLAN_FINISH_DEVELOP_DATE",
            ["realStartDevelopDate"] = "REAL_START_DEVELOP_DATE",
            ["realFinishDevelopDate"] = "REAL_FINISH_DEVELOP_DATE",
            ["planWorkHours"] = "PLAN_WORK_HOURS",
            ["realWorkHours"] = "REAL_WORK_HOURS"
        };

        private static readonly HashSet<string> IdmQueryAllowedColumns = new(
            IdmQueryFieldMap.Values.Concat(new[]
            {
                "ROWID",
                "PROGRAM_ID",
                "PROGRAM_NO",
                "DISPLAY_CODE",
                "PURPOSE",
                "PROGRAM_TYPE",
                "EMPLOYEE_ID",
                "VENDOR_ID",
                "PERSON_ID",
                "ENTRY_ID",
                "TR_ID",
                "PLAN_START_DEVELOP_DATE",
                "PLAN_FINISH_DEVELOP_DATE",
                "REAL_START_DEVELOP_DATE",
                "REAL_FINISH_DEVELOP_DATE",
                "PLAN_WORK_HOURS",
                "REAL_WORK_HOURS"
            }),
            StringComparer.OrdinalIgnoreCase);

        private sealed class RoleFunctionColumnMeta
        {
            public string SelectExpr { get; set; } = "CAST(NULL AS VARCHAR2(100)) AS FUNCTION_NO";
            public string OrderExpr { get; set; } = "ROLE_NO";
            public string UpdateClause { get; set; } = string.Empty;
            public string InsertColumn { get; set; } = string.Empty;
            public string InsertValue { get; set; } = string.Empty;
            public string ParameterName { get; set; } = string.Empty;
            public bool UseNumericParameter { get; set; } = false;
        }

        private sealed class SelectPredicate
        {
            public string Key { get; set; } = string.Empty;
            public string Column { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Condition { get; set; } = "=";
            public string AndOr { get; set; } = "AND";
            public string DataType { get; set; } = "string";
        }

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
                Title = "撠?∪極 (Employee)",
                Api = $"/api/lov/query?sql={employeeLovSql}",
                Columns = "蝺刻?,?迂,ID",
                Fields = "employee_no,employee_name,employee_id",
                KeyHidden = "employee_id",
                KeyCode = "employee_no",
                KeyName = "employee_name",
                BufferView = true,
                PageSize = 50,
                SortEnabled = true
            };
        }

        private static LovInputConfig BuildEmployeeLovConfigClean()
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
                Title = "?∪極鞈? (Employee)",
                Api = $"/api/lov/query?sql={employeeLovSql}",
                Columns = "?∪極蝺刻?,?∪極憪?,ID",
                Fields = "employee_no,employee_name,employee_id",
                KeyHidden = "employee_id",
                KeyCode = "employee_no",
                KeyName = "employee_name",
                BufferView = true,
                PageSize = 50,
                SortEnabled = true
            };
        }

        [HttpGet("Idm/IDMGD01")]
        public IActionResult IDMGD01()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");
            return RedirectToAction("Index", "IDMGD01", new { area = "MIS" });
        }

        [HttpGet("Idm/select")]
        public async Task<IActionResult> Select(
            string DataMember,
            string? programNo = null,
            string? employeeId = null,
            string? displayCode = null)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                var sql = new StringBuilder(@"
                    SELECT ROWID, PROGRAM_ID, PURPOSE, EMPLOYEE_ID, VENDOR_ID, PERSON_ID,
                           PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                           REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                           PLAN_WORK_HOURS, REAL_WORK_HOURS,
                           ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE,
                           PROGRAM_NO, DISPLAY_CODE, PROGRAM_TYPE
                    FROM idm_program
                    WHERE 1=1");

                var predicates = BuildSelectPredicates(Request.Query);
                AddLegacyPredicate(predicates, "programNo", programNo, "%");
                AddLegacyPredicate(predicates, "employeeId", employeeId, "=");
                AddLegacyPredicate(predicates, "displayCode", displayCode, "=");

                var whereSegment = BuildWhereSegment(predicates, out var dbParameters);
                if (!string.IsNullOrWhiteSpace(whereSegment))
                {
                    sql.Append(" AND (");
                    sql.Append(whereSegment);
                    sql.Append(")");
                }
                sql.Append(" ORDER BY program_no");

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql.ToString(),
                    dbParameters.ToArray());

                return Ok(new { status = "success", data = DataTableToRows(dt) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/update")]
        public async Task<IActionResult> Update(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null || !payload.ContainsKey("ROWID"))
                return BadRequest(new { status = "error", message = "Missing ROWID" });

            try
            {
                var rowId = GetString(payload, "ROWID");
                var displayCode = GetString(payload, "DISPLAY_CODE");
                var purpose = GetString(payload, "PURPOSE");
                var programType = GetString(payload, "PROGRAM_TYPE");
                var employeeId = GetString(payload, "EMPLOYEE_ID");
                var planStart = GetDateString(payload, "PLAN_START_DEVELOP_DATE");
                var planFinish = GetDateString(payload, "PLAN_FINISH_DEVELOP_DATE");
                var realStart = GetDateString(payload, "REAL_START_DEVELOP_DATE");
                var realFinish = GetDateString(payload, "REAL_FINISH_DEVELOP_DATE");
                var planHours = GetDecimal(payload, "PLAN_WORK_HOURS");
                var realHours = GetDecimal(payload, "REAL_WORK_HOURS");

                const string sql = @"
                    UPDATE idm_program SET
                        DISPLAY_CODE              = :display_code,
                        PURPOSE                   = :purpose,
                        PROGRAM_TYPE              = :program_type,
                        EMPLOYEE_ID               = :employee_id,
                        PLAN_START_DEVELOP_DATE   = TO_DATE(:plan_start,  'YYYY-MM-DD'),
                        PLAN_FINISH_DEVELOP_DATE  = TO_DATE(:plan_finish, 'YYYY-MM-DD'),
                        REAL_START_DEVELOP_DATE   = TO_DATE(:real_start,  'YYYY-MM-DD'),
                        REAL_FINISH_DEVELOP_DATE  = TO_DATE(:real_finish, 'YYYY-MM-DD'),
                        PLAN_WORK_HOURS           = :plan_hours,
                        REAL_WORK_HOURS           = :real_hours,
                        TR_ID                     = :tr_id,
                        TR_DATE                   = SYSDATE
                    WHERE ROWID = :row_id";

                var rows = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("display_code", string.IsNullOrEmpty(displayCode) ? (object)DBNull.Value : displayCode),
                        DbHelper.CreateParameter("purpose", string.IsNullOrEmpty(purpose) ? (object)DBNull.Value : purpose),
                        DbHelper.CreateParameter("program_type", string.IsNullOrEmpty(programType) ? (object)DBNull.Value : programType),
                        DbHelper.CreateParameter("employee_id", string.IsNullOrEmpty(employeeId) ? (object)DBNull.Value : employeeId),
                        DbHelper.CreateParameter("plan_start", string.IsNullOrEmpty(planStart) ? (object)DBNull.Value : planStart),
                        DbHelper.CreateParameter("plan_finish", string.IsNullOrEmpty(planFinish) ? (object)DBNull.Value : planFinish),
                        DbHelper.CreateParameter("real_start", string.IsNullOrEmpty(realStart) ? (object)DBNull.Value : realStart),
                        DbHelper.CreateParameter("real_finish", string.IsNullOrEmpty(realFinish) ? (object)DBNull.Value : realFinish),
                        DbHelper.CreateParameter("plan_hours", planHours.HasValue ? (object)planHours.Value : DBNull.Value),
                        DbHelper.CreateParameter("real_hours", realHours.HasValue ? (object)realHours.Value : DBNull.Value),
                        DbHelper.CreateParameter("tr_id", username),
                        DbHelper.CreateParameter("row_id", rowId)
                    });

                if (rows == 0)
                    return NotFound(new { status = "error", message = "Record not found" });

                return Ok(new { status = "success", message = "Saved OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/insert")]
        public async Task<IActionResult> Insert(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var programNo = GetString(payload, "PROGRAM_NO")?.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(programNo))
                return BadRequest(new { status = "error", message = "PROGRAM_NO is required" });

            try
            {
                var conn = BuildDbConnectionString(tns);
                var dupObj = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    "SELECT COUNT(1) FROM idm_program WHERE PROGRAM_NO = :program_no",
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_no", programNo)
                    });
                if (Convert.ToInt32(dupObj) > 0)
                    return Conflict(new { status = "error", message = $"PROGRAM_NO {programNo} already exists" });

                var idObj = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    "SELECT NVL(MAX(PROGRAM_ID), 0) + 1 FROM idm_program");
                var nextProgramId = Convert.ToInt64(idObj);

                const string sql = @"
                    INSERT INTO idm_program (
                        PROGRAM_ID, PROGRAM_NO, DISPLAY_CODE, PURPOSE, PROGRAM_TYPE, EMPLOYEE_ID,
                        PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                        REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                        PLAN_WORK_HOURS, REAL_WORK_HOURS,
                        ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
                    ) VALUES (
                        :program_id, :program_no, :display_code, :purpose, :program_type, :employee_id,
                        TO_DATE(:plan_start,  'YYYY-MM-DD'), TO_DATE(:plan_finish, 'YYYY-MM-DD'),
                        TO_DATE(:real_start,  'YYYY-MM-DD'), TO_DATE(:real_finish, 'YYYY-MM-DD'),
                        :plan_hours, :real_hours,
                        :entry_id, SYSDATE, :tr_id, SYSDATE
                    )";

                await DbHelper.ExecuteNonQueryAsync(
                    conn,
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_id", nextProgramId),
                        DbHelper.CreateParameter("program_no", programNo),
                        DbHelper.CreateParameter("display_code", ToDb(GetString(payload, "DISPLAY_CODE") ?? "Y")),
                        DbHelper.CreateParameter("purpose", ToDb(GetString(payload, "PURPOSE"))),
                        DbHelper.CreateParameter("program_type", ToDb(GetString(payload, "PROGRAM_TYPE"))),
                        DbHelper.CreateParameter("employee_id", ToDb(GetString(payload, "EMPLOYEE_ID"))),
                        DbHelper.CreateParameter("plan_start", ToDb(GetDateString(payload, "PLAN_START_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("plan_finish", ToDb(GetDateString(payload, "PLAN_FINISH_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("real_start", ToDb(GetDateString(payload, "REAL_START_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("real_finish", ToDb(GetDateString(payload, "REAL_FINISH_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("plan_hours", ToDb(GetDecimal(payload, "PLAN_WORK_HOURS"))),
                        DbHelper.CreateParameter("real_hours", ToDb(GetDecimal(payload, "REAL_WORK_HOURS"))),
                        DbHelper.CreateParameter("entry_id", username),
                        DbHelper.CreateParameter("tr_id", username)
                    });

                return Ok(new { status = "success", message = "Inserted OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/delete")]
        public async Task<IActionResult> Delete(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var rowId = GetString(payload, "ROWID");
            var programNo = GetString(payload, "PROGRAM_NO");
            if (string.IsNullOrWhiteSpace(rowId) && string.IsNullOrWhiteSpace(programNo))
                return BadRequest(new { status = "error", message = "ROWID or PROGRAM_NO is required" });

            try
            {
                string sql;
                DbParameter[] ps;
                if (!string.IsNullOrWhiteSpace(rowId))
                {
                    sql = "DELETE FROM idm_program WHERE ROWID = :row_id";
                    ps = new DbParameter[] { DbHelper.CreateParameter("row_id", rowId) };
                }
                else
                {
                    sql = "DELETE FROM idm_program WHERE PROGRAM_NO = :program_no";
                    ps = new DbParameter[] { DbHelper.CreateParameter("program_no", programNo!.ToUpperInvariant()) };
                }

                var affected = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    ps);

                if (affected == 0)
                    return NotFound(new { status = "error", message = "Record not found" });

                return Ok(new { status = "success", message = "Deleted OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("Idm/selectRoleFunctionPrograms")]
        public async Task<IActionResult> SelectRoleFunctionPrograms(string DataMember, long? programId, int positionNo = 1)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            if (!programId.HasValue)
                return Ok(new { status = "success", data = new List<Dictionary<string, object>>() });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                var conn = BuildDbConnectionString(tns);
                var rows = await QueryRoleFunctionProgramsAsync(conn, programId.Value, positionNo);
                return Ok(new { status = "success", data = rows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/upsertRoleFunctionProgram")]
        public async Task<IActionResult> UpsertRoleFunctionProgram(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var programId = GetLong(payload, "PROGRAM_ID");
            if (!programId.HasValue)
                return BadRequest(new { status = "error", message = "PROGRAM_ID is required" });

            var rowId = GetString(payload, "ROWID");
            var positionNo = GetLong(payload, "POSITION_NO") ?? 1;
            var roleId = GetLong(payload, "ROLE_ID");
            var roleNo = GetString(payload, "ROLE_NO");
            var roleName = GetString(payload, "ROLE_NAME");
            var functionNo = GetString(payload, "FUNCTION_NO");
            var functionId = ParseNullableLong(functionNo);
            var displayOrder = GetLong(payload, "DISPLAY_ORDER");
            var displayColor = GetString(payload, "DISPLAY_COLOR");

            try
            {
                var conn = BuildDbConnectionString(tns);
                var funcMeta = await ResolveRoleFunctionColumnMetaAsync(conn);
                if (!string.IsNullOrWhiteSpace(rowId))
                {
                    var updateSql = $@"
                        UPDATE idm_role_function_program SET
                            ROLE_ID = :role_id,
                            ROLE_NO = :role_no,
                            ROLE_NAME = :role_name,
                            {funcMeta.UpdateClause}
                            DISPLAY_ORDER = :display_order,
                            DISPLAY_COLOR = :display_color,
                            POSITION_NO = :position_no,
                            TR_ID = :tr_id,
                            TR_DATE = SYSDATE
                        WHERE ROWID = :row_id";

                    var updateParams = new List<DbParameter>
                    {
                        DbHelper.CreateParameter("role_id", ToDb(roleId)),
                        DbHelper.CreateParameter("role_no", ToDb(roleNo)),
                        DbHelper.CreateParameter("role_name", ToDb(roleName)),
                        DbHelper.CreateParameter("display_order", ToDb(displayOrder)),
                        DbHelper.CreateParameter("display_color", ToDb(displayColor)),
                        DbHelper.CreateParameter("position_no", positionNo),
                        DbHelper.CreateParameter("tr_id", username),
                        DbHelper.CreateParameter("row_id", rowId)
                    };
                    if (!string.IsNullOrEmpty(funcMeta.ParameterName))
                    {
                        updateParams.Add(DbHelper.CreateParameter(
                            funcMeta.ParameterName,
                            funcMeta.UseNumericParameter ? ToDb(functionId) : ToDb(functionNo)));
                    }

                    var affected = await DbHelper.ExecuteNonQueryAsync(
                        conn,
                        CommandType.Text,
                        updateSql,
                        updateParams.ToArray());

                    if (affected == 0)
                        return NotFound(new { status = "error", message = "Detail record not found" });
                }
                else
                {
                    var nextIdObj = await DbHelper.ExecuteScalarAsync(
                        conn,
                        CommandType.Text,
                        "SELECT NVL(MAX(ROLE_FUNCTION_ID), 0) + 1 FROM idm_role_function_program");
                    var nextId = Convert.ToInt64(nextIdObj);

                    var insertSql = $@"
                        INSERT INTO idm_role_function_program (
                            ROLE_FUNCTION_ID, PROGRAM_ID, POSITION_NO,
                            ROLE_ID, ROLE_NO, ROLE_NAME{funcMeta.InsertColumn}, DISPLAY_ORDER, DISPLAY_COLOR,
                            ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
                        ) VALUES (
                            :role_function_id, :program_id, :position_no,
                            :role_id, :role_no, :role_name{funcMeta.InsertValue}, :display_order, :display_color,
                            :entry_id, SYSDATE, :tr_id, SYSDATE
                        )";

                    var insertParams = new List<DbParameter>
                    {
                        DbHelper.CreateParameter("role_function_id", nextId),
                        DbHelper.CreateParameter("program_id", programId.Value),
                        DbHelper.CreateParameter("position_no", positionNo),
                        DbHelper.CreateParameter("role_id", ToDb(roleId)),
                        DbHelper.CreateParameter("role_no", ToDb(roleNo)),
                        DbHelper.CreateParameter("role_name", ToDb(roleName)),
                        DbHelper.CreateParameter("display_order", ToDb(displayOrder)),
                        DbHelper.CreateParameter("display_color", ToDb(displayColor)),
                        DbHelper.CreateParameter("entry_id", username),
                        DbHelper.CreateParameter("tr_id", username)
                    };
                    if (!string.IsNullOrEmpty(funcMeta.ParameterName))
                    {
                        insertParams.Add(DbHelper.CreateParameter(
                            funcMeta.ParameterName,
                            funcMeta.UseNumericParameter ? ToDb(functionId) : ToDb(functionNo)));
                    }

                    await DbHelper.ExecuteNonQueryAsync(
                        conn,
                        CommandType.Text,
                        insertSql,
                        insertParams.ToArray());
                }

                return Ok(new { status = "success", message = "Saved OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/deleteRoleFunctionProgram")]
        public async Task<IActionResult> DeleteRoleFunctionProgram(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var rowId = GetString(payload, "ROWID");
            var roleFunctionId = GetLong(payload, "ROLE_FUNCTION_ID");
            if (string.IsNullOrWhiteSpace(rowId) && !roleFunctionId.HasValue)
                return BadRequest(new { status = "error", message = "ROWID or ROLE_FUNCTION_ID is required" });

            try
            {
                string sql;
                DbParameter[] ps;

                if (!string.IsNullOrWhiteSpace(rowId))
                {
                    sql = "DELETE FROM idm_role_function_program WHERE ROWID = :row_id";
                    ps = new[] { DbHelper.CreateParameter("row_id", rowId) };
                }
                else
                {
                    sql = "DELETE FROM idm_role_function_program WHERE ROLE_FUNCTION_ID = :role_function_id";
                    ps = new[] { DbHelper.CreateParameter("role_function_id", roleFunctionId!.Value) };
                }

                var affected = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    ps);

                if (affected == 0)
                    return NotFound(new { status = "error", message = "Detail record not found" });

                return Ok(new { status = "success", message = "Deleted OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private static string? GetString(Dictionary<string, object> payload, string key) =>
            payload.ContainsKey(key) ? payload[key]?.ToString() : null;

        private static List<SelectPredicate> BuildSelectPredicates(IQueryCollection query)
        {
            var result = new List<SelectPredicate>();
            var keys = query.Keys.ToList();
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (key.Equals("DataMember", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.EndsWith("_condition", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.EndsWith("_andOr", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.EndsWith("_dataType", StringComparison.OrdinalIgnoreCase)) continue;

                var rawValue = query[key].ToString();
                if (string.IsNullOrWhiteSpace(rawValue)) continue;
                if (!TryResolveColumnName(key, out var columnName)) continue;

                var condition = query[$"{key}_condition"].ToString();
                var andOr = query[$"{key}_andOr"].ToString();
                var dataType = query[$"{key}_dataType"].ToString();

                result.Add(new SelectPredicate
                {
                    Key = key,
                    Column = columnName,
                    Value = rawValue,
                    Condition = NormalizeCondition(condition, key),
                    AndOr = NormalizeAndOr(andOr),
                    DataType = NormalizeDataType(dataType)
                });
            }
            return result;
        }

        private static void AddLegacyPredicate(List<SelectPredicate> predicates, string key, string? value, string defaultCondition)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (predicates.Any(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase))) return;
            if (!TryResolveColumnName(key, out var columnName)) return;

            predicates.Add(new SelectPredicate
            {
                Key = key,
                Column = columnName,
                Value = value!,
                Condition = defaultCondition,
                AndOr = "AND",
                DataType = "string"
            });
        }

        private static string BuildWhereSegment(List<SelectPredicate> predicates, out List<DbParameter> parameters)
        {
            parameters = new List<DbParameter>();
            if (predicates.Count == 0) return string.Empty;

            var where = new StringBuilder();
            var idx = 0;
            foreach (var p in predicates)
            {
                if (!TryBuildClause(p, idx, out var clause, out var dbValue))
                    continue;

                if (where.Length > 0)
                {
                    where.Append(' ');
                    where.Append(p.AndOr);
                    where.Append(' ');
                }

                where.Append(clause);
                parameters.Add(DbHelper.CreateParameter($"q_{idx}", dbValue));
                idx++;
            }
            return where.ToString();
        }

        private static bool TryBuildClause(SelectPredicate predicate, int idx, out string clause, out object value)
        {
            clause = string.Empty;
            value = DBNull.Value;

            if (string.IsNullOrWhiteSpace(predicate.Value))
                return false;

            var paramName = $"q_{idx}";
            var op = NormalizeCondition(predicate.Condition, predicate.Key);
            if (op is "%" or "%%")
            {
                var raw = predicate.Value.Trim();
                value = op == "%" ? (raw.EndsWith('%') ? raw : raw + "%") : $"%{raw.Trim('%')}%";
                clause = $"{predicate.Column} LIKE :{paramName}";
                return true;
            }

            if (!TryConvertPredicateValue(predicate.Value, predicate.DataType, out var converted))
                return false;

            value = converted;
            clause = $"{predicate.Column} {op} :{paramName}";
            return true;
        }

        private static bool TryConvertPredicateValue(string value, string dataType, out object converted)
        {
            converted = value;
            switch (NormalizeDataType(dataType))
            {
                case "number":
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ||
                        decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out dec))
                    {
                        converted = dec;
                        return true;
                    }
                    return false;

                case "datetime":
                    if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt) ||
                        DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
                    {
                        converted = dt;
                        return true;
                    }
                    return false;

                case "guid":
                    if (Guid.TryParse(value, out var guid))
                    {
                        converted = guid.ToString();
                        return true;
                    }
                    return false;

                default:
                    converted = value;
                    return true;
            }
        }

        private static bool TryResolveColumnName(string key, out string columnName)
        {
            columnName = string.Empty;
            if (string.IsNullOrWhiteSpace(key)) return false;

            if (IdmQueryFieldMap.TryGetValue(key, out var mapped))
            {
                columnName = mapped;
                return true;
            }

            var upper = key.Trim().ToUpperInvariant();
            if (!IdmQueryAllowedColumns.Contains(upper)) return false;

            columnName = upper;
            return true;
        }

        private static string NormalizeCondition(string? condition, string? key = null)
        {
            var c = (condition ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(c))
            {
                if (!string.IsNullOrWhiteSpace(key) && key.Equals("programNo", StringComparison.OrdinalIgnoreCase))
                    return "%";
                return "=";
            }

            return c switch
            {
                "=" => "=",
                "!=" => "<>",
                "<>" => "<>",
                ">" => ">",
                ">=" => ">=",
                "<" => "<",
                "<=" => "<=",
                "%" => "%",
                "%%" => "%%",
                _ => "="
            };
        }

        private static string NormalizeAndOr(string? andOr)
        {
            return string.Equals(andOr?.Trim(), "OR", StringComparison.OrdinalIgnoreCase) ? "OR" : "AND";
        }

        private static string NormalizeDataType(string? dataType)
        {
            return (dataType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "number" => "number",
                "datetime" => "datetime",
                "guid" => "guid",
                _ => "string"
            };
        }

        private static string? GetDateString(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return DateTime.TryParse(raw, out var dt) ? dt.ToString("yyyy-MM-dd") : null;
        }

        private static decimal? GetDecimal(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d;
            return null;
        }

        private static long? GetLong(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)) return val;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out val)) return val;
            return null;
        }

        private static long? ParseNullableLong(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out v)) return v;
            return null;
        }

        private static async Task<bool> ColumnExistsAsync(string conn, string tableName, string columnName)
        {
            var obj = await DbHelper.ExecuteScalarAsync(
                conn,
                CommandType.Text,
                @"SELECT COUNT(1)
                  FROM USER_TAB_COLUMNS
                  WHERE TABLE_NAME = :table_name
                    AND COLUMN_NAME = :column_name",
                new DbParameter[]
                {
                    DbHelper.CreateParameter("table_name", tableName.ToUpperInvariant()),
                    DbHelper.CreateParameter("column_name", columnName.ToUpperInvariant())
                });

            return Convert.ToInt32(obj) > 0;
        }

        private static async Task<RoleFunctionColumnMeta> ResolveRoleFunctionColumnMetaAsync(string conn)
        {
            const string tableName = "IDM_ROLE_FUNCTION_PROGRAM";
            if (await ColumnExistsAsync(conn, tableName, "FUNCTION_NO"))
            {
                return new RoleFunctionColumnMeta
                {
                    SelectExpr = "FUNCTION_NO",
                    OrderExpr = "FUNCTION_NO",
                    UpdateClause = "FUNCTION_NO = :function_no,",
                    InsertColumn = ", FUNCTION_NO",
                    InsertValue = ", :function_no",
                    ParameterName = "function_no",
                    UseNumericParameter = false
                };
            }

            if (await ColumnExistsAsync(conn, tableName, "FUNCTION_ID"))
            {
                return new RoleFunctionColumnMeta
                {
                    SelectExpr = "TO_CHAR(FUNCTION_ID) AS FUNCTION_NO",
                    OrderExpr = "FUNCTION_ID",
                    UpdateClause = "FUNCTION_ID = :function_id,",
                    InsertColumn = ", FUNCTION_ID",
                    InsertValue = ", :function_id",
                    ParameterName = "function_id",
                    UseNumericParameter = true
                };
            }

            return new RoleFunctionColumnMeta
            {
                SelectExpr = "CAST(NULL AS VARCHAR2(100)) AS FUNCTION_NO",
                OrderExpr = "ROLE_NO",
                UpdateClause = string.Empty,
                InsertColumn = string.Empty,
                InsertValue = string.Empty,
                ParameterName = string.Empty,
                UseNumericParameter = false
            };
        }

        private static async Task<List<Dictionary<string, object>>> QueryRoleFunctionProgramsAsync(string conn, long programId, int positionNo)
        {
            try
            {
                var funcMeta = await ResolveRoleFunctionColumnMetaAsync(conn);
                var sql = $@"
                    SELECT ROWID, ROLE_FUNCTION_ID, PROGRAM_ID, POSITION_NO,
                           ROLE_ID, ROLE_NO, ROLE_NAME, {funcMeta.SelectExpr},
                           DISPLAY_ORDER, DISPLAY_COLOR,
                           ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
                    FROM idm_role_function_program
                    WHERE PROGRAM_ID = :program_id
                      AND POSITION_NO = :position_no
                    ORDER BY NVL(DISPLAY_ORDER, 999999), ROLE_NO, {funcMeta.OrderExpr}";

                var dt = await DbHelper.GetDataTableAsync(
                    conn,
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_id", programId),
                        DbHelper.CreateParameter("position_no", positionNo)
                    });

                return DataTableToRows(dt);
            }
            catch
            {
                var fallbackSql = @"
                    SELECT ROWID, ROLE_FUNCTION_ID, PROGRAM_ID, POSITION_NO,
                           ROLE_ID, ROLE_NO, ROLE_NAME,
                           CAST(NULL AS VARCHAR2(100)) AS FUNCTION_NO,
                           DISPLAY_ORDER, DISPLAY_COLOR,
                           ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
                    FROM idm_role_function_program
                    WHERE PROGRAM_ID = :program_id
                      AND POSITION_NO = :position_no
                    ORDER BY ROLE_FUNCTION_ID";

                var dt = await DbHelper.GetDataTableAsync(
                    conn,
                    CommandType.Text,
                    fallbackSql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_id", programId),
                        DbHelper.CreateParameter("position_no", positionNo)
                    });

                return DataTableToRows(dt);
            }
        }

        private static List<Dictionary<string, object>> DataTableToRows(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn c in dt.Columns)
                    dict[c.ColumnName] = row[c] == DBNull.Value ? string.Empty : row[c];
                rows.Add(dict);
            }
            return rows;
        }

        private static object ToDb(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
        private static object ToDb(decimal? value) => value.HasValue ? value.Value : DBNull.Value;
        private static object ToDb(long? value) => value.HasValue ? value.Value : DBNull.Value;
    }
}


