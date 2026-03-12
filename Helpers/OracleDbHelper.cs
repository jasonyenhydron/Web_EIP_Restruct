using System.Data;
using System.Data.Common;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace Web_EIP_Restruct.Helpers
{
    public static class OracleDbHelper
    {
        private static readonly object SyncRoot = new();
        private static readonly ConcurrentDictionary<string, DbParameter[]> ParameterCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex OracleObjectNameRegex = new(@"^[A-Za-z][A-Za-z0-9_$#]*(\.[A-Za-z][A-Za-z0-9_$#]*){0,2}$", RegexOptions.Compiled);
        private static DbProviderFactory _providerFactory = OracleClientFactory.Instance;
        private static string _defaultConnectionString = string.Empty;

        public static int MaxRetryCount { get; set; } = 10;

        public static int RetryWaitMilliseconds { get; set; } = 6000;

        public static int DefaultCommandTimeout { get; set; } = 0;

        public static string DefaultConnectionString => _defaultConnectionString;

        public static void Configure(string defaultConnectionString, DbProviderFactory? providerFactory = null)
        {
            lock (SyncRoot)
            {
                _defaultConnectionString = defaultConnectionString?.Trim() ?? string.Empty;
                _providerFactory = providerFactory ?? OracleClientFactory.Instance;
            }
        }

        public static DbConnection CreateConnection(string connectionString)
        {
            var conn = _providerFactory.CreateConnection() ?? throw new InvalidOperationException("Cannot create DbConnection.");
            conn.ConnectionString = connectionString;
            return conn;
        }

        public static OracleConnection GetConnection()
        {
            return new OracleConnection(GetDefaultConnectionString());
        }

        public static OracleConnection GetConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }

        public static DbParameter CreateParameter(
            string name,
            object? value,
            DbType? dbType = null,
            ParameterDirection direction = ParameterDirection.Input,
            int? size = null)
        {
            var p = _providerFactory.CreateParameter() ?? throw new InvalidOperationException("Cannot create DbParameter.");
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            p.Direction = direction;
            if (dbType.HasValue) p.DbType = dbType.Value;
            if (size.HasValue) p.Size = size.Value;
            return p;
        }

        public static void CacheParameters(string cacheKey, params DbParameter[]? parameters)
        {
            if (string.IsNullOrWhiteSpace(cacheKey) || parameters == null)
                return;

            ParameterCache[cacheKey] = CloneParameters(parameters);
        }

        public static DbParameter[]? GetCachedParameters(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
                return null;

            return ParameterCache.TryGetValue(cacheKey, out var cached)
                ? CloneParameters(cached)
                : null;
        }

        public static int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteNonQuery(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static int ExecuteNonQueryText(string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteNonQuery(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            using var conn = CreateConnection(connectionString);
            using var cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            var val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        public static async Task<int> ExecuteNonQueryAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? commandParameters = null, CancellationToken cancellationToken = default)
        {
            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, commandParameters, cancellationToken);
            var val = await cmd.ExecuteNonQueryAsync(cancellationToken);
            cmd.Parameters.Clear();
            return val;
        }

        public static int ExecuteNonQueryRetry(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            return Retry(() => ExecuteNonQuery(connectionString, cmdType, cmdText, commandParameters));
        }

        public static object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteScalar(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static object? ExecuteScalarText(string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteScalar(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static object? ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            using var conn = CreateConnection(connectionString);
            using var cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            var val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        public static async Task<object?> ExecuteScalarAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? commandParameters = null, CancellationToken cancellationToken = default)
        {
            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, commandParameters, cancellationToken);
            var val = await cmd.ExecuteScalarAsync(cancellationToken);
            cmd.Parameters.Clear();
            return val;
        }

        public static object? ExecuteScalarRetry(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            return Retry(() => ExecuteScalar(connectionString, cmdType, cmdText, commandParameters));
        }

        public static DbDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteReader(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static DbDataReader ExecuteReaderText(string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteReader(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static DbDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            var conn = CreateConnection(connectionString);
            var cmd = conn.CreateCommand();
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return reader;
            }
            catch
            {
                conn.Close();
                conn.Dispose();
                cmd.Dispose();
                throw;
            }
        }

        public static DataTable GetDataTable(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            GetDataTable(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static DataTable GetDataTableText(string cmdText, params DbParameter[]? commandParameters) =>
            GetDataTable(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static DataTable GetDataTable(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            var dt = new DataTable();
            using var conn = CreateConnection(connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 0;
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            using var adapter = _providerFactory.CreateDataAdapter() ?? throw new InvalidOperationException("Cannot create DbDataAdapter.");
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            cmd.Parameters.Clear();
            return dt;
        }

        public static async Task<DataTable> GetDataTableAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? commandParameters = null, CancellationToken cancellationToken = default)
        {
            var dt = new DataTable();
            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, commandParameters, cancellationToken);
            using var adapter = _providerFactory.CreateDataAdapter() ?? throw new InvalidOperationException("Cannot create DbDataAdapter.");
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            cmd.Parameters.Clear();
            return dt;
        }

        public static DataTable GetDataTableRetry(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            return Retry(() => GetDataTable(connectionString, cmdType, cmdText, commandParameters));
        }

        public static void ExecuteSqlTran(Dictionary<string, DbParameter[]?> sqlStringList) =>
            ExecuteSqlTran(GetDefaultConnectionString(), CommandType.Text, sqlStringList);

        public static void ExecuteSqlTran(string connectionString, CommandType cmdType, Dictionary<string, DbParameter[]?> sqlStringList)
        {
            using var conn = CreateConnection(connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            try
            {
                foreach (var item in sqlStringList)
                {
                    PrepareCommand(cmd, conn, trans, cmdType, item.Key, item.Value);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public static void ExecuteSqlTran(string connectionString, CommandType cmdType, IEnumerable<SqlBatchItem> items)
        {
            using var conn = CreateConnection(connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            try
            {
                foreach (var item in items)
                {
                    PrepareCommand(cmd, conn, trans, cmdType, item.CommandText, item.Parameters);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public static bool Exists(string connectionString, string sql, params DbParameter[]? commandParameters)
        {
            var scalar = ExecuteScalar(connectionString, CommandType.Text, sql, commandParameters);
            if (scalar == null || scalar == DBNull.Value) return false;
            return Convert.ToInt64(scalar) > 0;
        }

        public static bool Exists(string sql, params DbParameter[]? commandParameters) =>
            Exists(GetDefaultConnectionString(), sql, commandParameters);

        private static void PrepareCommand(
            DbCommand cmd,
            DbConnection conn,
            DbTransaction? trans,
            CommandType cmdType,
            string cmdText,
            params DbParameter[]? cmdParams)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            if (trans != null) cmd.Transaction = trans;
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = DefaultCommandTimeout;
            if (cmd is OracleCommand oracleCmd)
                oracleCmd.BindByName = true;
            if (cmdParams != null && cmdParams.Length > 0)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(cmdParams);
            }
        }

        private static async Task PrepareCommandAsync(
            DbCommand cmd,
            DbConnection conn,
            DbTransaction? trans,
            CommandType cmdType,
            string cmdText,
            DbParameter[]? cmdParams,
            CancellationToken cancellationToken)
        {
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(cancellationToken);
            if (trans != null) cmd.Transaction = trans;
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = DefaultCommandTimeout;
            if (cmd is OracleCommand oracleCmd)
                oracleCmd.BindByName = true;
            if (cmdParams != null && cmdParams.Length > 0)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(cmdParams);
            }
        }

        public static async Task<Dictionary<string, object?>> ExecuteProcedureWithOutputsAsync(
            string connectionString,
            string procedureName,
            DbParameter[]? commandParameters = null,
            CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(procedureName, nameof(procedureName));
            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, CommandType.StoredProcedure, procedureName, commandParameters, cancellationToken);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return CollectOutputValues(cmd.Parameters);
        }

        public static Task<Dictionary<string, object?>> ExecuteProcedureWithOutputsAsync(
            string procedureName,
            DbParameter[]? commandParameters = null,
            CancellationToken cancellationToken = default) =>
            ExecuteProcedureWithOutputsAsync(GetDefaultConnectionString(), procedureName, commandParameters, cancellationToken);

        public static Task<Dictionary<string, object?>> ExecutePackageProcedureWithOutputsAsync(
            string connectionString,
            string packageName,
            string procedureName,
            DbParameter[]? commandParameters = null,
            CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(packageName, nameof(packageName));
            EnsureOracleObjectName(procedureName, nameof(procedureName));
            return ExecuteProcedureWithOutputsAsync(connectionString, $"{packageName}.{procedureName}", commandParameters, cancellationToken);
        }

        public static Task<Dictionary<string, object?>> ExecutePackageProcedureWithOutputsAsync(
            string packageName,
            string procedureName,
            DbParameter[]? commandParameters = null,
            CancellationToken cancellationToken = default) =>
            ExecutePackageProcedureWithOutputsAsync(GetDefaultConnectionString(), packageName, procedureName, commandParameters, cancellationToken);

        public static async Task<object?> ExecuteFunctionScalarAsync(
            string connectionString,
            string functionName,
            DbType returnType = DbType.String,
            DbParameter[]? inputParameters = null,
            CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(functionName, nameof(functionName));

            var returnParam = CreateParameter("return_value", null, returnType, ParameterDirection.ReturnValue);
            var allParams = new List<DbParameter> { returnParam };
            if (inputParameters != null && inputParameters.Length > 0)
                allParams.AddRange(inputParameters);

            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, CommandType.StoredProcedure, functionName, allParams.ToArray(), cancellationToken);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return NormalizeDbValue(returnParam.Value);
        }

        public static Task<object?> ExecuteFunctionScalarAsync(
            string functionName,
            DbType returnType = DbType.String,
            DbParameter[]? inputParameters = null,
            CancellationToken cancellationToken = default) =>
            ExecuteFunctionScalarAsync(GetDefaultConnectionString(), functionName, returnType, inputParameters, cancellationToken);

        public static async Task CreateSchedulerJobAsync(
            string connectionString,
            string jobName,
            string jobType,
            string jobAction,
            DateTime? startDate = null,
            string? repeatInterval = null,
            bool enabled = true,
            bool autoDrop = false,
            string? comments = null,
            CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(jobName, nameof(jobName));
            if (string.IsNullOrWhiteSpace(jobType)) throw new ArgumentException("jobType is required.", nameof(jobType));
            if (string.IsNullOrWhiteSpace(jobAction)) throw new ArgumentException("jobAction is required.", nameof(jobAction));

            const string sql = @"
BEGIN
    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => :job_name,
        job_type        => :job_type,
        job_action      => :job_action,
        start_date      => :start_date,
        repeat_interval => :repeat_interval,
        enabled         => CASE WHEN :enabled_flag = 1 THEN TRUE ELSE FALSE END,
        auto_drop       => CASE WHEN :auto_drop_flag = 1 THEN TRUE ELSE FALSE END,
        comments        => :comments
    );
END;";

            await ExecuteNonQueryAsync(
                connectionString,
                CommandType.Text,
                sql,
                new[]
                {
                    CreateParameter("job_name", jobName),
                    CreateParameter("job_type", jobType),
                    CreateParameter("job_action", jobAction),
                    CreateParameter("start_date", startDate.HasValue ? (object)startDate.Value : DBNull.Value, DbType.DateTime),
                    CreateParameter("repeat_interval", string.IsNullOrWhiteSpace(repeatInterval) ? (object)DBNull.Value : repeatInterval),
                    CreateParameter("enabled_flag", enabled ? 1 : 0, DbType.Int32),
                    CreateParameter("auto_drop_flag", autoDrop ? 1 : 0, DbType.Int32),
                    CreateParameter("comments", string.IsNullOrWhiteSpace(comments) ? (object)DBNull.Value : comments)
                },
                cancellationToken);
        }

        public static Task CreateSchedulerJobAsync(
            string jobName,
            string jobType,
            string jobAction,
            DateTime? startDate = null,
            string? repeatInterval = null,
            bool enabled = true,
            bool autoDrop = false,
            string? comments = null,
            CancellationToken cancellationToken = default) =>
            CreateSchedulerJobAsync(GetDefaultConnectionString(), jobName, jobType, jobAction, startDate, repeatInterval, enabled, autoDrop, comments, cancellationToken);

        public static Task<int> DropSchedulerJobAsync(string connectionString, string jobName, bool force = true, CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(jobName, nameof(jobName));
            const string sql = @"
BEGIN
    DBMS_SCHEDULER.DROP_JOB(
        job_name => :job_name,
        force    => CASE WHEN :force_flag = 1 THEN TRUE ELSE FALSE END
    );
END;";

            return ExecuteNonQueryAsync(
                connectionString,
                CommandType.Text,
                sql,
                new[]
                {
                    CreateParameter("job_name", jobName),
                    CreateParameter("force_flag", force ? 1 : 0, DbType.Int32)
                },
                cancellationToken);
        }

        public static Task<int> DropSchedulerJobAsync(string jobName, bool force = true, CancellationToken cancellationToken = default) =>
            DropSchedulerJobAsync(GetDefaultConnectionString(), jobName, force, cancellationToken);

        public static Task<int> RunSchedulerJobAsync(string connectionString, string jobName, bool useCurrentSession = false, CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(jobName, nameof(jobName));
            const string sql = @"
BEGIN
    DBMS_SCHEDULER.RUN_JOB(
        job_name            => :job_name,
        use_current_session => CASE WHEN :use_current_session_flag = 1 THEN TRUE ELSE FALSE END
    );
END;";

            return ExecuteNonQueryAsync(
                connectionString,
                CommandType.Text,
                sql,
                new[]
                {
                    CreateParameter("job_name", jobName),
                    CreateParameter("use_current_session_flag", useCurrentSession ? 1 : 0, DbType.Int32)
                },
                cancellationToken);
        }

        public static Task<int> RunSchedulerJobAsync(string jobName, bool useCurrentSession = false, CancellationToken cancellationToken = default) =>
            RunSchedulerJobAsync(GetDefaultConnectionString(), jobName, useCurrentSession, cancellationToken);

        public static Task<int> EnableSchedulerJobAsync(string connectionString, string jobName, CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(jobName, nameof(jobName));
            const string sql = "BEGIN DBMS_SCHEDULER.ENABLE(name => :job_name); END;";
            return ExecuteNonQueryAsync(
                connectionString,
                CommandType.Text,
                sql,
                new[] { CreateParameter("job_name", jobName) },
                cancellationToken);
        }

        public static Task<int> EnableSchedulerJobAsync(string jobName, CancellationToken cancellationToken = default) =>
            EnableSchedulerJobAsync(GetDefaultConnectionString(), jobName, cancellationToken);

        public static Task<int> DisableSchedulerJobAsync(string connectionString, string jobName, bool force = false, CancellationToken cancellationToken = default)
        {
            EnsureOracleObjectName(jobName, nameof(jobName));
            const string sql = @"
BEGIN
    DBMS_SCHEDULER.DISABLE(
        name  => :job_name,
        force => CASE WHEN :force_flag = 1 THEN TRUE ELSE FALSE END
    );
END;";

            return ExecuteNonQueryAsync(
                connectionString,
                CommandType.Text,
                sql,
                new[]
                {
                    CreateParameter("job_name", jobName),
                    CreateParameter("force_flag", force ? 1 : 0, DbType.Int32)
                },
                cancellationToken);
        }

        public static Task<int> DisableSchedulerJobAsync(string jobName, bool force = false, CancellationToken cancellationToken = default) =>
            DisableSchedulerJobAsync(GetDefaultConnectionString(), jobName, force, cancellationToken);

        public static Task<DataTable> GetSchedulerJobsAsync(string connectionString, string? owner = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                const string userSql = @"
SELECT JOB_NAME, ENABLED, STATE, JOB_TYPE, JOB_ACTION, REPEAT_INTERVAL, START_DATE, NEXT_RUN_DATE, LAST_START_DATE
FROM USER_SCHEDULER_JOBS
ORDER BY JOB_NAME";
                return GetDataTableAsync(connectionString, CommandType.Text, userSql, null, cancellationToken);
            }

            EnsureOracleObjectName(owner, nameof(owner));
            const string allSql = @"
SELECT OWNER, JOB_NAME, ENABLED, STATE, JOB_TYPE, JOB_ACTION, REPEAT_INTERVAL, START_DATE, NEXT_RUN_DATE, LAST_START_DATE
FROM ALL_SCHEDULER_JOBS
WHERE OWNER = UPPER(:owner)
ORDER BY JOB_NAME";
            return GetDataTableAsync(connectionString, CommandType.Text, allSql, new[] { CreateParameter("owner", owner) }, cancellationToken);
        }

        public static Task<DataTable> GetSchedulerJobsAsync(string? owner = null, CancellationToken cancellationToken = default) =>
            GetSchedulerJobsAsync(GetDefaultConnectionString(), owner, cancellationToken);

        private static void EnsureOracleObjectName(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Object name is required.", paramName);
            if (!OracleObjectNameRegex.IsMatch(value.Trim()))
                throw new ArgumentException("Invalid Oracle object name.", paramName);
        }

        private static Dictionary<string, object?> CollectOutputValues(DbParameterCollection parameters)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DbParameter parameter in parameters)
            {
                if (parameter.Direction == ParameterDirection.Output ||
                    parameter.Direction == ParameterDirection.InputOutput ||
                    parameter.Direction == ParameterDirection.ReturnValue)
                {
                    var key = parameter.ParameterName?.Trim().TrimStart(':') ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(key))
                        continue;
                    result[key] = NormalizeDbValue(parameter.Value);
                }
            }
            return result;
        }

        private static object? NormalizeDbValue(object? value) =>
            value == null || value == DBNull.Value ? null : value;

        private static string GetDefaultConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_defaultConnectionString))
                throw new InvalidOperationException("OracleDbHelper default connection string not configured.");
            return _defaultConnectionString;
        }

        private static T Retry<T>(Func<T> action)
        {
            Exception? last = null;
            for (var attempt = 1; attempt <= MaxRetryCount; attempt++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (attempt >= MaxRetryCount) break;
                    if (attempt >= 2) Thread.Sleep(RetryWaitMilliseconds);
                }
            }

            throw last ?? new InvalidOperationException("Retry failed with unknown error.");
        }

        private static DbParameter[] CloneParameters(DbParameter[] source)
        {
            var clones = new DbParameter[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                var p = source[i];
                if (p is ICloneable cloneable)
                {
                    clones[i] = (DbParameter)cloneable.Clone();
                    continue;
                }

                var np = _providerFactory.CreateParameter() ?? throw new InvalidOperationException("Cannot create DbParameter.");
                np.ParameterName = p.ParameterName;
                np.DbType = p.DbType;
                np.Direction = p.Direction;
                np.Size = p.Size;
                np.Precision = p.Precision;
                np.Scale = p.Scale;
                np.SourceColumn = p.SourceColumn;
                np.SourceVersion = p.SourceVersion;
                np.IsNullable = p.IsNullable;
                np.Value = p.Value;
                clones[i] = np;
            }
            return clones;
        }
    }

    public sealed class SqlBatchItem
    {
        public string CommandText { get; set; } = string.Empty;
        public DbParameter[]? Parameters { get; set; }
    }

    public sealed class OracleSqlExecutor : IDisposable
    {
        private readonly OracleConnection _connection;

        public OracleSqlExecutor(string? connectionString = null)
        {
            var conn = string.IsNullOrWhiteSpace(connectionString)
                ? OracleDbHelper.DefaultConnectionString
                : connectionString;
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("OracleDbHelper default connection string not configured.");
            _connection = new OracleConnection(conn);
        }

        private void EnsureConnectionOpen()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public int ExecuteNonQuery(string sql, OracleParameter[]? parameters = null)
        {
            EnsureConnectionOpen();
            using var cmd = new OracleCommand(sql, _connection) { BindByName = true };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public DataTable GetDataTable(string sql, OracleParameter[]? parameters = null)
        {
            EnsureConnectionOpen();
            using var cmd = new OracleCommand(sql, _connection) { BindByName = true };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            using var adapter = new OracleDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public void Dispose()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
            _connection.Dispose();
        }
    }
}


