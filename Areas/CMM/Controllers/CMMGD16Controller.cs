using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Infrastructure;

namespace Web_EIP_Restruct.Areas.CMM.Controllers;

[Area("CMM")]
[Route("CMM/CMMGD16")]
[RequireLogon]
public sealed class CMMGD16Controller : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.ProgramTitle = "CMMGD16 經銷商資料維護";
        return View("~/Views/MisPrograms/CMMGD16.cshtml");
    }
}
