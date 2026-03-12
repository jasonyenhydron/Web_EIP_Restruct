using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Controllers;
using Web_EIP_Restruct.Infrastructure;

namespace Web_EIP_Restruct.Areas.PRM.Controllers;

[ApiController]
[Area("PRM")]
[RequireLogon]
public sealed class PRMPQ11ApiController : PrmPq11Controller
{
    [HttpGet("/api/internal/grid/PRMPQ11")]
    [HttpGet("/api/internal/grid/PRMPQ11/select")]
    public Task<IActionResult> Grid(
        [FromQuery] int organizationId = 10611,
        [FromQuery] string? vendorId = null,
        [FromQuery] string? vendorShipNo = null,
        [FromQuery] string? partReceiptNoFrom = null,
        [FromQuery] string? partReceiptNoTo = null,
        [FromQuery] string? companyId = null,
        [FromQuery] string? departmentId = null,
        [FromQuery] string? orderKindId = null,
        [FromQuery] string? receiptDateFrom = null,
        [FromQuery] string? receiptDateTo = null,
        [FromQuery] string? partNo = null,
        [FromQuery] string? expireDateFrom = null,
        [FromQuery] string? expireDateTo = null,
        [FromQuery] string? manufactureDateFrom = null,
        [FromQuery] string? manufactureDateTo = null,
        [FromQuery] string? lotNumber = null,
        [FromQuery] string? asnNo = null,
        [FromQuery] string? statusFrom = null,
        [FromQuery] string? statusTo = "95") =>
        SelectHead("PRMPQ11", organizationId, vendorId, vendorShipNo, partReceiptNoFrom, partReceiptNoTo, companyId, departmentId, orderKindId, receiptDateFrom, receiptDateTo, partNo, expireDateFrom, expireDateTo, manufactureDateFrom, manufactureDateTo, lotNumber, asnNo, statusFrom, statusTo);

    [HttpGet("/api/internal/grid/PRMPQ11/detail")]
    public Task<IActionResult> Detail([FromQuery] decimal? partReceiptHeadId) =>
        SelectDetail("PRMPQ11", partReceiptHeadId);
}
