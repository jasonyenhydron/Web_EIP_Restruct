using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Infrastructure;
using Web_EIP_Restruct.Models.Programs;
using Web_EIP_Restruct.Services.Programs;

namespace Web_EIP_Restruct.Areas.MIS.Controllers;

[ApiController]
[Area("MIS")]
[RequireLogon]
public class IDMGD01ApiController(
    IIDMGD01QueryService queryService,
    IIDMGD01CommandService commandService) : ControllerBase
{
    [HttpGet("/api/internal/grid/IDMGD01")]
    [HttpGet("/api/internal/grid/IDMGD01/select")]
    public async Task<IActionResult> Grid(
        [FromQuery] string? programNo,
        [FromQuery] string? employeeId,
        [FromQuery] string? displayCode,
        CancellationToken cancellationToken)
    {
        var data = await queryService.QueryAsync(programNo, employeeId, displayCode, cancellationToken);
        return Ok(new { status = "success", data });
    }

    [HttpGet("/api/internal/form/IDMGD01/{programNo}")]
    public async Task<IActionResult> Form(string programNo, CancellationToken cancellationToken)
    {
        var data = await queryService.GetAsync(programNo, cancellationToken);
        return data is null
            ? NotFound(new { status = "error", message = "Program not found." })
            : Ok(new { status = "success", data });
    }

    [HttpPost("/api/internal/form/IDMGD01")]
    [HttpPost("/api/internal/grid/IDMGD01/insert")]
    [HttpPost("/api/internal/grid/IDMGD01/update")]
    public async Task<IActionResult> Save([FromBody] IDMGD01Dto dto, CancellationToken cancellationToken)
    {
        var saved = await commandService.SaveAsync(dto, cancellationToken);
        return Ok(new { status = "success", message = "Saved.", data = saved });
    }

    [HttpPost("/api/internal/grid/IDMGD01/delete")]
    public async Task<IActionResult> DeleteByPayload([FromBody] IDMGD01Dto dto, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteAsync(dto.ProgramNo, cancellationToken);
        return deleted
            ? Ok(new { status = "success", message = "Deleted.", data = dto.ProgramNo })
            : NotFound(new { status = "error", message = "Program not found." });
    }

    [HttpDelete("/api/internal/form/IDMGD01/{programNo}")]
    public async Task<IActionResult> Delete(string programNo, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteAsync(programNo, cancellationToken);
        return deleted
            ? Ok(new { status = "success", message = "Deleted.", data = programNo })
            : NotFound(new { status = "error", message = "Program not found." });
    }

    [HttpGet("/api/internal/lov/EMPLOYEE")]
    public async Task<IActionResult> EmployeeLov([FromQuery] string? query, CancellationToken cancellationToken)
    {
        var data = await queryService.QueryEmployeesAsync(query, cancellationToken);
        return Ok(new { status = "success", data });
    }
}
