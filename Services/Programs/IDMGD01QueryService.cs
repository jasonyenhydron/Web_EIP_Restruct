using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models.Programs;

namespace Web_EIP_Restruct.Services.Programs;

public interface IIDMGD01QueryService
{
    Task<IReadOnlyList<IDMGD01Dto>> QueryAsync(string? programNo, string? employeeId, string? displayCode, CancellationToken cancellationToken = default);
    Task<IDMGD01Dto?> GetAsync(string programNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> QueryEmployeesAsync(string? query, CancellationToken cancellationToken = default);
}

public sealed class IDMGD01QueryService(IHttpContextAccessor httpContextAccessor) : IIDMGD01QueryService
{
    public async Task<IReadOnlyList<IDMGD01Dto>> QueryAsync(
        string? programNo,
        string? employeeId,
        string? displayCode,
        CancellationToken cancellationToken = default)
    {
        var conn = GetConnectionString();
        const string sql = @"
SELECT p.PROGRAM_NO,
       p.DISPLAY_CODE,
       p.EMPLOYEE_ID,
       NVL(e.EMPLOYEE_NAME, '') AS EMPLOYEE_NAME,
       p.PROGRAM_TYPE,
       p.PURPOSE,
       p.PLAN_START_DEVELOP_DATE,
       p.PLAN_FINISH_DEVELOP_DATE,
       p.PLAN_WORK_HOURS,
       p.REAL_WORK_HOURS,
       p.TR_ID,
       p.TR_DATE
  FROM IDM_PROGRAM p
  LEFT JOIN HRM_EMPLOYEE_V e
    ON e.EMPLOYEE_ID = p.EMPLOYEE_ID
 WHERE (:program_no IS NULL OR UPPER(p.PROGRAM_NO) LIKE UPPER(:program_no))
   AND (:employee_id IS NULL OR p.EMPLOYEE_ID = :employee_id)
   AND (:display_code IS NULL OR p.DISPLAY_CODE = :display_code)
 ORDER BY p.PROGRAM_NO";

        var dt = await DbHelper.GetDataTableAsync(
            conn,
            CommandType.Text,
            sql,
            new DbParameter[]
            {
                DbHelper.CreateParameter("program_no", string.IsNullOrWhiteSpace(programNo) ? DBNull.Value : $"{programNo.Trim().ToUpperInvariant()}%"),
                DbHelper.CreateParameter("employee_id", string.IsNullOrWhiteSpace(employeeId) ? DBNull.Value : employeeId.Trim()),
                DbHelper.CreateParameter("display_code", string.IsNullOrWhiteSpace(displayCode) ? DBNull.Value : displayCode.Trim().ToUpperInvariant())
            },
            cancellationToken);

        return dt.AsEnumerable().Select(MapDto).ToList();
    }

    public async Task<IDMGD01Dto?> GetAsync(string programNo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(programNo))
        {
            return null;
        }

        var conn = GetConnectionString();
        const string sql = @"
SELECT p.PROGRAM_NO,
       p.DISPLAY_CODE,
       p.EMPLOYEE_ID,
       NVL(e.EMPLOYEE_NAME, '') AS EMPLOYEE_NAME,
       p.PROGRAM_TYPE,
       p.PURPOSE,
       p.PLAN_START_DEVELOP_DATE,
       p.PLAN_FINISH_DEVELOP_DATE,
       p.PLAN_WORK_HOURS,
       p.REAL_WORK_HOURS,
       p.TR_ID,
       p.TR_DATE
  FROM IDM_PROGRAM p
  LEFT JOIN HRM_EMPLOYEE_V e
    ON e.EMPLOYEE_ID = p.EMPLOYEE_ID
 WHERE p.PROGRAM_NO = :program_no";

        var dt = await DbHelper.GetDataTableAsync(
            conn,
            CommandType.Text,
            sql,
            new[] { DbHelper.CreateParameter("program_no", programNo.Trim().ToUpperInvariant()) },
            cancellationToken);

        return dt.Rows.Count == 0 ? null : MapDto(dt.Rows[0]);
    }

    public async Task<IReadOnlyList<object>> QueryEmployeesAsync(string? query, CancellationToken cancellationToken = default)
    {
        var conn = GetConnectionString();
        const string sql = @"
SELECT employee_id, employee_no, employee_name
FROM (
    SELECT employee_id, employee_no, employee_name,
           ROW_NUMBER() OVER (ORDER BY employee_no ASC) AS rn
    FROM hrm_employee_v
    WHERE (:q IS NULL OR UPPER(employee_no) LIKE :q OR UPPER(employee_name) LIKE :q)
)
WHERE rn <= 50";

        object q = string.IsNullOrWhiteSpace(query)
            ? DBNull.Value
            : $"%{query.Trim().ToUpperInvariant()}%";
        var dt = await DbHelper.GetDataTableAsync(
            conn,
            CommandType.Text,
            sql,
            new[] { DbHelper.CreateParameter("q", q) },
            cancellationToken);

        return dt.AsEnumerable()
            .Select(row => (object)new
            {
                employeeId = row["EMPLOYEE_ID"] == DBNull.Value ? string.Empty : row["EMPLOYEE_ID"].ToString() ?? string.Empty,
                employeeNo = row["EMPLOYEE_NO"] == DBNull.Value ? string.Empty : row["EMPLOYEE_NO"].ToString() ?? string.Empty,
                employeeName = row["EMPLOYEE_NAME"] == DBNull.Value ? string.Empty : row["EMPLOYEE_NAME"].ToString() ?? string.Empty
            })
            .ToList();
    }

    private string GetConnectionString()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is unavailable.");
        var tns = httpContext.Session.GetString("tns");
        var username = httpContext.Session.GetString("username");
        var password = httpContext.Session.GetString("password");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(tns))
        {
            throw new InvalidOperationException("Session expired. Please log in again.");
        }

        return DbHelper.BuildConnectionString(tns);
    }

    private static IDMGD01Dto MapDto(DataRow row)
    {
        return new IDMGD01Dto
        {
            ProgramNo = row["PROGRAM_NO"] == DBNull.Value ? string.Empty : row["PROGRAM_NO"].ToString() ?? string.Empty,
            DisplayCode = row["DISPLAY_CODE"] == DBNull.Value ? string.Empty : row["DISPLAY_CODE"].ToString() ?? string.Empty,
            EmployeeId = row["EMPLOYEE_ID"] == DBNull.Value ? string.Empty : row["EMPLOYEE_ID"].ToString() ?? string.Empty,
            EmployeeName = row["EMPLOYEE_NAME"] == DBNull.Value ? string.Empty : row["EMPLOYEE_NAME"].ToString() ?? string.Empty,
            ProgramType = row["PROGRAM_TYPE"] == DBNull.Value ? string.Empty : row["PROGRAM_TYPE"].ToString() ?? string.Empty,
            Purpose = row["PURPOSE"] == DBNull.Value ? string.Empty : row["PURPOSE"].ToString() ?? string.Empty,
            PlanStartDevelopDate = row["PLAN_START_DEVELOP_DATE"] == DBNull.Value ? null : Convert.ToDateTime(row["PLAN_START_DEVELOP_DATE"]),
            PlanFinishDevelopDate = row["PLAN_FINISH_DEVELOP_DATE"] == DBNull.Value ? null : Convert.ToDateTime(row["PLAN_FINISH_DEVELOP_DATE"]),
            PlanWorkHours = row["PLAN_WORK_HOURS"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PLAN_WORK_HOURS"]),
            RealWorkHours = row["REAL_WORK_HOURS"] == DBNull.Value ? 0 : Convert.ToDecimal(row["REAL_WORK_HOURS"]),
            UpdatedBy = row["TR_ID"] == DBNull.Value ? string.Empty : row["TR_ID"].ToString() ?? string.Empty,
            UpdatedAt = row["TR_DATE"] == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(row["TR_DATE"])
        };
    }
}
