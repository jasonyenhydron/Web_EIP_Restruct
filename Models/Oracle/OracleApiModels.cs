namespace Web_EIP_Restruct.Models.Oracle
{
    public class OracleBindParameterRequest
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string? DbType { get; set; }
        public string? Direction { get; set; }
        public int? Size { get; set; }
    }

    public class OracleProcExecuteRequest
    {
        public string? PackageName { get; set; }
        public string ProcedureName { get; set; } = string.Empty;
        public List<OracleBindParameterRequest>? Parameters { get; set; }
    }

    public class OracleFunctionExecuteRequest
    {
        public string FunctionName { get; set; } = string.Empty;
        public string? ReturnDbType { get; set; }
        public List<OracleBindParameterRequest>? Parameters { get; set; }
    }

    public class OracleSchedulerJobCreateRequest
    {
        public string JobName { get; set; } = string.Empty;
        public string JobType { get; set; } = "PLSQL_BLOCK";
        public string JobAction { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string? RepeatInterval { get; set; }
        public bool Enabled { get; set; } = true;
        public bool AutoDrop { get; set; } = false;
        public string? Comments { get; set; }
    }

    public class OracleSchedulerJobActionRequest
    {
        public string JobName { get; set; } = string.Empty;
        public bool Force { get; set; } = false;
        public bool UseCurrentSession { get; set; } = false;
    }
}


