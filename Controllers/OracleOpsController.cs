
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models.Oracle;

namespace Web_EIP_Restruct.Controllers
{
    [ApiController]
    [Route("api/oracle")]
    public class OracleOpsController : ControllerBase
    {
        private static string BuildDbConnectionString(string tns) => DbHelper.BuildConnectionString(tns);

        private bool CheckSession(out string username, out string password, out string tns)
        {
            username = HttpContext.Session.GetString("username") ?? string.Empty;
            password = HttpContext.Session.GetString("password") ?? string.Empty;
            tns = HttpContext.Session.GetString("tns") ?? string.Empty;
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tns);
        }

        [HttpPost("proc/execute")]
        public async Task<IActionResult> ExecuteProcedure([FromBody] OracleProcExecuteRequest request, CancellationToken cancellationToken)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (request == null || string.IsNullOrWhiteSpace(request.ProcedureName))
                return BadRequest(new { status = "error", message = "ProcedureName is required." });

            try
            {
                var parameters = BuildDbParameters(request.Parameters);
                Dictionary<string, object?> outputs;
                if (string.IsNullOrWhiteSpace(request.PackageName))
                {
                    outputs = await OracleDbHelper.ExecuteProcedureWithOutputsAsync(
                        BuildDbConnectionString(tns),
                        request.ProcedureName,
                        parameters,
                        cancellationToken);
                }
                else
                {
                    outputs = await OracleDbHelper.ExecutePackageProcedureWithOutputsAsync(
                        BuildDbConnectionString(tns),
                        request.PackageName,
                        request.ProcedureName,
                        parameters,
                        cancellationToken);
                }

                return Ok(new { status = "success", outputs });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("func/execute")]
        public async Task<IActionResult> ExecuteFunction([FromBody] OracleFunctionExecuteRequest request, CancellationToken cancellationToken)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (request == null || string.IsNullOrWhiteSpace(request.FunctionName))
                return BadRequest(new { status = "error", message = "FunctionName is required." });

            try
            {
                var parameters = BuildDbParameters(request.Parameters);
                var returnType = ParseDbTypeOrDefault(request.ReturnDbType, DbType.String);
                var result = await OracleDbHelper.ExecuteFunctionScalarAsync(
                    BuildDbConnectionString(tns),
                    request.FunctionName,
                    returnType,
                    parameters,
                    cancellationToken);

                return Ok(new { status = "success", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("job/create")]
        public async Task<IActionResult> CreateJob([FromBody] OracleSchedulerJobCreateRequest request, CancellationToken cancellationToken)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (request == null || string.IsNullOrWhiteSpace(request.JobName) ||
                string.IsNullOrWhiteSpace(request.JobType) || string.IsNullOrWhiteSpace(request.JobAction))
            {
                return BadRequest(new { status = "error", message = "JobName, JobType, JobAction are required." });
            }

            try
            {
                await OracleDbHelper.CreateSchedulerJobAsync(
                    BuildDbConnectionString(tns),
                    request.JobName,
                    request.JobType,
                    request.JobAction,
                    request.StartDate,
                    request.RepeatInterval,
                    request.Enabled,
                    request.AutoDrop,
                    request.Comments,
                    cancellationToken);

                return Ok(new { status = "success", message = "Job created." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("job/run")]
        public async Task<IActionResult> RunJob([FromBody] OracleSchedulerJobActionRequest request, CancellationToken cancellationToken)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            if (request == null || string.IsNullOrWhiteSpace(request.JobName))
                return BadRequest(new { status = "error", message = "JobName is required." });

            try
            {
                await OracleDbHelper.RunSchedulerJobAsync(
                    BuildDbConnectionString(tns),
                    request.JobName,
                    request.UseCurrentSession,
                    cancellationToken);
                return Ok(new { status = "success", message = "Job started." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("job/enable")]
        public async Task<IActionResult> EnableJob([FromBody] OracleSchedulerJobActionRequest request, CancellationToken cancellationToken)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            if (request == null || string.IsNullOrWhiteSpace(request.JobName))
                return BadRequest(new { status = "error", message = "JobName is required." });

            try
            {
                await OracleDbHelper.EnableSchedulerJobAsync(
                    BuildDbConnectionString(tns),
                    request.JobName,
                    cancellationToken);
                return Ok(new { status = "success", message = "Job enabled." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("job/disable")]
        public async Task<IActionResult> DisableJob([FromBody] OracleSchedulerJobActionRequest request, CancellationToken cancellationToken)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            if (request == null || string.IsNullOrWhiteSpace(request.JobName))
                return BadRequest(new { status = "error", message = "JobName is required." });

            try
            {
                await OracleDbHelper.DisableSchedulerJobAsync(
                    BuildDbConnectionString(tns),
                    request.JobName,
                    request.Force,
                    cancellationToken);
                return Ok(new { status = "success", message = "Job disabled." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("job/drop")]
        public async Task<IActionResult> DropJob([FromBody] OracleSchedulerJobActionRequest request, CancellationToken cancellationToken)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            if (request == null || string.IsNullOrWhiteSpace(request.JobName))
                return BadRequest(new { status = "error", message = "JobName is required." });

            try
            {
                await OracleDbHelper.DropSchedulerJobAsync(
                    BuildDbConnectionString(tns),
                    request.JobName,
                    request.Force,
                    cancellationToken);
                return Ok(new { status = "success", message = "Job dropped." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("job/list")]
        public async Task<IActionResult> GetJobs([FromQuery] string? owner = null, CancellationToken cancellationToken = default)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                var dt = await OracleDbHelper.GetSchedulerJobsAsync(BuildDbConnectionString(tns), owner, cancellationToken);
                var data = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    var item = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (DataColumn col in dt.Columns)
                        item[col.ColumnName.ToLowerInvariant()] = row[col]?.ToString() ?? string.Empty;
                    data.Add(item);
                }

                return Ok(new { status = "success", data });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private static DbParameter[] BuildDbParameters(List<OracleBindParameterRequest>? req)
        {
            if (req == null || req.Count == 0)
                return Array.Empty<DbParameter>();

            var list = new List<DbParameter>(req.Count);
            foreach (var p in req)
            {
                if (string.IsNullOrWhiteSpace(p.Name))
                    throw new ArgumentException("Parameter name is required.");

                var dbType = ParseDbTypeOrNull(p.DbType);
                var direction = ParseDirectionOrDefault(p.Direction, ParameterDirection.Input);
                var normalizedValue = NormalizeValue(p.Value);
                list.Add(DbHelper.CreateParameter(p.Name, normalizedValue, dbType, direction, p.Size));
            }
            return list.ToArray();
        }

        private static object? NormalizeValue(object? value)
        {
            if (value == null) return DBNull.Value;
            if (value is JsonElement je) return JsonElementToObject(je);
            return value;
        }

        private static object? JsonElementToObject(JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.Null => DBNull.Value,
                JsonValueKind.Undefined => DBNull.Value,
                JsonValueKind.String => je.GetString() ?? (object)DBNull.Value,
                JsonValueKind.Number when je.TryGetInt64(out var l) => l,
                JsonValueKind.Number when je.TryGetDecimal(out var d) => d,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => je.GetRawText()
            };
        }

        private static DbType? ParseDbTypeOrNull(string? dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return null;
            return Enum.TryParse<DbType>(dbType, true, out var parsed) ? parsed : null;
        }

        private static DbType ParseDbTypeOrDefault(string? dbType, DbType fallback)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return fallback;
            return Enum.TryParse<DbType>(dbType, true, out var parsed) ? parsed : fallback;
        }

        private static ParameterDirection ParseDirectionOrDefault(string? direction, ParameterDirection fallback)
        {
            if (string.IsNullOrWhiteSpace(direction)) return fallback;
            return Enum.TryParse<ParameterDirection>(direction, true, out var parsed) ? parsed : fallback;
        }
    }
}


