using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Web_EIP_Restruct.Helpers;

namespace Web_EIP_Restruct.Controllers
{
    public class CmmController : Controller
    {
        private readonly ILogger<CmmController> _logger;
        private const long DefaultOrganizationId = 10611;
        public CmmController(ILogger<CmmController> logger)
        {
            _logger = logger;
        }

        private static readonly Dictionary<string, string> CmmQueryFieldMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["agentNo"] = "CA.AGENT_NO",
            ["agentName"] = "CA.AGENT_NAME",
            ["employeeId"] = "CA.EMPLOYEE_ID",
            ["bonusType"] = "CA.BONUS_TYPE",
            ["productLineId"] = "CA.PRODUCT_LINE_ID",
            ["agentAreaCode"] = "CA.AGENT_AREA_CODE",
            ["statusFrom"] = "CA.AGENT_STATUS",
            ["statusTo"] = "CA.AGENT_STATUS",
            ["organizationId"] = "CA.ORGANIZATION_ID"
        };

        private static readonly HashSet<string> CmmQueryAllowedColumns = new(
            new[]
            {
                "CA.AGENT_ID",
                "CA.ORGANIZATION_ID",
                "CA.EMPLOYEE_ID",
                "CA.DEPARTMENT_ID",
                "CA.COMPANY_ID",
                "CA.TR_ID",
                "CA.ENTRY_ID",
                "CA.ENTRY_DATE",
                "CA.TR_DATE",
                "CA.AGENT_NO",
                "CA.AGENT_NAME",
                "CA.PARENT_AGENT_ID",
                "CA.BONUS_TYPE",
                "CA.AGENT_AREA_CODE",
                "CA.AGENT_STATUS",
                "CA.PRODUCT_LINE_ID",
                "CA.MOBIL_PHONE",
                "CA.ADDRESS",
                "CA.AGENT_NAME2"
            },
            StringComparer.OrdinalIgnoreCase);

        private sealed class SelectPredicate
        {
            public string Key { get; set; } = string.Empty;
            public string Column { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Condition { get; set; } = "=";
            public string AndOr { get; set; } = "AND";
            public string DataType { get; set; } = "string";
        }

        [HttpGet("Cmm/select")]
        public async Task<IActionResult> Select(string DataMember, bool debugSql = false)
        {
            if (!string.Equals(DataMember, "CMMGD16", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            if (!TryGetSessionCredentials(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                var sql = new StringBuilder(@"
SELECT ROWID,
       ca.AGENT_ID,
       ca.ORGANIZATION_ID,
       ca.EMPLOYEE_ID,
       ca.DEPARTMENT_ID,
       ca.COMPANY_ID,
       ca.TR_ID,
       ca.ENTRY_ID,
       ca.ENTRY_DATE,
       ca.TR_DATE,
       ca.AGENT_NO,
       ca.AGENT_NAME,
       ca.PARENT_AGENT_ID,
       ca.BONUS_TYPE,
       ca.AGENT_AREA_CODE,
       ca.AGENT_STATUS,
       ca.PRODUCT_LINE_ID,
       ca.MOBIL_PHONE,
       ca.ADDRESS,
       ca.AGENT_NAME2
  FROM cmm_agent ca
 WHERE  (1=1 or ca.organization_id = :org_id)");

                var predicates = BuildSelectPredicates(Request.Query);
                var whereSegment = BuildWhereSegment(predicates, out var queryParameters);
                if (!string.IsNullOrWhiteSpace(whereSegment))
                {
                    sql.Append(" AND (");
                    sql.Append(whereSegment);
                    sql.Append(')');
                }
                sql.Append(" ORDER BY ca.agent_no");

                var parameters = new List<DbParameter> { DbHelper.CreateParameter("org_id", DefaultOrganizationId) };
                parameters.AddRange(queryParameters);
                var finalSql = sql.ToString();
                var resolvedSql = BuildDebugSql(finalSql, parameters);
                _logger.LogInformation("CMMGD16 Select SQL => {Sql}", resolvedSql);
                if (debugSql)
                {
                    return Ok(new { status = "debug", sql = resolvedSql });
                }

                var dt = await DbHelper.GetDataTableAsync(
                    DbHelper.BuildConnectionString(tns),
                    CommandType.Text,
                    finalSql,
                    parameters.ToArray());

                return Ok(new { status = "success", data = DataTableToRows(dt) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Cmm/insert")]
        public async Task<IActionResult> Insert(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (!string.Equals(DataMember, "CMMGD16", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            if (!TryGetSessionCredentials(out var username, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });
            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var agentNo = (GetString(payload, "AGENT_NO") ?? string.Empty).Trim().ToUpperInvariant();
            var agentName = (GetString(payload, "AGENT_NAME") ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(agentNo) || string.IsNullOrWhiteSpace(agentName))
                return BadRequest(new { status = "error", message = "AGENT_NO / AGENT_NAME is required" });

            try
            {
                var conn = DbHelper.BuildConnectionString(tns);
                var dupObj = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    @"SELECT COUNT(1)
                        FROM cmm_agent
                       WHERE organization_id = :org_id
                         AND agent_no = :agent_no",
                    new[]
                    {
                        DbHelper.CreateParameter("org_id", DefaultOrganizationId),
                        DbHelper.CreateParameter("agent_no", agentNo)
                    });
                if (Convert.ToInt32(dupObj) > 0)
                    return Conflict(new { status = "error", message = $"AGENT_NO {agentNo} already exists" });

                var idObj = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    "SELECT NVL(MAX(agent_id), 0) + 1 FROM cmm_agent");
                var nextAgentId = Convert.ToInt64(idObj);

                const string sql = @"
INSERT INTO cmm_agent (
    AGENT_ID, ORGANIZATION_ID, EMPLOYEE_ID, DEPARTMENT_ID, COMPANY_ID,
    TR_ID, ENTRY_ID, ENTRY_DATE, TR_DATE,
    AGENT_NO, AGENT_NAME, PARENT_AGENT_ID, BONUS_TYPE, AGENT_AREA_CODE,
    AGENT_STATUS, PRODUCT_LINE_ID, MOBIL_PHONE, ADDRESS, AGENT_NAME2
) VALUES (
    :agent_id, :organization_id, :employee_id, :department_id, :company_id,
    :tr_id, :entry_id, SYSDATE, SYSDATE,
    :agent_no, :agent_name, :parent_agent_id, :bonus_type, :agent_area_code,
    :agent_status, :product_line_id, :mobil_phone, :address, :agent_name2
)";

                await DbHelper.ExecuteNonQueryAsync(
                    conn,
                    CommandType.Text,
                    sql,
                    new[]
                    {
                        DbHelper.CreateParameter("agent_id", nextAgentId),
                        DbHelper.CreateParameter("organization_id", DefaultOrganizationId),
                        DbHelper.CreateParameter("employee_id", ToDb(GetString(payload, "EMPLOYEE_ID"))),
                        DbHelper.CreateParameter("department_id", ToDb(GetString(payload, "DEPARTMENT_ID"))),
                        DbHelper.CreateParameter("company_id", ToDb(GetString(payload, "COMPANY_ID"))),
                        DbHelper.CreateParameter("tr_id", username),
                        DbHelper.CreateParameter("entry_id", username),
                        DbHelper.CreateParameter("agent_no", agentNo),
                        DbHelper.CreateParameter("agent_name", agentName),
                        DbHelper.CreateParameter("parent_agent_id", ToDb(GetString(payload, "PARENT_AGENT_ID"))),
                        DbHelper.CreateParameter("bonus_type", ToDb(GetString(payload, "BONUS_TYPE"))),
                        DbHelper.CreateParameter("agent_area_code", ToDb(GetString(payload, "AGENT_AREA_CODE"))),
                        DbHelper.CreateParameter("agent_status", ToDb(GetString(payload, "AGENT_STATUS") ?? "00")),
                        DbHelper.CreateParameter("product_line_id", ToDb(GetString(payload, "PRODUCT_LINE_ID"))),
                        DbHelper.CreateParameter("mobil_phone", ToDb(GetString(payload, "MOBIL_PHONE"))),
                        DbHelper.CreateParameter("address", ToDb(GetString(payload, "ADDRESS"))),
                        DbHelper.CreateParameter("agent_name2", ToDb(GetString(payload, "AGENT_NAME2")))
                    });

                return Ok(new { status = "success", message = "Inserted OK", id = nextAgentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Cmm/update")]
        public async Task<IActionResult> Update(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (!string.Equals(DataMember, "CMMGD16", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            if (!TryGetSessionCredentials(out var username, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });
            if (payload == null || !payload.ContainsKey("ROWID"))
                return BadRequest(new { status = "error", message = "Missing ROWID" });

            var rowId = GetString(payload, "ROWID");
            var agentNo = (GetString(payload, "AGENT_NO") ?? string.Empty).Trim().ToUpperInvariant();
            var agentName = (GetString(payload, "AGENT_NAME") ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rowId) || string.IsNullOrWhiteSpace(agentNo) || string.IsNullOrWhiteSpace(agentName))
                return BadRequest(new { status = "error", message = "ROWID / AGENT_NO / AGENT_NAME is required" });

            try
            {
                const string sql = @"
UPDATE cmm_agent
   SET EMPLOYEE_ID = :employee_id,
       DEPARTMENT_ID = :department_id,
       COMPANY_ID = :company_id,
       TR_ID = :tr_id,
       TR_DATE = SYSDATE,
       AGENT_NO = :agent_no,
       AGENT_NAME = :agent_name,
       PARENT_AGENT_ID = :parent_agent_id,
       BONUS_TYPE = :bonus_type,
       AGENT_AREA_CODE = :agent_area_code,
       AGENT_STATUS = :agent_status,
       PRODUCT_LINE_ID = :product_line_id,
       MOBIL_PHONE = :mobil_phone,
       ADDRESS = :address,
       AGENT_NAME2 = :agent_name2
 WHERE ROWID = :row_id";

                var affected = await DbHelper.ExecuteNonQueryAsync(
                    DbHelper.BuildConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new[]
                    {
                        DbHelper.CreateParameter("employee_id", ToDb(GetString(payload, "EMPLOYEE_ID"))),
                        DbHelper.CreateParameter("department_id", ToDb(GetString(payload, "DEPARTMENT_ID"))),
                        DbHelper.CreateParameter("company_id", ToDb(GetString(payload, "COMPANY_ID"))),
                        DbHelper.CreateParameter("tr_id", username),
                        DbHelper.CreateParameter("agent_no", agentNo),
                        DbHelper.CreateParameter("agent_name", agentName),
                        DbHelper.CreateParameter("parent_agent_id", ToDb(GetString(payload, "PARENT_AGENT_ID"))),
                        DbHelper.CreateParameter("bonus_type", ToDb(GetString(payload, "BONUS_TYPE"))),
                        DbHelper.CreateParameter("agent_area_code", ToDb(GetString(payload, "AGENT_AREA_CODE"))),
                        DbHelper.CreateParameter("agent_status", ToDb(GetString(payload, "AGENT_STATUS"))),
                        DbHelper.CreateParameter("product_line_id", ToDb(GetString(payload, "PRODUCT_LINE_ID"))),
                        DbHelper.CreateParameter("mobil_phone", ToDb(GetString(payload, "MOBIL_PHONE"))),
                        DbHelper.CreateParameter("address", ToDb(GetString(payload, "ADDRESS"))),
                        DbHelper.CreateParameter("agent_name2", ToDb(GetString(payload, "AGENT_NAME2"))),
                        DbHelper.CreateParameter("row_id", rowId)
                    });

                if (affected == 0)
                    return NotFound(new { status = "error", message = "Record not found" });

                return Ok(new { status = "success", message = "Saved OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Cmm/delete")]
        public async Task<IActionResult> Delete(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (!string.Equals(DataMember, "CMMGD16", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            if (!TryGetSessionCredentials(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });
            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var rowId = GetString(payload, "ROWID");
            var agentId = ParseNullableLong(GetString(payload, "AGENT_ID"));
            if (string.IsNullOrWhiteSpace(rowId) && !agentId.HasValue)
                return BadRequest(new { status = "error", message = "ROWID or AGENT_ID is required" });

            try
            {
                string sql;
                DbParameter[] ps;
                if (!string.IsNullOrWhiteSpace(rowId))
                {
                    sql = "DELETE FROM cmm_agent WHERE ROWID = :row_id";
                    ps = new[] { DbHelper.CreateParameter("row_id", rowId) };
                }
                else
                {
                    sql = "DELETE FROM cmm_agent WHERE organization_id = :org_id AND agent_id = :agent_id";
                    ps = new[]
                    {
                        DbHelper.CreateParameter("org_id", DefaultOrganizationId),
                        DbHelper.CreateParameter("agent_id", agentId!.Value)
                    };
                }

                var affected = await DbHelper.ExecuteNonQueryAsync(
                    DbHelper.BuildConnectionString(tns),
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

        [HttpGet("Cmm/selectRelatedAgents")]
        public async Task<IActionResult> SelectRelatedAgents(string DataMember, long? parentAgentId, int positionNo = 1)
        {
            if (!string.Equals(DataMember, "CMMGD16", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            if (!TryGetSessionCredentials(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });
            if (!parentAgentId.HasValue)
                return Ok(new { status = "success", data = new List<Dictionary<string, object>>() });

            var slot = Math.Clamp(positionNo, 1, 3) - 1;

            try
            {
                const string sql = @"
SELECT *
  FROM (
        SELECT ROWID,
               AGENT_ID,
               AGENT_NO,
               AGENT_NAME,
               EMPLOYEE_ID,
               AGENT_STATUS,
               AGENT_AREA_CODE,
               MOBIL_PHONE,
               ROW_NUMBER() OVER (ORDER BY AGENT_NO) AS RN
          FROM cmm_agent
         WHERE organization_id = :org_id
           AND parent_agent_id = :parent_agent_id
       )
 WHERE MOD(RN - 1, 3) = :slot
 ORDER BY AGENT_NO";

                var dt = await DbHelper.GetDataTableAsync(
                    DbHelper.BuildConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new[]
                    {
                        DbHelper.CreateParameter("org_id", DefaultOrganizationId),
                        DbHelper.CreateParameter("parent_agent_id", parentAgentId.Value),
                        DbHelper.CreateParameter("slot", slot)
                    });

                return Ok(new { status = "success", data = DataTableToRows(dt) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private bool TryGetSessionCredentials(out string username, out string password, out string tns)
        {
            username = string.Empty;
            password = string.Empty;
            tns = string.Empty;

            username = HttpContext.Session.GetString("username") ?? string.Empty;
            password = HttpContext.Session.GetString("password") ?? string.Empty;
            tns = HttpContext.Session.GetString("tns") ?? string.Empty;
            return !string.IsNullOrWhiteSpace(username)
                && !string.IsNullOrWhiteSpace(password)
                && !string.IsNullOrWhiteSpace(tns);
        }

        private static List<SelectPredicate> BuildSelectPredicates(IQueryCollection query)
        {
            var result = new List<SelectPredicate>();
            foreach (var key in query.Keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (key.Equals("DataMember", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.EndsWith("_condition", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.EndsWith("_andOr", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.EndsWith("_dataType", StringComparison.OrdinalIgnoreCase)) continue;

                var rawValue = query[key].ToString();
                if (string.IsNullOrWhiteSpace(rawValue)) continue;
                if (!TryResolveColumnName(key, out var columnName)) continue;

                result.Add(new SelectPredicate
                {
                    Key = key,
                    Column = columnName,
                    Value = rawValue,
                    Condition = NormalizeCondition(query[$"{key}_condition"].ToString(), key),
                    AndOr = NormalizeAndOr(query[$"{key}_andOr"].ToString()),
                    DataType = NormalizeDataType(query[$"{key}_dataType"].ToString())
                });
            }

            return result;
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

            if (CmmQueryFieldMap.TryGetValue(key, out var mapped))
            {
                columnName = mapped;
                return true;
            }

            var upper = key.Trim().ToUpperInvariant();
            if (!upper.StartsWith("CA.", StringComparison.OrdinalIgnoreCase))
                upper = $"CA.{upper}";
            if (!CmmQueryAllowedColumns.Contains(upper)) return false;

            columnName = upper;
            return true;
        }

        private static string NormalizeCondition(string? condition, string? key = null)
        {
            var c = (condition ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(c))
            {
                if (string.Equals(key, "agentNo", StringComparison.OrdinalIgnoreCase)) return "%";
                if (string.Equals(key, "agentName", StringComparison.OrdinalIgnoreCase)) return "%%";
                if (string.Equals(key, "statusFrom", StringComparison.OrdinalIgnoreCase)) return ">=";
                if (string.Equals(key, "statusTo", StringComparison.OrdinalIgnoreCase)) return "<=";
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

        private static string? GetString(Dictionary<string, object> payload, string key)
            => payload.ContainsKey(key) ? payload[key]?.ToString() : null;

        private static string BuildDebugSql(string sql, IEnumerable<DbParameter> parameters)
        {
            var result = sql;
            foreach (var p in parameters.OrderByDescending(x => x.ParameterName?.Length ?? 0))
            {
                var rawName = p.ParameterName ?? string.Empty;
                var name = rawName.Trim().TrimStart(':', '@', '?');
                if (string.IsNullOrWhiteSpace(name)) continue;
                result = Regex.Replace(
                    result,
                    $@":{Regex.Escape(name)}\b",
                    ToSqlLiteral(p.Value),
                    RegexOptions.IgnoreCase);
            }
            return result;
        }

        private static string ToSqlLiteral(object? value)
        {
            if (value == null || value == DBNull.Value) return "NULL";
            if (value is string s) return $"'{s.Replace("'", "''")}'";
            if (value is DateTime dt)
                return $"TO_DATE('{dt:yyyy-MM-dd HH:mm:ss}','YYYY-MM-DD HH24:MI:SS')";
            if (value is bool b) return b ? "1" : "0";
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "NULL";
        }

        private static long? ParseNullableLong(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out v)) return v;
            return null;
        }

        private static object ToDb(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;

        private static List<Dictionary<string, object>> DataTableToRows(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in dt.Columns)
                    dict[col.ColumnName] = row[col] == DBNull.Value ? string.Empty : row[col];
                rows.Add(dict);
            }
            return rows;
        }
    }
}

