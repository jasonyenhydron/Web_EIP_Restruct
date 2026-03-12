using System.Data;
using System.Data.Common;
using System.Text;
using Web_EIP_Restruct.Helpers;

namespace Web_EIP_Restruct.Models.Lov
{
    public static class HrmLovRepository
    {
        public static async Task<LovPageResult<HrmLeaveTypeLovItem>> QueryLeaveTypesAsync(
            string connectionString,
            string query,
            int page,
            int pageSize,
            int languageId = 1)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var offset = (page - 1) * pageSize;
            var endRow = offset + pageSize;

            var sql = new StringBuilder();
            sql.AppendLine("SELECT leave_id, leave_name");
            sql.AppendLine("FROM (");
            sql.AppendLine("    SELECT t.leave_id, t.leave_name,");
            sql.AppendLine("           ROW_NUMBER() OVER (ORDER BY t.leave_id ASC) AS rn");
            sql.AppendLine("    FROM hrm_leave_l t");
            sql.AppendLine("    WHERE t.language_id = :language_id");
            sql.AppendLine("      AND (UPPER(TO_CHAR(t.leave_id)) LIKE :q OR UPPER(t.leave_name) LIKE :q)");
            sql.AppendLine(")");
            sql.AppendLine("WHERE rn > :offset AND rn <= :end_row");

            var parameters = new DbParameter[]
            {
                DbHelper.CreateParameter("language_id", languageId),
                DbHelper.CreateParameter("q", $"%{(query ?? string.Empty).ToUpperInvariant()}%"),
                DbHelper.CreateParameter("offset", offset),
                DbHelper.CreateParameter("end_row", endRow)
            };

            var dt = await DbHelper.GetDataTableAsync(connectionString, CommandType.Text, sql.ToString(), parameters);
            var items = new List<HrmLeaveTypeLovItem>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new HrmLeaveTypeLovItem
                {
                    LeaveId = row["LEAVE_ID"]?.ToString() ?? string.Empty,
                    LeaveName = row["LEAVE_NAME"]?.ToString() ?? string.Empty
                });
            }

            return new LovPageResult<HrmLeaveTypeLovItem>
            {
                Data = items,
                Page = page,
                PageSize = pageSize,
                HasMore = items.Count >= pageSize
            };
        }

        public static async Task<LovPageResult<HrmEmployeeLovItem>> QueryEmployeesAsync(
            string connectionString,
            string query,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var offset = (page - 1) * pageSize;
            var endRow = offset + pageSize;

            var sql = @"
SELECT employee_id, employee_no, employee_name
FROM (
    SELECT t.employee_id, t.employee_no, t.employee_name,
           ROW_NUMBER() OVER (ORDER BY t.employee_no ASC) AS rn
    FROM hrm_employee_v t
    WHERE (UPPER(t.employee_no) LIKE :q OR UPPER(t.employee_name) LIKE :q)
)
WHERE rn > :offset AND rn <= :end_row";

            var parameters = new DbParameter[]
            {
                DbHelper.CreateParameter("q", $"%{(query ?? string.Empty).ToUpperInvariant()}%"),
                DbHelper.CreateParameter("offset", offset),
                DbHelper.CreateParameter("end_row", endRow)
            };

            var dt = await DbHelper.GetDataTableAsync(connectionString, CommandType.Text, sql, parameters);
            var items = new List<HrmEmployeeLovItem>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new HrmEmployeeLovItem
                {
                    EmployeeId = row["EMPLOYEE_ID"]?.ToString() ?? string.Empty,
                    EmployeeNo = row["EMPLOYEE_NO"]?.ToString() ?? string.Empty,
                    EmployeeName = row["EMPLOYEE_NAME"]?.ToString() ?? string.Empty
                });
            }

            return new LovPageResult<HrmEmployeeLovItem>
            {
                Data = items,
                Page = page,
                PageSize = pageSize,
                HasMore = items.Count >= pageSize
            };
        }

        public static async Task<LovPageResult<HrmBookingDepartmentLovItem>> QueryBookingDepartmentsAsync(
            string connectionString,
            string query,
            int page,
            int pageSize,
            string? employeeId,
            int languageId = 1)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var offset = (page - 1) * pageSize;
            var endRow = offset + pageSize;

            var sql = @"
SELECT department_id, department_no, department_name
FROM (
    SELECT t.booking_department_id AS department_id,
           t.booking_department_no AS department_no,
           t.booking_department_name AS department_name,
           ROW_NUMBER() OVER (ORDER BY t.booking_department_no ASC) AS rn
    FROM hrm_booking_department_v t
    WHERE t.language_id = :language_id
      AND (UPPER(t.booking_department_no) LIKE :q OR UPPER(t.booking_department_name) LIKE :q)
      AND (:employee_id IS NULL OR t.employee_id = :employee_id)
)
WHERE rn > :offset AND rn <= :end_row";

            object employeeIdObj = string.IsNullOrWhiteSpace(employeeId) ? DBNull.Value : employeeId.Trim();
            var parameters = new DbParameter[]
            {
                DbHelper.CreateParameter("language_id", languageId),
                DbHelper.CreateParameter("q", $"%{(query ?? string.Empty).ToUpperInvariant()}%"),
                DbHelper.CreateParameter("employee_id", employeeIdObj),
                DbHelper.CreateParameter("offset", offset),
                DbHelper.CreateParameter("end_row", endRow)
            };

            var dt = await DbHelper.GetDataTableAsync(connectionString, CommandType.Text, sql, parameters);
            var items = new List<HrmBookingDepartmentLovItem>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new HrmBookingDepartmentLovItem
                {
                    DepartmentId = row["DEPARTMENT_ID"]?.ToString() ?? string.Empty,
                    DepartmentNo = row["DEPARTMENT_NO"]?.ToString() ?? string.Empty,
                    DepartmentName = row["DEPARTMENT_NAME"]?.ToString() ?? string.Empty
                });
            }

            return new LovPageResult<HrmBookingDepartmentLovItem>
            {
                Data = items,
                Page = page,
                PageSize = pageSize,
                HasMore = items.Count >= pageSize
            };
        }
    }
}

