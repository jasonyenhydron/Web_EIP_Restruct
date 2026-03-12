using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Infrastructure;

namespace Web_EIP_Restruct.Areas.HRM.Controllers;

[Area("HRM")]
[Route("HRM/HRMGD47")]
[RequireLogon]
public sealed class HRMGD47Controller : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        var username = HttpContext.Session.GetString("username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.UserId = username;
        ViewBag.UserName = HttpContext.Session.GetString("user_name") ?? username;

        var numericId = new string(username.Where(char.IsDigit).ToArray());
        ViewBag.NumericUserId = string.IsNullOrEmpty(numericId) ? username : numericId;

        ViewBag.ProgramTitle = "HRMGD47 請假申請";
        ViewBag.LeaveTypeLovApi = "/api/lov/hrm/leave-types";
        ViewBag.EmployeeLovApi = "/api/lov/hrm/employees";
        ViewBag.BookingDeptLovApi = "/api/lov/hrm/booking-departments";

        return View("~/Views/MisPrograms/HRMGD47.cshtml");
    }
}
