namespace CheckInSystem.Application.Common.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int Code { get; init; }
    public T? Data { get; init; }
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string message = "Success", int code = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Code = code,
            Data = data
        };
    }

    public static ApiResponse<object?> Fail(string message, int code, object? data = null)
    {
        return new ApiResponse<object?>
        {
            Success = false,
            Message = message,
            Code = code,
            Data = data
        };
    }
}
