
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models.Lov;

namespace Web_EIP_Restruct.Controllers
{
    [ApiController]
    [Route("api/lov")]
    public class LovController : ControllerBase
    {
        private static string BuildDbConnectionString(string tns) => DbHelper.BuildConnectionString(tns);

        private bool CheckSession(out string username, out string password, out string tns)
        {
            username = HttpContext.Session.GetString("username") ?? string.Empty;
            password = HttpContext.Session.GetString("password") ?? string.Empty;
            tns = HttpContext.Session.GetString("tns") ?? string.Empty;
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tns);
        }

        [HttpGet("query")]
        public async Task<IActionResult> QueryLov(
            [FromQuery] string sql = "",
            [FromQuery] string query = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                var extra = Request.Query
                    .ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);

                var result = await GenericLovRepository.QueryAsync(
                    BuildDbConnectionString(tns),
                    sql,
                    query,
                    page,
                    pageSize,
                    extra);

                return Ok(new
                {
                    status = "success",
                    data = result.Data,
                    page = result.Page,
                    pageSize = result.PageSize,
                    hasMore = result.HasMore
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("hrm/leave-types")]
        public async Task<IActionResult> QueryHrmLeaveTypes(
            [FromQuery] string query = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int languageId = 1)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                var result = await HrmLovRepository.QueryLeaveTypesAsync(
                    BuildDbConnectionString(tns),
                    query,
                    page,
                    pageSize,
                    languageId);

                return Ok(new
                {
                    status = "success",
                    data = result.Data,
                    page = result.Page,
                    pageSize = result.PageSize,
                    hasMore = result.HasMore
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("hrm/employees")]
        public async Task<IActionResult> QueryHrmEmployees(
            [FromQuery] string query = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                var result = await HrmLovRepository.QueryEmployeesAsync(
                    BuildDbConnectionString(tns),
                    query,
                    page,
                    pageSize);

                return Ok(new
                {
                    status = "success",
                    data = result.Data,
                    page = result.Page,
                    pageSize = result.PageSize,
                    hasMore = result.HasMore
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("hrm/booking-departments")]
        public async Task<IActionResult> QueryHrmBookingDepartments(
            [FromQuery] string query = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? employeeId = null,
            [FromQuery] int languageId = 1)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                var result = await HrmLovRepository.QueryBookingDepartmentsAsync(
                    BuildDbConnectionString(tns),
                    query,
                    page,
                    pageSize,
                    employeeId,
                    languageId);

                return Ok(new
                {
                    status = "success",
                    data = result.Data,
                    page = result.Page,
                    pageSize = result.PageSize,
                    hasMore = result.HasMore
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}

