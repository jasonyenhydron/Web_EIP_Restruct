using Web_EIP_Restruct.Models.Programs;

namespace Web_EIP_Restruct.Services;

public interface IDemoProgramRepository
{
    IReadOnlyList<IDMGD01Dto> ListPrograms(string? programNo = null, string? employeeId = null, string? displayCode = null);
    IDMGD01Dto? GetProgram(string programNo);
    IDMGD01Dto SaveProgram(IDMGD01Dto dto);
    bool DeleteProgram(string programNo);
    IReadOnlyList<object> ListEmployees(string? query = null);
}
