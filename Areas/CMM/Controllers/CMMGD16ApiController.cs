using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Controllers;
using Web_EIP_Restruct.Infrastructure;

namespace Web_EIP_Restruct.Areas.CMM.Controllers;

[ApiController]
[Area("CMM")]
[RequireLogon]
public sealed class CMMGD16ApiController : CmmController
{
    public CMMGD16ApiController(ILogger<CmmController> logger) : base(logger)
    {
    }

    [HttpGet("/api/internal/grid/CMMGD16")]
    [HttpGet("/api/internal/grid/CMMGD16/select")]
    public Task<IActionResult> Grid([FromQuery] bool debugSql = false) =>
        Select("CMMGD16", debugSql);

    [HttpPost("/api/internal/grid/CMMGD16/insert")]
    [HttpPost("/api/internal/form/CMMGD16")]
    public Task<IActionResult> InsertInternal([FromBody] Dictionary<string, object> payload) =>
        Insert("CMMGD16", payload);

    [HttpPost("/api/internal/grid/CMMGD16/update")]
    public Task<IActionResult> UpdateInternal([FromBody] Dictionary<string, object> payload) =>
        Update("CMMGD16", payload);

    [HttpPost("/api/internal/grid/CMMGD16/delete")]
    public Task<IActionResult> DeleteInternal([FromBody] Dictionary<string, object> payload) =>
        Delete("CMMGD16", payload);
}
