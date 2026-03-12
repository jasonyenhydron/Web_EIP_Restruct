using Microsoft.AspNetCore.Mvc;
using Web_EIP_Restruct.Infrastructure;
using Web_EIP_Restruct.Models.Lov;

namespace Web_EIP_Restruct.Areas.MIS.Controllers;

[Area("MIS")]
[Route("MIS/IDMGD01")]
[RequireLogon]
public class IDMGD01Controller : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.ProgramTitle = "IDMGD01 程式資料維護";
        ViewBag.EmployeeLov = BuildEmployeeLovConfig();
        return View("~/Views/MisPrograms/IDMGD01.cshtml");
    }

    private static LovInputConfig BuildEmployeeLovConfig()
    {
        var employeeLovSql = Uri.EscapeDataString(@"
SELECT employee_id, employee_no, employee_name
FROM (
    SELECT employee_id, employee_no, employee_name,
           ROW_NUMBER() OVER (ORDER BY employee_no ASC) AS rn
    FROM hrm_employee_v
    WHERE (UPPER(employee_no) LIKE :q OR UPPER(employee_name) LIKE :q)
)
WHERE rn > :offset AND rn <= :endRow");

        return new LovInputConfig
        {
            Title = "員工資料 (Employee)",
            Api = $"/api/lov/query?sql={employeeLovSql}",
            Columns = "員工編號,員工姓名,ID",
            Fields = "employee_no,employee_name,employee_id",
            KeyHidden = "employee_id",
            KeyCode = "employee_no",
            KeyName = "employee_name",
            BufferView = true,
            PageSize = 50,
            SortEnabled = true
        };
    }
}
