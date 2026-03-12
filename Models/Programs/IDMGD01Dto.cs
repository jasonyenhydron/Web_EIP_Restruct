namespace Web_EIP_Restruct.Models.Programs;

public sealed class IDMGD01Dto
{
    public string ProgramNo { get; set; } = string.Empty;
    public string DisplayCode { get; set; } = "Y";
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime? PlanStartDevelopDate { get; set; }
    public DateTime? PlanFinishDevelopDate { get; set; }
    public decimal PlanWorkHours { get; set; }
    public decimal RealWorkHours { get; set; }
    public string UpdatedBy { get; set; } = "system";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
