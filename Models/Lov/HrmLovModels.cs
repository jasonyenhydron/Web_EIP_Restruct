using System.Text.Json.Serialization;

namespace Web_EIP_Restruct.Models.Lov
{
    public sealed class HrmLeaveTypeLovItem
    {
        [JsonPropertyName("leave_id")]
        public string LeaveId { get; set; } = string.Empty;

        [JsonPropertyName("leave_name")]
        public string LeaveName { get; set; } = string.Empty;
    }

    public sealed class HrmEmployeeLovItem
    {
        [JsonPropertyName("employee_id")]
        public string EmployeeId { get; set; } = string.Empty;

        [JsonPropertyName("employee_no")]
        public string EmployeeNo { get; set; } = string.Empty;

        [JsonPropertyName("employee_name")]
        public string EmployeeName { get; set; } = string.Empty;
    }

    public sealed class HrmBookingDepartmentLovItem
    {
        [JsonPropertyName("department_id")]
        public string DepartmentId { get; set; } = string.Empty;

        [JsonPropertyName("department_no")]
        public string DepartmentNo { get; set; } = string.Empty;

        [JsonPropertyName("department_name")]
        public string DepartmentName { get; set; } = string.Empty;
    }

    public sealed class LovPageResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }
}

