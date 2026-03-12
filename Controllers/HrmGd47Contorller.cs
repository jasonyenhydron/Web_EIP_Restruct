using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models;

namespace Web_EIP_Restruct.Controllers
{
    public class HrmGd47Contorller : Controller
    {
        private static string BuildDbConnectionString(string tns) =>
            DbHelper.BuildConnectionString(tns);

        [HttpGet("Hrm/leave-application")]
        public IActionResult GetLeaveApplicationTemplate()
        {
            if (!TryGetLoginContext(out var username, out _, out _))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            var numericId = new string(username.Where(char.IsDigit).ToArray());
            var employeeId = string.IsNullOrEmpty(numericId) ? username : numericId;

            return Ok(new
            {
                status = "success",
                data = new
                {
                    emAskForLeaveId = (long?)null,
                    employeeId,
                    employeeNo = username,
                    employeeName = HttpContext.Session.GetString("user_name") ?? username,
                    employeeDisplay = $"{username} - {HttpContext.Session.GetString("user_name") ?? username}",
                    leaveId = (long?)null,
                    leaveTypeDisplay = string.Empty,
                    startTime = string.Empty,
                    endTime = string.Empty,
                    leaveHours = (decimal?)null,
                    leaveDays = (decimal?)null,
                    punchDate = string.Empty,
                    agentEmployeeId = (long?)null,
                    agentEmployeeName = string.Empty,
                    askForLeaveReason = string.Empty,
                    destinationPlace = string.Empty,
                    talkingAbout = string.Empty,
                    returnYn = "N",
                    overseasYn = "N",
                    emAskForLeaveStatus = "00",
                    flowYn = "N"
                }
            });
        }

        [HttpPost("Hrm/leave-application")]
        public Task<IActionResult> CreateLeaveApplication([FromBody] HrmEmAskForLeave model)
            => InsertLeaveApplication(model);

        [HttpPut("Hrm/leave-application/{id:long}")]
        public Task<IActionResult> UpdateLeaveApplicationRest(long id, [FromBody] HrmEmAskForLeave model)
        {
            model.EmAskForLeaveId = id;
            return UpdateLeaveApplication(model);
        }

        [HttpDelete("Hrm/leave-application/{id:long}")]
        public Task<IActionResult> DeleteLeaveApplicationRest(long id)
            => DeleteLeaveApplication(new HrmEmAskForLeave { EmAskForLeaveId = id });

        [HttpPost("Hrm/leave-application/check-duplicate")]
        public async Task<IActionResult> CheckLeaveApplicationDuplicate([FromBody] HrmEmAskForLeave model)
        {
            if (!TryGetLoginContext(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                const string sql = @"
                    SELECT COUNT(1)
                    FROM hrm_em_ask_for_leave
                    WHERE employee_id = :employee_id
                      AND leave_id = :leave_id
                      AND start_time = :start_time
                      AND end_time = :end_time
                      AND (:em_ask_for_leave_id IS NULL OR em_ask_for_leave_id <> :em_ask_for_leave_id)";

                var countObj = await DbHelper.ExecuteScalarAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("employee_id", model.EmployeeId),
                        DbHelper.CreateParameter("leave_id", model.LeaveId),
                        DbHelper.CreateParameter("start_time", model.StartTime),
                        DbHelper.CreateParameter("end_time", model.EndTime),
                        DbHelper.CreateParameter("em_ask_for_leave_id", model.EmAskForLeaveId ?? (object)DBNull.Value)
                    });

                var count = countObj == null || countObj == DBNull.Value ? 0 : Convert.ToInt32(countObj);
                return Ok(new { status = "success", isDuplicate = count > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("Hrm/leave-types")]
        public async Task<IActionResult> GetLeaveTypes([FromQuery] string query = "")
        {
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                var result = await Models.Lov.HrmLovRepository.QueryLeaveTypesAsync(
                    BuildDbConnectionString(tns),
                    query,
                    page: 1,
                    pageSize: 200,
                    languageId: 1);

                return Json(new
                {
                    status = "success",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("Hrm/select")]
        public async Task<IActionResult> SelectLeaveApplications(
            [FromQuery] long? emAskForLeaveId = null,
            [FromQuery] long? employeeId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!TryGetLoginContext(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var offset = (page - 1) * pageSize;
            var endRow = offset + pageSize;

            try
            {
                const string countSql = @"
                    SELECT COUNT(1)
                    FROM hrm_em_ask_for_leave
                    WHERE (:em_ask_for_leave_id IS NULL OR em_ask_for_leave_id = :em_ask_for_leave_id)
                      AND (:employee_id IS NULL OR employee_id = :employee_id)
                      AND (:start_date IS NULL OR start_time >= :start_date)
                      AND (:end_date IS NULL OR end_time <= :end_date)";

                var totalObj = await DbHelper.ExecuteScalarAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    countSql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("em_ask_for_leave_id", emAskForLeaveId ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("employee_id", employeeId ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("start_date", startDate ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("end_date", endDate ?? (object)DBNull.Value)
                    });

                var total = totalObj == null || totalObj == DBNull.Value ? 0 : Convert.ToInt32(totalObj);

                const string dataSql = @"
                    SELECT *
                    FROM (
                        SELECT
                            em_ask_for_leave_id, employee_id, start_time, end_time, leave_id,
                            leave_hours, leave_days, ask_for_leave_reason, em_ask_for_leave_status,
                            flow_yn, agent_employee_id, destination_place, talking_about, return_yn,
                            overseas_yn, entry_id, entry_date, tr_id, tr_date,
                            ROW_NUMBER() OVER (ORDER BY entry_date DESC, em_ask_for_leave_id DESC) rn
                        FROM hrm_em_ask_for_leave
                        WHERE (:em_ask_for_leave_id IS NULL OR em_ask_for_leave_id = :em_ask_for_leave_id)
                          AND (:employee_id IS NULL OR employee_id = :employee_id)
                          AND (:start_date IS NULL OR start_time >= :start_date)
                          AND (:end_date IS NULL OR end_time <= :end_date)
                    )
                    WHERE rn > :offset AND rn <= :end_row";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    dataSql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("em_ask_for_leave_id", emAskForLeaveId ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("employee_id", employeeId ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("start_date", startDate ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("end_date", endDate ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("offset", offset),
                        DbHelper.CreateParameter("end_row", endRow)
                    });

                var rows = dt.AsEnumerable().Select(r => new
                {
                    emAskForLeaveId = r["EM_ASK_FOR_LEAVE_ID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["EM_ASK_FOR_LEAVE_ID"]),
                    employeeId = r["EMPLOYEE_ID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["EMPLOYEE_ID"]),
                    employeeDisplay = r["EMPLOYEE_ID"] == DBNull.Value ? string.Empty : r["EMPLOYEE_ID"].ToString(),
                    startTime = r["START_TIME"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["START_TIME"]),
                    endTime = r["END_TIME"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["END_TIME"]),
                    leaveId = r["LEAVE_ID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LEAVE_ID"]),
                    leaveTypeDisplay = r["LEAVE_ID"] == DBNull.Value ? string.Empty : r["LEAVE_ID"].ToString(),
                    leaveHours = r["LEAVE_HOURS"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["LEAVE_HOURS"]),
                    leaveDays = r["LEAVE_DAYS"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["LEAVE_DAYS"]),
                    askForLeaveReason = r["ASK_FOR_LEAVE_REASON"]?.ToString(),
                    emAskForLeaveStatus = r["EM_ASK_FOR_LEAVE_STATUS"]?.ToString(),
                    flowYn = r["FLOW_YN"]?.ToString(),
                    agentEmployeeId = r["AGENT_EMPLOYEE_ID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["AGENT_EMPLOYEE_ID"]),
                    agentEmployeeDisplay = r["AGENT_EMPLOYEE_ID"] == DBNull.Value ? string.Empty : r["AGENT_EMPLOYEE_ID"].ToString(),
                    destinationPlace = r["DESTINATION_PLACE"]?.ToString(),
                    talkingAbout = r["TALKING_ABOUT"]?.ToString(),
                    returnYn = r["RETURN_YN"]?.ToString(),
                    overseasYn = r["OVERSEAS_YN"]?.ToString(),
                    entryId = r["ENTRY_ID"]?.ToString(),
                    entryDate = r["ENTRY_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["ENTRY_DATE"]),
                    trId = r["TR_ID"]?.ToString(),
                    trDate = r["TR_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["TR_DATE"])
                }).ToList();

                return Ok(new { status = "success", page, pageSize, total, data = rows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Hrm/insert")]
        public async Task<IActionResult> InsertLeaveApplication([FromBody] HrmEmAskForLeave model)
        {
            if (!TryGetLoginContext(out var username, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (!ModelState.IsValid)
                return BadRequest(new { status = "error", message = "Data validation failed", errors = ModelState });

            try
            {
                var generatedId = await InsertLeaveAsync(model, username, tns);
                return Ok(new { status = "success", message = "Insert completed.", id = generatedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Hrm/update")]
        public async Task<IActionResult> UpdateLeaveApplication([FromBody] HrmEmAskForLeave model)
        {
            if (!TryGetLoginContext(out var username, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (!model.EmAskForLeaveId.HasValue || model.EmAskForLeaveId.Value <= 0)
                return BadRequest(new { status = "error", message = "EmAskForLeaveId is required." });

            if (!ModelState.IsValid)
                return BadRequest(new { status = "error", message = "Data validation failed", errors = ModelState });

            try
            {
                const string sql = @"
                    UPDATE hrm_em_ask_for_leave
                    SET employee_id = :employee_id,
                        start_time = :start_time,
                        end_time = :end_time,
                        leave_id = :leave_id,
                        leave_hours = :leave_hours,
                        leave_days = :leave_days,
                        ask_for_leave_reason = :ask_for_leave_reason,
                        agent_employee_id = :agent_employee_id,
                        destination_place = :destination_place,
                        talking_about = :talking_about,
                        return_yn = :return_yn,
                        overseas_yn = :overseas_yn,
                        tr_id = :tr_id,
                        tr_date = SYSDATE
                    WHERE em_ask_for_leave_id = :em_ask_for_leave_id";

                var affected = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("employee_id", model.EmployeeId),
                        DbHelper.CreateParameter("start_time", model.StartTime),
                        DbHelper.CreateParameter("end_time", model.EndTime),
                        DbHelper.CreateParameter("leave_id", model.LeaveId),
                        DbHelper.CreateParameter("leave_hours", model.LeaveHours ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("leave_days", model.LeaveDays ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("ask_for_leave_reason", model.AskForLeaveReason ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("agent_employee_id", model.AgentEmployeeId ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("destination_place", model.DestinationPlace ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("talking_about", model.TalkingAbout ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("return_yn", model.ReturnYn ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("overseas_yn", model.OverseasYn ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("tr_id", username.ToUpperInvariant()),
                        DbHelper.CreateParameter("em_ask_for_leave_id", model.EmAskForLeaveId.Value)
                    });

                if (affected <= 0)
                    return NotFound(new { status = "error", message = "No record updated." });

                return Ok(new { status = "success", message = "Update completed.", id = model.EmAskForLeaveId.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Hrm/delete")]
        public async Task<IActionResult> DeleteLeaveApplication([FromBody] HrmEmAskForLeave model)
        {
            if (!TryGetLoginContext(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (!model.EmAskForLeaveId.HasValue || model.EmAskForLeaveId.Value <= 0)
                return BadRequest(new { status = "error", message = "EmAskForLeaveId is required." });

            try
            {
                const string sql = @"DELETE FROM hrm_em_ask_for_leave WHERE em_ask_for_leave_id = :em_ask_for_leave_id";
                var affected = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("em_ask_for_leave_id", model.EmAskForLeaveId.Value)
                    });

                if (affected <= 0)
                    return NotFound(new { status = "error", message = "No record deleted." });

                return Ok(new { status = "success", message = "Delete completed.", id = model.EmAskForLeaveId.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Hrm/submit")]
        public async Task<IActionResult> SubmitLeaveApplication([FromBody] HrmEmAskForLeave model)
        {
            return await InsertLeaveApplication(model);
        }

        private bool TryGetLoginContext(out string username, out string password, out string tns)
        {
            username = HttpContext.Session.GetString("username") ?? string.Empty;
            password = HttpContext.Session.GetString("password") ?? string.Empty;
            tns = HttpContext.Session.GetString("tns") ?? string.Empty;
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tns);
        }

        private async Task<long?> InsertLeaveAsync(HrmEmAskForLeave model, string username, string tns)
        {
            const string sql = @"
                INSERT INTO hrm_em_ask_for_leave (
                    employee_id, start_time, end_time, leave_id,
                    system_leave_hours, leave_hours, leave_days,
                    ask_for_leave_reason, em_ask_for_leave_status,
                    flow_yn, agent_employee_id,
                    destination_place, talking_about, return_yn,
                    overseas_yn, entry_id, entry_date
                ) VALUES (
                    :employee_id, :start_time, :end_time, :leave_id,
                    :system_leave_hours, :leave_hours, :leave_days,
                    :ask_for_leave_reason, :em_ask_for_leave_status,
                    :flow_yn, :agent_employee_id,
                    :destination_place, :talking_about, :return_yn,
                    :overseas_yn, :entry_id, SYSDATE
                ) RETURNING em_ask_for_leave_id INTO :new_id";

            var newIdParam = DbHelper.CreateParameter("new_id", null, DbType.Int64, ParameterDirection.Output);

            await DbHelper.ExecuteNonQueryAsync(
                BuildDbConnectionString(tns),
                CommandType.Text,
                sql,
                new DbParameter[]
                {
                    DbHelper.CreateParameter("employee_id", model.EmployeeId),
                    DbHelper.CreateParameter("start_time", model.StartTime),
                    DbHelper.CreateParameter("end_time", model.EndTime),
                    DbHelper.CreateParameter("leave_id", model.LeaveId),
                    DbHelper.CreateParameter("system_leave_hours", model.SystemLeaveHours ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("leave_hours", model.LeaveHours ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("leave_days", model.LeaveDays ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("ask_for_leave_reason", model.AskForLeaveReason ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("em_ask_for_leave_status", string.IsNullOrWhiteSpace(model.EmAskForLeaveStatus) ? "00" : model.EmAskForLeaveStatus),
                    DbHelper.CreateParameter("flow_yn", string.IsNullOrWhiteSpace(model.FlowYn) ? "N" : model.FlowYn),
                    DbHelper.CreateParameter("agent_employee_id", model.AgentEmployeeId ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("destination_place", model.DestinationPlace ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("talking_about", model.TalkingAbout ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("return_yn", model.ReturnYn ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("overseas_yn", model.OverseasYn ?? (object)DBNull.Value),
                    DbHelper.CreateParameter("entry_id", username.ToUpperInvariant()),
                    newIdParam
                });

            if (newIdParam.Value == null || newIdParam.Value == DBNull.Value)
                return null;

            return Convert.ToInt64(newIdParam.Value);
        }
    }
}




