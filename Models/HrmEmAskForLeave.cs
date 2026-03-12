using System;
using System.ComponentModel.DataAnnotations;

namespace Web_EIP_Restruct.Models
{
    public class HrmEmAskForLeave
    {
        public long? EmAskForLeaveId { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public long LeaveId { get; set; }

        public decimal? SystemLeaveHours { get; set; }
        public decimal? LeaveHours { get; set; }
        public decimal? LeaveDays { get; set; }
        public long? PeriodId { get; set; }

        [MaxLength(256)]
        public string AskForLeaveReason { get; set; } = string.Empty;

        public DateTime? TimeCardDate { get; set; }

        [MaxLength(2)]
        public string EmAskForLeaveStatus { get; set; } = "00";

        [MaxLength(1)]
        public string FlowYn { get; set; } = "N";

        public long? OvertimePeriodId { get; set; }

        [MaxLength(30)]
        public string ConfirmUser { get; set; } = string.Empty;

        public long? AgentEmployeeId { get; set; }

        [MaxLength(100)]
        public string DestinationPlace { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TalkingAbout { get; set; } = string.Empty;

        [MaxLength(1)]
        public string ReturnYn { get; set; } = "N";

        public long? PayApplyId { get; set; }

        [MaxLength(30)]
        public string TransportationCode { get; set; } = string.Empty;

        public decimal? UnitAmt { get; set; }
        public decimal? Amt { get; set; }
        public decimal? Distance { get; set; }
        public long? CompanyCarId { get; set; }

        [MaxLength(30)]
        public string EntryId { get; set; } = string.Empty;

        public DateTime? EntryDate { get; set; }

        [MaxLength(30)]
        public string TrId { get; set; } = string.Empty;

        public DateTime? TrDate { get; set; }

        [MaxLength(1)]
        public string OfferedCertificateYn { get; set; } = "N";

        public DateTime? VestingDate { get; set; }

        [MaxLength(2)]
        public string ReconHoursStatus { get; set; } = string.Empty;

        [MaxLength(1)]
        public string OverseasYn { get; set; } = "N";

        [MaxLength(1)]
        public string DocHrConfirmYn { get; set; } = "N";

        [MaxLength(30)]
        public string DocHrConfirmUser { get; set; } = string.Empty;

        public DateTime? DocHrConfirmTime { get; set; }
    }
}

