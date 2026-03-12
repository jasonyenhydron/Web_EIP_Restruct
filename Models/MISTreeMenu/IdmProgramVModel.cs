namespace Web_EIP_Restruct.Models.MISTreeMenu
{
    public class IdmProgramVModel
    {
        public int? LanguageId { get; set; }
        public long? ProgramId { get; set; }
        public string ProgramNo { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string PlanStartDevelopDate { get; set; } = string.Empty;
        public string PlanFinishDevelopDate { get; set; } = string.Empty;
        public string RealStartDevelopDate { get; set; } = string.Empty;
        public string RealFinishDevelopDate { get; set; } = string.Empty;
        public decimal? PlanWorkHours { get; set; }
        public decimal? RealWorkHours { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string ProgramType { get; set; } = string.Empty;
    }
}

