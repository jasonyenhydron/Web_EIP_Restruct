using Web_EIP_Restruct.Models.Dashboard;

namespace Web_EIP_Restruct.Services.Dashboard;

public sealed class DashboardQueryService(IDemoProgramRepository repository) : IDashboardQueryService
{
    public DashboardSummaryDto GetSummary()
    {
        var programs = repository.ListPrograms();
        var employees = repository.ListEmployees()
            .Select(x => new
            {
                EmployeeId = (string?)x.GetType().GetProperty("employeeId")?.GetValue(x) ?? string.Empty,
                EmployeeName = (string?)x.GetType().GetProperty("employeeName")?.GetValue(x) ?? string.Empty
            })
            .ToList();

        var employeeSummaries = programs
            .GroupBy(x => new { x.EmployeeId, x.EmployeeName })
            .Select(x => new EmployeeProgramSummaryDto
            {
                EmployeeId = x.Key.EmployeeId,
                EmployeeName = x.Key.EmployeeName,
                ProgramCount = x.Count()
            })
            .OrderByDescending(x => x.ProgramCount)
            .ThenBy(x => x.EmployeeId)
            .ToList();

        return new DashboardSummaryDto
        {
            ProgramCount = programs.Count,
            VisibleProgramCount = programs.Count(x => x.DisplayCode.Equals("Y", StringComparison.OrdinalIgnoreCase)),
            HiddenProgramCount = programs.Count(x => x.DisplayCode.Equals("N", StringComparison.OrdinalIgnoreCase)),
            EmployeeCount = employees.Count,
            RecentPrograms = programs.OrderByDescending(x => x.UpdatedAt).Take(6).ToList(),
            EmployeeSummaries = employeeSummaries
        };
    }
}
