using Web_EIP_Restruct.Models.Dashboard;

namespace Web_EIP_Restruct.Services.Dashboard;

public interface IDashboardQueryService
{
    DashboardSummaryDto GetSummary();
}
