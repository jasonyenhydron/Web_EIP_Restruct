using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Web_EIP_Restruct.Helpers;

namespace Web_EIP_Restruct.Models.Lov
{
    public static class GenericLovRepository
    {
        private static readonly Regex BindRegex = new(@":([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        public static async Task<LovPageResult<Dictionary<string, string>>> QueryAsync(
            string connectionString,
            string sql,
            string query,
            int page,
            int pageSize,
            IReadOnlyDictionary<string, string> extraQuery)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("Missing sql.", nameof(sql));

            var normalized = sql.Trim();
            if (!normalized.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only SELECT is allowed.");
            if (normalized.Contains(";"))
                throw new InvalidOperationException("Semicolon is not allowed.");

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
            var offset = Math.Max(0, (page - 1) * pageSize);
            var endRow = offset + pageSize;

            var foundBinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in BindRegex.Matches(normalized))
            {
                if (m.Success) foundBinds.Add(m.Groups[1].Value);
            }

            var parameters = new List<DbParameter>();
            if (foundBinds.Contains("q"))
                parameters.Add(DbHelper.CreateParameter("q", $"%{(query ?? string.Empty).ToUpperInvariant()}%"));
            if (foundBinds.Contains("offset"))
                parameters.Add(DbHelper.CreateParameter("offset", offset));
            if (foundBinds.Contains("endRow"))
                parameters.Add(DbHelper.CreateParameter("endRow", endRow));

            foreach (var bind in foundBinds)
            {
                if (bind.Equals("q", StringComparison.OrdinalIgnoreCase) ||
                    bind.Equals("offset", StringComparison.OrdinalIgnoreCase) ||
                    bind.Equals("endRow", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                extraQuery.TryGetValue(bind, out var raw);
                if (string.IsNullOrEmpty(raw))
                    parameters.Add(DbHelper.CreateParameter(bind, DBNull.Value));
                else
                    parameters.Add(DbHelper.CreateParameter(bind, raw));
            }

            var dt = await DbHelper.GetDataTableAsync(
                connectionString,
                CommandType.Text,
                normalized,
                parameters.ToArray());

            var data = new List<Dictionary<string, string>>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                var item = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in dt.Columns)
                {
                    item[col.ColumnName.ToLowerInvariant()] = row[col]?.ToString() ?? string.Empty;
                }
                data.Add(item);
            }

            return new LovPageResult<Dictionary<string, string>>
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                HasMore = data.Count >= pageSize
            };
        }
    }
}

