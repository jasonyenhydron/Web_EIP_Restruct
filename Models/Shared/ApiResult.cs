namespace Web_EIP_Restruct.Models.Shared;

public sealed class ApiResult<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static ApiResult<T> Ok(T data, string message = "") => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResult<T> Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}
