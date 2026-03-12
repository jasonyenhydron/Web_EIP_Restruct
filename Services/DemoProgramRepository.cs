using System.Collections.Concurrent;
using Web_EIP_Restruct.Models.Programs;

namespace Web_EIP_Restruct.Services;

public sealed class DemoProgramRepository : IDemoProgramRepository
{
    private readonly ConcurrentDictionary<string, IDMGD01Dto> _programs = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Id, string Name)> _employees =
    [
        ("A001", "Jason Yen"),
        ("A002", "Mia Chen"),
        ("A003", "Kevin Lin")
    ];

    public DemoProgramRepository()
    {
        Seed(new IDMGD01Dto
        {
            ProgramNo = "IDMGD01",
            DisplayCode = "Y",
            EmployeeId = "A001",
            EmployeeName = "Jason Yen",
            ProgramType = "ERP",
            Purpose = "Program catalog maintenance",
            PlanStartDevelopDate = DateTime.Today.AddDays(-14),
            PlanFinishDevelopDate = DateTime.Today.AddDays(7),
            PlanWorkHours = 32,
            RealWorkHours = 18
        });

        Seed(new IDMGD01Dto
        {
            ProgramNo = "HRMGD47",
            DisplayCode = "N",
            EmployeeId = "A002",
            EmployeeName = "Mia Chen",
            ProgramType = "HRM",
            Purpose = "Leave request approval setup",
            PlanStartDevelopDate = DateTime.Today.AddDays(-30),
            PlanFinishDevelopDate = DateTime.Today.AddDays(-5),
            PlanWorkHours = 60,
            RealWorkHours = 64
        });
    }

    public IReadOnlyList<IDMGD01Dto> ListPrograms(string? programNo = null, string? employeeId = null, string? displayCode = null)
    {
        IEnumerable<IDMGD01Dto> query = _programs.Values.OrderBy(x => x.ProgramNo);

        if (!string.IsNullOrWhiteSpace(programNo))
        {
            query = query.Where(x => x.ProgramNo.Contains(programNo, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(employeeId))
        {
            query = query.Where(x => x.EmployeeId.Equals(employeeId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(displayCode))
        {
            query = query.Where(x => x.DisplayCode.Equals(displayCode, StringComparison.OrdinalIgnoreCase));
        }

        return query.Select(Clone).ToList();
    }

    public IDMGD01Dto? GetProgram(string programNo) =>
        _programs.TryGetValue(programNo, out var program) ? Clone(program) : null;

    public IDMGD01Dto SaveProgram(IDMGD01Dto dto)
    {
        dto.EmployeeName = _employees.FirstOrDefault(x => x.Id.Equals(dto.EmployeeId, StringComparison.OrdinalIgnoreCase)).Name ?? dto.EmployeeName;
        dto.UpdatedAt = DateTime.UtcNow;
        _programs.AddOrUpdate(dto.ProgramNo, _ => Clone(dto), (_, _) => Clone(dto));
        return Clone(dto);
    }

    public bool DeleteProgram(string programNo) => _programs.TryRemove(programNo, out _);

    public IReadOnlyList<object> ListEmployees(string? query = null)
    {
        var source = _employees.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            source = source.Where(x =>
                x.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return source.Select(x => (object)new
        {
            employeeId = x.Id,
            employeeNo = x.Id,
            employeeName = x.Name
        }).ToList();
    }

    private void Seed(IDMGD01Dto dto) => _programs[dto.ProgramNo] = Clone(dto);

    private static IDMGD01Dto Clone(IDMGD01Dto dto) => new()
    {
        ProgramNo = dto.ProgramNo,
        DisplayCode = dto.DisplayCode,
        EmployeeId = dto.EmployeeId,
        EmployeeName = dto.EmployeeName,
        ProgramType = dto.ProgramType,
        Purpose = dto.Purpose,
        PlanStartDevelopDate = dto.PlanStartDevelopDate,
        PlanFinishDevelopDate = dto.PlanFinishDevelopDate,
        PlanWorkHours = dto.PlanWorkHours,
        RealWorkHours = dto.RealWorkHours,
        UpdatedAt = dto.UpdatedAt,
        UpdatedBy = dto.UpdatedBy
    };
}
