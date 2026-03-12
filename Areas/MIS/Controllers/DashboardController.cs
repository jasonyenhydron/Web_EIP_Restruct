using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Infrastructure;

namespace Web_EIP_Restruct.Areas.MIS.Controllers;

[Area("MIS")]
[Route("MIS")]
[RequireLogon]
public class DashboardController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();
}
