using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace Web_EIP_Restruct.Helpers
{
    public static class DbHelper
    {
        private static string _provider = "oracle";
        private static IConfiguration? _configuration;

        public static string Provider => _provider;

        public static void Configure(IConfiguration configuration)
        {
            _configuration = configuration;
            var provider = (configuration["Database:Provider"] ?? "oracledbhelper").Trim().ToLowerInvariant();
            _provider = provider switch
            {
                "oracle" => "oracle",
                "oracledbhelper" => "oracle",
                "sqlsrv" => "sqlsrv",
                "sqlserver" => "sqlsrv",
                "sqlsrvdbhelper" => "sqlsrv",
                "sqlite" => "sqlite",
                "sqllitedbhelper" => "sqlite",
                "msyql" => "msyql",
                "mysql" => "msyql",
                "mysqldbhelper" => "msyql",
                "msyqldbhelper" => "msyql",
                _ => "oracle"
            };

            switch (_provider)
            {
                case "sqlsrv":
                {
                    var sqlSrvConn = configuration.GetConnectionString("sqlsrvConn");
                    if (string.IsNullOrWhiteSpace(sqlSrvConn))
                        throw new InvalidOperationException("ConnectionStrings:sqlsrvConn is required when Database:Provider=sqlsrv.");
                    SqlSrvDbHelper.Configure(sqlSrvConn);
                    break;
                }
                case "sqlite":
                {
                    var sqliteConn = configuration.GetConnectionString("sqliteConn");
                    if (string.IsNullOrWhiteSpace(sqliteConn))
                        throw new InvalidOperationException("ConnectionStrings:sqliteConn is required when Database:Provider=sqlite.");
                    SqliteDbHelper.Configure(sqliteConn);
                    break;
                }
                case "msyql":
                {
                    var mysqlConn = configuration.GetConnectionString("mysqlConn");
                    if (string.IsNullOrWhiteSpace(mysqlConn))
                        throw new InvalidOperationException("ConnectionStrings:mysqlConn is required when Database:Provider=msyql/mysql.");
                    MsyqlDbHelper.Configure(mysqlConn);
                    break;
                }
                default:
                {
                    var oracleConn = configuration.GetConnectionString("oracleConn");
                    if (string.IsNullOrWhiteSpace(oracleConn))
                        throw new InvalidOperationException("ConnectionStrings:oracleConn is required when Database:Provider=oracle.");
                    OracleDbHelper.Configure(oracleConn);
                    break;
                }
            }
        }

        public static string DefaultConnectionString => _provider switch
        {
            "sqlsrv" => SqlSrvDbHelper.DefaultConnectionString,
            "sqlite" => SqliteDbHelper.DefaultConnectionString,
            "msyql" => MsyqlDbHelper.DefaultConnectionString,
            _ => OracleDbHelper.DefaultConnectionString
        };

        public static string BuildConnectionString(string? tns = null)
        {
            var conn = DefaultConnectionString;
            if (_provider != "oracle" || string.IsNullOrWhiteSpace(tns))
                return conn;

            var tnsKey = tns.Trim().ToUpperInvariant();
            var namedConn = _configuration?.GetConnectionString($"oracleConn_{tnsKey}");
            if (!string.IsNullOrWhiteSpace(namedConn))
                return namedConn;

            var builder = new DbConnectionStringBuilder { ConnectionString = conn };
            builder["Data Source"] = tns.Trim();
            return builder.ConnectionString;
        }

        public static DbParameter CreateParameter(
            string name,
            object? value,
            DbType? dbType = null,
            ParameterDirection direction = ParameterDirection.Input,
            int? size = null)
        {
            return _provider switch
            {
                "sqlsrv" => SqlSrvDbHelper.CreateParameter(name, value, dbType),
                "sqlite" => SqliteDbHelper.CreateParameter(name, value, dbType),
                "msyql" => MsyqlDbHelper.CreateParameter(name, value, dbType),
                _ => OracleDbHelper.CreateParameter(name, value, dbType, direction, size)
            };
        }

        public static DataTable GetDataTable(CommandType cmdType, string cmdText, params DbParameter[]? parameters)
        {
            return _provider switch
            {
                "sqlsrv" => SqlSrvDbHelper.GetDataTable(cmdType, cmdText, parameters),
                "sqlite" => SqliteDbHelper.GetDataTable(cmdType, cmdText, parameters),
                "msyql" => MsyqlDbHelper.GetDataTable(cmdType, cmdText, parameters),
                _ => OracleDbHelper.GetDataTable(cmdType, cmdText, parameters)
            };
        }

        public static Task<DataTable> GetDataTableAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            return _provider switch
            {
                "sqlsrv" => Task.FromResult(SqlSrvDbHelper.GetDataTable(cmdType, cmdText, parameters)),
                "sqlite" => Task.FromResult(SqliteDbHelper.GetDataTable(cmdType, cmdText, parameters)),
                "msyql" => Task.FromResult(MsyqlDbHelper.GetDataTable(cmdType, cmdText, parameters)),
                _ => OracleDbHelper.GetDataTableAsync(connectionString, cmdType, cmdText, parameters, cancellationToken)
            };
        }

        public static Task<int> ExecuteNonQueryAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            return _provider switch
            {
                "sqlsrv" => Task.FromResult(SqlSrvDbHelper.ExecuteNonQuery(cmdType, cmdText, parameters)),
                "sqlite" => Task.FromResult(SqliteDbHelper.ExecuteNonQuery(cmdType, cmdText, parameters)),
                "msyql" => Task.FromResult(MsyqlDbHelper.ExecuteNonQuery(cmdType, cmdText, parameters)),
                _ => OracleDbHelper.ExecuteNonQueryAsync(connectionString, cmdType, cmdText, parameters, cancellationToken)
            };
        }

        public static Task<object?> ExecuteScalarAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            return _provider switch
            {
                "sqlsrv" => Task.FromResult(SqlSrvDbHelper.ExecuteScalar(cmdType, cmdText, parameters)),
                "sqlite" => Task.FromResult(SqliteDbHelper.ExecuteScalar(cmdType, cmdText, parameters)),
                "msyql" => Task.FromResult(MsyqlDbHelper.ExecuteScalar(cmdType, cmdText, parameters)),
                _ => OracleDbHelper.ExecuteScalarAsync(connectionString, cmdType, cmdText, parameters, cancellationToken)
            };
        }
    }
}

