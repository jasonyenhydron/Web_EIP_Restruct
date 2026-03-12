using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Infrastructure;
using Web_EIP_Restruct.Models.Dashboard;
using Web_EIP_Restruct.Models.Shared;
using Web_EIP_Restruct.Services.Dashboard;

namespace Web_EIP_Restruct.Controllers;

[ApiController]
[RequireLogon]
public sealed class DashboardApiController(IDashboardQueryService dashboardQueryService) : ControllerBase
{
    [HttpGet("/api/internal/dashboard/summary")]
    public IActionResult Summary()
    {
        var data = dashboardQueryService.GetSummary();
        return Ok(ApiResult<DashboardSummaryDto>.Ok(data));
    }
}
