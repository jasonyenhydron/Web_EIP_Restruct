using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Models.ViewModels;

namespace Web_EIP_Restruct.Controllers
{
    public class PrmPq11Controller : Controller
    {
        private static string BuildDbConnectionString(string? tns) => DbHelper.BuildConnectionString(tns);

        [HttpGet("Prm/PRMPQ11")]
        [HttpGet("Prm/PRMPQ01")]
        [HttpGet("PRMPQ11")]
        [HttpGet("PRMPQ01")]
        public IActionResult PRMPQ11()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            return RedirectToAction("Index", "PRMPQ11", new { area = "PRM" });
        }

        [HttpGet("PrmPq11/selectHead")]
        public async Task<IActionResult> SelectHead(
            string DataMember,
            int organizationId = 10611,
            string? vendorId = null,
            string? vendorShipNo = null,
            string? partReceiptNoFrom = null,
            string? partReceiptNoTo = null,
            string? companyId = null,
            string? departmentId = null,
            string? orderKindId = null,
            string? receiptDateFrom = null,
            string? receiptDateTo = null,
            string? partNo = null,
            string? expireDateFrom = null,
            string? expireDateTo = null,
            string? manufactureDateFrom = null,
            string? manufactureDateTo = null,
            string? lotNumber = null,
            string? asnNo = null,
            string? statusFrom = null,
            string? statusTo = "95")
        {
            if (!IsSupportedDataMember(DataMember))
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            const string sql = @"
SELECT ROWID,
       RH.ENTRY_DATE,
       RH.TR_DATE,
       RH.PART_RECEIPT_HEAD_ID,
       RH.ORGANIZATION_ID,
       RH.COMPANY_ID,
       RH.DEPARTMENT_ID,
       RH.RECEIPT_DATE,
       RH.TAX_AMT,
       RH.ENTRY_ID,
       RH.TR_ID,
       RH.ACCOUNT_PROCESS_YN,
       RH.IN_DATE,
       RH.PART_RECEIPT_NO,
       RH.ORDER_KIND_ID,
       RH.VENDOR_ID,
       RH.TAX_RATE,
       RH.TAX_TYPE_ID,
       RH.CURRENCY_ID,
       RH.EXCHANGE_RATE,
       RH.VENDOR_INVOICE_NO,
       RH.VENDOR_SHIP_NO,
       RH.MEMO,
       RH.PURCHASING_AGENT_ID,
       RH.AUDIT_USER_NO,
       RH.AUDIT_DATE,
       RH.CONFIRM_USER_NO,
       RH.CONFIRM_DATE,
       RH.PART_RECEIPT_HEAD_STATUS,
       RH.RESEND_TYPE,
       RH.CURRENCY_NON_TAX_AMT,
       RH.CURRENCY_TAX_AMT,
       RH.PART_RECEIPT_HEAD_SRC_KIND,
       RH.PART_RECEIPT_HEAD_SRC_PK,
       RH.ACCEPTANCE_MEETING_DATE
  FROM PRM_PART_RECEIPT_HEAD RH
 WHERE RH.ORGANIZATION_ID = :organization_id
   AND (RH.VENDOR_ID = :vendor_id OR :vendor_id IS NULL)
   AND (:vendor_ship_no IS NULL
        OR RH.VENDOR_SHIP_NO LIKE :vendor_ship_no || '%'
        OR EXISTS (
            SELECT 'x'
              FROM PRM_PART_RECEIPT_DETAIL RD
             WHERE RD.PART_RECEIPT_HEAD_ID = RH.PART_RECEIPT_HEAD_ID
               AND RD.VENDOR_SHIP_NO LIKE :vendor_ship_no || '%'))
   AND (RH.PART_RECEIPT_NO >= :part_receipt_no_from OR :part_receipt_no_from IS NULL)
   AND (RH.PART_RECEIPT_NO <= :part_receipt_no_to OR :part_receipt_no_to IS NULL)
   AND (RH.COMPANY_ID = :company_id OR :company_id IS NULL)
   AND (RH.DEPARTMENT_ID = :department_id OR :department_id IS NULL)
   AND (RH.ORDER_KIND_ID = :order_kind_id OR :order_kind_id IS NULL)
   AND (RH.RECEIPT_DATE >= TO_DATE(:receipt_date_from, 'YYYY-MM-DD') OR :receipt_date_from IS NULL)
   AND (RH.RECEIPT_DATE <= TO_DATE(:receipt_date_to, 'YYYY-MM-DD') OR :receipt_date_to IS NULL)
   AND ((:part_no IS NULL
         AND :expire_date_from IS NULL
         AND :expire_date_to IS NULL
         AND :manufacture_date_from IS NULL
         AND :manufacture_date_to IS NULL
         AND :lot_number IS NULL
         AND :asn_no IS NULL)
        OR RH.PART_RECEIPT_HEAD_ID IN (
            SELECT D.PART_RECEIPT_HEAD_ID
              FROM PRM_PART_RECEIPT_DETAIL D
              JOIN IVM_PART PT
                ON PT.PART_ID = D.PART_ID
             WHERE PT.PART_NO LIKE :part_no || '%'
               AND (D.EXPIRE_DATE >= TO_DATE(:expire_date_from, 'YYYY-MM-DD') OR :expire_date_from IS NULL)
               AND (D.EXPIRE_DATE <= TO_DATE(:expire_date_to, 'YYYY-MM-DD') OR :expire_date_to IS NULL)
               AND (D.MANUFACTURE_DATE >= TO_DATE(:manufacture_date_from, 'YYYY-MM-DD') OR :manufacture_date_from IS NULL)
               AND (D.MANUFACTURE_DATE <= TO_DATE(:manufacture_date_to, 'YYYY-MM-DD') OR :manufacture_date_to IS NULL)
               AND (D.LOT_NUMBER LIKE :lot_number || '%' OR :lot_number IS NULL)
               AND (D.ASN_NO = :asn_no OR :asn_no IS NULL)))
   AND RH.ORDER_KIND_ID IN (
       SELECT ORDER_KIND_ID
         FROM PRM_ROLE_ORDER_KIND_V
        WHERE ORGANIZATION_ID = :organization_id
          AND LANGUAGE_ID = '1'
          AND USER_NO = :user_no
          AND PROGRAM_ID = 81141)
   AND (RH.PART_RECEIPT_HEAD_STATUS >= :status_from OR :status_from IS NULL)
   AND (RH.PART_RECEIPT_HEAD_STATUS <= :status_to OR :status_to IS NULL)
 ORDER BY RH.PART_RECEIPT_NO";

            try
            {
                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("organization_id", organizationId),
                        DbHelper.CreateParameter("vendor_id", ToDb(vendorId)),
                        DbHelper.CreateParameter("vendor_ship_no", ToDb(vendorShipNo)),
                        DbHelper.CreateParameter("part_receipt_no_from", ToDb(partReceiptNoFrom)),
                        DbHelper.CreateParameter("part_receipt_no_to", ToDb(partReceiptNoTo)),
                        DbHelper.CreateParameter("company_id", ToDb(companyId)),
                        DbHelper.CreateParameter("department_id", ToDb(departmentId)),
                        DbHelper.CreateParameter("order_kind_id", ToDb(orderKindId)),
                        DbHelper.CreateParameter("receipt_date_from", ToDb(receiptDateFrom)),
                        DbHelper.CreateParameter("receipt_date_to", ToDb(receiptDateTo)),
                        DbHelper.CreateParameter("part_no", ToDb(partNo)),
                        DbHelper.CreateParameter("expire_date_from", ToDb(expireDateFrom)),
                        DbHelper.CreateParameter("expire_date_to", ToDb(expireDateTo)),
                        DbHelper.CreateParameter("manufacture_date_from", ToDb(manufactureDateFrom)),
                        DbHelper.CreateParameter("manufacture_date_to", ToDb(manufactureDateTo)),
                        DbHelper.CreateParameter("lot_number", ToDb(lotNumber)),
                        DbHelper.CreateParameter("asn_no", ToDb(asnNo)),
                        DbHelper.CreateParameter("user_no", username.ToUpperInvariant()),
                        DbHelper.CreateParameter("status_from", ToDb(statusFrom)),
                        DbHelper.CreateParameter("status_to", ToDb(statusTo))
                    });

                return Ok(new { status = "success", data = ToRows(dt) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("PrmPq11/selectDetail")]
        public async Task<IActionResult> SelectDetail(string DataMember, decimal? partReceiptHeadId)
        {
            if (!IsSupportedDataMember(DataMember))
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (!partReceiptHeadId.HasValue)
                return Ok(new { status = "success", data = Array.Empty<object>() });

            const string sql = @"
SELECT D.ROWID,
       D.PART_RECEIPT_DETAIL_ID,
       D.PART_RECEIPT_HEAD_ID,
       D.PART_ID,
       (SELECT PT.PART_NO FROM IVM_PART PT WHERE PT.PART_ID = D.PART_ID) AS PART_NO,
       D.VENDOR_SHIP_NO,
       D.LOT_NUMBER,
       D.MANUFACTURE_DATE,
       D.EXPIRE_DATE,
       D.ASN_NO
  FROM PRM_PART_RECEIPT_DETAIL D
 WHERE D.PART_RECEIPT_HEAD_ID = :part_receipt_head_id
 ORDER BY D.PART_RECEIPT_DETAIL_ID";

            try
            {
                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("part_receipt_head_id", partReceiptHeadId.Value)
                    });

                return Ok(new { status = "success", data = ToRows(dt) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private static object? ToDb(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

        private static bool IsSupportedDataMember(string? dataMember)
        {
            return string.Equals(dataMember, "PRMPQ11", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(dataMember, "PRMPQ01", StringComparison.OrdinalIgnoreCase);
        }

        private static List<Dictionary<string, object>> ToRows(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var item = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    item[col.ColumnName] = row[col] == DBNull.Value ? string.Empty : row[col];
                list.Add(item);
            }

            return list;
        }
    }
}

