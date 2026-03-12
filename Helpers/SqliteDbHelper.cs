using System.Data;
using System.Data.Common;

namespace Web_EIP_Restruct.Helpers
{
    public static class SqliteDbHelper
    {
        private static readonly object SyncRoot = new();
        private static DbProviderFactory? _providerFactory;
        private static string _defaultConnectionString = string.Empty;

        public static string DefaultConnectionString => _defaultConnectionString;

        public static void Configure(string defaultConnectionString, DbProviderFactory? providerFactory = null)
        {
            lock (SyncRoot)
            {
                _defaultConnectionString = defaultConnectionString?.Trim() ?? string.Empty;
                _providerFactory = providerFactory ?? ResolveFactory();
            }
        }

        public static DbConnection CreateConnection(string? connectionString = null)
        {
            var factory = _providerFactory ?? ResolveFactory();
            var conn = factory.CreateConnection() ?? throw new InvalidOperationException("Cannot create SQLite DbConnection.");
            conn.ConnectionString = string.IsNullOrWhiteSpace(connectionString) ? GetDefaultConnectionString() : connectionString;
            return conn;
        }

        public static DbParameter CreateParameter(string name, object? value, DbType? dbType = null)
        {
            var factory = _providerFactory ?? ResolveFactory();
            var p = factory.CreateParameter() ?? throw new InvalidOperationException("Cannot create SQLite DbParameter.");
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            if (dbType.HasValue) p.DbType = dbType.Value;
            return p;
        }

        public static int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[]? parameters)
        {
            using var conn = CreateConnection();
            using var cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, cmdType, cmdText, parameters);
            return cmd.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[]? parameters)
        {
            using var conn = CreateConnection();
            using var cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, cmdType, cmdText, parameters);
            return cmd.ExecuteScalar();
        }

        public static DataTable GetDataTable(CommandType cmdType, string cmdText, params DbParameter[]? parameters)
        {
            var factory = _providerFactory ?? ResolveFactory();
            var dt = new DataTable();
            using var conn = CreateConnection();
            using var cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, cmdType, cmdText, parameters);
            using var adapter = factory.CreateDataAdapter() ?? throw new InvalidOperationException("Cannot create SQLite DbDataAdapter.");
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            return dt;
        }

        private static void PrepareCommand(DbCommand cmd, DbConnection conn, CommandType cmdType, string cmdText, DbParameter[]? parameters)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            cmd.Connection = conn;
            cmd.CommandType = cmdType;
            cmd.CommandText = cmdText;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(parameters);
            }
        }

        private static DbProviderFactory ResolveFactory()
        {
            var t = Type.GetType("Microsoft.Data.Sqlite.SqliteFactory, Microsoft.Data.Sqlite", throwOnError: false);
            if (t?.GetProperty("Instance")?.GetValue(null) is DbProviderFactory f)
                return f;

            try { return DbProviderFactories.GetFactory("Microsoft.Data.Sqlite"); } catch { }
            throw new InvalidOperationException("SQLite provider not found. Install Microsoft.Data.Sqlite.");
        }

        private static string GetDefaultConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_defaultConnectionString))
                throw new InvalidOperationException("SqliteDbHelper default connection string not configured.");
            return _defaultConnectionString;
        }
    }
}

