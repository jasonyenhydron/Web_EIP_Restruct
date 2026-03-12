using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Infrastructure;
using Web_EIP_Restruct.Models.ViewModels;

namespace Web_EIP_Restruct.Areas.PRM.Controllers;

[Area("PRM")]
[Route("PRM/PRMPQ11")]
[RequireLogon]
public sealed class PRMPQ11Controller : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.ProgramTitle = "PRMPQ11 收料查詢";
        return View("~/Views/MisPrograms/PRMPQ11.cshtml", new PrmPq11ViewModel());
    }
}
