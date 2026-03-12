using Web_EIP_Restruct.Models.Programs;

namespace Web_EIP_Restruct.Models.Dashboard;

public sealed class DashboardSummaryDto
{
    public int ProgramCount { get; set; }
    public int VisibleProgramCount { get; set; }
    public int HiddenProgramCount { get; set; }
    public int EmployeeCount { get; set; }
    public IReadOnlyList<IDMGD01Dto> RecentPrograms { get; set; } = [];
    public IReadOnlyList<EmployeeProgramSummaryDto> EmployeeSummaries { get; set; } = [];
}

public sealed class EmployeeProgramSummaryDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int ProgramCount { get; set; }
}
