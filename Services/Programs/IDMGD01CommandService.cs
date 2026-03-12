using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models.Programs;

namespace Web_EIP_Restruct.Services.Programs;

public interface IIDMGD01CommandService
{
    Task<IDMGD01Dto> SaveAsync(IDMGD01Dto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string programNo, CancellationToken cancellationToken = default);
}

public sealed class IDMGD01CommandService(
    IHttpContextAccessor httpContextAccessor,
    IIDMGD01QueryService queryService) : IIDMGD01CommandService
{
    public async Task<IDMGD01Dto> SaveAsync(IDMGD01Dto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ProgramNo))
        {
            throw new InvalidOperationException("ProgramNo is required.");
        }

        var conn = GetConnectionString(out var username);
        var programNo = dto.ProgramNo.Trim().ToUpperInvariant();
        var existsObj = await DbHelper.ExecuteScalarAsync(
            conn,
            CommandType.Text,
            "SELECT COUNT(1) FROM IDM_PROGRAM WHERE PROGRAM_NO = :program_no",
            new[] { DbHelper.CreateParameter("program_no", programNo) },
            cancellationToken);

        var exists = Convert.ToInt32(existsObj ?? 0) > 0;
        if (exists)
        {
            const string updateSql = @"
UPDATE IDM_PROGRAM
   SET DISPLAY_CODE = :display_code,
       PURPOSE = :purpose,
       PROGRAM_TYPE = :program_type,
       EMPLOYEE_ID = :employee_id,
       PLAN_START_DEVELOP_DATE = :plan_start_develop_date,
       PLAN_FINISH_DEVELOP_DATE = :plan_finish_develop_date,
       PLAN_WORK_HOURS = :plan_work_hours,
       REAL_WORK_HOURS = :real_work_hours,
       TR_ID = :tr_id,
       TR_DATE = SYSDATE
 WHERE PROGRAM_NO = :program_no";

            await DbHelper.ExecuteNonQueryAsync(
                conn,
                CommandType.Text,
                updateSql,
                BuildSaveParameters(dto, username, programNo),
                cancellationToken);
        }
        else
        {
            var nextIdObj = await DbHelper.ExecuteScalarAsync(
                conn,
                CommandType.Text,
                "SELECT NVL(MAX(PROGRAM_ID), 0) + 1 FROM IDM_PROGRAM",
                null,
                cancellationToken);

            const string insertSql = @"
INSERT INTO IDM_PROGRAM (
    PROGRAM_ID, PROGRAM_NO, DISPLAY_CODE, PURPOSE, PROGRAM_TYPE, EMPLOYEE_ID,
    PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE, PLAN_WORK_HOURS, REAL_WORK_HOURS,
    ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
) VALUES (
    :program_id, :program_no, :display_code, :purpose, :program_type, :employee_id,
    :plan_start_develop_date, :plan_finish_develop_date, :plan_work_hours, :real_work_hours,
    :entry_id, SYSDATE, :tr_id, SYSDATE
)";

            var parameters = BuildSaveParameters(dto, username, programNo).ToList();
            parameters.Add(DbHelper.CreateParameter("program_id", Convert.ToInt64(nextIdObj ?? 1)));
            parameters.Add(DbHelper.CreateParameter("entry_id", username));

            await DbHelper.ExecuteNonQueryAsync(
                conn,
                CommandType.Text,
                insertSql,
                parameters.ToArray(),
                cancellationToken);
        }

        return await queryService.GetAsync(programNo, cancellationToken)
            ?? throw new InvalidOperationException("Program saved but could not be reloaded.");
    }

    public async Task<bool> DeleteAsync(string programNo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(programNo))
        {
            return false;
        }

        var conn = GetConnectionString(out _);
        var affected = await DbHelper.ExecuteNonQueryAsync(
            conn,
            CommandType.Text,
            "DELETE FROM IDM_PROGRAM WHERE PROGRAM_NO = :program_no",
            new[] { DbHelper.CreateParameter("program_no", programNo.Trim().ToUpperInvariant()) },
            cancellationToken);

        return affected > 0;
    }

    private string GetConnectionString(out string username)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is unavailable.");
        var tns = httpContext.Session.GetString("tns");
        username = httpContext.Session.GetString("username") ?? string.Empty;
        var password = httpContext.Session.GetString("password");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(tns))
        {
            throw new InvalidOperationException("Session expired. Please log in again.");
        }

        return DbHelper.BuildConnectionString(tns);
    }

    private static DbParameter[] BuildSaveParameters(IDMGD01Dto dto, string username, string programNo)
    {
        return
        [
            DbHelper.CreateParameter("program_no", programNo),
            DbHelper.CreateParameter("display_code", ToDb(dto.DisplayCode?.Trim().ToUpperInvariant())),
            DbHelper.CreateParameter("purpose", ToDb(dto.Purpose)),
            DbHelper.CreateParameter("program_type", ToDb(dto.ProgramType)),
            DbHelper.CreateParameter("employee_id", ToDb(dto.EmployeeId)),
            DbHelper.CreateParameter("plan_start_develop_date", dto.PlanStartDevelopDate.HasValue ? dto.PlanStartDevelopDate.Value : DBNull.Value),
            DbHelper.CreateParameter("plan_finish_develop_date", dto.PlanFinishDevelopDate.HasValue ? dto.PlanFinishDevelopDate.Value : DBNull.Value),
            DbHelper.CreateParameter("plan_work_hours", dto.PlanWorkHours),
            DbHelper.CreateParameter("real_work_hours", dto.RealWorkHours),
            DbHelper.CreateParameter("tr_id", username)
        ];
    }

    private static object ToDb(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
}
