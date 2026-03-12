using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Controllers;
using Web_EIP_Restruct.Infrastructure;
using Web_EIP_Restruct.Models;

namespace Web_EIP_Restruct.Areas.HRM.Controllers;

[ApiController]
[Area("HRM")]
[RequireLogon]
public sealed class HRMGD47ApiController : HrmGd47Contorller
{
    [HttpGet("/api/internal/form/HRMGD47")]
    public IActionResult GetForm() => GetLeaveApplicationTemplate();

    [HttpGet("/api/internal/form/HRMGD47/template")]
    public IActionResult Template() => GetLeaveApplicationTemplate();

    [HttpGet("/api/internal/grid/HRMGD47")]
    public Task<IActionResult> Grid(
        [FromQuery] long? emAskForLeaveId = null,
        [FromQuery] long? employeeId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        SelectLeaveApplications(emAskForLeaveId, employeeId, startDate, endDate, page, pageSize);

    [HttpPost("/api/internal/form/HRMGD47")]
    public Task<IActionResult> Save([FromBody] HrmEmAskForLeave model) =>
        CreateLeaveApplication(model);

    [HttpPut("/api/internal/form/HRMGD47/{id:long}")]
    public Task<IActionResult> Update(long id, [FromBody] HrmEmAskForLeave model) =>
        UpdateLeaveApplicationRest(id, model);

    [HttpDelete("/api/internal/form/HRMGD47/{id:long}")]
    public Task<IActionResult> Delete(long id) =>
        DeleteLeaveApplicationRest(id);

    [HttpPost("/api/internal/form/HRMGD47/check-duplicate")]
    public Task<IActionResult> CheckDuplicate([FromBody] HrmEmAskForLeave model) =>
        CheckLeaveApplicationDuplicate(model);
}
